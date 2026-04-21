import { api } from "@/services/api";
import type { LoginRequest, LoginResponse } from "@/types/auth";

export const authService = {
  async login(payload: LoginRequest) {
    const { data } = await api.post<LoginResponse>("/usuarios/login", payload);
    return data;
  },
};
