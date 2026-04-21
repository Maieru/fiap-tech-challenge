import type { PagedResponse } from "@/types/api";

export interface Veiculo {
  id: string;
  clienteId: string;
  placa: string;
  marca: string;
  modelo: string;
  ano: number;
}

export interface CreateVeiculoPayload {
  clienteId: string;
  placa: string;
  marca: string;
  modelo: string;
  ano: number;
}

export interface UpdateVeiculoPayload {
  placa: string;
  marca: string;
  modelo: string;
  ano: number;
}

export interface ListVeiculosResponse extends PagedResponse {
  veiculos: Veiculo[];
}
