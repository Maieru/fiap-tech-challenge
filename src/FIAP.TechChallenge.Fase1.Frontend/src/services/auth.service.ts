import { osApi } from "@/services/api";
import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from "@/types/auth";

export const authService = {
  async login(payload: LoginRequest) {
    const { data } = await osApi.post<LoginResponse>("/usuarios/login", payload);
    return data;
  },

  async register(payload: RegisterRequest) {
    const { data } = await osApi.post<RegisterResponse>("/usuarios", payload);
    return data;
  },
};
