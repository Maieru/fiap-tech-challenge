"""Run the sample built from PackageReference, then verify health, correlation and JSON logs."""
import json
from pathlib import Path
import socket
import subprocess
import time
import urllib.request
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
sample = ROOT / 'src/Chassis/Maieru.FiapTechChallenge.Chassis.Sample'
assert not ET.parse(sample / 'Maieru.FiapTechChallenge.Chassis.Sample.csproj').findall('.//ProjectReference')
with socket.socket() as listener:
    listener.bind(('127.0.0.1', 0))
    port = listener.getsockname()[1]
url = f'http://127.0.0.1:{port}'
log = ROOT / 'artifacts/chassis-sample.log'
log.parent.mkdir(parents=True, exist_ok=True)
with log.open('w', encoding='utf-8') as output:
    process = subprocess.Popen(['dotnet', str(sample / 'bin/Release/net10.0/Maieru.FiapTechChallenge.Chassis.Sample.dll'), '--urls', url],
                               stdout=output, stderr=subprocess.STDOUT, cwd=ROOT)
    try:
        for attempt in range(100):
            if process.poll() is not None:
                raise RuntimeError('Sample exited before readiness; see artifacts/chassis-sample.log')
            try:
                with urllib.request.urlopen(url + '/health/ready', timeout=1) as response:
                    assert json.load(response)['status'] == 'Healthy'
                break
            except OSError:
                time.sleep(0.1)
        else:
            raise TimeoutError('Sample did not become ready')
        request = urllib.request.Request(url, headers={'X-Correlation-ID': 'nuget-consumer-smoke'})
        with urllib.request.urlopen(request, timeout=5) as response:
            assert response.headers['X-Correlation-ID'] == 'nuget-consumer-smoke'
            assert json.load(response)['service'] == 'chassis-sample'
    finally:
        process.terminate()
        process.wait(timeout=10)
records = [json.loads(line) for line in log.read_text(encoding='utf-8').splitlines() if line.startswith('{')]
assert records and all('LogLevel' in record for record in records)
assert any('nuget-consumer-smoke' in json.dumps(record) for record in records)
print('Packaged consumer: HTTP health, correlation and JSON logs verified.')
