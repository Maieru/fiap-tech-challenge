export interface LoginRequest {
  usuario: string;
  senha: string;
}

export interface RegisterRequest {
  usuario: string;
  senha: string;
}

export interface RegisterResponse {
  id: string;
  usuario: string;
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
