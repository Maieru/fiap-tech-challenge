import { api } from "@/services/api";
import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from "@/types/auth";

export const authService = {
  async login(payload: LoginRequest) {
    const { data } = await api.post<LoginResponse>("/usuarios/login", payload);
    return data;
  },

  async register(payload: RegisterRequest) {
    const { data } = await api.post<RegisterResponse>("/usuarios", payload);
    return data;
  },
};
