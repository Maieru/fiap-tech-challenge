import { useEffect, useMemo, useState, type ReactNode } from "react";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { StatusBadge } from "@/components/common/StatusBadge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { formatCurrency, formatDateTime } from "@/lib/utils";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import { ordensServicoService } from "@/services/ordensServico.service";
import { pecasInsumosService } from "@/services/pecasInsumos.service";
import { servicosService } from "@/services/servicos.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";
import type { OrdemServicoDetalhes, StatusOrdemServico } from "@/types/ordemServico";
import type { PecaInsumo } from "@/types/pecaInsumo";
import type { Servico } from "@/types/servico";
import type { Veiculo } from "@/types/veiculo";

export function OrdemServicoDetailsPage() {
  const { id } = useParams();

  const [ordem, setOrdem] = useState<OrdemServicoDetalhes | null>(null);
  const [cliente, setCliente] = useState<Cliente | null>(null);
  const [veiculo, setVeiculo] = useState<Veiculo | null>(null);
  const [servicosDisponiveis, setServicosDisponiveis] = useState<Servico[]>([]);
  const [pecasDisponiveis, setPecasDisponiveis] = useState<PecaInsumo[]>([]);
  const [servicoSelecionadoId, setServicoSelecionadoId] = useState("");
  const [servicoQuantidade, setServicoQuantidade] = useState("1");
  const [pecaSelecionadaId, setPecaSelecionadaId] = useState("");
  const [pecaQuantidade, setPecaQuantidade] = useState("1");
  const [loading, setLoading] = useState(true);
  const [updatingStatus, setUpdatingStatus] = useState(false);
  const [addingItem, setAddingItem] = useState(false);

  async function loadData() {
    if (!id) return;

    setLoading(true);
    try {
      const ordemResponse = await ordensServicoService.getById(id);
      setOrdem(ordemResponse);

      const [clienteResponse, veiculoResponse, servicosResponse, pecasResponse] = await Promise.all([
        clientesService.getById(ordemResponse.clienteId),
        veiculosService.getById(ordemResponse.veiculoId),
        servicosService.list({ pageSize: 300 }),
        pecasInsumosService.list({ pageSize: 300 }),
      ]);

      setCliente(clienteResponse);
      setVeiculo(veiculoResponse);
      setServicosDisponiveis(servicosResponse.servicos);
      setPecasDisponiveis(pecasResponse.pecasInsumos.filter((item) => item.ativo));
    } catch {
      toast.error("Não foi possível carregar os detalhes da ordem.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadData();
  }, [id]);

  const nextAction = useMemo(() => {
    if (!ordem) return null;

    const actions: Record<StatusOrdemServico, { label: string; run: () => Promise<unknown> } | null> = {
      1: { label: "Iniciar diagnóstico", run: () => ordensServicoService.iniciarDiagnostico(ordem.id) },
      2: { label: "Solicitar aprovação", run: () => ordensServicoService.solicitarAprovacao(ordem.id) },
      3: { label: "Aprovar execução", run: () => ordensServicoService.aprovarExecucao(ordem.id) },
      4: { label: "Finalizar OS", run: () => ordensServicoService.finalizar(ordem.id) },
      5: { label: "Marcar como entregue", run: () => ordensServicoService.entregar(ordem.id) },
      6: null,
    };

    return actions[ordem.status];
  }, [ordem]);

  const isEmDiagnostico = ordem?.status === 2;

  async function handleStatusAdvance() {
    if (!nextAction) return;

    setUpdatingStatus(true);
    try {
      await nextAction.run();
      toast.success("Status atualizado com sucesso.");
      await loadData();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao atualizar status."));
    } finally {
      setUpdatingStatus(false);
    }
  }

  async function handleAdicionarServico() {
    if (!ordem) return;

    const quantidade = Number(servicoQuantidade);
    if (!servicoSelecionadoId || quantidade <= 0) {
      toast.error("Selecione um serviço e informe uma quantidade válida.");
      return;
    }

    setAddingItem(true);
    try {
      await ordensServicoService.addServico(ordem.id, {
        servicoId: servicoSelecionadoId,
        quantidade,
      });

      toast.success("Serviço adicionado com sucesso.");
      setServicoSelecionadoId("");
      setServicoQuantidade("1");
      await loadData();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao adicionar serviço."));
    } finally {
      setAddingItem(false);
    }
  }

  async function handleAdicionarPecaInsumo() {
    if (!ordem) return;

    const quantidade = Number(pecaQuantidade);
    if (!pecaSelecionadaId || quantidade <= 0) {
      toast.error("Selecione uma peça/insumo e informe uma quantidade válida.");
      return;
    }

    setAddingItem(true);
    try {
      await ordensServicoService.addPecaInsumo(ordem.id, {
        pecaInsumoId: pecaSelecionadaId,
        quantidade,
      });

      toast.success("Peça/insumo adicionado com sucesso.");
      setPecaSelecionadaId("");
      setPecaQuantidade("1");
      await loadData();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao adicionar peça/insumo."));
    } finally {
      setAddingItem(false);
    }
  }

  return (
    <div>
      <PageHeader
        title={ordem ? `OS #${ordem.id.slice(0, 8).toUpperCase()}` : "Detalhes da Ordem de Serviço"}
        actions={
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" asChild>
              <Link to="/ordens-servico">Voltar</Link>
            </Button>
            {nextAction && (
              <Button onClick={handleStatusAdvance} disabled={updatingStatus}>
                {updatingStatus ? "Atualizando..." : nextAction.label}
              </Button>
            )}
          </div>
        }
      />

      {loading || !ordem ? (
        <Card>
          <CardContent className="pt-6 text-sm text-muted-foreground">Carregando...</CardContent>
        </Card>
      ) : (
        <div className="space-y-6">
          <div className="grid gap-4 xl:grid-cols-3">
            <Card className="xl:col-span-2">
              <CardHeader>
                <CardTitle>Resumo da OS</CardTitle>
              </CardHeader>
              <CardContent className="grid gap-3 md:grid-cols-2">
                <DetailItem label="Status" value={<StatusBadge status={ordem.status} />} />
                <DetailItem label="Abertura" value={formatDateTime(ordem.dataCriacao)} />
                <DetailItem label="Cliente" value={cliente?.nome ?? ordem.clienteId} />
                <DetailItem
                  label="Veículo"
                  value={veiculo ? `${veiculo.marca} ${veiculo.modelo} (${veiculo.placa})` : ordem.veiculoId}
                />
                <DetailItem label="Descrição do problema" value={ordem.descricaoProblema} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Orçamento</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                <p>Serviços: {formatCurrency(ordem.valorTotalServicos)}</p>
                <p>Peças/Insumos: {formatCurrency(ordem.valorTotalPecasInsumos)}</p>
                <p className="text-base font-semibold">Total: {formatCurrency(ordem.valorTotalOrdemServico)}</p>
              </CardContent>
            </Card>
          </div>

          {isEmDiagnostico && (
            <Card>
              <CardHeader>
                <CardTitle>Adicionar itens ao diagnóstico</CardTitle>
              </CardHeader>
              <CardContent className="grid gap-6 lg:grid-cols-2">
                <div className="space-y-3 rounded-md border p-4">
                  <Label>Adicionar serviço</Label>
                  <div className="grid gap-3 md:grid-cols-[1fr_120px_auto]">
                    <Select value={servicoSelecionadoId} onValueChange={setServicoSelecionadoId}>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione um serviço" />
                      </SelectTrigger>
                      <SelectContent>
                        {servicosDisponiveis.map((servico) => (
                          <SelectItem key={servico.id} value={servico.id}>
                            {servico.descricao} - {formatCurrency(servico.valorUnitario)}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <Input type="number" min="1" value={servicoQuantidade} onChange={(event) => setServicoQuantidade(event.target.value)} />
                    <Button type="button" variant="outline" onClick={handleAdicionarServico} disabled={addingItem}>
                      Adicionar
                    </Button>
                  </div>
                </div>

                <div className="space-y-3 rounded-md border p-4">
                  <Label>Adicionar peça/insumo</Label>
                  <div className="grid gap-3 md:grid-cols-[1fr_120px_auto]">
                    <Select value={pecaSelecionadaId} onValueChange={setPecaSelecionadaId}>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione uma peça/insumo" />
                      </SelectTrigger>
                      <SelectContent>
                        {pecasDisponiveis.map((peca) => (
                          <SelectItem key={peca.id} value={peca.id}>
                            {peca.nome} - {formatCurrency(peca.precoUnitario)} (estoque: {peca.quantidadeEstoque})
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <Input type="number" min="1" value={pecaQuantidade} onChange={(event) => setPecaQuantidade(event.target.value)} />
                    <Button type="button" variant="outline" onClick={handleAdicionarPecaInsumo} disabled={addingItem}>
                      Adicionar
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          <Card>
            <CardHeader>
              <CardTitle>Serviços adicionados</CardTitle>
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
              <CardTitle>Peças e insumos adicionados</CardTitle>
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
