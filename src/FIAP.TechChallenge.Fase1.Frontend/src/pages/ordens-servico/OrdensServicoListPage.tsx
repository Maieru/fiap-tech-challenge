import { useCallback, useEffect, useMemo, useRef, useState, type MouseEvent as ReactMouseEvent } from "react";
import { ArrowDown, ArrowUp, ArrowUpDown, Check, Copy, ExternalLink, Filter, Trash2 } from "lucide-react";
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
import type { OrdemServico, SortDirection, StatusOrdemServico } from "@/types/ordemServico";
import type { Veiculo } from "@/types/veiculo";

type SortColumn = "status" | "dataAbertura";
type SortState = Partial<Record<SortColumn, SortDirection>>;
type MenuPosition = { top: number; left: number };

const STATUS_FILTER_OPTIONS: StatusOrdemServico[] = [1, 2, 3, 4, 5, 6, 7];
const STATUS_FILTER_LABELS: Record<StatusOrdemServico, string> = {
  1: "Recebida",
  2: "Diagnostico",
  3: "Aguard. aprov.",
  4: "Execucao",
  5: "Finalizada",
  6: "Entregue",
  7: "Cancelada",
};
const STATUS_FILTER_MENU_WIDTH = 300;

export function OrdensServicoListPage() {
  const [ordens, setOrdens] = useState<OrdemServico[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [veiculos, setVeiculos] = useState<Veiculo[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [sortState, setSortState] = useState<SortState>({});
  const [statusFilter, setStatusFilter] = useState<StatusOrdemServico[]>([]);
  const [statusMenuOpen, setStatusMenuOpen] = useState(false);
  const [statusMenuPosition, setStatusMenuPosition] = useState<MenuPosition>({ top: 0, left: 0 });
  const statusMenuRef = useRef<HTMLDivElement | null>(null);
  const statusMenuButtonRef = useRef<HTMLButtonElement | null>(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [ordensResponse, clientesResponse, veiculosResponse] = await Promise.all([
        ordensServicoService.list({
          pageSize: 100,
          status: statusFilter,
          statusSortDirection: sortState.status,
          dataAberturaSortDirection: sortState.dataAbertura,
        }),
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
  }, [sortState, statusFilter]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  useEffect(() => {
    if (!statusMenuOpen) return;

    function handleMouseDown(event: globalThis.MouseEvent) {
      const target = event.target as Node;

      if (statusMenuRef.current?.contains(target) || statusMenuButtonRef.current?.contains(target)) {
        return;
      }

      setStatusMenuOpen(false);
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setStatusMenuOpen(false);
      }
    }

    function handleViewportChange() {
      setStatusMenuOpen(false);
    }

    document.addEventListener("mousedown", handleMouseDown);
    document.addEventListener("keydown", handleKeyDown);
    window.addEventListener("resize", handleViewportChange);
    window.addEventListener("scroll", handleViewportChange, true);

    return () => {
      document.removeEventListener("mousedown", handleMouseDown);
      document.removeEventListener("keydown", handleKeyDown);
      window.removeEventListener("resize", handleViewportChange);
      window.removeEventListener("scroll", handleViewportChange, true);
    };
  }, [statusMenuOpen]);

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

  function handleSort(column: SortColumn) {
    setSortState((current) => {
      const nextDirection = getNextSortDirection(current[column]);
      const next = { ...current };

      if (nextDirection) {
        next[column] = nextDirection;
      } else {
        delete next[column];
      }

      return next;
    });
  }

  function handleStatusFilter(status: StatusOrdemServico) {
    setStatusFilter((current) => {
      if (current.includes(status)) {
        return current.filter((item) => item !== status);
      }

      return [...current, status];
    });
  }

  function handleStatusMenuToggle(event: ReactMouseEvent<HTMLButtonElement>) {
    const rect = event.currentTarget.getBoundingClientRect();
    const maxLeft = Math.max(12, window.innerWidth - STATUS_FILTER_MENU_WIDTH - 12);
    const left = Math.min(Math.max(12, rect.right - STATUS_FILTER_MENU_WIDTH), maxLeft);

    setStatusMenuPosition({
      top: rect.bottom + 8,
      left,
    });
    setStatusMenuOpen((current) => !current);
  }

  function renderSortableHeader(label: string, column: SortColumn) {
    const direction = sortState[column];
    const Icon = direction === "Asc" ? ArrowUp : direction === "Desc" ? ArrowDown : ArrowUpDown;
    const title = direction === "Asc" ? "Ordenado ascendente" : direction === "Desc" ? "Ordenado descendente" : "Sem ordenacao";

    return (
      <button
        type="button"
        className="-ml-2 inline-flex h-8 items-center gap-1.5 rounded-md px-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
        title={title}
        onClick={() => handleSort(column)}
      >
        {label}
        <Icon className="h-3.5 w-3.5" />
      </button>
    );
  }

  function renderStatusHeader() {
    return (
      <div className="flex min-w-[150px] items-center gap-1.5">
        {renderSortableHeader("Status", "status")}
        <button
          ref={statusMenuButtonRef}
          type="button"
          className={[
            "relative inline-flex h-8 items-center gap-1.5 rounded-md border px-2 text-xs font-medium transition-colors",
            statusFilter.length > 0 || statusMenuOpen
              ? "border-primary bg-primary text-primary-foreground"
              : "border-input bg-background text-muted-foreground hover:bg-muted hover:text-foreground",
          ].join(" ")}
          title="Filtrar status"
          aria-expanded={statusMenuOpen}
          onClick={handleStatusMenuToggle}
        >
          <Filter className="h-3.5 w-3.5" />
          {statusFilter.length > 0 ? <span>{statusFilter.length}</span> : null}
        </button>
      </div>
    );
  }

  function renderStatusFilterMenu() {
    if (!statusMenuOpen) return null;

    return (
      <div
        ref={statusMenuRef}
        className="fixed z-50 rounded-md border bg-popover p-2 text-popover-foreground shadow-lg"
        style={{ top: statusMenuPosition.top, left: statusMenuPosition.left, width: STATUS_FILTER_MENU_WIDTH }}
      >
        <div className="flex items-center justify-between gap-3 px-2 py-1.5">
          <span className="text-sm font-semibold text-foreground">Filtrar status</span>
          {statusFilter.length > 0 ? (
            <button
              type="button"
              className="rounded px-2 py-1 text-xs font-medium text-primary transition-colors hover:bg-muted"
              onClick={() => setStatusFilter([])}
            >
              Limpar
            </button>
          ) : null}
        </div>

        <div className="mt-1 space-y-1">
          {STATUS_FILTER_OPTIONS.map((status) => {
            const selected = statusFilter.includes(status);

            return (
              <button
                key={status}
                type="button"
                className={[
                  "flex w-full items-center gap-2 rounded-md px-2 py-2 text-left text-sm transition-colors",
                  selected ? "bg-muted text-foreground" : "text-muted-foreground hover:bg-muted hover:text-foreground",
                ].join(" ")}
                aria-pressed={selected}
                onClick={() => handleStatusFilter(status)}
              >
                <span
                  className={[
                    "flex h-4 w-4 items-center justify-center rounded border",
                    selected ? "border-primary bg-primary text-primary-foreground" : "border-input bg-background",
                  ].join(" ")}
                >
                  {selected ? <Check className="h-3 w-3" /> : null}
                </span>
                <span>{STATUS_FILTER_LABELS[status]}</span>
              </button>
            );
          })}
        </div>
      </div>
    );
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
        <div>
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
              { key: "status", title: renderStatusHeader(), render: (ordem) => <StatusBadge status={ordem.status} /> },
              { key: "abertura", title: renderSortableHeader("Abertura", "dataAbertura"), render: (ordem) => formatDateTime(ordem.dataCriacao) },
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
          {renderStatusFilterMenu()}
        </div>
      )}
    </div>
  );
}

function getNextSortDirection(direction?: SortDirection): SortDirection | undefined {
  if (!direction) return "Asc";
  if (direction === "Asc") return "Desc";
  return undefined;
}
