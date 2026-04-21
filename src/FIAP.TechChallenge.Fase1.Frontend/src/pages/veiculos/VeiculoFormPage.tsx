import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";

const initialForm = {
  clienteId: "",
  placa: "",
  marca: "",
  modelo: "",
  ano: String(new Date().getFullYear()),
};

export function VeiculoFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [formData, setFormData] = useState(initialForm);
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    async function loadData() {
      setLoading(true);
      try {
        const clientesResponse = await clientesService.list({ pageSize: 200 });
        setClientes(clientesResponse.clientes);

        if (id) {
          const veiculo = await veiculosService.getById(id);
          setFormData({
            clienteId: veiculo.clienteId,
            placa: veiculo.placa,
            marca: veiculo.marca,
            modelo: veiculo.modelo,
            ano: String(veiculo.ano),
          });
        }
      } catch {
        toast.error("Não foi possível carregar os dados do veículo.");
      } finally {
        setLoading(false);
      }
    }

    void loadData();
  }, [id]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);

    try {
      const payload = {
        placa: formData.placa.toUpperCase(),
        marca: formData.marca,
        modelo: formData.modelo,
        ano: Number(formData.ano),
      };

      if (isEdit && id) {
        await veiculosService.update(id, payload);
        toast.success("Veículo atualizado com sucesso.");
      } else {
        await veiculosService.create({
          ...payload,
          clienteId: formData.clienteId,
        });
        toast.success("Veículo cadastrado com sucesso.");
      }

      navigate("/veiculos");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao salvar veículo."));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div>
      <PageHeader title={isEdit ? "Editar veículo" : "Novo veículo"} />
      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <p className="text-sm text-muted-foreground">Carregando...</p>
          ) : (
            <form className="space-y-4" onSubmit={handleSubmit}>
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="cliente">Cliente</Label>
                  <Select
                    value={formData.clienteId}
                    onValueChange={(value) => setFormData((prev) => ({ ...prev, clienteId: value }))}
                    disabled={isEdit}
                  >
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
                  {isEdit && <p className="text-xs text-muted-foreground">O cliente do veículo não pode ser alterado.</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="placa">Placa</Label>
                  <Input
                    id="placa"
                    value={formData.placa}
                    onChange={(event) => setFormData((prev) => ({ ...prev, placa: event.target.value }))}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="ano">Ano</Label>
                  <Input
                    id="ano"
                    type="number"
                    value={formData.ano}
                    onChange={(event) => setFormData((prev) => ({ ...prev, ano: event.target.value }))}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="marca">Marca</Label>
                  <Input
                    id="marca"
                    value={formData.marca}
                    onChange={(event) => setFormData((prev) => ({ ...prev, marca: event.target.value }))}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="modelo">Modelo</Label>
                  <Input
                    id="modelo"
                    value={formData.modelo}
                    onChange={(event) => setFormData((prev) => ({ ...prev, modelo: event.target.value }))}
                    required
                  />
                </div>
              </div>

              <div className="flex justify-end gap-2">
                <Button variant="outline" asChild>
                  <Link to="/veiculos">Cancelar</Link>
                </Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Salvando..." : "Salvar"}
                </Button>
              </div>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
