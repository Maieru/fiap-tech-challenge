import { useEffect, useState } from "react";
import { Trash2 } from "lucide-react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import type { Cliente } from "@/types/cliente";

export function ClientesListPage() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  async function loadClientes() {
    setLoading(true);
    try {
      const response = await clientesService.list();
      setClientes(response.clientes);
    } catch {
      toast.error("Nao foi possivel carregar os clientes.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadClientes();
  }, []);

  async function handleDelete(cliente: Cliente) {
    const confirmed = window.confirm(`Excluir ${cliente.nome}?`);
    if (!confirmed) return;

    setDeletingId(cliente.id);
    try {
      await clientesService.remove(cliente.id);
      toast.success("Cliente excluido com sucesso.");
      await loadClientes();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao excluir cliente."));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Clientes"
        description="Gerencie o cadastro de clientes da oficina."
        actions={
          <Button asChild>
            <Link to="/clientes/novo">Novo cliente</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={clientes}
          rowKey={(cliente) => cliente.id}
          emptyMessage="Nenhum cliente cadastrado."
          columns={[
            { key: "nome", title: "Nome", render: (cliente) => cliente.nome },
            {
              key: "documento",
              title: "CPF/CNPJ",
              render: (cliente) => cliente.cpf ?? cliente.cnpj ?? "-",
            },
            { key: "telefone", title: "Telefone", render: (cliente) => cliente.telefone },
            { key: "email", title: "Email", render: (cliente) => cliente.email ?? "-" },
            {
              key: "acoes",
              title: "Acoes",
              className: "sticky right-0 z-10 w-[280px] bg-card",
              render: (cliente) => (
                <div className="flex flex-wrap gap-2">
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/clientes/${cliente.id}`}>Detalhes</Link>
                  </Button>
                  <Button variant="secondary" size="sm" asChild>
                    <Link to={`/clientes/${cliente.id}/editar`}>Editar</Link>
                  </Button>
                  <Button variant="destructive" size="sm" onClick={() => handleDelete(cliente)} disabled={deletingId === cliente.id}>
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
