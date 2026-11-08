import {test} from 'node:test';
import assert from 'node:assert/strict';
import {createServer} from 'node:http';
import {once} from 'node:events';
import {randomUUID} from 'node:crypto';
import {runSmoke} from './smoke-flow.mjs';

// Servidor de contrato do script; nao substitui os testes distribuidos das APIs/Kafka.
async function platform(t, options = {}) {
  const calls = [], orders = [], services = new Map(), parts = new Map();
  let billingReads = 0;
  const server = createServer(async (request, response) => {
    let raw = '';
    for await (const chunk of request) raw += chunk;
    const body = raw ? JSON.parse(raw) : undefined;
    const {method, url:path} = request;
    calls.push({method, path, body, authorization:request.headers.authorization,
      correlation:request.headers['x-correlation-id']});
    const send = (status, data) => response.writeHead(status, {'content-type':'application/json'})
      .end(data === undefined ? undefined : JSON.stringify(data));
    if (path.endsWith('/health/ready')) return send(200, {status:'Healthy'});
    if (path === '/') return response.writeHead(200, {'content-type':'text/html'}).end('<!doctype html>');
    if (method === 'POST' && path === '/api/os/v1/usuarios') return send(201, {id:'smoke-user'});
    if (path === '/api/os/v1/usuarios/login') return send(200, {token:'test-jwt'});
    if (request.headers.authorization !== 'Bearer test-jwt') return send(401);
    if (method === 'DELETE') return send(options.cleanupFailure ? 503 : 204);
    if (method === 'GET' && ['/api/execution/v1/servicos','/api/billing/v1/cobrancas'].includes(path)) return send(200, []);
    if (method === 'POST' && ['/api/os/v1/clientes','/api/os/v1/veiculos'].includes(path)) return send(201, {id:randomUUID()});
    if (method === 'POST' && path === '/api/os/v1/ordens') {
      if (options.writeFailure) return send(503);
      const order = {id:randomUUID(), budget:randomUUID(), charge:randomUUID(), status:1};
      orders.push(order);
      return send(201, {id:order.id});
    }
    if (method === 'POST' && path === '/api/execution/v1/servicos') {
      const id = randomUUID();
      services.set(id, body);
      return send(201, {id});
    }
    if (method === 'POST' && path === '/api/execution/v1/pecas-insumos') {
      const id = randomUUID();
      parts.set(id, {...body});
      return send(201, {id});
    }
    if (method === 'GET' && path.startsWith('/api/execution/v1/pecas-insumos/'))
      return send(200, parts.get(path.split('/').at(-1)));
    const order = orders.find(order => path.includes('/' + order.id));
    if (!order) return send(500, {error:'Rota inesperada no servidor de contrato'});
    const amount = () => (services.get(order.service)?.valorUnitario ?? 0) + (parts.get(order.part)?.precoUnitario ?? 0) * 2;
    if (method === 'POST' && path.endsWith('/addservico')) { order.service = body.servicoId; return send(201, {}); }
    if (method === 'POST' && path.endsWith('/addpecainsumo')) {
      order.part = body.pecaInsumoId;
      parts.get(order.part).quantidadeEstoque -= body.quantidade;
      return send(201, {});
    }
    if (method === 'PUT') {
      if (path.endsWith('/solicitar-aprovacao')) order.status = 3;
      if (path.endsWith('/aprovar-execucao')) {
        if (amount() === 0 && !options.blockAuthorization) order.status = 4;
        if (amount() > 0 && options.prematureAuthorization) order.status = 4;
        return send(202, {});
      }
      if (path.endsWith('/finalizar')) order.status = 5;
      return send(200, {});
    }
    if (path.endsWith('/cobranca')) {
      if (amount() > 0 && billingReads++ < (options.delayedCheckout ? 2 : 0))
        return send(billingReads === 1 ? 404 : 503);
      return send(200, {id:order.charge, orcamentoId:order.budget, valor:amount(), moeda:'BRL',
        situacao:amount() > 0 ? 0 : 1, gatewayOrderId:amount() > 0 ? 'ORD-test' : null,
        checkoutUrl:amount() > 0 ? (options.invalidCheckout ? 'http://invalid.test' : 'https://checkout.test/pay') : null,
        gatewayPaymentId:amount() === 0 ? 'sem-cobranca-' + order.id : null});
    }
    if (path.startsWith('/api/os/')) return send(200, {
      status:order.status, orcamentoId:order.budget,
      pagamento:{etapa:amount() > 0 ? 0 : 3, valor:amount(), cobrancaId:order.charge}
    });
    return send(200, {execucao:{status:order.status, servicos:[{id:'service-item'}]}, valorTotal:amount()});
  });
  server.listen(0, '127.0.0.1');
  await once(server, 'listening');
  t.after(() => new Promise(resolve => { server.close(resolve); server.closeAllConnections(); }));
  return {base:'http://127.0.0.1:' + server.address().port, calls, orders};
}

const config = {correlation:'smoke-regression', waitTimeoutMs:1000, intervalMs:5,
  readiness:{intervalMs:5, timeoutMs:3000}, report:() => {}};

test('valida checkout pendente e conclui somente a ordem zero, sem webhook artificial', async t => {
  const app = await platform(t, {delayedCheckout:true});
  const messages = [];
  const result = await runSmoke(app.base, {...config, report:message => messages.push(message)});
  const [paid, free] = app.orders;
  assert.equal(paid.status, 3);
  assert.equal(free.status, 5);
  assert.deepEqual(app.calls.filter(c => c.path === '/api/execution/v1/servicos' && c.method === 'POST')
    .map(c => c.body.valorUnitario), [100.25, 0]);
  assert.deepEqual(app.calls.filter(c => c.path === '/api/execution/v1/pecas-insumos' && c.method === 'POST')
    .map(c => c.body.precoUnitario), [20.15, 0]);
  assert.equal(app.calls.filter(c => c.path.endsWith(paid.id + '/cobranca')).length, 3);
  assert.equal(app.calls.filter(c => c.path.endsWith('/finalizar')).length, 1);
  assert.ok(app.calls.find(c => c.path.endsWith(free.id + '/finalizar')));
  assert.ok(app.calls.every(c => !c.path.includes('webhooks')));
  assert.ok(app.calls.every(c => c.correlation === config.correlation));
  assert.equal(app.calls.at(-1).method, 'DELETE');
  assert.match(result, /checkout pendente.*valor zero.*aceite manual pendente/);
  assert.doesNotMatch(messages.join('\n'), /test-jwt|https:\/\/checkout.test/);
});

test('falha se o checkout estiver invalido, sem anunciar sucesso', async t => {
  const app = await platform(t, {invalidCheckout:true});
  await assert.rejects(runSmoke(app.base, config), /Checkout pendente\/valor\/estoque divergentes/);
  assert.equal(app.orders.length, 1);
  assert.equal(app.calls.at(-1).method, 'DELETE');
});

test('rejeita inicio de execucao antes da confirmacao do pagamento', async t => {
  const app = await platform(t, {prematureAuthorization:true});
  await assert.rejects(runSmoke(app.base, config), /Checkout pendente\/valor\/estoque divergentes/);
  assert.equal(app.calls.filter(c => c.path.endsWith('/finalizar')).length, 0);
});

test('timeout informa etapa, status e correlacao; falha de limpeza nao esconde a causa', async t => {
  const app = await platform(t, {blockAuthorization:true, cleanupFailure:true});
  const messages = [];
  await assert.rejects(runSmoke(app.base, {...config, waitTimeoutMs:200, report:message => messages.push(message)}),
    /Prazo excedido aguardando autorizacao da execucao de valor zero:.*ultimo estado: HTTP 200;.*"status":3.*correlacao smoke-regression/);
  assert.ok(messages.some(message => /Falha adicional.*HTTP 503/.test(message)));
  assert.equal(app.calls.filter(c => c.path.endsWith('/finalizar')).length, 0);
});

test('HTTP 503 em escrita permanece erro imediato e nao repete POST', async t => {
  const app = await platform(t, {writeFailure:true});
  await assert.rejects(runSmoke(app.base, config), /POST \/api\/os\/v1\/ordens retornou HTTP 503/);
  assert.equal(app.calls.filter(c => c.path === '/api/os/v1/ordens').length, 1);
  assert.equal(app.calls.at(-1).method, 'DELETE');
});
