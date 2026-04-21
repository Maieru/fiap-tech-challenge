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
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import { ordensServicoService } from "@/services/ordensServico.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";
import type { Veiculo } from "@/types/veiculo";

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
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [descricaoProblema, setDescricaoProblema] = useState("");
  const [clienteId, setClienteId] = useState("");
  const [veiculoId, setVeiculoId] = useState("");
  const [usarNovoCliente, setUsarNovoCliente] = useState(false);
  const [novoCliente, setNovoCliente] = useState(initialNovoCliente);
  const [cadastrarNovoVeiculo, setCadastrarNovoVeiculo] = useState(false);
  const [novoVeiculo, setNovoVeiculo] = useState(initialNovoVeiculo);

  useEffect(() => {
    async function loadData() {
      setLoading(true);
      try {
        const [clientesResponse, veiculosResponse] = await Promise.all([
          clientesService.list({ pageSize: 300 }),
          veiculosService.list({ pageSize: 300 }),
        ]);

        setClientes(clientesResponse.clientes);
        setVeiculos(veiculosResponse.veiculos);
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
      const deveCadastrarNovoVeiculo = usarNovoCliente || cadastrarNovoVeiculo;

      if (deveCadastrarNovoVeiculo) {
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
        description="Cadastre cliente, veículo e descrição do problema para abrir a OS."
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
              <div className="space-y-2">
                <Label>Cliente</Label>
                <div className="space-y-2 rounded-md border p-3">
                  <label className="flex items-center gap-2 text-sm">
                    <input
                      type="radio"
                      name="tipo-cliente"
                      checked={usarNovoCliente}
                      onChange={() => {
                        setUsarNovoCliente(true);
                        setClienteId("");
                        setCadastrarNovoVeiculo(true);
                        setVeiculoId("");
                      }}
                    />
                    Criar novo cliente
                  </label>
                  <label className="flex items-center gap-2 text-sm">
                    <input
                      type="radio"
                      name="tipo-cliente"
                      checked={!usarNovoCliente}
                      onChange={() => {
                        setUsarNovoCliente(false);
                        setClienteId("");
                      }}
                    />
                    Usar cliente existente
                  </label>
                </div>
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

              {usarNovoCliente ? (
                <div className="rounded-md border border-dashed p-3 text-sm text-muted-foreground">
                  Para novo cliente, o veículo também será cadastrado como novo nesta OS.
                </div>
              ) : (
                <div className="space-y-2">
                  <Label>Veículo</Label>
                  <div className="space-y-2 rounded-md border p-3">
                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="radio"
                        name="tipo-veiculo"
                        checked={cadastrarNovoVeiculo}
                        onChange={() => {
                          setCadastrarNovoVeiculo(true);
                          setVeiculoId("");
                        }}
                      />
                      Criar novo veículo
                    </label>
                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="radio"
                        name="tipo-veiculo"
                        checked={!cadastrarNovoVeiculo}
                        onChange={() => {
                          setCadastrarNovoVeiculo(false);
                          setVeiculoId("");
                        }}
                      />
                      Usar veículo existente
                    </label>
                  </div>
                </div>
              )}

              {usarNovoCliente || cadastrarNovoVeiculo ? (
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
            <CardContent className="flex justify-end gap-2 pt-6">
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
