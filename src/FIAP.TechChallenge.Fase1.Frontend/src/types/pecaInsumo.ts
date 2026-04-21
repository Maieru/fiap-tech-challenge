import type { PagedResponse } from "@/types/api";

export interface PecaInsumo {
  id: string;
  nome: string;
  codigo: string;
  descricao?: string | null;
  precoUnitario: number;
  quantidadeEstoque: number;
  ativo: boolean;
}

export interface CreatePecaInsumoPayload {
  nome: string;
  codigo: string;
  descricao?: string;
  precoUnitario: number;
  quantidadeEstoque: number;
}

export interface UpdatePecaInsumoPayload {
  nome: string;
  codigo: string;
  descricao?: string;
  precoUnitario: number;
  ativo: boolean;
}

export interface EntradaEstoquePayload {
  quantidade: number;
}

export interface ListPecasInsumosResponse extends PagedResponse {
  pecasInsumos: PecaInsumo[];
}
