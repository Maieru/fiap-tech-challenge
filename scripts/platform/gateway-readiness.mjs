import {setTimeout as pause} from 'node:timers/promises';

const probes = [
  ...['os','execution','billing'].map(service => ({path:`/api/${service}/v1/health/ready`,expected:200,health:true})),
  {path:'/',expected:200,html:true},
  ...['/api/os/v1/clientes','/api/execution/v1/servicos','/api/billing/v1/cobrancas'].map(path => ({path,expected:401}))
];

// Somente GETs de startup podem ser repetidos. Falhas de autenticacao/roteamento
// e operacoes de escrita permanecem erros imediatos no smoke funcional.
export async function waitForGateway(base, {
  correlation, timeoutMs=180000, intervalMs=2000, requestTimeoutMs=10000,
  consecutivePasses=3, report=console.log
} = {}) {
  const deadline = performance.now() + timeoutMs;
  let stable = 0;
  let lastFailure = 'nenhuma rodada completa';
  while (performance.now() < deadline) {
    let transient = false;
    for (const probe of probes) {
      const remaining = Math.floor(deadline - performance.now());
      if (remaining <= 0) { transient = true; break; }
      let response;
      try {
        response = await fetch(base + probe.path, {
          method:'GET', headers:correlation ? {'X-Correlation-Id':correlation} : {},
          signal:AbortSignal.timeout(Math.max(1,Math.min(requestTimeoutMs,remaining)))
        });
        const description = `GET ${probe.path}: HTTP ${response.status}; esperado ${probe.expected}`;
        if (response.status !== probe.expected) {
          const requestId = response.headers.get('apigw-requestid');
          if (![502,503,504].includes(response.status)) throw new Error(description);
          lastFailure = description + (requestId ? '; requestId=' + requestId : '');
          transient = true;
          break;
        }
        if (probe.health) {
          const contentType = response.headers.get('content-type') || '';
          if (!contentType.includes('application/json') || (await response.json()).status !== 'Healthy')
            throw new Error('Resposta de readiness invalida: ' + probe.path);
        }
        if (probe.html && !(response.headers.get('content-type') || '').includes('text/html'))
          throw new Error('Frontend nao retornou HTML.');
      } catch (error) {
        if (!['TypeError','AbortError','TimeoutError'].includes(error.name)) throw error;
        lastFailure = 'GET ' + probe.path + ': falha de conexao ou timeout';
        transient = true;
        break;
      } finally {
        if (response?.body && !response.bodyUsed) await response.body.cancel();
      }
    }
    if (transient) {
      stable = 0;
      report('Entrada publica ainda estabilizando: ' + lastFailure);
    } else if (++stable >= consecutivePasses) {
      report(`Entrada publica estavel: ${stable} rodadas de readiness, frontend e HTTP 401 sem token.`);
      return;
    }
    const remaining = deadline - performance.now();
    if (remaining > 0) await pause(Math.min(intervalMs,remaining));
  }
  throw new Error('Entrada publica nao estabilizou dentro do prazo: ' + lastFailure);
}
