import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { clientesService } from "@/services/clientes.service";
import type { Cliente } from "@/types/cliente";

export function ClientesListPage() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);

  async function loadClientes() {
    setLoading(true);
    try {
      const response = await clientesService.list();
      setClientes(response.clientes);
    } catch {
      toast.error("Não foi possível carregar os clientes.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadClientes();
  }, []);

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
              title: "Ações",
              className: "w-[190px]",
              render: (cliente) => (
                <div className="flex gap-2">
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/clientes/${cliente.id}`}>Detalhes</Link>
                  </Button>
                  <Button variant="secondary" size="sm" asChild>
                    <Link to={`/clientes/${cliente.id}/editar`}>Editar</Link>
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
