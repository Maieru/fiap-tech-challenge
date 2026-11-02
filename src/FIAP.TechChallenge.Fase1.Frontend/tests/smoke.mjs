// Exclusivamente contra a stack isolada descrita no README. Cria dados sintéticos.
import assert from "node:assert/strict";
import { mkdir, writeFile } from "node:fs/promises";
import { randomBytes } from "node:crypto";
import { setTimeout as delay } from "node:timers/promises";

const base = process.env.FRONTEND_SMOKE_URL || "http://localhost:15173";
assert.equal(process.env.FRONTEND_SMOKE_ISOLATED, "true", "Confirme a stack isolada com FRONTEND_SMOKE_ISOLATED=true");
const suffix = Date.now().toString().slice(-8);
const cpfDigits = Array.from(randomBytes(9), (value) => value % 10);
for (const weight of [10, 11]) {
  const remainder = cpfDigits.reduce((sum, digit, index) => sum + digit * (weight - index), 0) % 11;
  cpfDigits.push(remainder < 2 ? 0 : 11 - remainder);
}
const credentials = { usuario: `frontend-${suffix}`, senha: `Smoke!${randomBytes(12).toString("hex")}` };
let jwt;
async function request(method, path, body, authenticated = true, expected = undefined) {
  const response = await fetch(`${base}${path}`, {
    method,
    headers: { "Content-Type": "application/json", ...(authenticated && jwt ? { Authorization: `Bearer ${jwt}` } : {}) },
    ...(body === undefined ? {} : { body: JSON.stringify(body) }),
    signal: AbortSignal.timeout(10000),
  });
  if (expected) assert.equal(response.status, expected, `${method} ${path.split("?")[0]}`);
  else assert.ok(response.ok, `${method} ${path.split("?")[0]} => ${response.status}: ${await response.clone().text()}`);
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}
async function until(read, predicate, label) {
  for (let attempt = 0; attempt < 60; attempt++) {
    const value = await read();
    if (predicate(value)) return value;
    await delay(500);
  }
  throw new Error(`Timeout: ${label}`);
}
const os = "/api/os/v1", execution = "/api/execution/v1", billing = "/api/billing/v1";
await request("POST", `${os}/usuarios`, credentials, false, 201);
jwt = (await request("POST", `${os}/usuarios/login`, credentials, false)).token;
const cliente = await request("POST", `${os}/clientes`, { nome: "Cliente sintético frontend", telefone: "11999999999", cpf: cpfDigits.join("") });
const veiculo = await request("POST", `${os}/veiculos`, { clienteId: cliente.id, placa: `TST${suffix.slice(-4)}`, marca: "Teste", modelo: "Smoke", ano: 2025 });
const servico = await request("POST", `${execution}/servicos`, { descricao: "Diagnóstico sintético", valorUnitario: 100 });
const peca = await request("POST", `${execution}/pecas-insumos`, { nome: "Peça sintética", codigo: `SMOKE-${suffix}`, precoUnitario: 25, quantidadeEstoque: 10 });

async function diagnosedOrder() {
  const ordem = await request("POST", `${os}/ordens`, { clienteId: cliente.id, veiculoId: veiculo.id, descricaoProblema: "Verificação sintética da migração do frontend" });
  await until(async () => {
    const response = await fetch(`${base}${execution}/ordens/${ordem.id}`, { headers: { Authorization: `Bearer ${jwt}` } });
    if (response.status === 404) return null;
    assert.ok(response.ok);
    return response.json();
  }, Boolean, "criação Execution");
  await request("PUT", `${execution}/ordens/${ordem.id}/iniciar-diagnostico`);
  await request("POST", `${execution}/ordens/${ordem.id}/addservico`, { servicoId: servico.id, quantidade: 1 });
  await request("POST", `${execution}/ordens/${ordem.id}/addpecainsumo`, { pecaInsumoId: peca.id, quantidade: 2 });
  await request("PUT", `${execution}/ordens/${ordem.id}/solicitar-aprovacao`);
  await until(() => request("GET", `${os}/ordens/${ordem.id}`), (value) => value.status === 3, "orçamento OS");
  return ordem;
}
const ordem = await diagnosedOrder();
const first = await request("POST", `${os}/ordens/${ordem.id}/acesso-cliente`);
const acesso = await request("POST", `${os}/ordens/${ordem.id}/acesso-cliente`);
await request("GET", `${os}/ordens/acompanhamento/${ordem.id}?token=${first.token}`, undefined, false, 401);
const trackingPath = `${os}/ordens/acompanhamento/${ordem.id}?token=${acesso.token}`;
const tracking = await request("GET", trackingPath, undefined, false);
assert.equal(tracking.valorOrcamento, 150);
assert.equal(tracking.clienteId, undefined);
assert.equal(tracking.orcamento.servicos[0].valorTotal, 100);
await request("PUT", `${os}/ordens/acompanhamento/${ordem.id}/aprovar-execucao?token=${acesso.token}`, undefined, false, 202);
await until(() => request("GET", `${os}/ordens/${ordem.id}`), (value) => value.status === 4, "Saga pagamento");
const charge = await request("GET", `${billing}/ordens/${ordem.id}/cobranca`);
assert.equal(charge.situacao, 1);
assert.equal(charge.valor, 150);
const active = await request("GET", `${execution}/ordens/${ordem.id}`);
await request("PUT", `${execution}/ordens/${ordem.id}/servicos/${active.execucao.servicos[0].id}/concluir`, { tempoGastoMinutos: 15 });
await request("PUT", `${execution}/ordens/${ordem.id}/finalizar`);
await until(() => request("GET", `${os}/ordens/${ordem.id}`), (value) => value.status === 5, "finalização OS");
await request("PUT", `${os}/ordens/${ordem.id}/entregar`);
assert.equal((await request("GET", `${os}/ordens/${ordem.id}`)).status, 6);
assert.equal((await request("GET", `${execution}/ordens/${ordem.id}`)).execucao.status, 5);

const cancelada = await diagnosedOrder();
const cancelAccess = await request("POST", `${os}/ordens/${cancelada.id}/acesso-cliente`);
const cancelPath = `${os}/ordens/acompanhamento/${cancelada.id}/cancelar?token=${cancelAccess.token}`;
await request("PUT", cancelPath, undefined, false, 202);
await until(() => request("GET", `${os}/ordens/${cancelada.id}`), (value) => value.status === 7, "cancelamento OS");
await request("PUT", cancelPath, undefined, false, 202);
assert.equal((await request("GET", `${execution}/pecas-insumos/${peca.id}`)).quantidadeEstoque, 8);

// Terceira ordem permanece aguardando decisão para inspeção no navegador.
const browserOrder = await diagnosedOrder();
const browserAccess = await request("POST", `${os}/ordens/${browserOrder.id}/acesso-cliente`);
const page = await fetch(`${base}/acompanhar-ordem/${browserOrder.id}?token=${browserAccess.token}`);
assert.equal(page.status, 200);
assert.equal(page.headers.get("referrer-policy"), "no-referrer");
assert.equal(page.headers.get("cache-control"), "no-store");
await mkdir("../../artifacts/frontend", { recursive: true });
await writeFile("../../artifacts/frontend/smoke-browser.json", JSON.stringify({
  credentials, ordemId: ordem.id, browserOrderId: browserOrder.id,
  trackingUrl: `${base}/acompanhar-ordem/${browserOrder.id}?token=${browserAccess.token}`,
}, null, 2));
console.log("Smoke aprovado: autenticação, cadastros, três APIs via Nginx, diagnóstico, orçamento, rotação de link, pagamento, conclusão, entrega e cancelamento idempotente.");
