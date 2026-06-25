import axios from "axios";

/**
 * Instância Axios base do ControlService ERP.
 *
 * Interceptores configurados:
 *  - Request: injeta Authorization (JWT) e X-CNPJ-Id (multitenancy)
 *  - Response: trata erros globais (401 → logout, 500 → notificação)
 */
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5000",
  timeout: 15_000,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json",
  },
});

// ── Request Interceptor ───────────────────────────────────────────────────────
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("access_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  // Header de multitenancy: CNPJ ativo selecionado pelo usuário
  const cnpjId = localStorage.getItem("active_cnpj_id");
  if (cnpjId) {
    config.headers["X-CNPJ-Id"] = cnpjId;
  }

  return config;
});

// ── Response Interceptor ──────────────────────────────────────────────────────
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // TODO: redirecionar para login / limpar sessão
      localStorage.removeItem("access_token");
    }
    return Promise.reject(error);
  }
);

export default api;
