import type { PagedResponse } from "@/types/api";

export type StatusOrdemServico = 1 | 2 | 3 | 4 | 5 | 6 | 7;
export type SortDirection = "Asc" | "Desc";

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
  decisao: 0 | 1 | 2;
  etapaPagamento: EtapaPagamento | null;
  statusExecucao: StatusOrdemServico | null;
  execucaoDisponivel: boolean;
  servicos: OrdemServicoServicoItem[];
  pecasInsumos: OrdemServicoPecaItem[];
  valorTotalServicos: number | null;
  valorTotalPecasInsumos: number | null;
  valorTotalOrdemServico: number | null;
}

export type EtapaPagamento = 0 | 1 | 2 | 3 | 4 | 5;
export const ETAPA_PAGAMENTO_LABELS: Record<EtapaPagamento, string> = {
  0: "Aguardando pagamento",
  1: "Pagamento confirmado; aguardando início da execução",
  2: "Execução autorizada",
  3: "Pagamento e execução concluídos",
  4: "Pagamento recusado; aguardando cancelamento e devolução do estoque",
  5: "Compensação concluída",
};

export interface ItemOrcamento {
  itemId: string;
  descricao: string;
  quantidade: number;
  valorUnitario: number;
  valorTotal: number;
}

export interface AcompanhamentoOrdemServico {
  ordemServicoId: string;
  descricaoProblema: string;
  status: StatusOrdemServico;
  dataCriacao: string;
  dataInicioDiagnostico: string | null;
  dataEnvioAprovacao: string | null;
  dataInicioExecucao: string | null;
  dataFinalizacao: string | null;
  dataEntrega: string | null;
  dataCancelamento: string | null;
  valorOrcamento: number | null;
  orcamento: { servicos: ItemOrcamento[]; pecasInsumos: ItemOrcamento[] } | null;
  etapaPagamento: EtapaPagamento | null;
}

export interface OrdemOs extends OrdemServico {
  decisao: 0 | 1 | 2;
  pagamento: { etapa: EtapaPagamento } | null;
}

export interface ExecucaoResponse {
  execucao: {
    status: StatusOrdemServico;
    servicos: Omit<OrdemServicoServicoItem, "valorTotal">[];
    pecasInsumos: Omit<OrdemServicoPecaItem, "valorTotal">[];
  };
  valorTotal: number;
}

export interface DecisaoClienteResponse {
  ordemServicoId: string;
  status: StatusOrdemServico;
  decisao: 1 | 2;
  pendente: boolean;
  etapaPagamento: EtapaPagamento | null;
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

export interface ConcluirServicoOrdemPayload {
  tempoGastoMinutos: number;
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
  7: "Cancelada",
};
