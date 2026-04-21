import { Badge } from "@/components/ui/badge";
import { STATUS_ORDEM_LABELS, type StatusOrdemServico } from "@/types/ordemServico";

interface StatusBadgeProps {
  status: StatusOrdemServico;
}

const variantByStatus: Record<StatusOrdemServico, "secondary" | "info" | "warning" | "default" | "success"> = {
  1: "secondary",
  2: "info",
  3: "warning",
  4: "default",
  5: "success",
  6: "success",
};

export function StatusBadge({ status }: StatusBadgeProps) {
  return <Badge variant={variantByStatus[status]}>{STATUS_ORDEM_LABELS[status]}</Badge>;
}
