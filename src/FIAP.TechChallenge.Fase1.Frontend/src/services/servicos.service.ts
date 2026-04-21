import { api } from "@/services/api";
import type { ListServicosResponse, Servico, ServicoPayload, TempoMedioServico } from "@/types/servico";

interface ListServicosParams {
  pageNumber?: number;
  pageSize?: number;
}

export const servicosService = {
  async list(params: ListServicosParams = {}) {
    const { data } = await api.get<ListServicosResponse>("/servicos", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 30,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await api.get<Servico>(`/servicos/${id}`);
    return data;
  },

  async getTempoMedio(id: string) {
    const { data } = await api.get<TempoMedioServico>(`/servicos/${id}/tempo-medio`);
    return data;
  },

  async create(payload: ServicoPayload) {
    const { data } = await api.post<{ id: string }>("/servicos", payload);
    return data;
  },

  async update(id: string, payload: ServicoPayload) {
    const { data } = await api.put(`/servicos/${id}`, payload);
    return data;
  },
};
