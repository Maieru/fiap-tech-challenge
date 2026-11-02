import type { ExecucaoResponse, OrdemOs, OrdemServicoDetalhes } from "@/types/ordemServico";

export function mapOrdemDetalhes(ordem: OrdemOs, response: ExecucaoResponse | null): OrdemServicoDetalhes {
  const servicos = response?.execucao.servicos.map((item) => ({
    ...item, valorTotal: item.valorUnitario * item.quantidade,
  })) ?? [];
  const pecasInsumos = response?.execucao.pecasInsumos.map((item) => ({
    ...item, valorTotal: item.precoUnitario * item.quantidade,
  })) ?? [];
  return {
    ...ordem,
    // OS preserva status/datas administrativos, inclusive a entrega (6).
    etapaPagamento: ordem.pagamento?.etapa ?? null,
    statusExecucao: response?.execucao.status ?? null,
    execucaoDisponivel: response !== null,
    servicos,
    pecasInsumos,
    valorTotalServicos: response ? servicos.reduce((sum, item) => sum + item.valorTotal, 0) : null,
    valorTotalPecasInsumos: response ? pecasInsumos.reduce((sum, item) => sum + item.valorTotal, 0) : null,
    valorTotalOrdemServico: response?.valorTotal ?? null,
  };
}
