import type { PagedResponse } from "@/types/api";

export interface Servico {
  id: string;
  descricao: string;
  valorUnitario: number;
}

export interface ServicoPayload {
  descricao: string;
  valorUnitario: number;
}

export interface ListServicosResponse extends PagedResponse {
  servicos: Servico[];
}
