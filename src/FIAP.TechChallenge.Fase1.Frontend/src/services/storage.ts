import type { AuthSession } from "@/types/auth";

const AUTH_STORAGE_KEY = "oficina.auth.session";

export function saveAuthSession(session: AuthSession) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
}

export function getAuthSession() {
  const rawValue = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!rawValue) return null;

  try {
    return JSON.parse(rawValue) as AuthSession;
  } catch {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function clearAuthSession() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}

export function isTokenExpired(expiresAt: string) {
  return new Date(expiresAt).getTime() <= Date.now();
}
