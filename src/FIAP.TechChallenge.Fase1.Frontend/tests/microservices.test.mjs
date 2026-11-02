import assert from "node:assert/strict";
import { after, before, test } from "node:test";
import { createServer } from "vite";

let server, api, service, mapper, resolveApiEndpoints;
const storage = new Map();
globalThis.localStorage = {
  getItem: (key) => storage.get(key) ?? null,
  setItem: (key, value) => storage.set(key, value),
  removeItem: (key) => storage.delete(key),
};
before(async () => {
  server = await createServer({ server: { middlewareMode: true }, appType: "custom" });
  api = await server.ssrLoadModule("/src/services/api.ts");
  service = (await server.ssrLoadModule("/src/services/ordensServico.service.ts")).ordensServicoService;
  mapper = (await server.ssrLoadModule("/src/services/ordemServico.mapper.ts")).mapOrdemDetalhes;
  ({ resolveApiEndpoints } = await server.ssrLoadModule("/src/services/endpoints.ts"));
});
after(async () => { await server?.close(); });

const ordem = { id: "ordem", status: 6, decisao: 1, pagamento: { etapa: 3 }, dataEntrega: "2026-11-02T12:00:00Z" };
const execucao = { execucao: { status: 5, servicos: [{ id: "item", valorUnitario: 20, quantidade: 3 }], pecasInsumos: [{ precoUnitario: 5, quantidade: 2 }] }, valorTotal: 70 };
function response(config, data = {}) { return { data, status: 200, headers: {}, statusText: "OK", config }; }
function rejectStatus(config, status) {
  throw Object.assign(new Error(`HTTP ${status}`), { isAxiosError: true, config, response: { status } });
}
function login() {
  localStorage.setItem("oficina.auth.session", JSON.stringify({ token: "admin-jwt", expiresAt: new Date(Date.now() + 60000).toISOString() }));
}

test("resolve mesma origem, gateway com stage e bases independentes", () => {
  assert.equal(resolveApiEndpoints({}).os, "/api/os/v1");
  assert.equal(resolveApiEndpoints({ VITE_API_GATEWAY_URL: "https://gateway.example/stage/" }).execution, "https://gateway.example/stage/api/execution/v1");
  assert.equal(resolveApiEndpoints({ VITE_OS_API_BASE_URL: "https://os.example/custom" }).os, "https://os.example/custom");
});

test("logs de desenvolvimento removem token da query", async () => {
  const { redactPrivateQuery } = await server.ssrLoadModule("/src/observability/privacy.ts");
  assert.equal(redactPrivateQuery("proxy error /api/os/v1/ordens/acompanhamento/id?token=secret&x=1"), "proxy error /api/os/v1/ordens/acompanhamento/id?token=[redacted]&x=1");
});

test("compõe itens/totais do Execution preservando entrega e Saga do OS", () => {
  const result = mapper(ordem, execucao);
  assert.equal(result.status, 6);
  assert.equal(result.statusExecucao, 5);
  assert.equal(result.dataEntrega, ordem.dataEntrega);
  assert.equal(result.etapaPagamento, 3);
  assert.equal(result.servicos[0].valorTotal, 60);
  assert.equal(result.pecasInsumos[0].valorTotal, 10);
  assert.equal(result.valorTotalOrdemServico, 70);
});

test("projeção ainda ausente não inventa total zero", () => {
  const result = mapper(ordem, null);
  assert.equal(result.execucaoDisponivel, false);
  assert.equal(result.valorTotalOrdemServico, null);
  assert.equal(result.valorTotalServicos, null);
});

test("JWT compartilhado pelos três serviços; cliente público não recebe Authorization", async () => {
  login();
  for (const client of [api.osApi, api.executionApi, api.billingApi]) {
    client.defaults.adapter = async (config) => {
      assert.equal(config.headers.Authorization, "Bearer admin-jwt");
      return response(config);
    };
    await client.get("/check");
  }
  api.publicOsApi.defaults.adapter = async (config) => {
    assert.equal(config.headers.Authorization, undefined);
    return response(config);
  };
  await service.getAcompanhamentoById("ordem", "private-token");
});

test("401 de link privado preserva login; 401 administrativo encerra sessão", async () => {
  login();
  let logouts = 0;
  api.setUnauthorizedHandler(() => { logouts++; });
  api.publicOsApi.defaults.adapter = (config) => rejectStatus(config, 401);
  await assert.rejects(service.getAcompanhamentoById("ordem", "invalid"));
  assert.ok(localStorage.getItem("oficina.auth.session"));
  assert.equal(logouts, 0);
  api.billingApi.defaults.adapter = (config) => rejectStatus(config, 401);
  await assert.rejects(api.billingApi.get("/check"));
  assert.equal(localStorage.getItem("oficina.auth.session"), null);
  assert.equal(logouts, 1);
  api.setUnauthorizedHandler(null);
});

test("JWT expirado não envia comando sem autenticação", async () => {
  localStorage.setItem("oficina.auth.session", JSON.stringify({ token: "expired", expiresAt: "2020-01-01T00:00:00Z" }));
  let sent = false;
  api.osApi.defaults.adapter = async (config) => { sent = true; return response(config); };
  await assert.rejects(service.entregar("ordem"));
  assert.equal(sent, false);
});

test("somente 404 do Execution é tratado como projeção pendente", async () => {
  api.osApi.defaults.adapter = async (config) => response(config, ordem);
  api.executionApi.defaults.adapter = (config) => rejectStatus(config, 404);
  assert.equal((await service.getById("ordem")).execucaoDisponivel, false);
  api.executionApi.defaults.adapter = (config) => rejectStatus(config, 503);
  await assert.rejects(service.getById("ordem"));
});

test("comandos usam serviço, rota e identificadores do novo contrato", async () => {
  const calls = [];
  for (const [name, client] of [["os", api.osApi], ["execution", api.executionApi], ["public", api.publicOsApi]]) {
    client.defaults.adapter = async (config) => { calls.push([name, config.method, config.url, config.params]); return response(config, { pendente: true }); };
  }
  await service.create({ clienteId: "cliente", veiculoId: "veiculo", descricaoProblema: "problema" });
  await service.addServico("ordem", { servicoId: "servico", quantidade: 1 });
  await service.concluirServico("ordem", "item", { tempoGastoMinutos: 15 });
  await service.emitirAcessoCliente("ordem");
  await service.aprovarExecucao("ordem", "private-token");
  await service.cancelar("ordem");
  await service.entregar("ordem");
  assert.deepEqual(calls, [
    ["os", "post", "/ordens", undefined],
    ["execution", "post", "/ordens/ordem/addservico", undefined],
    ["execution", "put", "/ordens/ordem/servicos/item/concluir", undefined],
    ["os", "post", "/ordens/ordem/acesso-cliente", undefined],
    ["public", "put", "/ordens/acompanhamento/ordem/aprovar-execucao", { token: "private-token" }],
    ["os", "put", "/ordens/ordem/cancelar", undefined],
    ["os", "put", "/ordens/ordem/entregar", undefined],
  ]);
});
