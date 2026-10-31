param(
    [Parameter(Mandatory)][ValidateSet('legacy', 'chassis', 'os')][string]$Component,
    [string]$ChassisVersion,
    [string]$ResultsDirectory = 'artifacts/verification',
    [string]$Python = 'python'
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
Push-Location $repository
try {
    if (!$ChassisVersion) { $ChassisVersion = (Get-Content artifacts/chassis-version.txt -Raw).Trim() }
    $projects = switch ($Component) {
        'legacy' {
            'Domain', 'Application', 'Infaestructure', 'API', 'PostgreSQL' | ForEach-Object {
                "src/FIAP.TechChallenge.Fase1.$_.Tests/FIAP.TechChallenge.Fase1.$_.Tests.csproj"
            }
        }
        'chassis' { 'src/Chassis/Maieru.FiapTechChallenge.Chassis.Tests/Maieru.FiapTechChallenge.Chassis.Tests.csproj' }
        'os' {
            'Domain', 'Application', 'Integration' | ForEach-Object {
                "src/Services/OrdensServico/FIAP.TechChallenge.OS.$_.Tests/FIAP.TechChallenge.OS.$_.Tests.csproj"
            }
        }
    }
    # A unique run prevents stale coverage from masking changes or failed tests.
    $output = Join-Path $ResultsDirectory ($Component + '/' + [Guid]::NewGuid().ToString('N'))
    foreach ($project in $projects) {
        $name = [IO.Path]::GetFileNameWithoutExtension($project)
        dotnet test $project -c Release "-p:ChassisVersion=$ChassisVersion" --settings coverage.runsettings --collect:'XPlat Code Coverage' --results-directory "$output/$name" --logger 'trx' --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Tests failed: $project" }
    }
    & $Python scripts/check_coverage.py $Component $output --minimum 80 --output "$output/summary.json"
    if ($LASTEXITCODE -ne 0) { throw "Coverage gate failed: $Component" }
    if ($Component -eq 'chassis') {
        dotnet build src/Chassis/Maieru.FiapTechChallenge.Chassis.Sample/Maieru.FiapTechChallenge.Chassis.Sample.csproj -c Release "-p:ChassisVersion=$ChassisVersion"
        if ($LASTEXITCODE -ne 0) { throw 'Package consumer build failed.' }
        & $Python scripts/smoke_chassis.py
        if ($LASTEXITCODE -ne 0) { throw 'Packaged consumer smoke test failed.' }
    }
} finally { Pop-Location }
