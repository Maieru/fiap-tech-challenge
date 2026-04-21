import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/services/api";
import { pecasInsumosService } from "@/services/pecasInsumos.service";

const initialForm = {
  nome: "",
  codigo: "",
  descricao: "",
  precoUnitario: "",
  quantidadeEstoque: "0",
  ativo: "true",
};

export function PecaInsumoFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [formData, setFormData] = useState(initialForm);
  const [loading, setLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    const pecaId = id;

    async function loadPeca() {
      setLoading(true);
      try {
        const peca = await pecasInsumosService.getById(pecaId);
        setFormData({
          nome: peca.nome,
          codigo: peca.codigo,
          descricao: peca.descricao ?? "",
          precoUnitario: String(peca.precoUnitario),
          quantidadeEstoque: String(peca.quantidadeEstoque),
          ativo: peca.ativo ? "true" : "false",
        });
      } catch {
        toast.error("Não foi possível carregar os dados do item.");
      } finally {
        setLoading(false);
      }
    }

    void loadPeca();
  }, [id]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);

    try {
      if (isEdit && id) {
        await pecasInsumosService.update(id, {
          nome: formData.nome,
          codigo: formData.codigo,
          descricao: formData.descricao || undefined,
          precoUnitario: Number(formData.precoUnitario),
          ativo: formData.ativo === "true",
        });
        toast.success("Item atualizado com sucesso.");
      } else {
        await pecasInsumosService.create({
          nome: formData.nome,
          codigo: formData.codigo,
          descricao: formData.descricao || undefined,
          precoUnitario: Number(formData.precoUnitario),
          quantidadeEstoque: Number(formData.quantidadeEstoque),
        });
        toast.success("Item cadastrado com sucesso.");
      }

      navigate("/pecas-insumos");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao salvar item."));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div>
      <PageHeader title={isEdit ? "Editar peça/insumo" : "Nova peça/insumo"} />
      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <p className="text-sm text-muted-foreground">Carregando...</p>
          ) : (
            <form className="space-y-4" onSubmit={handleSubmit}>
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="nome">Nome</Label>
                  <Input
                    id="nome"
                    value={formData.nome}
                    onChange={(event) => setFormData((prev) => ({ ...prev, nome: event.target.value }))}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="codigo">Código</Label>
                  <Input
                    id="codigo"
                    value={formData.codigo}
                    onChange={(event) => setFormData((prev) => ({ ...prev, codigo: event.target.value }))}
                    required
                  />
                </div>

                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="descricao">Descrição</Label>
                  <Textarea
                    id="descricao"
                    value={formData.descricao}
                    onChange={(event) => setFormData((prev) => ({ ...prev, descricao: event.target.value }))}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="preco">Preço unitário</Label>
                  <Input
                    id="preco"
                    type="number"
                    min="0"
                    step="0.01"
                    value={formData.precoUnitario}
                    onChange={(event) => setFormData((prev) => ({ ...prev, precoUnitario: event.target.value }))}
                    required
                  />
                </div>

                {!isEdit ? (
                  <div className="space-y-2">
                    <Label htmlFor="quantidade">Quantidade em estoque</Label>
                    <Input
                      id="quantidade"
                      type="number"
                      min="0"
                      value={formData.quantidadeEstoque}
                      onChange={(event) => setFormData((prev) => ({ ...prev, quantidadeEstoque: event.target.value }))}
                      required
                    />
                  </div>
                ) : (
                  <div className="space-y-2">
                    <Label htmlFor="ativo">Status</Label>
                    <Select
                      value={formData.ativo}
                      onValueChange={(value) => setFormData((prev) => ({ ...prev, ativo: value }))}
                    >
                      <SelectTrigger id="ativo">
                        <SelectValue placeholder="Selecione" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="true">Ativo</SelectItem>
                        <SelectItem value="false">Inativo</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                )}
              </div>

              <div className="flex justify-end gap-2">
                <Button variant="outline" asChild>
                  <Link to="/pecas-insumos">Cancelar</Link>
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
