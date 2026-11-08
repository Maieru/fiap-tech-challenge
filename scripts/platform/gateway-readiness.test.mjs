import {test} from 'node:test';
import assert from 'node:assert/strict';
import {createServer} from 'node:http';
import {once} from 'node:events';
import {waitForGateway} from './gateway-readiness.mjs';

async function gateway(t, handler) {
  const requests = [];
  const server = createServer((request,response) => {
    requests.push({method:request.method,path:request.url,authorization:request.headers.authorization});
    if (handler?.(request,response)) return;
    if (request.url.endsWith('/health/ready')) {
      response.writeHead(200,{'content-type':'application/json'}).end(JSON.stringify({status:'Healthy'}));
    } else if (request.url === '/') {
      response.writeHead(200,{'content-type':'text/html'}).end('<!doctype html>');
    } else response.writeHead(401,{'www-authenticate':'Bearer'}).end();
  });
  server.listen(0,'127.0.0.1');
  await once(server,'listening');
  t.after(() => new Promise(resolve => { server.close(resolve); server.closeAllConnections(); }));
  return {base:'http://127.0.0.1:'+server.address().port,requests};
}
const options = {timeoutMs:3000,intervalMs:5,requestTimeoutMs:500,consecutivePasses:2,report:()=>{}};

test('aguarda 401 estavel mesmo quando readiness ja passou e a rota protegida retorna 503', async t => {
  let failures = 1;
  const app = await gateway(t,(request,response) => {
    if (request.url === '/api/os/v1/clientes' && failures-- > 0) { response.writeHead(503).end(); return true; }
  });
  await waitForGateway(app.base,options);
  assert.equal(app.requests.filter(r=>r.path==='/api/os/v1/clientes').length,3);
  assert.equal(app.requests.filter(r=>r.path==='/api/billing/v1/cobrancas').length,2);
  assert.ok(app.requests.every(r=>r.method==='GET' && r.authorization===undefined));
});

test('reinicia a janela de estabilidade apos nova falha transitoria', async t => {
  let rounds = 0;
  const app = await gateway(t,(request,response) => {
    if (request.url==='/api/os/v1/health/ready' && ++rounds===2) { response.writeHead(502).end(); return true; }
  });
  await waitForGateway(app.base,options);
  assert.equal(rounds,4);
});

test('falha imediatamente se uma rota protegida aceitar acesso sem token', async t => {
  const app = await gateway(t,(request,response) => {
    if (request.url==='/api/os/v1/clientes') { response.writeHead(200).end('[]'); return true; }
  });
  await assert.rejects(waitForGateway(app.base,options),/clientes: HTTP 200; esperado 401/);
  assert.equal(app.requests.filter(r=>r.path==='/api/os/v1/clientes').length,1);
});

test('503 persistente encerra no prazo e inclui o requestId', async t => {
  const messages = [];
  const app = await gateway(t,(_request,response) => {
    response.writeHead(503,{'apigw-requestid':'gateway-test-id'}).end(); return true;
  });
  await assert.rejects(waitForGateway(app.base,{...options,timeoutMs:250,report:message=>messages.push(message)}),/nao estabilizou/);
  assert.match(messages.join('\n'),/HTTP 503.*requestId=gateway-test-id/);
});

test('repete somente a verificacao inicial apos timeout de conexao', async t => {
  let held = false;
  const app = await gateway(t,(request,_response) => {
    if (!held && request.url==='/api/os/v1/health/ready') { held=true; return true; }
  });
  await waitForGateway(app.base,{...options,requestTimeoutMs:100});
  assert.ok(app.requests.filter(r=>r.path==='/api/os/v1/health/ready').length>=3);
  assert.ok(app.requests.every(r=>r.method==='GET'));
});

test('nao aceita HTML de fallback no endpoint de readiness', async t => {
  const app = await gateway(t,(_request,response) => {
    response.writeHead(200,{'content-type':'text/html'}).end('<!doctype html>'); return true;
  });
  await assert.rejects(waitForGateway(app.base,options),/readiness invalida/);
});
