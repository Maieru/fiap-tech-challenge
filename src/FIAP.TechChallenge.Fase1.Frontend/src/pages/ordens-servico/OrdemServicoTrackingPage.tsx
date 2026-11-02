import { useCallback, useEffect, useState } from "react";
import { RefreshCw } from "lucide-react";
import { useParams, useSearchParams } from "react-router-dom";
import { StatusBadge } from "@/components/common/StatusBadge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { formatCurrency, formatDateTime } from "@/lib/utils";
import { getApiErrorMessage } from "@/services/api";
import { ordensServicoService } from "@/services/ordensServico.service";
import { ETAPA_PAGAMENTO_LABELS, type AcompanhamentoOrdemServico, type ItemOrcamento } from "@/types/ordemServico";

export function OrdemServicoTrackingPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") ?? "";
  const [ordem, setOrdem] = useState<AcompanhamentoOrdemServico | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deciding, setDeciding] = useState(false);
  const [pending, setPending] = useState(false);
  const [message, setMessage] = useState("");

  const loadData = useCallback(async (signal?: AbortSignal) => {
    if (!id || !token) {
      setError("O link de acompanhamento está incompleto. Solicite um novo link à oficina.");
      setLoading(false);
      return;
    }
    try {
      const response = await ordensServicoService.getAcompanhamentoById(id, token);
      if (signal?.aborted) return;
      setOrdem(response);
      setError(null);
      if (response.status !== 3) { setPending(false); setMessage(""); }
    } catch (requestError) {
      if (signal?.aborted) return;
      setError(getApiErrorMessage(requestError, "Não foi possível atualizar. Tente novamente ou solicite um novo link à oficina."));
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [id, token]);

  useEffect(() => {
    const controller = new AbortController();
    let running = false;
    const refresh = async () => {
      if (running) return;
      running = true;
      try { await loadData(controller.signal); } finally { running = false; }
    };
    void refresh();
    const timer = window.setInterval(() => { if (!document.hidden) void refresh(); }, 3000);
    return () => { controller.abort(); window.clearInterval(timer); };
  }, [loadData]);

  async function decide(action: "aprovarExecucao" | "cancelar") {
    if (!id || !token || deciding || pending) return;
    const prompt = action === "aprovarExecucao"
      ? "Aprovar este orçamento e solicitar o pagamento para autorizar a execução?"
      : "Cancelar esta ordem e solicitar a devolução dos itens ao estoque?";
    if (!window.confirm(prompt)) return;
    setDeciding(true);
    setMessage("");
    try {
      const response = await ordensServicoService[action](id, token);
      setPending(response.pendente);
      setMessage(response.pendente ? "Decisão registrada. Aguarde a confirmação do processamento." : "Decisão já processada.");
      await loadData();
    } catch (requestError) {
      setMessage(getApiErrorMessage(requestError, "Não foi possível registrar a decisão. Atualize e tente novamente."));
      await loadData();
    } finally { setDeciding(false); }
  }

  const canDecide = !error && ordem?.status === 3 && ordem.etapaPagamento === null && !pending;

  return (
    <main className="min-h-screen bg-background">
      <div className="mx-auto w-full max-w-6xl space-y-6 px-4 py-6 sm:px-6 lg:px-8">
        <header className="flex flex-wrap items-center justify-between gap-4 border-b pb-5">
          <h1 className="text-2xl font-semibold text-foreground sm:text-3xl">
            {ordem ? `OS #${ordem.ordemServicoId.slice(0, 8).toUpperCase()}` : "Acompanhamento da ordem"}
          </h1>
          <Button variant="outline" onClick={() => void loadData()} disabled={loading}>
            <RefreshCw className="h-4 w-4" /> Atualizar
          </Button>
        </header>
        {error && <p role="alert" className="text-sm text-destructive">{error}</p>}
        {loading ? <p role="status">Carregando acompanhamento...</p> : ordem && !error && (
          <>
            <Card>
              <CardHeader><CardTitle>Status da ordem</CardTitle></CardHeader>
              <CardContent className="space-y-4">
                <StatusBadge status={ordem.status} />
                <p>Aberta em {formatDateTime(ordem.dataCriacao)}</p>
                <p>{ordem.descricaoProblema}</p>
                {ordem.etapaPagamento !== null && <p role="status">{ETAPA_PAGAMENTO_LABELS[ordem.etapaPagamento]}</p>}
                {message && <p role="status">{message}</p>}
                {pending && <p role="status">Processando sua decisão. Esta página será atualizada automaticamente.</p>}
                {canDecide && (
                  <div className="flex flex-wrap gap-3">
                    <Button onClick={() => void decide("aprovarExecucao")} disabled={deciding}>Aprovar orçamento</Button>
                    <Button variant="destructive" onClick={() => void decide("cancelar")} disabled={deciding}>Cancelar ordem</Button>
                  </div>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle>Orçamento</CardTitle></CardHeader>
              <CardContent className="space-y-4">
                {!ordem.orcamento ? <p>O orçamento detalhado ainda não está disponível.</p> : (
                  <>
                    <BudgetItems title="Serviços" items={ordem.orcamento.servicos} />
                    <BudgetItems title="Peças e insumos" items={ordem.orcamento.pecasInsumos} />
                  </>
                )}
                <p className="font-semibold">Total: {formatCurrency(ordem.valorOrcamento)}</p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle>Linha do tempo</CardTitle></CardHeader>
              <CardContent className="grid gap-3 text-sm sm:grid-cols-2">
                <p>Diagnóstico iniciado: {formatDateTime(ordem.dataInicioDiagnostico)}</p>
                <p>Aprovação solicitada: {formatDateTime(ordem.dataEnvioAprovacao)}</p>
                <p>Execução iniciada: {formatDateTime(ordem.dataInicioExecucao)}</p>
                <p>Finalização: {formatDateTime(ordem.dataFinalizacao)}</p>
                <p>Entrega: {formatDateTime(ordem.dataEntrega)}</p>
                <p>Cancelamento: {formatDateTime(ordem.dataCancelamento)}</p>
              </CardContent>
            </Card>
          </>
        )}
      </div>
    </main>
  );
}

function BudgetItems({ title, items }: { title: string; items: ItemOrcamento[] }) {
  return (
    <section aria-label={title}>
      <h2 className="mb-2 font-medium">{title}</h2>
      <Table>
        <TableHeader><TableRow>
          <TableHead>Descrição</TableHead><TableHead>Quantidade</TableHead>
          <TableHead>Valor unitário</TableHead><TableHead>Total</TableHead>
        </TableRow></TableHeader>
        <TableBody>{items.length === 0 ? (
          <TableRow><TableCell colSpan={4}>Nenhum item no orçamento.</TableCell></TableRow>
        ) : items.map((item) => (
          <TableRow key={item.itemId}>
            <TableCell>{item.descricao}</TableCell><TableCell>{item.quantidade}</TableCell>
            <TableCell>{formatCurrency(item.valorUnitario)}</TableCell><TableCell>{formatCurrency(item.valorTotal)}</TableCell>
          </TableRow>
        ))}</TableBody>
      </Table>
    </section>
  );
}
