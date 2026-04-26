import { useEffect, useMemo, useState } from "react";
import { Copy, ExternalLink, Trash2 } from "lucide-react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { StatusBadge } from "@/components/common/StatusBadge";
import { Button } from "@/components/ui/button";
import { formatDateTime } from "@/lib/utils";
import { getApiErrorMessage } from "@/services/api";
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
  const [deletingId, setDeletingId] = useState<string | null>(null);

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
      toast.error("Nao foi possivel carregar as ordens de servico.");
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

  async function handleGenerateTrackingLink(ordem: OrdemServico) {
    const url = `${window.location.origin}/acompanhar-ordem/${ordem.id}`;

    try {
      await navigator.clipboard.writeText(url);
      toast.success("Link de acompanhamento copiado.");
    } catch {
      toast.error("Nao foi possivel copiar o link de acompanhamento.");
    }
  }

  async function handleDelete(ordem: OrdemServico) {
    const confirmed = window.confirm(`Excluir a OS #${ordem.id.slice(0, 8).toUpperCase()}?`);
    if (!confirmed) return;

    setDeletingId(ordem.id);
    try {
      await ordensServicoService.remove(ordem.id);
      toast.success("Ordem de servico excluida com sucesso.");
      await loadData();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao excluir ordem de servico."));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Ordens de Servico"
        description="Acompanhe status, clientes e execucao das ordens de servico."
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
          emptyMessage="Nenhuma ordem de servico registrada."
          columns={[
            {
              key: "codigo",
              title: "Codigo",
              render: (ordem) => `#${ordem.id.slice(0, 8).toUpperCase()}`,
            },
            { key: "cliente", title: "Cliente", render: (ordem) => clienteById[ordem.clienteId] ?? ordem.clienteId },
            { key: "veiculo", title: "Veiculo", render: (ordem) => veiculoById[ordem.veiculoId] ?? ordem.veiculoId },
            { key: "status", title: "Status", render: (ordem) => <StatusBadge status={ordem.status} /> },
            { key: "abertura", title: "Abertura", render: (ordem) => formatDateTime(ordem.dataCriacao) },
            {
              key: "acoes",
              title: "Acoes",
              className: "sticky right-0 z-10 w-[330px] bg-card",
              render: (ordem) => (
                <div className="flex flex-wrap gap-2">
                  <Button variant="outline" size="sm" type="button" onClick={() => handleGenerateTrackingLink(ordem)}>
                    <Copy className="h-3.5 w-3.5" />
                    Gerar link
                  </Button>
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/ordens-servico/${ordem.id}`}>
                      <ExternalLink className="h-3.5 w-3.5" />
                      Detalhes
                    </Link>
                  </Button>
                  <Button variant="destructive" size="sm" onClick={() => handleDelete(ordem)} disabled={deletingId === ordem.id}>
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
