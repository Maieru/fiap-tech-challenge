import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export function formatDateTime(date: string | Date | null | undefined) {
  if (!date) return "-";

  const value = typeof date === "string" ? new Date(date) : date;
  if (Number.isNaN(value.getTime())) return "-";

  return value.toLocaleString("pt-BR");
}
