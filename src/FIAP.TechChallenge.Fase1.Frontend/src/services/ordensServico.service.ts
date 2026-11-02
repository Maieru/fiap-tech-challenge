import axios from "axios";
import { executionApi, osApi, publicOsApi } from "@/services/api";
import { mapOrdemDetalhes } from "./ordemServico.mapper";
import type {
  AcompanhamentoOrdemServico, AddPecaOrdemPayload, AddServicoOrdemPayload,
  ConcluirServicoOrdemPayload, CreateOrdemServicoPayload, DecisaoClienteResponse,
  ExecucaoResponse, ListOrdensServicoResponse, OrdemOs, SortDirection, StatusOrdemServico,
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
    const { data } = await osApi.get<ListOrdensServicoResponse>("/ordens", {
      params: { ...params, pageNumber: params.pageNumber ?? 1, pageSize: params.pageSize ?? 30 },
      paramsSerializer: { indexes: null },
    });
    return data;
  },
  async getById(id: string) {
    const { data: ordem } = await osApi.get<OrdemOs>(`/ordens/${id}`);
    let execucao: ExecucaoResponse | null = null;
    try {
      execucao = (await executionApi.get<ExecucaoResponse>(`/ordens/${id}`)).data;
    } catch (error) {
      // A criação da execução é assíncrona. Só 404 representa projeção pendente.
      if (!axios.isAxiosError(error) || error.response?.status !== 404) throw error;
    }
    return mapOrdemDetalhes(ordem, execucao);
  },
  async emitirAcessoCliente(id: string) {
    return (await osApi.post<{ token: string; acompanhamentoUrl: string }>(`/ordens/${id}/acesso-cliente`)).data;
  },
  async getAcompanhamentoById(id: string, token: string) {
    return (await publicOsApi.get<AcompanhamentoOrdemServico>(`/ordens/acompanhamento/${id}`, { params: { token } })).data;
  },
  async create(payload: CreateOrdemServicoPayload) {
    return (await osApi.post<{ id: string }>("/ordens", payload)).data;
  },
  async addServico(id: string, payload: AddServicoOrdemPayload) {
    return (await executionApi.post(`/ordens/${id}/addservico`, payload)).data;
  },
  async addPecaInsumo(id: string, payload: AddPecaOrdemPayload) {
    return (await executionApi.post(`/ordens/${id}/addpecainsumo`, payload)).data;
  },
  async iniciarDiagnostico(id: string) {
    return (await executionApi.put(`/ordens/${id}/iniciar-diagnostico`)).data;
  },
  async solicitarAprovacao(id: string) {
    return (await executionApi.put(`/ordens/${id}/solicitar-aprovacao`)).data;
  },
  async aprovarExecucao(id: string, token?: string) {
    if (token) return (await publicOsApi.put<DecisaoClienteResponse>(`/ordens/acompanhamento/${id}/aprovar-execucao`, undefined, { params: { token } })).data;
    return (await osApi.put(`/ordens/${id}/aprovar-execucao`)).data;
  },
  async cancelar(id: string, token?: string) {
    if (token) return (await publicOsApi.put<DecisaoClienteResponse>(`/ordens/acompanhamento/${id}/cancelar`, undefined, { params: { token } })).data;
    return (await osApi.put(`/ordens/${id}/cancelar`)).data;
  },
  async concluirServico(id: string, itemId: string, payload: ConcluirServicoOrdemPayload) {
    return (await executionApi.put(`/ordens/${id}/servicos/${itemId}/concluir`, payload)).data;
  },
  async finalizar(id: string) {
    return (await executionApi.put(`/ordens/${id}/finalizar`)).data;
  },
  async entregar(id: string) {
    return (await osApi.put(`/ordens/${id}/entregar`)).data;
  },
};
