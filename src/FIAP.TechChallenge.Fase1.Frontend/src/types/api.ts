export interface PagedResponse {
  pageNumber: number;
  pageSize: number;
  totalItems: number;
}

export interface ApiValidationError {
  [key: string]: string[];
}

export interface ApiErrorResponse {
  error?: string;
  errorCode?: string;
  title?: string;
  detail?: string;
  message?: string;
  errors?: ApiValidationError;
}
