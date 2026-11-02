import axios, { AxiosError } from "axios";
import { clearAuthSession, getAuthSession, isTokenExpired } from "@/services/storage";
import type { ApiErrorResponse } from "@/types/api";
import { resolveApiEndpoints } from "./endpoints";

type UnauthorizedHandler = () => void;

let unauthorizedHandler: UnauthorizedHandler | null = null;

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null) {
  unauthorizedHandler = handler;
}

const endpoints = resolveApiEndpoints(import.meta.env);

function createAdminApi(baseURL: string) {
  const client = axios.create({ baseURL, timeout: 20000 });

  client.interceptors.request.use((config) => {
    const session = getAuthSession();
    if (!session) return config;

    if (isTokenExpired(session.expiresAt)) {
      clearAuthSession();
      unauthorizedHandler?.();
      return Promise.reject(new axios.CanceledError("Sessão expirada. Entre novamente."));
    }

    config.headers.Authorization = `Bearer ${session.token}`;
    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    (error: AxiosError<ApiErrorResponse>) => {
      if (error.response?.status === 401) {
        clearAuthSession();
        unauthorizedHandler?.();
      }

      return Promise.reject(error);
    },
  );
  return client;
}

export const osApi = createAdminApi(endpoints.os);
export const executionApi = createAdminApi(endpoints.execution);
export const billingApi = createAdminApi(endpoints.billing);
// O token privado não autentica um administrador. Um 401 aqui não encerra o login.
export const publicOsApi = axios.create({ baseURL: endpoints.os, timeout: 20000 });

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
