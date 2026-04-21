import { useEffect, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { EntityTable } from "@/components/common/EntityTable";
import { LoadingState } from "@/components/common/LoadingState";
import { PageHeader } from "@/components/common/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { formatCurrency } from "@/lib/utils";
import { getApiErrorMessage } from "@/services/api";
import { pecasInsumosService } from "@/services/pecasInsumos.service";
import type { PecaInsumo } from "@/types/pecaInsumo";

export function PecasInsumosListPage() {
  const [pecas, setPecas] = useState<PecaInsumo[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedPeca, setSelectedPeca] = useState<PecaInsumo | null>(null);
  const [quantidadeEntrada, setQuantidadeEntrada] = useState("1");
  const [isSavingStock, setIsSavingStock] = useState(false);

  async function loadPecas() {
    setLoading(true);
    try {
      const response = await pecasInsumosService.list();
      setPecas(response.pecasInsumos);
    } catch {
      toast.error("Não foi possível carregar as peças e insumos.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadPecas();
  }, []);

  function openEntradaEstoqueDialog(peca: PecaInsumo) {
    setSelectedPeca(peca);
    setQuantidadeEntrada("1");
    setDialogOpen(true);
  }

  async function handleEntradaEstoque(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedPeca) return;

    setIsSavingStock(true);
    try {
      await pecasInsumosService.entradaEstoque(selectedPeca.id, {
        quantidade: Number(quantidadeEntrada),
      });
      toast.success("Estoque atualizado com sucesso.");
      setDialogOpen(false);
      await loadPecas();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao atualizar estoque."));
    } finally {
      setIsSavingStock(false);
    }
  }

  return (
    <div>
      <PageHeader
        title="Peças e Insumos"
        description="Controle de cadastro e estoque de peças e materiais da oficina."
        actions={
          <Button asChild>
            <Link to="/pecas-insumos/novo">Nova peça/insumo</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={pecas}
          rowKey={(peca) => peca.id}
          emptyMessage="Nenhuma peça cadastrada."
          columns={[
            { key: "nome", title: "Item", render: (peca) => peca.nome },
            { key: "codigo", title: "Código", render: (peca) => peca.codigo },
            { key: "valor", title: "Valor unitário", render: (peca) => formatCurrency(peca.precoUnitario) },
            { key: "estoque", title: "Estoque", render: (peca) => peca.quantidadeEstoque },
            {
              key: "status",
              title: "Status",
              render: (peca) =>
                peca.ativo ? <Badge variant="success">Ativo</Badge> : <Badge variant="secondary">Inativo</Badge>,
            },
            {
              key: "acoes",
              title: "Ações",
              className: "w-[280px]",
              render: (peca) => (
                <div className="flex flex-wrap gap-2">
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/pecas-insumos/${peca.id}`}>Detalhes</Link>
                  </Button>
                  <Button variant="secondary" size="sm" asChild>
                    <Link to={`/pecas-insumos/${peca.id}/editar`}>Editar</Link>
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => openEntradaEstoqueDialog(peca)}>
                    Entrada estoque
                  </Button>
                </div>
              ),
            },
          ]}
        />
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Entrada de estoque</DialogTitle>
            <DialogDescription>
              {selectedPeca ? `Adicionar unidades para ${selectedPeca.nome} (${selectedPeca.codigo})` : ""}
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleEntradaEstoque} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="quantidade-entrada">Quantidade</Label>
              <Input
                id="quantidade-entrada"
                type="number"
                min="1"
                value={quantidadeEntrada}
                onChange={(event) => setQuantidadeEntrada(event.target.value)}
                required
              />
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>
                Cancelar
              </Button>
              <Button type="submit" disabled={isSavingStock}>
                {isSavingStock ? "Salvando..." : "Confirmar"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
