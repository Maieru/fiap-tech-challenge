import { useEffect, useMemo, useState } from "react";
import { Trash2 } from "lucide-react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";
import type { Veiculo } from "@/types/veiculo";

export function VeiculosListPage() {
  const [veiculos, setVeiculos] = useState<Veiculo[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  async function loadData() {
    setLoading(true);
    try {
      const [veiculosResponse, clientesResponse] = await Promise.all([
        veiculosService.list(),
        clientesService.list({ pageSize: 200 }),
      ]);
      setVeiculos(veiculosResponse.veiculos);
      setClientes(clientesResponse.clientes);
    } catch {
      toast.error("Nao foi possivel carregar os veiculos.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadData();
  }, []);

  const clienteById = useMemo(
    () =>
      clientes.reduce<Record<string, string>>((acc, cliente) => {
        acc[cliente.id] = cliente.nome;
        return acc;
      }, {}),
    [clientes],
  );

  async function handleDelete(veiculo: Veiculo) {
    const confirmed = window.confirm(`Excluir o veiculo ${veiculo.placa}?`);
    if (!confirmed) return;

    setDeletingId(veiculo.id);
    try {
      await veiculosService.remove(veiculo.id);
      toast.success("Veiculo excluido com sucesso.");
      await loadData();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao excluir veiculo."));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Veiculos"
        description="Mantenha os veiculos dos clientes atualizados para abertura de OS."
        actions={
          <Button asChild>
            <Link to="/veiculos/novo">Novo veiculo</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={veiculos}
          rowKey={(veiculo) => veiculo.id}
          emptyMessage="Nenhum veiculo cadastrado."
          columns={[
            { key: "placa", title: "Placa", render: (veiculo) => veiculo.placa },
            { key: "modelo", title: "Modelo", render: (veiculo) => `${veiculo.marca} ${veiculo.modelo}` },
            { key: "ano", title: "Ano", render: (veiculo) => veiculo.ano },
            { key: "cliente", title: "Cliente", render: (veiculo) => clienteById[veiculo.clienteId] ?? veiculo.clienteId },
            {
              key: "acoes",
              title: "Acoes",
              className: "sticky right-0 z-10 w-[280px] bg-card",
              render: (veiculo) => (
                <div className="flex flex-wrap gap-2">
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/veiculos/${veiculo.id}`}>Detalhes</Link>
                  </Button>
                  <Button variant="secondary" size="sm" asChild>
                    <Link to={`/veiculos/${veiculo.id}/editar`}>Editar</Link>
                  </Button>
                  <Button variant="destructive" size="sm" onClick={() => handleDelete(veiculo)} disabled={deletingId === veiculo.id}>
                    <Trash2 className="h-3.5 w-3.5" />
                    Excluir
                  </Button>
                </div>
              ),
            },
          ]}
        />
      )}
    </div>
  );
}
