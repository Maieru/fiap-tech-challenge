import { useEffect, useState, type FormEvent } from "react";
import { Trash2 } from "lucide-react";
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
  const [deletingId, setDeletingId] = useState<string | null>(null);

  async function loadPecas() {
    setLoading(true);
    try {
      const response = await pecasInsumosService.list();
      setPecas(response.pecasInsumos);
    } catch {
      toast.error("Nao foi possivel carregar as pecas e insumos.");
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

  async function handleDelete(peca: PecaInsumo) {
    const confirmed = window.confirm(`Excluir ${peca.nome}?`);
    if (!confirmed) return;

    setDeletingId(peca.id);
    try {
      await pecasInsumosService.remove(peca.id);
      toast.success("Peca/insumo excluido com sucesso.");
      await loadPecas();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao excluir peca/insumo."));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Pecas e Insumos"
        description="Controle de cadastro e estoque de pecas e materiais da oficina."
        actions={
          <Button asChild>
            <Link to="/pecas-insumos/novo">Nova peca/insumo</Link>
          </Button>
        }
      />

      {loading ? (
        <LoadingState />
      ) : (
        <EntityTable
          data={pecas}
          rowKey={(peca) => peca.id}
          emptyMessage="Nenhuma peca cadastrada."
          columns={[
            { key: "nome", title: "Item", render: (peca) => peca.nome },
            { key: "codigo", title: "Codigo", render: (peca) => peca.codigo },
            { key: "valor", title: "Valor unitario", render: (peca) => formatCurrency(peca.precoUnitario) },
            { key: "estoque", title: "Estoque", render: (peca) => peca.quantidadeEstoque },
            {
              key: "status",
              title: "Status",
              render: (peca) =>
                peca.ativo ? <Badge variant="success">Ativo</Badge> : <Badge variant="secondary">Inativo</Badge>,
            },
            {
              key: "acoes",
              title: "Acoes",
              className: "sticky right-0 z-10 w-[360px] bg-card",
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
                  <Button variant="destructive" size="sm" onClick={() => handleDelete(peca)} disabled={deletingId === peca.id}>
                    <Trash2 className="h-3.5 w-3.5" />
                    Excluir
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
