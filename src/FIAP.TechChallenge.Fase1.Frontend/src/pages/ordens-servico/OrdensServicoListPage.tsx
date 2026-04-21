import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { StatusBadge } from "@/components/common/StatusBadge";
import { Button } from "@/components/ui/button";
import { formatDateTime } from "@/lib/utils";
import { clientesService } from "@/services/clientes.service";
import { ordensServicoService } from "@/services/ordensServico.service";
import { veiculosService } from "@/services/veiculos.service";
import type { Cliente } from "@/types/cliente";
import type { OrdemServico } from "@/types/ordemServico";
import type { Veiculo } from "@/types/veiculo";

export function OrdensServicoListPage() {
  const [ordens, setOrdens] = useState<OrdemServico[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [veiculos, setVeiculos] = useState<Veiculo[]>([]);
  const [loading, setLoading] = useState(true);

  async function loadData() {
    setLoading(true);
    try {
      const [ordensResponse, clientesResponse, veiculosResponse] = await Promise.all([
        ordensServicoService.list({ pageSize: 100 }),
        clientesService.list({ pageSize: 300 }),
        veiculosService.list({ pageSize: 300 }),
      ]);

      setOrdens(ordensResponse.ordensServico);
      setClientes(clientesResponse.clientes);
      setVeiculos(veiculosResponse.veiculos);
    } catch {
      toast.error("Não foi possível carregar as ordens de serviço.");
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

  const veiculoById = useMemo(
    () =>
      veiculos.reduce<Record<string, string>>((acc, veiculo) => {
        acc[veiculo.id] = `${veiculo.marca} ${veiculo.modelo} (${veiculo.placa})`;
        return acc;
      }, {}),
    [veiculos],
  );

  return (
    <div>
      <PageHeader
        title="Ordens de Serviço"
        description="Acompanhe status, clientes e execução das ordens de serviço."
        actions={
          <Button asChild>
            <Link to="/ordens-servico/nova">Nova OS</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={ordens}
          rowKey={(ordem) => ordem.id}
          emptyMessage="Nenhuma ordem de serviço registrada."
          columns={[
            {
              key: "codigo",
              title: "Código",
              render: (ordem) => `#${ordem.id.slice(0, 8).toUpperCase()}`,
            },
            { key: "cliente", title: "Cliente", render: (ordem) => clienteById[ordem.clienteId] ?? ordem.clienteId },
            { key: "veiculo", title: "Veículo", render: (ordem) => veiculoById[ordem.veiculoId] ?? ordem.veiculoId },
            { key: "status", title: "Status", render: (ordem) => <StatusBadge status={ordem.status} /> },
            { key: "abertura", title: "Abertura", render: (ordem) => formatDateTime(ordem.dataCriacao) },
            {
              key: "acoes",
              title: "Ações",
              className: "w-[100px]",
              render: (ordem) => (
                <Button variant="outline" size="sm" asChild>
                  <Link to={`/ordens-servico/${ordem.id}`}>Detalhes</Link>
                </Button>
              ),
            },
          ]}
        />
      )}
    </div>
  );
}
