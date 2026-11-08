import {randomUUID, randomInt, randomBytes} from 'node:crypto';
import {setTimeout as pause} from 'node:timers/promises';
import {waitForGateway} from './gateway-readiness.mjs';

// O CLI valida o endpoint AWS; a funcao permite exercitar o protocolo HTTP no CI.
export async function runSmoke(base, {
  correlation = 'smoke-' + randomUUID(), report = console.log,
  waitTimeoutMs = 180000, intervalMs = 2000, readiness = {}
} = {}) {
  let token;
  async function request(method, path, body, expected = 200, authenticated = true) {
    const response = await fetch(base + path, {
      method, headers: {'content-type':'application/json', 'X-Correlation-Id':correlation,
        ...(authenticated && token ? {Authorization:'Bearer ' + token} : {})},
      ...(body === undefined ? {} : {body:JSON.stringify(body)}), signal:AbortSignal.timeout(30000)
    });
    try {
      if (response.status !== expected)
        throw new Error(`${method} ${path} retornou HTTP ${response.status}; esperado ${expected}; correlacao ${correlation}`);
      if (expected === 204) return;
      const content = await response.text();
      return content ? JSON.parse(content) : undefined;
    } finally {
      if (response.body && !response.bodyUsed) await response.body.cancel();
    }
  }
  async function wait(path, accept, stage) {
    const deadline = performance.now() + waitTimeoutMs;
    let last = 'sem resposta';
    report(`Aguardando ${stage}: ${path}; correlacao ${correlation}`);
    while (performance.now() < deadline) {
      let response;
      try {
        try {
          response = await fetch(base + path, {
            headers: {Authorization:'Bearer ' + token, 'X-Correlation-Id':correlation},
            signal:AbortSignal.timeout(Math.max(1, Math.min(15000, Math.ceil(deadline - performance.now()))))
          });
        } catch (error) {
          if (!['TypeError','AbortError','TimeoutError'].includes(error.name)) throw error;
          last = 'falha de conexao ou timeout';
        }
        if (response) last = 'HTTP ' + response.status;
        if (response?.ok) {
          const data = await response.json();
          // Somente estados operacionais; nao registrar payloads, JWT ou checkoutUrl.
          last += '; ' + JSON.stringify({status:data.execucao?.status ?? data.status,
            situacao:data.situacao, etapaPagamento:data.pagamento?.etapa});
          if (accept(data)) return data;
        } else if (response && ![404,502,503,504].includes(response.status)) {
          throw new Error(`Falha HTTP ${response.status} aguardando ${stage}: ${path}; correlacao ${correlation}`);
        }
      } finally {
        if (response?.body && !response.bodyUsed) await response.body.cancel();
      }
      const remaining = deadline - performance.now();
      if (remaining > 0) await pause(Math.min(intervalMs, remaining));
    }
    throw new Error(`Prazo excedido aguardando ${stage}: ${path}; ultimo estado: ${last}; correlacao ${correlation}`);
  }

  await waitForGateway(base, {...readiness, correlation, report});
  const credentials = {usuario:'smoke-' + randomUUID().slice(0,18), senha:randomBytes(24).toString('base64url')};
  const user = await request('POST', '/api/os/v1/usuarios', credentials, 201, false);
  let failure;
  try {
    const login = await request('POST', '/api/os/v1/usuarios/login', credentials, 200, false);
    token = login.token;
    if (!token) throw new Error('OS nao emitiu JWT.');
    await request('GET', '/api/execution/v1/servicos');
    await request('GET', '/api/billing/v1/cobrancas');
    const digits = String(randomInt(100000000,999999999)).split('').map(Number);
    for (let length = 9; length <= 10; length++) {
      const remainder = digits.reduce((sum,digit,index) => sum + digit * (length + 1 - index),0) * 10 % 11;
      digits.push(remainder === 10 ? 0 : remainder);
    }
    const customer = await request('POST', '/api/os/v1/clientes', {
      nome:'Cliente demonstracao AWS', cpf:digits.join(''), telefone:'11987654321'
    }, 201);
    const vehicle = await request('POST', '/api/os/v1/veiculos', {
      clienteId:customer.id, placa:'SM' + String.fromCharCode(65 + randomInt(26)) + String(randomInt(10000)).padStart(4,'0'),
      marca:'Toyota', modelo:'Corolla', ano:2024
    }, 201);

    async function diagnose(servicePrice, partPrice) {
      const order = await request('POST', '/api/os/v1/ordens', {
        clienteId:customer.id, veiculoId:vehicle.id, descricaoProblema:'Validacao automatizada AWS'
      }, 201);
      const os = '/api/os/v1/ordens/' + order.id;
      const execution = '/api/execution/v1/ordens/' + order.id;
      const billing = '/api/billing/v1/ordens/' + order.id + '/cobranca';
      await wait(execution, data => data.execucao.status === 1, 'recebimento da ordem no Execution');
      const service = await request('POST', '/api/execution/v1/servicos', {
        descricao:'Revisao smoke AWS', valorUnitario:servicePrice
      }, 201);
      const part = await request('POST', '/api/execution/v1/pecas-insumos', {
        nome:'Filtro smoke AWS', codigo:'SM-' + randomUUID(), precoUnitario:partPrice, quantidadeEstoque:5
      }, 201);
      await request('PUT', execution + '/iniciar-diagnostico', {});
      await request('POST', execution + '/addservico', {servicoId:service.id, quantidade:1}, 201);
      await request('POST', execution + '/addpecainsumo', {pecaInsumoId:part.id, quantidade:2}, 201);
      await request('PUT', execution + '/solicitar-aprovacao', {});
      await wait(os, data => data.status === 3, 'projecao do diagnostico no OS');
      await request('PUT', os + '/aprovar-execucao', {}, 202);
      return {id:order.id, os, execution, billing, part};
    }

    // Valor positivo cria checkout pendente. A compra e o webhook reais sao um aceite manual.
    const paid = await diagnose(100.25, 20.15);
    const pending = await wait(paid.billing, () => true, 'criacao do checkout no Billing');
    const awaiting = await request('GET', paid.os);
    const held = await request('GET', paid.execution);
    const reserved = await request('GET', '/api/execution/v1/pecas-insumos/' + paid.part.id);
    if (pending.situacao !== 0 || pending.valor !== 140.55 || pending.moeda !== 'BRL'
      || !pending.gatewayOrderId || !URL.canParse(pending.checkoutUrl) || new URL(pending.checkoutUrl).protocol !== 'https:'
      || awaiting.status !== 3 || awaiting.pagamento?.etapa !== 0 || awaiting.pagamento?.valor !== 140.55
      || pending.orcamentoId !== awaiting.orcamentoId || held.execucao.status !== 3
      || held.valorTotal !== 140.55 || reserved.quantidadeEstoque !== 3)
      throw new Error(`Checkout pendente/valor/estoque divergentes. Ordem ${paid.id}; correlacao ${correlation}`);
    report(`Checkout pendente validado: ordem ${paid.id}; valor BRL 140.55. Compra e webhook exigem aceite manual.`);

    // Valor zero usa a regra real de Billing, sem pagamento externo, e percorre a Saga completa.
    const free = await diagnose(0, 0);
    const charge = await wait(free.billing, data => data.situacao === 1, 'confirmacao da cobranca de valor zero');
    const authorized = await wait(free.execution, data => data.execucao.status === 4, 'autorizacao da execucao de valor zero');
    await request('PUT', free.execution + '/servicos/' + authorized.execucao.servicos[0].id + '/concluir', {tempoGastoMinutos:45});
    await request('PUT', free.execution + '/finalizar', {});
    const projected = await wait(free.os, data => data.status === 5, 'finalizacao no OS');
    const finished = await wait(free.execution, data => data.execucao.status === 5, 'finalizacao no Execution');
    const stock = await request('GET', '/api/execution/v1/pecas-insumos/' + free.part.id);
    if (charge.valor !== 0 || charge.gatewayPaymentId !== 'sem-cobranca-' + free.id || charge.checkoutUrl
      || finished.valorTotal !== 0 || projected.pagamento?.cobrancaId !== charge.id || stock.quantidadeEstoque !== 3)
      throw new Error(`Pagamento/estoque divergentes no fluxo de valor zero. Ordem ${free.id}; correlacao ${correlation}`);
    const message = `Smoke AWS aprovado: frontend, JWT, checkout pendente (ordem ${paid.id}) e Saga OS → Billing → Execution de valor zero (ordem ${free.id}). Compra/webhook Mercado Pago: aceite manual pendente. Correlacao ${correlation}.`;
    report(message);
    return message;
  } catch (error) {
    failure = error;
    throw error;
  } finally {
    if (token && user?.id) {
      try { await request('DELETE', '/api/os/v1/usuarios/' + user.id, undefined, 204); }
      catch (error) {
        if (!failure) throw error;
        report('Falha adicional ao desativar usuario do smoke: ' + error.message);
      }
    }
  }
}
