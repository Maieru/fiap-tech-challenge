"""Exercise the built image with an isolated PostgreSQL network; clean only owned resources."""
import base64
import hashlib
import hmac
import json
from pathlib import Path
import secrets
import subprocess
import sys
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
image = sys.argv[1] if len(sys.argv) > 1 else 'fiap-os:local'
suffix = secrets.token_hex(5)
network, database, api = (f'os-smoke-{part}-{suffix}' for part in ['net', 'db', 'api'])
owned_containers = []
network_created = False


def docker(*args):
    return subprocess.run(['docker', *args], check=True, capture_output=True, text=True).stdout.strip()


def token(key):
    def encode(value):
        return base64.urlsafe_b64encode(json.dumps(value, separators=(',', ':')).encode()).rstrip(b'=')
    content = encode({'alg': 'HS256', 'typ': 'JWT'}) + b'.' + encode({
        'iss': 'smoke-issuer', 'aud': 'smoke-audience', 'sub': 'smoke-operator', 'exp': int(time.time()) + 600
    })
    signature = base64.urlsafe_b64encode(hmac.new(key.encode(), content, hashlib.sha256).digest()).rstrip(b'=')
    return (content + b'.' + signature).decode()


try:
    docker('network', 'create', network)
    network_created = True
    password = secrets.token_hex(24)
    key = secrets.token_hex(32)
    docker('run', '-d', '--name', database, '--network', network, '--network-alias', 'database',
           '-e', 'POSTGRES_DB=fiap_os', '-e', 'POSTGRES_USER=fiap_os', '-e', f'POSTGRES_PASSWORD={password}', 'postgres:18.3')
    owned_containers.append(database)
    for _ in range(120):
        check = subprocess.run(['docker', 'exec', database, 'pg_isready', '-h', '127.0.0.1', '-U', 'fiap_os', '-d', 'fiap_os'], capture_output=True)
        if check.returncode == 0:
            break
        time.sleep(.25)
    else:
        raise TimeoutError('PostgreSQL did not become ready')
    docker('run', '-d', '--name', api, '--network', network, '-p', '127.0.0.1::8080',
           '-e', 'ASPNETCORE_ENVIRONMENT=Development', '-e', 'ASPNETCORE_HTTP_PORTS=8080',
           '-e', f'ConnectionStrings__DefaultConnection=Host=database;Database=fiap_os;Username=fiap_os;Password={password}',
           '-e', 'Database__ApplyMigrations=true', '-e', 'Jwt__Issuer=smoke-issuer', '-e', 'Jwt__Audience=smoke-audience',
           '-e', f'Jwt__SigningKey={key}', image)
    owned_containers.append(api)
    port = json.loads(docker('inspect', api))[0]['NetworkSettings']['Ports']['8080/tcp'][0]['HostPort']
    base = f'http://127.0.0.1:{port}'
    access_token = token(key)

    def request(method, path, data=None, authenticated=True):
        headers = {'X-Correlation-ID': 'image-smoke'}
        if authenticated:
            headers['Authorization'] = 'Bearer ' + access_token
        body = None if data is None else json.dumps(data).encode()
        if body is not None:
            headers['Content-Type'] = 'application/json'
        req = urllib.request.Request(base + path, data=body, headers=headers, method=method)
        with urllib.request.urlopen(req, timeout=5) as response:
            text = response.read()
            return response.status, json.loads(text) if text else None

    def wait_ready():
        for _ in range(120):
            try:
                status, health = request('GET', '/api/os/v1/health/ready', authenticated=False)
                if status == 200 and health['status'] == 'Healthy':
                    return
            except (OSError, urllib.error.URLError):
                time.sleep(.25)
        raise TimeoutError('Image did not become ready')

    wait_ready()
    status, customer = request('POST', '/api/os/v1/clientes', {'nome': 'Cliente Imagem', 'cpf': '52998224725', 'telefone': '11987654321'})
    assert status == 201
    status, vehicle = request('POST', '/api/os/v1/veiculos', {'clienteId': customer['id'], 'placa': 'ABC1234', 'marca': 'Toyota', 'modelo': 'Corolla', 'ano': 2024})
    assert status == 201
    status, order = request('POST', '/api/os/v1/ordens', {'clienteId': customer['id'], 'veiculoId': vehicle['id'], 'descricaoProblema': 'Verificar motor'})
    assert status == 201
    assert request('PUT', '/api/os/v1/clientes/' + customer['id'], {'nome': 'Nome Atualizado', 'telefone': '11987654321'})[0] == 200
    assert request('DELETE', '/api/os/v1/clientes/' + customer['id'])[0] == 204
    assert request('DELETE', '/api/os/v1/veiculos/' + vehicle['id'])[0] == 204
    try:
        request('GET', '/api/os/v1/clientes', authenticated=False)
        raise AssertionError('Unauthenticated call was accepted')
    except urllib.error.HTTPError as error:
        assert error.code == 401
    docker('restart', api)
    # Docker may allocate a different host port after restart when HostPort was empty.
    port = json.loads(docker('inspect', api))[0]['NetworkSettings']['Ports']['8080/tcp'][0]['HostPort']
    base = f'http://127.0.0.1:{port}'
    wait_ready()
    assert request('GET', '/api/os/v1/ordens/' + order['id'])[1]['clienteId'] == customer['id']
    history = request('GET', '/api/os/v1/ordens/' + order['id'] + '/historico')[1]
    assert len(history) == 1 and history[0]['correlationId'] == 'image-smoke'
    spec = request('GET', '/openapi/v1.json', authenticated=False)[1]
    assert 'delete' in spec['paths']['/api/os/v1/clientes/{id}']
    assert 'put' in spec['paths']['/api/os/v1/veiculos/{id}']
    (ROOT / 'artifacts').mkdir(exist_ok=True)
    (ROOT / 'artifacts/os-openapi.json').write_text(json.dumps(spec, ensure_ascii=False, indent=2), encoding='utf-8')
    print('OS image verified: CRUD, JWT, migrations, soft delete, history, OpenAPI and persistence after restart.')
except Exception:
    if api in owned_containers:
        print(docker('logs', api), file=sys.stderr)
    raise
finally:
    for container in reversed(owned_containers):
        subprocess.run(['docker', 'rm', '-f', container], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if network_created:
        subprocess.run(['docker', 'network', 'rm', network], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
