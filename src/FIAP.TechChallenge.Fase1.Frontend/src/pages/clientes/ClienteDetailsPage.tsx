import { useEffect, useState } from "react";
import { Trash2 } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";
import type { Cliente } from "@/types/cliente";

export function ClienteDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [cliente, setCliente] = useState<Cliente | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (!id) return;
    const clienteId = id;

    async function loadCliente() {
      setLoading(true);
      try {
        const response = await clientesService.getById(clienteId);
        setCliente(response);
      } catch {
        toast.error("Não foi possível carregar os detalhes do cliente.");
      } finally {
        setLoading(false);
      }
    }

    void loadCliente();
  }, [id]);

  async function handleDelete() {
    if (!cliente) return;

    const confirmed = window.confirm(`Excluir ${cliente.nome}?`);
    if (!confirmed) return;

    setDeleting(true);
    try {
      await clientesService.remove(cliente.id);
      toast.success("Cliente excluido com sucesso.");
      navigate("/clientes");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao excluir cliente."));
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div>
      <PageHeader
        title="Detalhes do cliente"
        actions={
          <div className="flex flex-wrap gap-2">
            {id && (
              <Button variant="secondary" asChild>
                <Link to={`/clientes/${id}/editar`}>Editar</Link>
              </Button>
            )}
            {cliente && (
              <Button variant="destructive" onClick={handleDelete} disabled={deleting}>
                <Trash2 className="h-4 w-4" />
                {deleting ? "Excluindo..." : "Excluir"}
              </Button>
            )}
            <Button variant="outline" asChild>
              <Link to="/clientes">Voltar</Link>
            </Button>
          </div>
        }
      />

      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <p className="text-sm text-muted-foreground">Carregando...</p>
          ) : !cliente ? (
            <p className="text-sm text-muted-foreground">Cliente não encontrado.</p>
          ) : (
            <div className="grid gap-4 md:grid-cols-2">
              <DetailItem label="Nome" value={cliente.nome} />
              <DetailItem label="Telefone" value={cliente.telefone} />
              <DetailItem label="CPF/CNPJ" value={cliente.cpf ?? cliente.cnpj ?? "-"} />
              <DetailItem label="Email" value={cliente.email ?? "-"} />
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
