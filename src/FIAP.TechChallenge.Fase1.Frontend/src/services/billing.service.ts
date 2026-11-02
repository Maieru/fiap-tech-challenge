import axios from "axios";
import { billingApi } from "./api";

export interface Cobranca {
  id: string;
  ordemServicoId: string;
  valor: number;
  moeda: string;
  situacao: 0 | 1 | 2;
  motivo: string | null;
}

export const billingService = {
  async getByOrdem(id: string) {
    try {
      return (await billingApi.get<Cobranca>(`/ordens/${id}/cobranca`)).data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) return null;
      throw error;
    }
  },
};
