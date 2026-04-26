import { useEffect, useState } from "react";
import { Trash2 } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";
import type { Veiculo } from "@/types/veiculo";

export function VeiculoDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [veiculo, setVeiculo] = useState<Veiculo | null>(null);
  const [cliente, setCliente] = useState<Cliente | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (!id) return;
    const veiculoId = id;

    async function loadData() {
      setLoading(true);
      try {
        const veiculoResponse = await veiculosService.getById(veiculoId);
        setVeiculo(veiculoResponse);

        const clienteResponse = await clientesService.getById(veiculoResponse.clienteId);
        setCliente(clienteResponse);
      } catch {
        toast.error("Não foi possível carregar os detalhes do veículo.");
      } finally {
        setLoading(false);
      }
    }

    void loadData();
  }, [id]);

  async function handleDelete() {
    if (!veiculo) return;

    const confirmed = window.confirm(`Excluir o veiculo ${veiculo.placa}?`);
    if (!confirmed) return;

    setDeleting(true);
    try {
      await veiculosService.remove(veiculo.id);
      toast.success("Veiculo excluido com sucesso.");
      navigate("/veiculos");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao excluir veiculo."));
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div>
      <PageHeader
        title="Detalhes do veículo"
        actions={
          <div className="flex flex-wrap gap-2">
            {id && (
              <Button variant="secondary" asChild>
                <Link to={`/veiculos/${id}/editar`}>Editar</Link>
              </Button>
            )}
            {veiculo && (
              <Button variant="destructive" onClick={handleDelete} disabled={deleting}>
                <Trash2 className="h-4 w-4" />
                {deleting ? "Excluindo..." : "Excluir"}
              </Button>
            )}
            <Button variant="outline" asChild>
              <Link to="/veiculos">Voltar</Link>
            </Button>
          </div>
        }
      />

      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <p className="text-sm text-muted-foreground">Carregando...</p>
          ) : !veiculo ? (
            <p className="text-sm text-muted-foreground">Veículo não encontrado.</p>
          ) : (
            <div className="grid gap-4 md:grid-cols-2">
              <DetailItem label="Cliente" value={cliente?.nome ?? veiculo.clienteId} />
              <DetailItem label="Placa" value={veiculo.placa} />
              <DetailItem label="Marca" value={veiculo.marca} />
              <DetailItem label="Modelo" value={veiculo.modelo} />
              <DetailItem label="Ano" value={String(veiculo.ano)} />
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function DetailItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border bg-muted/30 p-4">
      <p className="text-xs uppercase text-muted-foreground">{label}</p>
      <p className="mt-1 font-medium">{value}</p>
    </div>
  );
}
