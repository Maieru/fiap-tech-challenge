import { api } from "@/services/api";
import type {
  AcompanhamentoOrdemServico,
  AddPecaOrdemPayload,
  AddServicoOrdemPayload,
  ConcluirServicoOrdemPayload,
  CreateOrdemServicoComClienteEVeiculoPayload,
  CreateOrdemServicoPayload,
  ListOrdensServicoResponse,
  OrdemServicoDetalhes,
  SortDirection,
  StatusOrdemServico,
} from "@/types/ordemServico";

interface ListOrdensParams {
  pageNumber?: number;
  pageSize?: number;
  status?: StatusOrdemServico[];
  statusSortDirection?: SortDirection;
  dataAberturaSortDirection?: SortDirection;
}

export const ordensServicoService = {
  async list(params: ListOrdensParams = {}) {
    const { data } = await api.get<ListOrdensServicoResponse>("/ordensservico", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 30,
        status: params.status,
        statusSortDirection: params.statusSortDirection,
        dataAberturaSortDirection: params.dataAberturaSortDirection,
      },
      paramsSerializer: {
        indexes: null,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await api.get<OrdemServicoDetalhes>(`/ordensservico/${id}`);
    return data;
  },

  async getAcompanhamentoById(id: string) {
    const { data } = await api.get<AcompanhamentoOrdemServico>(`/ordensservico/acompanhamento/${id}`);
    return data;
  },

  async create(payload: CreateOrdemServicoPayload) {
    const { data } = await api.post<{ id: string }>("/ordensservico", payload);
    return data;
  },

  async createWithClienteEVeiculo(payload: CreateOrdemServicoComClienteEVeiculoPayload) {
    const { data } = await api.post<{ id: string }>("/ordensservico/com-cliente-veiculo", payload);
    return data;
  },

  async addServico(id: string, payload: AddServicoOrdemPayload) {
    const { data } = await api.post(`/ordensservico/${id}/addservico`, payload);
    return data;
  },

  async addPecaInsumo(id: string, payload: AddPecaOrdemPayload) {
    const { data } = await api.post(`/ordensservico/${id}/addpecainsumo`, payload);
    return data;
  },

  async iniciarDiagnostico(id: string) {
    const { data } = await api.put(`/ordensservico/${id}/iniciar-diagnostico`);
    return data;
  },

  async solicitarAprovacao(id: string) {
    const { data } = await api.put(`/ordensservico/${id}/solicitar-aprovacao`);
    return data;
  },

  async aprovarExecucao(id: string, codigoAprovacao: string) {
    const { data } = await api.put(`/ordensservico/${id}/aprovar-execucao`, { codigoAprovacao });
    return data;
  },

  async cancelar(id: string) {
    const { data } = await api.put(`/ordensservico/${id}/cancelar`);
    return data;
  },

  async concluirServico(servicoDaOrdemServicoId: string, payload: ConcluirServicoOrdemPayload) {
    const { data } = await api.put(`/ordensservico/servicos/${servicoDaOrdemServicoId}/concluir`, payload);
    return data;
  },

  async finalizar(id: string) {
    const { data } = await api.put(`/ordensservico/${id}/finalizar`);
    return data;
  },

  async entregar(id: string) {
    const { data } = await api.put(`/ordensservico/${id}/entregar`);
    return data;
  },

  async remove(id: string) {
    await api.delete(`/ordensservico/${id}`);
  },
};
