import { useEffect, useState } from "react";
import { Trash2 } from "lucide-react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { formatCurrency } from "@/lib/utils";
import { getApiErrorMessage } from "@/services/api";
import { servicosService } from "@/services/servicos.service";
import type { Servico } from "@/types/servico";

export function ServicosListPage() {
  const [servicos, setServicos] = useState<Servico[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  async function loadServicos() {
    setLoading(true);
    try {
      const response = await servicosService.list();
      setServicos(response.servicos);
    } catch {
      toast.error("Nao foi possivel carregar os servicos.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadServicos();
  }, []);

  async function handleDelete(servico: Servico) {
    const confirmed = window.confirm(`Excluir o servico ${servico.descricao}?`);
    if (!confirmed) return;

    setDeletingId(servico.id);
    try {
      await servicosService.remove(servico.id);
      toast.success("Servico excluido com sucesso.");
      await loadServicos();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao excluir servico."));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Servicos"
        description="Cadastre os servicos oferecidos e seus valores unitarios."
        actions={
          <Button asChild>
            <Link to="/servicos/novo">Novo servico</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={servicos}
          rowKey={(servico) => servico.id}
          emptyMessage="Nenhum servico cadastrado."
          columns={[
            { key: "descricao", title: "Descricao", render: (servico) => servico.descricao },
            { key: "valor", title: "Valor unitario", render: (servico) => formatCurrency(servico.valorUnitario) },
            {
              key: "acoes",
              title: "Acoes",
              className: "sticky right-0 z-10 w-[220px] bg-card",
              render: (servico) => (
                <div className="flex flex-wrap gap-2">
                  <Button variant="secondary" size="sm" asChild>
                    <Link to={`/servicos/${servico.id}/editar`}>Editar</Link>
                  </Button>
                  <Button variant="destructive" size="sm" onClick={() => handleDelete(servico)} disabled={deletingId === servico.id}>
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
