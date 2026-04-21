export interface LoginRequest {
  usuario: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  tipoToken: string;
  expiresInSeconds: number;
}

export interface AuthSession {
  token: string;
  tokenType: string;
  expiresAt: string;
  username: string;
}
