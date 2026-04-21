import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { formatCurrency } from "@/lib/utils";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import { ordensServicoService } from "@/services/ordensServico.service";
import { pecasInsumosService } from "@/services/pecasInsumos.service";
import { servicosService } from "@/services/servicos.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";
import type { PecaInsumo } from "@/types/pecaInsumo";
import type { Servico } from "@/types/servico";
import type { Veiculo } from "@/types/veiculo";

interface ItemSelecionado {
  id: string;
  descricao: string;
  valorUnitario: number;
  quantidade: number;
}

const initialNovoCliente = {
  nome: "",
  telefone: "",
  email: "",
  documento: "",
};

const initialNovoVeiculo = {
  placa: "",
  marca: "",
  modelo: "",
  ano: String(new Date().getFullYear()),
};

export function OrdemServicoFormPage() {
  const navigate = useNavigate();

  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [veiculos, setVeiculos] = useState<Veiculo[]>([]);
  const [servicosDisponiveis, setServicosDisponiveis] = useState<Servico[]>([]);
  const [pecasDisponiveis, setPecasDisponiveis] = useState<PecaInsumo[]>([]);
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [descricaoProblema, setDescricaoProblema] = useState("");
  const [clienteId, setClienteId] = useState("");
  const [veiculoId, setVeiculoId] = useState("");
  const [usarNovoCliente, setUsarNovoCliente] = useState(false);
  const [novoCliente, setNovoCliente] = useState(initialNovoCliente);
  const [cadastrarNovoVeiculo, setCadastrarNovoVeiculo] = useState(false);
  const [novoVeiculo, setNovoVeiculo] = useState(initialNovoVeiculo);

  const [servicoSelecionadoId, setServicoSelecionadoId] = useState("");
  const [servicoQuantidade, setServicoQuantidade] = useState("1");
  const [servicosSelecionados, setServicosSelecionados] = useState<ItemSelecionado[]>([]);

  const [pecaSelecionadaId, setPecaSelecionadaId] = useState("");
  const [pecaQuantidade, setPecaQuantidade] = useState("1");
  const [pecasSelecionadas, setPecasSelecionadas] = useState<ItemSelecionado[]>([]);

  useEffect(() => {
    async function loadData() {
      setLoading(true);
      try {
        const [clientesResponse, veiculosResponse, servicosResponse, pecasResponse] = await Promise.all([
          clientesService.list({ pageSize: 300 }),
          veiculosService.list({ pageSize: 300 }),
          servicosService.list({ pageSize: 300 }),
          pecasInsumosService.list({ pageSize: 300 }),
        ]);

        setClientes(clientesResponse.clientes);
        setVeiculos(veiculosResponse.veiculos);
        setServicosDisponiveis(servicosResponse.servicos);
        setPecasDisponiveis(pecasResponse.pecasInsumos.filter((item) => item.ativo));
      } catch {
        toast.error("Não foi possível carregar os dados para abertura de OS.");
      } finally {
        setLoading(false);
      }
    }

    void loadData();
  }, []);

  const veiculosFiltrados = useMemo(() => {
    if (!clienteId) return veiculos;
    return veiculos.filter((veiculo) => veiculo.clienteId === clienteId);
  }, [clienteId, veiculos]);

  const totalServicos = useMemo(
    () => servicosSelecionados.reduce((acc, item) => acc + item.valorUnitario * item.quantidade, 0),
    [servicosSelecionados],
  );
  const totalPecas = useMemo(
    () => pecasSelecionadas.reduce((acc, item) => acc + item.valorUnitario * item.quantidade, 0),
    [pecasSelecionadas],
  );
  const totalOrcamento = totalServicos + totalPecas;

  function adicionarServico() {
    const servico = servicosDisponiveis.find((item) => item.id === servicoSelecionadoId);
    if (!servico) return;

    const quantidade = Number(servicoQuantidade);
    if (quantidade <= 0) return;

    setServicosSelecionados((prev) => {
      const existingIndex = prev.findIndex((item) => item.id === servico.id);
      if (existingIndex === -1) {
        return [...prev, { id: servico.id, descricao: servico.descricao, valorUnitario: servico.valorUnitario, quantidade }];
      }

      const copy = [...prev];
      copy[existingIndex] = {
        ...copy[existingIndex],
        quantidade: copy[existingIndex].quantidade + quantidade,
      };
      return copy;
    });

    setServicoSelecionadoId("");
    setServicoQuantidade("1");
  }

  function adicionarPeca() {
    const peca = pecasDisponiveis.find((item) => item.id === pecaSelecionadaId);
    if (!peca) return;

    const quantidade = Number(pecaQuantidade);
    if (quantidade <= 0) return;

    setPecasSelecionadas((prev) => {
      const existingIndex = prev.findIndex((item) => item.id === peca.id);
      if (existingIndex === -1) {
        return [...prev, { id: peca.id, descricao: peca.nome, valorUnitario: peca.precoUnitario, quantidade }];
      }

      const copy = [...prev];
      copy[existingIndex] = {
        ...copy[existingIndex],
        quantidade: copy[existingIndex].quantidade + quantidade,
      };
      return copy;
    });

    setPecaSelecionadaId("");
    setPecaQuantidade("1");
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);

    try {
      let finalClienteId = clienteId;

      if (usarNovoCliente) {
        const documentoLimpo = novoCliente.documento.replace(/\D/g, "");
        if (!novoCliente.nome || !novoCliente.telefone || !documentoLimpo) {
          toast.error("Preencha os dados do novo cliente.");
          return;
        }

        const clienteCriado = await clientesService.create({
          nome: novoCliente.nome,
          telefone: novoCliente.telefone,
          email: novoCliente.email || undefined,
          cpf: documentoLimpo.length <= 11 ? documentoLimpo : undefined,
          cnpj: documentoLimpo.length > 11 ? documentoLimpo : undefined,
        });
        finalClienteId = clienteCriado.id;
      }

      if (!finalClienteId) {
        toast.error("Selecione ou cadastre um cliente.");
        return;
      }

      let finalVeiculoId = veiculoId;

      if (cadastrarNovoVeiculo) {
        if (!novoVeiculo.placa || !novoVeiculo.marca || !novoVeiculo.modelo || !novoVeiculo.ano) {
          toast.error("Preencha os dados do novo veículo.");
          return;
        }

        const veiculoCriado = await veiculosService.create({
          clienteId: finalClienteId,
          placa: novoVeiculo.placa.toUpperCase(),
          marca: novoVeiculo.marca,
          modelo: novoVeiculo.modelo,
          ano: Number(novoVeiculo.ano),
        });
        finalVeiculoId = veiculoCriado.id;
      }

      if (!finalVeiculoId) {
        toast.error("Selecione ou cadastre um veículo.");
        return;
      }

      const ordemCriada = await ordensServicoService.create({
        clienteId: finalClienteId,
        veiculoId: finalVeiculoId,
        descricaoProblema,
      });

      for (const servico of servicosSelecionados) {
        await ordensServicoService.addServico(ordemCriada.id, {
          servicoId: servico.id,
          quantidade: servico.quantidade,
        });
      }

      for (const peca of pecasSelecionadas) {
        await ordensServicoService.addPecaInsumo(ordemCriada.id, {
          pecaInsumoId: peca.id,
          quantidade: peca.quantidade,
        });
      }

      toast.success("Ordem de serviço criada com sucesso.");
      navigate(`/ordens-servico/${ordemCriada.id}`);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao criar ordem de serviço."));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div>
      <PageHeader
        title="Nova Ordem de Serviço"
        description="Selecione cliente, veículo, itens e acompanhe o orçamento total antes de abrir a OS."
      />

      {loading ? (
        <Card>
          <CardContent className="pt-6 text-sm text-muted-foreground">Carregando dados...</CardContent>
        </Card>
      ) : (
        <form className="space-y-6" onSubmit={handleSubmit}>
          <Card>
            <CardHeader>
              <CardTitle>Cliente e veículo</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex flex-wrap gap-2">
                <Button
                  type="button"
                  variant={usarNovoCliente ? "default" : "outline"}
                  onClick={() => {
                    setUsarNovoCliente((prev) => !prev);
                    setClienteId("");
                    setCadastrarNovoVeiculo(true);
                    setVeiculoId("");
                  }}
                >
                  {usarNovoCliente ? "Usando novo cliente" : "Cadastrar novo cliente na OS"}
                </Button>
                <Button
                  type="button"
                  variant={cadastrarNovoVeiculo ? "default" : "outline"}
                  onClick={() => {
                    setCadastrarNovoVeiculo((prev) => !prev);
                    setVeiculoId("");
                  }}
                >
                  {cadastrarNovoVeiculo ? "Criar novo veículo" : "Selecionar veículo existente"}
                </Button>
              </div>

              {usarNovoCliente ? (
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2 md:col-span-2">
                    <Label htmlFor="novo-cliente-nome">Nome do cliente</Label>
                    <Input
                      id="novo-cliente-nome"
                      value={novoCliente.nome}
                      onChange={(event) => setNovoCliente((prev) => ({ ...prev, nome: event.target.value }))}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="novo-cliente-doc">CPF/CNPJ</Label>
                    <Input
                      id="novo-cliente-doc"
                      value={novoCliente.documento}
                      onChange={(event) => setNovoCliente((prev) => ({ ...prev, documento: event.target.value }))}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="novo-cliente-tel">Telefone</Label>
                    <Input
                      id="novo-cliente-tel"
                      value={novoCliente.telefone}
                      onChange={(event) => setNovoCliente((prev) => ({ ...prev, telefone: event.target.value }))}
                    />
                  </div>
                  <div className="space-y-2 md:col-span-2">
                    <Label htmlFor="novo-cliente-email">Email</Label>
                    <Input
                      id="novo-cliente-email"
                      type="email"
                      value={novoCliente.email}
                      onChange={(event) => setNovoCliente((prev) => ({ ...prev, email: event.target.value }))}
                    />
                  </div>
                </div>
              ) : (
                <div className="space-y-2">
                  <Label htmlFor="cliente">Cliente</Label>
                  <Select value={clienteId} onValueChange={setClienteId}>
                    <SelectTrigger id="cliente">
                      <SelectValue placeholder="Selecione um cliente" />
                    </SelectTrigger>
                    <SelectContent>
                      {clientes.map((cliente) => (
                        <SelectItem key={cliente.id} value={cliente.id}>
                          {cliente.nome}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              )}

              {cadastrarNovoVeiculo ? (
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="placa">Placa</Label>
                    <Input
                      id="placa"
                      value={novoVeiculo.placa}
                      onChange={(event) => setNovoVeiculo((prev) => ({ ...prev, placa: event.target.value }))}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="ano">Ano</Label>
                    <Input
                      id="ano"
                      type="number"
                      value={novoVeiculo.ano}
                      onChange={(event) => setNovoVeiculo((prev) => ({ ...prev, ano: event.target.value }))}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="marca">Marca</Label>
                    <Input
                      id="marca"
                      value={novoVeiculo.marca}
                      onChange={(event) => setNovoVeiculo((prev) => ({ ...prev, marca: event.target.value }))}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="modelo">Modelo</Label>
                    <Input
                      id="modelo"
                      value={novoVeiculo.modelo}
                      onChange={(event) => setNovoVeiculo((prev) => ({ ...prev, modelo: event.target.value }))}
                    />
                  </div>
                </div>
              ) : (
                <div className="space-y-2">
                  <Label htmlFor="veiculo">Veículo</Label>
                  <Select value={veiculoId} onValueChange={setVeiculoId}>
                    <SelectTrigger id="veiculo">
                      <SelectValue placeholder="Selecione um veículo" />
                    </SelectTrigger>
                    <SelectContent>
                      {veiculosFiltrados.map((veiculo) => (
                        <SelectItem key={veiculo.id} value={veiculo.id}>
                          {veiculo.marca} {veiculo.modelo} ({veiculo.placa})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Problema relatado</CardTitle>
            </CardHeader>
            <CardContent>
              <Textarea
                value={descricaoProblema}
                onChange={(event) => setDescricaoProblema(event.target.value)}
                placeholder="Descreva o problema informado pelo cliente..."
                required
              />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Serviços e peças</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid gap-3 md:grid-cols-[1fr_120px_auto]">
                <div className="space-y-2">
                  <Label>Adicionar serviço</Label>
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
                </div>
                <div className="space-y-2">
                  <Label>Qtd</Label>
                  <Input type="number" min="1" value={servicoQuantidade} onChange={(event) => setServicoQuantidade(event.target.value)} />
                </div>
                <div className="flex items-end">
                  <Button type="button" variant="outline" onClick={adicionarServico}>
                    Adicionar
                  </Button>
                </div>
              </div>

              <div className="space-y-2 rounded-lg border p-3">
                {servicosSelecionados.length === 0 ? (
                  <p className="text-sm text-muted-foreground">Nenhum serviço adicionado.</p>
                ) : (
                  servicosSelecionados.map((item) => (
                    <div key={item.id} className="flex items-center justify-between gap-2">
                      <span className="text-sm">
                        {item.descricao} x {item.quantidade}
                      </span>
                      <div className="flex items-center gap-2">
                        <span className="text-sm font-medium">{formatCurrency(item.valorUnitario * item.quantidade)}</span>
                        <Button
                          type="button"
                          size="sm"
                          variant="ghost"
                          onClick={() => setServicosSelecionados((prev) => prev.filter((current) => current.id !== item.id))}
                        >
                          Remover
                        </Button>
                      </div>
                    </div>
                  ))
                )}
              </div>

              <div className="grid gap-3 md:grid-cols-[1fr_120px_auto]">
                <div className="space-y-2">
                  <Label>Adicionar peça/insumo</Label>
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
                </div>
                <div className="space-y-2">
                  <Label>Qtd</Label>
                  <Input type="number" min="1" value={pecaQuantidade} onChange={(event) => setPecaQuantidade(event.target.value)} />
                </div>
                <div className="flex items-end">
                  <Button type="button" variant="outline" onClick={adicionarPeca}>
                    Adicionar
                  </Button>
                </div>
              </div>

              <div className="space-y-2 rounded-lg border p-3">
                {pecasSelecionadas.length === 0 ? (
                  <p className="text-sm text-muted-foreground">Nenhuma peça/insumo adicionado.</p>
                ) : (
                  pecasSelecionadas.map((item) => (
                    <div key={item.id} className="flex items-center justify-between gap-2">
                      <span className="text-sm">
                        {item.descricao} x {item.quantidade}
                      </span>
                      <div className="flex items-center gap-2">
                        <span className="text-sm font-medium">{formatCurrency(item.valorUnitario * item.quantidade)}</span>
                        <Button
                          type="button"
                          size="sm"
                          variant="ghost"
                          onClick={() => setPecasSelecionadas((prev) => prev.filter((current) => current.id !== item.id))}
                        >
                          Remover
                        </Button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="flex flex-col gap-2 pt-6 text-sm sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p>Total serviços: {formatCurrency(totalServicos)}</p>
                <p>Total peças/insumos: {formatCurrency(totalPecas)}</p>
                <p className="text-base font-semibold">Orçamento total: {formatCurrency(totalOrcamento)}</p>
              </div>
              <div className="flex gap-2">
                <Button variant="outline" asChild>
                  <Link to="/ordens-servico">Cancelar</Link>
                </Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Criando OS..." : "Criar ordem de serviço"}
                </Button>
              </div>
            </CardContent>
          </Card>
        </form>
      )}
    </div>
  );
}
