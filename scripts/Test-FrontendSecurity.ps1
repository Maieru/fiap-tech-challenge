# Requer PowerShell 7 e Docker. Usa apenas containers/rede efêmeros deste teste.
param([string]$Image = 'fiap-frontend:sonar-remediation')

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$project = 'fiap-frontend-tls-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
$fixture = Join-Path ([IO.Path]::GetTempPath()) $project
New-Item -ItemType Directory -Path $fixture | Out-Null
$compose = Join-Path $fixture 'compose.yaml'
$previousImage = $env:FRONTEND_SECURITY_IMAGE

function Invoke-Compose {
    & docker compose -p $project -f $compose @args
    if ($LASTEXITCODE -ne 0) { throw "Docker Compose falhou: $args" }
}

try {
    # CA e certificado válidos somente para este teste, sem importar na máquina.
    $caKey = [Security.Cryptography.RSA]::Create(2048)
    $caRequest = [Security.Cryptography.X509Certificates.CertificateRequest]::new(
        'CN=Frontend test CA', $caKey, [Security.Cryptography.HashAlgorithmName]::SHA256,
        [Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $caRequest.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new($true, $false, 0, $true))
    $caRequest.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new('KeyCertSign', $true))
    $start = [DateTimeOffset]::UtcNow.AddMinutes(-5)
    $ca = $caRequest.CreateSelfSigned($start, $start.AddDays(2))
    $serverKey = [Security.Cryptography.RSA]::Create(2048)
    $serverRequest = [Security.Cryptography.X509Certificates.CertificateRequest]::new(
        'CN=upstream', $serverKey, [Security.Cryptography.HashAlgorithmName]::SHA256,
        [Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $san = [Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new()
    $san.AddDnsName('upstream')
    $serverRequest.CertificateExtensions.Add($san.Build())
    $server = $serverRequest.Create($ca, $start, $start.AddDays(1),
        [Security.Cryptography.RandomNumberGenerator]::GetBytes(16))
    [IO.File]::WriteAllText((Join-Path $fixture 'ca.crt'), $ca.ExportCertificatePem())
    [IO.File]::WriteAllText((Join-Path $fixture 'server.crt'), $server.ExportCertificatePem())
    [IO.File]::WriteAllText((Join-Path $fixture 'server.key'), $serverKey.ExportPkcs8PrivateKeyPem())
    @'
server {
    listen 8443 ssl;
    ssl_certificate /fixture/server.crt;
    ssl_certificate_key /fixture/server.key;
    default_type application/json;
    location / {
        return 200 '{"uri":"$request_uri","authorization":"$http_authorization"}';
    }
}
'@ | Set-Content -LiteralPath (Join-Path $fixture 'upstream.conf')
    @'
services:
  upstream:
    image: ${FRONTEND_SECURITY_IMAGE}
    entrypoint: ["nginx", "-g", "daemon off;"]
    volumes:
      - ./upstream.conf:/etc/nginx/conf.d/default.conf:ro
      - ./:/fixture:ro
    networks:
      default:
        aliases: [upstream-wrong-name]
  trusted:
    image: ${FRONTEND_SECURITY_IMAGE}
    cap_drop: [ALL]
    security_opt: ["no-new-privileges:true"]
    ports: ["127.0.0.1::8080"]
    healthcheck: &ready
      test: ["CMD", "wget", "-q", "-O", "/dev/null", "http://127.0.0.1:8080/"]
      interval: 1s
      timeout: 2s
      retries: 20
    environment: &upstreams
      OS_UPSTREAM: https://upstream:8443
      EXECUTION_UPSTREAM: https://upstream:8443
      BILLING_UPSTREAM: https://upstream:8443
      OTEL_EXPORTER_OTLP_ENDPOINT: https://upstream:8443
      UPSTREAM_CA_CERTIFICATE: /fixture/ca.crt
    volumes: [".:/fixture:ro"]
    depends_on: [upstream]
  untrusted:
    image: ${FRONTEND_SECURITY_IMAGE}
    healthcheck: *ready
    ports: ["127.0.0.1::8080"]
    environment:
      <<: *upstreams
      UPSTREAM_CA_CERTIFICATE: /etc/ssl/certs/ca-certificates.crt
    depends_on: [upstream]
  wrong-name:
    image: ${FRONTEND_SECURITY_IMAGE}
    healthcheck: *ready
    ports: ["127.0.0.1::8080"]
    environment:
      <<: *upstreams
      OS_UPSTREAM: https://upstream-wrong-name:8443
    volumes: [".:/fixture:ro"]
    depends_on: [upstream]
'@ | Set-Content -LiteralPath $compose
    $env:FRONTEND_SECURITY_IMAGE = $Image
    Invoke-Compose up -d --wait --wait-timeout 30 | Out-Null
    $trusted = 'http://' + (Invoke-Compose port trusted 8080).Trim()
    $untrusted = 'http://' + (Invoke-Compose port untrusted 8080).Trim()
    $wrongName = 'http://' + (Invoke-Compose port wrong-name 8080).Trim()
    $uid = (Invoke-Compose exec -T trusted id -u).Trim()
    if ($uid -ne '101') { throw "Nginx deve executar sem root; UID=$uid" }
    Invoke-Compose exec -T trusted nginx -t
    foreach ($path in @('/api/os/v1/probe?check=1', '/api/execution/v1/probe', '/api/billing/v1/probe')) {
        $response = Invoke-RestMethod ($trusted + $path) -Headers @{Authorization='Bearer synthetic-test'}
        if ($response.uri -ne $path -or $response.authorization -ne 'Bearer synthetic-test') {
            throw "Proxy não preservou caminho e Authorization: $path"
        }
    }
    $telemetry = Invoke-RestMethod ($trusted + '/otlp/v1/traces')
    if ($telemetry.uri -ne '/v1/traces') { throw 'Rota OTLP incorreta' }
    foreach ($origin in @($untrusted, $wrongName)) {
        $response = Invoke-WebRequest ($origin + '/api/os/v1/probe') -SkipHttpErrorCheck
        if ($response.StatusCode -ne 502) { throw "Certificado inválido deveria produzir 502: $origin" }
    }
    $caLog = (Invoke-Compose logs --no-color --no-log-prefix untrusted) -join "`n"
    $nameLog = (Invoke-Compose logs --no-color --no-log-prefix wrong-name) -join "`n"
    if (-not $caLog.Contains('upstream SSL certificate verify error') -or
        -not $nameLog.Contains('upstream SSL certificate does not match')) {
        throw 'As respostas 502 devem ser causadas pela validação TLS, não por falha de rede.'
    }
    Write-Host 'PASS: UID 101, Nginx válido, TLS com CA confiável, rotas e Authorization preservados; CA desconhecida e hostname incorreto rejeitados.'
}
catch {
    if (Test-Path -LiteralPath $compose) { Invoke-Compose logs --tail 20 }
    throw
}
finally {
    if (Test-Path -LiteralPath $compose) { Invoke-Compose down -v | Out-Null }
    $env:FRONTEND_SECURITY_IMAGE = $previousImage
    foreach ($resource in @($server, $serverKey, $ca, $caKey)) {
        if ($null -ne $resource) { $resource.Dispose() }
    }
    # O caminho é criado pelo teste diretamente sob o diretório temporário.
    $resolved = [IO.Path]::GetFullPath($fixture)
    $expected = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) $project))
    if ($resolved -eq $expected -and (Split-Path $resolved -Leaf) -eq $project) {
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}
