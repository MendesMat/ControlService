/**
 * DTOs genéricos e compartilhados entre todos os módulos.
 * Não dependem de nenhum framework (React, Axios, etc.) — são estruturas puras.
 */

// ── Paginação ─────────────────────────────────────────────────────────────────
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
}

// ── Erros da API ──────────────────────────────────────────────────────────────
export interface ApiError {
  statusCode: number;
  message: string;
  errors?: Record<string, string[]>;
}

// ── Auditoria ─────────────────────────────────────────────────────────────────
export interface AuditFields {
  createdAt: string; // ISO 8601
  updatedAt: string;
  createdBy?: string;
}

// ── Seleção (para dropdowns) ──────────────────────────────────────────────────
export interface SelectOption {
  value: string | number;
  label: string;
}
