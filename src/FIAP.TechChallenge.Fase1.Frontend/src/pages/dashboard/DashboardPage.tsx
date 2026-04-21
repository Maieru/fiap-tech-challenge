import { useEffect, useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { clientesService } from "@/services/clientes.service";
import { ordensServicoService } from "@/services/ordensServico.service";
import { pecasInsumosService } from "@/services/pecasInsumos.service";
import { servicosService } from "@/services/servicos.service";
import { veiculosService } from "@/services/veiculos.service";
import type { StatusOrdemServico } from "@/types/ordemServico";

interface DashboardStats {
  clientes: number;
  veiculos: number;
  servicos: number;
  pecas: number;
  ordensAbertas: number;
}

const initialStats: DashboardStats = {
  clientes: 0,
  veiculos: 0,
  servicos: 0,
  pecas: 0,
  ordensAbertas: 0,
};

const statusEmAndamento: StatusOrdemServico[] = [1, 2, 3, 4];

export function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats>(initialStats);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadStats() {
      setLoading(true);
      try {
        const [clientes, veiculos, servicos, pecas, ordens] = await Promise.all([
          clientesService.list({ pageSize: 1 }),
          veiculosService.list({ pageSize: 1 }),
          servicosService.list({ pageSize: 1 }),
          pecasInsumosService.list({ pageSize: 1 }),
          ordensServicoService.list({ pageSize: 100 }),
        ]);

        setStats({
          clientes: clientes.totalItems,
          veiculos: veiculos.totalItems,
          servicos: servicos.totalItems,
          pecas: pecas.totalItems,
          ordensAbertas: ordens.ordensServico.filter((ordem) => statusEmAndamento.includes(ordem.status)).length,
        });
      } finally {
        setLoading(false);
      }
    }

    void loadStats();
  }, []);

  const cards = [
    { label: "Clientes", value: stats.clientes },
    { label: "Veículos", value: stats.veiculos },
    { label: "Serviços", value: stats.servicos },
    { label: "Peças/Insumos", value: stats.pecas },
    { label: "OS em aberto", value: stats.ordensAbertas },
  ];

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Bem-vindo ao painel da oficina</CardTitle>
          <CardDescription>
            Visualize rapidamente os principais números do sistema e navegue pelos módulos no menu lateral.
          </CardDescription>
        </CardHeader>
      </Card>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
        {cards.map((card) => (
          <Card key={card.label}>
            <CardHeader className="pb-2">
              <CardDescription>{card.label}</CardDescription>
              <CardTitle className="text-3xl">{loading ? "-" : card.value}</CardTitle>
            </CardHeader>
            <CardContent className="text-xs text-muted-foreground">Atualização automática ao abrir o dashboard.</CardContent>
          </Card>
        ))}
      </section>
    </div>
  );
}
