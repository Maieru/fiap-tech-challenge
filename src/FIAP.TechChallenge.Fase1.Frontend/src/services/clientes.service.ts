import { api } from "@/services/api";
import type {
  Cliente,
  CreateClientePayload,
  ListClientesResponse,
  UpdateClientePayload,
} from "@/types/cliente";

interface ListClientesParams {
  pageNumber?: number;
  pageSize?: number;
}

export const clientesService = {
  async list(params: ListClientesParams = {}) {
    const { data } = await api.get<ListClientesResponse>("/clientes", {
      params: {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 20,
      },
    });

    return data;
  },

  async getById(id: string) {
    const { data } = await api.get<Cliente>(`/clientes/${id}`);
    return data;
  },

  async create(payload: CreateClientePayload) {
    const { data } = await api.post<{ id: string }>("/clientes", payload);
    return data;
  },

  async update(id: string, payload: UpdateClientePayload) {
    const { data } = await api.put(`/clientes/${id}`, payload);
    return data;
  },
};
