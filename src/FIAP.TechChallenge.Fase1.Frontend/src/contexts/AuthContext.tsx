import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from "react";
import { authService } from "@/services/auth.service";
import { setUnauthorizedHandler } from "@/services/api";
import {
  clearAuthSession,
  getAuthSession,
  isTokenExpired,
  saveAuthSession,
} from "@/services/storage";
import type { AuthSession, LoginRequest } from "@/types/auth";

interface AuthContextValue {
  session: AuthSession | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedSession = getAuthSession();
    if (storedSession && !isTokenExpired(storedSession.expiresAt)) {
      setSession(storedSession);
    } else {
      clearAuthSession();
    }

    setIsLoading(false);
  }, []);

  const logout = useCallback(() => {
    clearAuthSession();
    setSession(null);
  }, []);

  useEffect(() => {
    setUnauthorizedHandler(logout);
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  const login = useCallback(async (payload: LoginRequest) => {
    const response = await authService.login(payload);
    const expiresAt = new Date(Date.now() + response.expiresInSeconds * 1000).toISOString();

    const nextSession: AuthSession = {
      token: response.token,
      tokenType: response.tipoToken,
      expiresAt,
      username: payload.usuario,
    };

    saveAuthSession(nextSession);
    setSession(nextSession);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: Boolean(session?.token),
      isLoading,
      login,
      logout,
    }),
    [isLoading, login, logout, session],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
