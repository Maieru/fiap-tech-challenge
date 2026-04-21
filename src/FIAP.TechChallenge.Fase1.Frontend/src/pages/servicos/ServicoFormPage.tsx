import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getApiErrorMessage } from "@/services/api";
import { servicosService } from "@/services/servicos.service";
import type { TempoMedioServico } from "@/types/servico";

const initialForm = {
  descricao: "",
  valorUnitario: "",
};

export function ServicoFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [formData, setFormData] = useState(initialForm);
  const [tempoMedio, setTempoMedio] = useState<TempoMedioServico | null>(null);
  const [loading, setLoading] = useState(isEdit);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    const servicoId = id;

    async function loadServico() {
      setLoading(true);
      try {
        const [servico, tempoMedioServico] = await Promise.all([
          servicosService.getById(servicoId),
          servicosService.getTempoMedio(servicoId),
        ]);

        setFormData({
          descricao: servico.descricao,
          valorUnitario: String(servico.valorUnitario),
        });
        setTempoMedio(tempoMedioServico);
      } catch {
        toast.error("Não foi possível carregar os dados do serviço.");
      } finally {
        setLoading(false);
      }
    }

    void loadServico();
  }, [id]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);

    try {
      const payload = {
        descricao: formData.descricao,
        valorUnitario: Number(formData.valorUnitario),
      };

      if (isEdit && id) {
        await servicosService.update(id, payload);
        toast.success("Serviço atualizado com sucesso.");
      } else {
        await servicosService.create(payload);
        toast.success("Serviço cadastrado com sucesso.");
      }

      navigate("/servicos");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao salvar serviço."));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div>
      <PageHeader title={isEdit ? "Editar serviço" : "Novo serviço"} />
      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <p className="text-sm text-muted-foreground">Carregando...</p>
          ) : (
            <form className="space-y-4" onSubmit={handleSubmit}>
              {isEdit && tempoMedio ? (
                <div className="grid grid-cols-1 gap-4 rounded-md border p-4 md:grid-cols-2">
                  <div className="space-y-1">
                    <p className="text-xs text-muted-foreground">Quantidade de execuções</p>
                    <p className="text-sm font-medium">{tempoMedio.quantidadeExecucoes}</p>
                  </div>
                  <div className="space-y-1">
                    <p className="text-xs text-muted-foreground">Tempo médio de execução</p>
                    <p className="text-sm font-medium">{tempoMedio.tempoMedioMinutos.toFixed(2)} min</p>
                  </div>
                </div>
              ) : null}

              <div className="space-y-2">
                <Label htmlFor="descricao">Descrição</Label>
                <Input
                  id="descricao"
                  value={formData.descricao}
                  onChange={(event) => setFormData((prev) => ({ ...prev, descricao: event.target.value }))}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="valor">Valor unitário</Label>
                <Input
                  id="valor"
                  type="number"
                  step="0.01"
                  min="0"
                  value={formData.valorUnitario}
                  onChange={(event) => setFormData((prev) => ({ ...prev, valorUnitario: event.target.value }))}
                  required
                />
              </div>

              <div className="flex justify-end gap-2">
                <Button variant="outline" asChild>
                  <Link to="/servicos">Cancelar</Link>
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
