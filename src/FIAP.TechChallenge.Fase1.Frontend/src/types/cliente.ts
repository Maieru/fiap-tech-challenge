import type { PagedResponse } from "@/types/api";

export interface Cliente {
  id: string;
  nome: string;
  telefone: string;
  cpf?: string | null;
  cnpj?: string | null;
  email?: string | null;
}

export interface ClienteFormData {
  nome: string;
  telefone: string;
  email?: string;
  documento?: string;
}

export interface CreateClientePayload {
  nome: string;
  telefone: string;
  cpf?: string;
  cnpj?: string;
  email?: string;
}

export interface UpdateClientePayload {
  nome: string;
  telefone: string;
  email?: string;
}

export interface ListClientesResponse extends PagedResponse {
  clientes: Cliente[];
}
