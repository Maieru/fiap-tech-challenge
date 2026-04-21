import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getApiErrorMessage } from "@/services/api";
import { clientesService } from "@/services/clientes.service";

const initialForm = {
  nome: "",
  telefone: "",
  email: "",
  documento: "",
};

export function ClienteFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [formData, setFormData] = useState(initialForm);
  const [isLoading, setIsLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    const clienteId = id;

    async function loadCliente() {
      setIsLoading(true);
      try {
        const cliente = await clientesService.getById(clienteId);
        setFormData({
          nome: cliente.nome,
          telefone: cliente.telefone,
          email: cliente.email ?? "",
          documento: cliente.cpf ?? cliente.cnpj ?? "",
        });
      } catch {
        toast.error("Não foi possível carregar o cliente.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadCliente();
  }, [id]);

  const documentoLabel = useMemo(() => {
    if (formData.documento.length <= 11) return "CPF";
    return "CNPJ";
  }, [formData.documento.length]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);

    try {
      if (isEdit && id) {
        await clientesService.update(id, {
          nome: formData.nome,
          telefone: formData.telefone,
          email: formData.email || undefined,
        });
        toast.success("Cliente atualizado com sucesso.");
      } else {
        const normalizedDocument = formData.documento.replace(/\D/g, "");
        await clientesService.create({
          nome: formData.nome,
          telefone: formData.telefone,
          email: formData.email || undefined,
          cpf: normalizedDocument.length <= 11 ? normalizedDocument : undefined,
          cnpj: normalizedDocument.length > 11 ? normalizedDocument : undefined,
        });
        toast.success("Cliente cadastrado com sucesso.");
      }

      navigate("/clientes");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao salvar cliente."));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div>
      <PageHeader
        title={isEdit ? "Editar cliente" : "Novo cliente"}
        description="Preencha os dados principais para cadastro e atendimento."
      />

      <Card>
        <CardContent className="pt-6">
          {isLoading ? (
            <p className="text-sm text-muted-foreground">Carregando...</p>
          ) : (
            <form className="space-y-4" onSubmit={handleSubmit}>
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="nome">Nome</Label>
                  <Input
                    id="nome"
                    value={formData.nome}
                    onChange={(event) => setFormData((prev) => ({ ...prev, nome: event.target.value }))}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="documento">{documentoLabel}</Label>
                  <Input
                    id="documento"
                    value={formData.documento}
                    disabled={isEdit}
                    placeholder="Somente números"
                    onChange={(event) => setFormData((prev) => ({ ...prev, documento: event.target.value }))}
                    required={!isEdit}
                  />
                  {isEdit && <p className="text-xs text-muted-foreground">CPF/CNPJ não pode ser alterado neste endpoint.</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="telefone">Telefone</Label>
                  <Input
                    id="telefone"
                    value={formData.telefone}
                    onChange={(event) => setFormData((prev) => ({ ...prev, telefone: event.target.value }))}
                    required
                  />
                </div>

                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    type="email"
                    value={formData.email}
                    onChange={(event) => setFormData((prev) => ({ ...prev, email: event.target.value }))}
                  />
                </div>
              </div>

              <div className="flex justify-end gap-2">
                <Button variant="outline" asChild>
                  <Link to="/clientes">Cancelar</Link>
                </Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Salvando..." : "Salvar"}
                </Button>
              </div>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
