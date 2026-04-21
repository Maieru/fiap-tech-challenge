import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { clientesService } from "@/services/clientes.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";
import type { Veiculo } from "@/types/veiculo";

export function VeiculosListPage() {
  const [veiculos, setVeiculos] = useState<Veiculo[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);

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
      toast.error("Não foi possível carregar os veículos.");
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

  return (
    <div>
      <PageHeader
        title="Veículos"
        description="Mantenha os veículos dos clientes atualizados para abertura de OS."
        actions={
          <Button asChild>
            <Link to="/veiculos/novo">Novo veículo</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={veiculos}
          rowKey={(veiculo) => veiculo.id}
          emptyMessage="Nenhum veículo cadastrado."
          columns={[
            { key: "placa", title: "Placa", render: (veiculo) => veiculo.placa },
            { key: "modelo", title: "Modelo", render: (veiculo) => `${veiculo.marca} ${veiculo.modelo}` },
            { key: "ano", title: "Ano", render: (veiculo) => veiculo.ano },
            { key: "cliente", title: "Cliente", render: (veiculo) => clienteById[veiculo.clienteId] ?? veiculo.clienteId },
            {
              key: "acoes",
              title: "Ações",
              className: "w-[190px]",
              render: (veiculo) => (
                <div className="flex gap-2">
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/veiculos/${veiculo.id}`}>Detalhes</Link>
                  </Button>
                  <Button variant="secondary" size="sm" asChild>
                    <Link to={`/veiculos/${veiculo.id}/editar`}>Editar</Link>
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
