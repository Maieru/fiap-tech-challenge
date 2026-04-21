import { api } from "@/services/api";
import type {
  CreatePecaInsumoPayload,
  EntradaEstoquePayload,
  ListPecasInsumosResponse,
  PecaInsumo,
  UpdatePecaInsumoPayload,
} from "@/types/pecaInsumo";

interface ListPecasParams {
  pageNumber?: number;
  pageSize?: number;
}

export const pecasInsumosService = {
  async list(params: ListPecasParams = {}) {
    const { data } = await api.get<ListPecasInsumosResponse>("/pecasinsumos", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 30,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await api.get<PecaInsumo>(`/pecasinsumos/${id}`);
    return data;
  },

  async create(payload: CreatePecaInsumoPayload) {
    const { data } = await api.post<{ id: string }>("/pecasinsumos", payload);
    return data;
  },

  async update(id: string, payload: UpdatePecaInsumoPayload) {
    const { data } = await api.put(`/pecasinsumos/${id}`, payload);
    return data;
  },

  async entradaEstoque(id: string, payload: EntradaEstoquePayload) {
    const { data } = await api.put(`/pecasinsumos/${id}/entrada-estoque`, payload);
    return data;
  },
};
