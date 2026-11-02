import { executionApi } from "@/services/api";
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
    const { data } = await executionApi.get<ListPecasInsumosResponse>("/pecas-insumos", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 30,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await executionApi.get<PecaInsumo>(`/pecas-insumos/${id}`);
    return data;
  },

  async create(payload: CreatePecaInsumoPayload) {
    const { data } = await executionApi.post<{ id: string }>("/pecas-insumos", payload);
    return data;
  },

  async update(id: string, payload: UpdatePecaInsumoPayload) {
    const { data } = await executionApi.put(`/pecas-insumos/${id}`, payload);
    return data;
  },

  async entradaEstoque(id: string, payload: EntradaEstoquePayload) {
    const { data } = await executionApi.put(`/pecas-insumos/${id}/entrada-estoque`, payload);
    return data;
  },

  async remove(id: string) {
    await executionApi.delete(`/pecas-insumos/${id}`);
  },
};
