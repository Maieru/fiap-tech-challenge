import { api } from "@/services/api";
import type {
  CreateVeiculoPayload,
  ListVeiculosResponse,
  UpdateVeiculoPayload,
  Veiculo,
} from "@/types/veiculo";

interface ListVeiculosParams {
  pageNumber?: number;
  pageSize?: number;
  clienteId?: string;
}

export const veiculosService = {
  async list(params: ListVeiculosParams = {}) {
    const { data } = await api.get<ListVeiculosResponse>("/veiculos", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 30,
        clienteId: params.clienteId,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await api.get<Veiculo>(`/veiculos/${id}`);
    return data;
  },

  async create(payload: CreateVeiculoPayload) {
    const { data } = await api.post<{ id: string }>("/veiculos", payload);
    return data;
  },

  async update(id: string, payload: UpdateVeiculoPayload) {
    const { data } = await api.put(`/veiculos/${id}`, payload);
    return data;
  },
};
