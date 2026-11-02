import { executionApi } from "@/services/api";
import type { ListServicosResponse, Servico, ServicoPayload, TempoMedioServico } from "@/types/servico";

interface ListServicosParams {
  pageNumber?: number;
  pageSize?: number;
}

export const servicosService = {
  async list(params: ListServicosParams = {}) {
    const { data } = await executionApi.get<ListServicosResponse>("/servicos", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 30,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await executionApi.get<Servico>(`/servicos/${id}`);
    return data;
  },

  async getTempoMedio(id: string) {
    const { data } = await executionApi.get<TempoMedioServico>(`/servicos/${id}/tempo-medio`);
    return data;
  },

  async create(payload: ServicoPayload) {
    const { data } = await executionApi.post<{ id: string }>("/servicos", payload);
    return data;
  },

  async update(id: string, payload: ServicoPayload) {
    const { data } = await executionApi.put(`/servicos/${id}`, payload);
    return data;
  },

  async remove(id: string) {
    await executionApi.delete(`/servicos/${id}`);
  },
};
