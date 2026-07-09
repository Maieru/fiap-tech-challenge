import http from 'k6/http';
import { check, fail, group, sleep } from 'k6';
import exec from 'k6/execution';

const BASE_URL = (__ENV.BASE_URL || 'http://localhost:8080').replace(/\/$/, '');
const API_URL = `${BASE_URL}/api`;
const DEFAULT_HEADERS = { 'Content-Type': 'application/json' };
const SHOULD_CLEANUP = (__ENV.CLEANUP || 'false').toLowerCase() === 'true';
const START_VUS = Number(__ENV.START_VUS || 10);
const MAX_VUS = Number(__ENV.MAX_VUS || 100);
const STAGE_DURATION = __ENV.STAGE_DURATION || '2m';
const HOLD_DURATION = __ENV.HOLD_DURATION || '5m';
const RAMP_DOWN_DURATION = __ENV.RAMP_DOWN_DURATION || '1m';
const RUN_SEED = Date.now();

export const options = {
  scenarios: {
    jornada_usuario: {
      executor: 'ramping-vus',
      startVUs: START_VUS,
      gracefulRampDown: '30s',
      stages: [
        { duration: STAGE_DURATION, target: START_VUS },
        { duration: STAGE_DURATION, target: 25 },
        { duration: STAGE_DURATION, target: 50 },
        { duration: STAGE_DURATION, target: 75 },
        { duration: STAGE_DURATION, target: MAX_VUS },
        { duration: HOLD_DURATION, target: MAX_VUS },
        { duration: RAMP_DOWN_DURATION, target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1500'],
    'http_req_duration{group:::01 - Health check}': ['p(95)<500'],
    'http_req_duration{group:::07 - Aprovacao e execucao}': ['p(95)<2000'],
  },
};

export default function () {
  const seed = buildSeed();
  const data = buildJourneyData(seed);
  const ids = {};
  let authHeaders;

  group('01 - Health check', () => {
    const res = http.get(`${API_URL}/health/ready`);
    ensure(res, [200], 'API pronta para receber requisicoes');
  });

  group('02 - Cadastro e login do usuario', () => {
    const createUser = post('/usuarios', {
      usuario: data.usuario,
      senha: data.senha,
    });
    ensure(createUser, [201], 'usuario criado');
    ids.usuarioId = get(createUser.json(), 'id');

    const login = post('/usuarios/login', {
      usuario: data.usuario,
      senha: data.senha,
    });
    ensure(login, [200], 'login realizado');

    const body = login.json();
    const token = get(body, 'token');
    const tipoToken = get(body, 'tipoToken') || 'Bearer';

    if (!token) {
      fail(`Login nao retornou token. Body: ${login.body}`);
    }

    authHeaders = {
      headers: {
        ...DEFAULT_HEADERS,
        Authorization: `${tipoToken} ${token}`,
      },
    };
  });

  group('03 - Catalogo de servicos e pecas', () => {
    const servico = post('/servicos', {
      descricao: data.servicoDescricao,
      valorUnitario: 180.5,
    }, authHeaders);
    ensure(servico, [201], 'servico cadastrado');
    ids.servicoId = requiredId(servico, 'servico');

    const peca = post('/pecasinsumos', {
      nome: data.pecaNome,
      codigo: data.pecaCodigo,
      descricao: 'Filtro usado na jornada automatizada do K6',
      precoUnitario: 74.9,
      quantidadeEstoque: 25,
    }, authHeaders);
    ensure(peca, [201], 'peca/insumo cadastrado');
    ids.pecaInsumoId = requiredId(peca, 'peca/insumo');

    ensure(getRequest('/servicos', authHeaders), [200], 'lista servicos');
    ensure(getRequest('/pecasinsumos', authHeaders), [200], 'lista pecas/insumos');
  });

  group('04 - Cliente e veiculo', () => {
    const cliente = post('/clientes', {
      nome: data.clienteNome,
      telefone: data.telefone,
      cpf: data.cpf,
      email: data.email,
    }, authHeaders);
    ensure(cliente, [201], 'cliente cadastrado');
    ids.clienteId = requiredId(cliente, 'cliente');

    const veiculo = post('/veiculos', {
      clienteId: ids.clienteId,
      placa: data.placa,
      marca: 'Toyota',
      modelo: 'Corolla',
      ano: 2023,
    }, authHeaders);
    ensure(veiculo, [201], 'veiculo cadastrado');
    ids.veiculoId = requiredId(veiculo, 'veiculo');

    ensure(getRequest(`/clientes/${ids.clienteId}`, authHeaders), [200], 'consulta cliente');
    ensure(getRequest(`/veiculos/${ids.veiculoId}`, authHeaders), [200], 'consulta veiculo');
  });

  group('05 - Abertura da ordem de servico', () => {
    const ordem = post('/ordensservico', {
      clienteId: ids.clienteId,
      veiculoId: ids.veiculoId,
      descricaoProblema: 'Cliente relata falha intermitente ao ligar o veiculo.',
    }, authHeaders);
    ensure(ordem, [201], 'ordem de servico criada');
    ids.ordemServicoId = requiredId(ordem, 'ordem de servico');

    ensure(getRequest(`/ordensservico/${ids.ordemServicoId}`, authHeaders), [200], 'consulta ordem criada');
    ensure(getRequest(`/ordensservico/acompanhamento/${ids.ordemServicoId}`), [200], 'acompanhamento publico');
  });

  group('06 - Diagnostico e orcamento', () => {
    ensure(put(`/ordensservico/${ids.ordemServicoId}/iniciar-diagnostico`, null, authHeaders), [200], 'diagnostico iniciado');

    const addServico = post(`/ordensservico/${ids.ordemServicoId}/addservico`, {
      servicoId: ids.servicoId,
      quantidade: 1,
    }, authHeaders);
    ensure(addServico, [201], 'servico adicionado a ordem');
    ids.servicoDaOrdemServicoId = requiredId(addServico, 'servico da ordem');

    const addPeca = post(`/ordensservico/${ids.ordemServicoId}/addpecainsumo`, {
      pecaInsumoId: ids.pecaInsumoId,
      quantidade: 2,
    }, authHeaders);
    ensure(addPeca, [201], 'peca/insumo adicionada a ordem');

    const solicitar = put(`/ordensservico/${ids.ordemServicoId}/solicitar-aprovacao`, null, authHeaders);
    ensure(solicitar, [200], 'aprovacao solicitada');

    const ordemAtualizada = getRequest(`/ordensservico/${ids.ordemServicoId}`, authHeaders);
    ensure(ordemAtualizada, [200], 'consulta ordem aguardando aprovacao');
    ids.codigoAprovacao = get(ordemAtualizada.json(), 'codigoAprovacao');

    if (!ids.codigoAprovacao) {
      fail(`Ordem nao retornou codigo de aprovacao. Body: ${ordemAtualizada.body}`);
    }
  });

  group('07 - Aprovacao e execucao', () => {
    ensure(put(`/ordensservico/${ids.ordemServicoId}/aprovar-execucao`, {
      codigoAprovacao: ids.codigoAprovacao,
    }), [200], 'execucao aprovada pelo cliente');

    ensure(put(`/ordensservico/servicos/${ids.servicoDaOrdemServicoId}/concluir`, {
      tempoGastoMinutos: 45,
    }, authHeaders), [200], 'servico concluido');

    ensure(getRequest(`/servicos/${ids.servicoId}/tempo-medio`, authHeaders), [200], 'tempo medio consultado');
  });

  group('08 - Finalizacao e entrega', () => {
    ensure(put(`/ordensservico/${ids.ordemServicoId}/finalizar`, null, authHeaders), [200], 'ordem finalizada');
    ensure(put(`/ordensservico/${ids.ordemServicoId}/entregar`, null, authHeaders), [200], 'ordem entregue');

    const status = getRequest(`/ordensservico/${ids.ordemServicoId}/status`, authHeaders);
    ensure(status, [200], 'status final consultado');

    const delivered = check(status, {
      'ordem terminou como entregue': (res) => {
        const statusValue = get(res.json(), 'status');
        return statusValue === 6 || statusValue === 'Entregue';
      },
    });

    if (!delivered) {
      fail(`Ordem nao terminou como entregue. Body: ${status.body}`);
    }
  });

  if (SHOULD_CLEANUP) {
    group('09 - Limpeza opcional', () => {
      ensure(del(`/ordensservico/${ids.ordemServicoId}`, authHeaders), [204, 404], 'remove ordem');
      ensure(del(`/veiculos/${ids.veiculoId}`, authHeaders), [204, 404], 'remove veiculo');
      ensure(del(`/clientes/${ids.clienteId}`, authHeaders), [204, 404], 'remove cliente');
      ensure(del(`/servicos/${ids.servicoId}`, authHeaders), [204, 404], 'remove servico');
      ensure(del(`/pecasinsumos/${ids.pecaInsumoId}`, authHeaders), [204, 404], 'remove peca/insumo');
      if (ids.usuarioId) {
        ensure(del(`/usuarios/${ids.usuarioId}`, authHeaders), [204, 404], 'remove usuario');
      }
    });
  }

  sleep(Number(__ENV.SLEEP_SECONDS || 1));
}

function getRequest(path, params = undefined) {
  return http.get(`${API_URL}${path}`, params);
}

function post(path, payload, params = undefined) {
  return http.post(`${API_URL}${path}`, JSON.stringify(payload), paramsWithJson(params));
}

function put(path, payload = null, params = undefined) {
  return http.put(`${API_URL}${path}`, payload === null ? null : JSON.stringify(payload), paramsWithJson(params));
}

function del(path, params = undefined) {
  return http.del(`${API_URL}${path}`, null, params);
}

function paramsWithJson(params = undefined) {
  if (!params) {
    return { headers: DEFAULT_HEADERS };
  }

  return {
    ...params,
    headers: {
      ...DEFAULT_HEADERS,
      ...(params.headers || {}),
    },
  };
}

function ensure(response, expectedStatuses, label) {
  const ok = check(response, {
    [`${label}: status ${expectedStatuses.join(' ou ')}`]: (res) => expectedStatuses.includes(res.status),
  });

  if (!ok) {
    fail(`${label} falhou. Status: ${response.status}. Body: ${response.body}`);
  }
}

function requiredId(response, entityName) {
  const id = get(response.json(), 'id');

  if (!id) {
    fail(`${entityName} nao retornou id. Body: ${response.body}`);
  }

  return id;
}

function get(source, key) {
  if (!source) {
    return undefined;
  }

  if (Object.prototype.hasOwnProperty.call(source, key)) {
    return source[key];
  }

  const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
  return source[pascalKey];
}

function buildSeed() {
  return RUN_SEED + (exec.scenario.iterationInTest * 1000) + exec.vu.idInTest;
}

function buildJourneyData(seed) {
  return {
    usuario: `k6_usuario_${seed}`,
    senha: 'SenhaForte!123',
    clienteNome: `Cliente K6 ${seed}`,
    telefone: buildPhone(seed),
    cpf: buildCpf(seed),
    email: `cliente.k6.${seed}@example.com`,
    placa: buildPlate(seed),
    servicoDescricao: `Diagnostico K6 ${seed}`,
    pecaNome: `Filtro K6 ${seed}`,
    pecaCodigo: `K6-${seed}`,
  };
}

function buildPhone(seed) {
  const subscriber = String(10000000 + (seed % 90000000)).padStart(8, '0');
  return `119${subscriber}`;
}

function buildPlate(seed) {
  const lettersNumber = seed % 17576;
  const first = Math.floor(lettersNumber / 676);
  const second = Math.floor((lettersNumber % 676) / 26);
  const third = lettersNumber % 26;
  const letters = [first, second, third].map((n) => String.fromCharCode(65 + n)).join('');
  const digits = String(seed % 10000).padStart(4, '0');

  return `${letters}${digits}`;
}

function buildCpf(seed) {
  const base = String(100000000 + (seed % 899999999)).padStart(9, '0');
  const firstDigit = cpfDigit(base, 10);
  const secondDigit = cpfDigit(`${base}${firstDigit}`, 11);

  return `${base}${firstDigit}${secondDigit}`;
}

function cpfDigit(source, weightStart) {
  let sum = 0;

  for (let i = 0; i < source.length; i += 1) {
    sum += Number(source[i]) * (weightStart - i);
  }

  const remainder = sum % 11;
  return remainder < 2 ? 0 : 11 - remainder;
}
