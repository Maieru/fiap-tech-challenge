param([string]$Version = ('0.1.0-local.' + [DateTime]::UtcNow.ToString('yyyyMMddHHmmssfff')))
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+(-[a-zA-Z0-9.-]+)?$') { throw 'Invalid package version.' }
$repository = Split-Path $PSScriptRoot -Parent
Push-Location $repository
try {
    New-Item -ItemType Directory -Force artifacts/packages | Out-Null
    dotnet pack src/Chassis/Maieru.FiapTechChallenge.Chassis/Maieru.FiapTechChallenge.Chassis.csproj -c Release -o artifacts/packages "-p:Version=$Version"
    if ($LASTEXITCODE -ne 0) { throw 'Chassis pack failed.' }
    Set-Content artifacts/chassis-version.txt $Version -Encoding utf8NoBOM
    Write-Host "Local package ready: $Version. Consumers use PackageReference and the local feed."
} finally { Pop-Location }
