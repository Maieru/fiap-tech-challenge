import axios, { AxiosError } from "axios";
import { clearAuthSession, getAuthSession, isTokenExpired } from "@/services/storage";
import type { ApiErrorResponse } from "@/types/api";

type UnauthorizedHandler = () => void;

let unauthorizedHandler: UnauthorizedHandler | null = null;

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null) {
  unauthorizedHandler = handler;
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "/api",
  timeout: 20000,
});

api.interceptors.request.use((config) => {
  const session = getAuthSession();
  if (!session) return config;

  if (isTokenExpired(session.expiresAt)) {
    clearAuthSession();
    unauthorizedHandler?.();
    return config;
  }

  config.headers.Authorization = `Bearer ${session.token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response?.status === 401) {
      clearAuthSession();
      unauthorizedHandler?.();
    }

    return Promise.reject(error);
  },
);

export function getApiErrorMessage(error: unknown, fallback = "Erro inesperado.") {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    const data = error.response?.data;
    if (data?.error) return data.error;
    if (data?.detail) return data.detail;
    if (data?.message) return data.message;
    if (data?.title) return data.title;

    const errorBag = data?.errors;
    if (errorBag) {
      const allMessages = Object.values(errorBag).flat().filter(Boolean);
      if (allMessages.length > 0) return allMessages[0];
    }
  }

  return fallback;
}
