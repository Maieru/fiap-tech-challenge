import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";
import { PageHeader } from "@/components/common/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { formatCurrency } from "@/lib/utils";
import { pecasInsumosService } from "@/services/pecasInsumos.service";
import type { PecaInsumo } from "@/types/pecaInsumo";

export function PecaInsumoDetailsPage() {
  const { id } = useParams();
  const [peca, setPeca] = useState<PecaInsumo | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    const pecaId = id;

    async function loadPeca() {
      setLoading(true);
      try {
        const response = await pecasInsumosService.getById(pecaId);
        setPeca(response);
      } catch {
        toast.error("Não foi possível carregar os detalhes.");
      } finally {
        setLoading(false);
      }
    }

    void loadPeca();
  }, [id]);

  return (
    <div>
      <PageHeader
        title="Detalhes da peça/insumo"
        actions={
          <div className="flex gap-2">
            {id && (
              <Button variant="secondary" asChild>
                <Link to={`/pecas-insumos/${id}/editar`}>Editar</Link>
              </Button>
            )}
            <Button variant="outline" asChild>
              <Link to="/pecas-insumos">Voltar</Link>
            </Button>
          </div>
        }
      />

      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <p className="text-sm text-muted-foreground">Carregando...</p>
          ) : !peca ? (
            <p className="text-sm text-muted-foreground">Item não encontrado.</p>
          ) : (
            <div className="grid gap-4 md:grid-cols-2">
              <DetailItem label="Nome" value={peca.nome} />
              <DetailItem label="Código" value={peca.codigo} />
              <DetailItem label="Descrição" value={peca.descricao ?? "-"} />
              <DetailItem label="Preço unitário" value={formatCurrency(peca.precoUnitario)} />
              <DetailItem label="Quantidade em estoque" value={String(peca.quantidadeEstoque)} />
              <div className="rounded-md border bg-muted/30 p-4">
                <p className="text-xs uppercase text-muted-foreground">Status</p>
                <div className="mt-1">
                  {peca.ativo ? <Badge variant="success">Ativo</Badge> : <Badge variant="secondary">Inativo</Badge>}
                </div>
              </div>
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
