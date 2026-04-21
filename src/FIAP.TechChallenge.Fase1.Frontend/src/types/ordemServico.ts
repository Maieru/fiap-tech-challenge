import type { PagedResponse } from "@/types/api";

export type StatusOrdemServico = 1 | 2 | 3 | 4 | 5 | 6;

export interface OrdemServico {
  id: string;
  clienteId: string;
  veiculoId: string;
  descricaoProblema: string;
  status: StatusOrdemServico;
  dataCriacao: string;
  dataInicioDiagnostico?: string | null;
  dataEnvioAprovacao?: string | null;
  dataInicioExecucao?: string | null;
  dataFinalizacao?: string | null;
  dataEntrega?: string | null;
}

export interface OrdemServicoServicoItem {
  id: string;
  ordemServicoId: string;
  servicoId: string;
  descricao: string;
  valorUnitario: number;
  quantidade: number;
  valorTotal: number;
  tempoGastoMinutos?: number | null;
  concluido: boolean;
}

export interface OrdemServicoPecaItem {
  id: string;
  ordemServicoId: string;
  pecaInsumoId: string;
  nome: string;
  codigo: string;
  descricao?: string | null;
  precoUnitario: number;
  quantidade: number;
  valorTotal: number;
}

export interface OrdemServicoDetalhes extends OrdemServico {
  servicos: OrdemServicoServicoItem[];
  pecasInsumos: OrdemServicoPecaItem[];
  valorTotalServicos: number;
  valorTotalPecasInsumos: number;
  valorTotalOrdemServico: number;
}

export interface CreateOrdemServicoPayload {
  clienteId: string;
  veiculoId: string;
  descricaoProblema: string;
}

export interface AddServicoOrdemPayload {
  servicoId: string;
  quantidade: number;
}

export interface AddPecaOrdemPayload {
  pecaInsumoId: string;
  quantidade: number;
}

export interface ListOrdensServicoResponse extends PagedResponse {
  ordensServico: OrdemServico[];
}

export const STATUS_ORDEM_LABELS: Record<StatusOrdemServico, string> = {
  1: "Recebida",
  2: "Em diagnóstico",
  3: "Aguardando aprovação",
  4: "Em execução",
  5: "Finalizada",
  6: "Entregue",
};
