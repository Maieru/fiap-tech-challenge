import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { formatCurrency } from "@/lib/utils";
import { servicosService } from "@/services/servicos.service";
import type { Servico } from "@/types/servico";

export function ServicosListPage() {
  const [servicos, setServicos] = useState<Servico[]>([]);
  const [loading, setLoading] = useState(true);

  async function loadServicos() {
    setLoading(true);
    try {
      const response = await servicosService.list();
      setServicos(response.servicos);
    } catch {
      toast.error("Não foi possível carregar os serviços.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadServicos();
  }, []);

  return (
    <div>
      <PageHeader
        title="Serviços"
        description="Cadastre os serviços oferecidos e seus valores unitários."
        actions={
          <Button asChild>
            <Link to="/servicos/novo">Novo serviço</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={servicos}
          rowKey={(servico) => servico.id}
          emptyMessage="Nenhum serviço cadastrado."
          columns={[
            { key: "descricao", title: "Descrição", render: (servico) => servico.descricao },
            { key: "valor", title: "Valor unitário", render: (servico) => formatCurrency(servico.valorUnitario) },
            {
              key: "acoes",
              title: "Ações",
              className: "w-[120px]",
              render: (servico) => (
                <Button variant="secondary" size="sm" asChild>
                  <Link to={`/servicos/${servico.id}/editar`}>Editar</Link>
                </Button>
              ),
            },
          ]}
        />
      )}
    </div>
  );
}
