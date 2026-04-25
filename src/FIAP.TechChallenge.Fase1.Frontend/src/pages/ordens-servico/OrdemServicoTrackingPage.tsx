import { useEffect, useMemo, useState, type ReactNode } from "react";
import { ClipboardList, RefreshCw } from "lucide-react";
import { useParams } from "react-router-dom";
import { StatusBadge } from "@/components/common/StatusBadge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { formatCurrency, formatDateTime } from "@/lib/utils";
import { getApiErrorMessage } from "@/services/api";
import { ordensServicoService } from "@/services/ordensServico.service";
import type { AcompanhamentoOrdemServico, StatusOrdemServico } from "@/types/ordemServico";

const statusSteps: Array<{ status: StatusOrdemServico; label: string }> = [
  { status: 1, label: "Recebida" },
  { status: 2, label: "Diagnóstico" },
  { status: 3, label: "Aprovação" },
  { status: 4, label: "Execução" },
  { status: 5, label: "Finalizada" },
  { status: 6, label: "Entregue" },
];

export function OrdemServicoTrackingPage() {
  const { id } = useParams();
  const [ordem, setOrdem] = useState<AcompanhamentoOrdemServico | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function loadData() {
    if (!id) return;

    setLoading(true);
    setError(null);
    try {
      const response = await ordensServicoService.getAcompanhamentoById(id);
      setOrdem(response);
    } catch (requestError) {
      setOrdem(null);
      setError(getApiErrorMessage(requestError, "Não foi possível localizar a ordem de serviço."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadData();
  }, [id]);

  const currentStepIndex = useMemo(() => {
    if (!ordem) return -1;
    return statusSteps.findIndex((step) => step.status === ordem.status);
  }, [ordem]);

  return (
    <main className="min-h-screen bg-background">
      <div className="mx-auto flex min-h-screen w-full max-w-6xl flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8">
        <header className="flex flex-col gap-4 border-b pb-5 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <div className="mb-2 flex items-center gap-2 text-sm font-medium text-primary">
              <ClipboardList className="h-4 w-4" />
              Acompanhamento de ordem de serviço
            </div>
            <h1 className="text-2xl font-semibold tracking-normal text-foreground sm:text-3xl">
              {ordem ? `OS #${ordem.id.slice(0, 8).toUpperCase()}` : "Ordem de serviço"}
            </h1>
          </div>
          <Button type="button" variant="outline" onClick={loadData} disabled={loading}>
            <RefreshCw className="h-4 w-4" />
            Atualizar
          </Button>
        </header>

        {loading ? (
          <Card>
            <CardContent className="pt-6 text-sm text-muted-foreground">Carregando acompanhamento...</CardContent>
          </Card>
        ) : error || !ordem ? (
          <Card>
            <CardContent className="space-y-4 pt-6">
              <p className="text-sm text-destructive">{error ?? "Ordem de serviço não encontrada."}</p>
              <Button type="button" variant="outline" onClick={loadData}>
                <RefreshCw className="h-4 w-4" />
                Tentar novamente
              </Button>
            </CardContent>
          </Card>
        ) : (
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Status atual</CardTitle>
              </CardHeader>
              <CardContent className="space-y-5">
                <div className="flex flex-wrap items-center gap-3">
                  <StatusBadge status={ordem.status} />
                  <span className="text-sm text-muted-foreground">Aberta em {formatDateTime(ordem.dataCriacao)}</span>
                </div>
                <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-6">
                  {statusSteps.map((step, index) => {
                    const isReached = index <= currentStepIndex;
                    return (
                      <div
                        key={step.status}
                        className={`rounded-md border p-3 text-sm ${
                          isReached ? "border-primary bg-primary/5 text-primary" : "bg-muted/40 text-muted-foreground"
                        }`}
                      >
                        <span className="block text-xs font-semibold uppercase">Etapa {index + 1}</span>
                        <span className="mt-1 block font-medium">{step.label}</span>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>

            <div className="grid gap-4 lg:grid-cols-3">
              <Card className="lg:col-span-2">
                <CardHeader>
                  <CardTitle>Dados da ordem</CardTitle>
                </CardHeader>
                <CardContent className="grid gap-3 md:grid-cols-2">
                  <DetailItem label="Cliente" value={ordem.clienteNome} />
                  <DetailItem
                    label="Veículo"
                    value={`${ordem.veiculoMarca} ${ordem.veiculoModelo} (${ordem.veiculoPlaca})`}
                  />
                  <DetailItem label="Ano do veículo" value={ordem.veiculoAno} />
                  <DetailItem label="Descrição do problema" value={ordem.descricaoProblema} />
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle>Valores</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  <p>Serviços: {formatCurrency(ordem.valorTotalServicos)}</p>
                  <p>Peças/Insumos: {formatCurrency(ordem.valorTotalPecasInsumos)}</p>
                  <p className="text-base font-semibold">Total: {formatCurrency(ordem.valorTotalOrdemServico)}</p>
                </CardContent>
              </Card>
            </div>

            <Card>
              <CardHeader>
                <CardTitle>Serviços</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Descrição</TableHead>
                      <TableHead>Qtd</TableHead>
                      <TableHead>Valor Unitário</TableHead>
                      <TableHead>Total</TableHead>
                      <TableHead>Concluído</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {ordem.servicos.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={5} className="text-center text-muted-foreground">
                          Nenhum serviço adicionado.
                        </TableCell>
                      </TableRow>
                    ) : (
                      ordem.servicos.map((servico) => (
                        <TableRow key={servico.id}>
                          <TableCell>{servico.descricao}</TableCell>
                          <TableCell>{servico.quantidade}</TableCell>
                          <TableCell>{formatCurrency(servico.valorUnitario)}</TableCell>
                          <TableCell>{formatCurrency(servico.valorTotal)}</TableCell>
                          <TableCell>{servico.concluido ? "Sim" : "Não"}</TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Peças e insumos</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Item</TableHead>
                      <TableHead>Código</TableHead>
                      <TableHead>Qtd</TableHead>
                      <TableHead>Valor Unitário</TableHead>
                      <TableHead>Total</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {ordem.pecasInsumos.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={5} className="text-center text-muted-foreground">
                          Nenhuma peça ou insumo adicionado.
                        </TableCell>
                      </TableRow>
                    ) : (
                      ordem.pecasInsumos.map((peca) => (
                        <TableRow key={peca.id}>
                          <TableCell>{peca.nome}</TableCell>
                          <TableCell>{peca.codigo}</TableCell>
                          <TableCell>{peca.quantidade}</TableCell>
                          <TableCell>{formatCurrency(peca.precoUnitario)}</TableCell>
                          <TableCell>{formatCurrency(peca.valorTotal)}</TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Linha do tempo</CardTitle>
              </CardHeader>
              <CardContent className="grid gap-3 text-sm md:grid-cols-2 xl:grid-cols-3">
                <DetailItem label="Diagnóstico iniciado" value={formatDateTime(ordem.dataInicioDiagnostico)} />
                <DetailItem label="Aprovação solicitada" value={formatDateTime(ordem.dataEnvioAprovacao)} />
                <DetailItem label="Execução iniciada" value={formatDateTime(ordem.dataInicioExecucao)} />
                <DetailItem label="OS finalizada" value={formatDateTime(ordem.dataFinalizacao)} />
                <DetailItem label="Veículo entregue" value={formatDateTime(ordem.dataEntrega)} />
              </CardContent>
            </Card>
          </div>
        )}
      </div>
    </main>
  );
}

function DetailItem({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="rounded-md border bg-muted/30 p-3">
      <p className="text-xs uppercase text-muted-foreground">{label}</p>
      <div className="mt-1 text-sm font-medium">{value}</div>
    </div>
  );
}
