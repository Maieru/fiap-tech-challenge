import { api } from "@/services/api";
import type {
  AddPecaOrdemPayload,
  AddServicoOrdemPayload,
  ConcluirServicoOrdemPayload,
  CreateOrdemServicoPayload,
  ListOrdensServicoResponse,
  OrdemServicoDetalhes,
} from "@/types/ordemServico";

interface ListOrdensParams {
  pageNumber?: number;
  pageSize?: number;
}

export const ordensServicoService = {
  async list(params: ListOrdensParams = {}) {
    const { data } = await api.get<ListOrdensServicoResponse>("/ordensservico", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 30,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await api.get<OrdemServicoDetalhes>(`/ordensservico/${id}`);
    return data;
  },

  async create(payload: CreateOrdemServicoPayload) {
    const { data } = await api.post<{ id: string }>("/ordensservico", payload);
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

  async aprovarExecucao(id: string) {
    const { data } = await api.put(`/ordensservico/${id}/aprovar-execucao`);
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
};
