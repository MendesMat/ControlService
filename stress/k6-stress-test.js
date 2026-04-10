// Teste de Estresse - k6
import http from "k6/http";
import { check, group, sleep } from "k6";
import { Counter, Rate, Trend } from "k6/metrics";

// Configuração do ambiente
const BASE_URL = __ENV.BASE_URL || "http://192.168.49.2:30080";
const API = `${BASE_URL}/api`;

// Métricas customizadas
const customerCreatedCount = new Counter("customers_created_total");
const customerCreationErrors = new Rate("customer_creation_error_rate");
const getLatency = new Trend("get_customers_duration", true);
const createLatency = new Trend("create_customer_duration", true);

// Opções do teste
export const options = {
  thresholds: {
    // P95 da latência HTTP deve ser abaixo de 1s durante o teste completo
    http_req_duration: ["p(95)<1000"],
    // Taxa de erros HTTP deve ficar abaixo de 5%
    http_req_failed: ["rate<0.05"],
    customer_creation_error_rate: ["rate<0.05"],
  },

  stages: [
    // Ramp-Up
    { duration: "1m", target: 50 },

    // Carga Sustentada
    { duration: "3m", target: 50 },

    // Spike
    { duration: "30s", target: 500 },

    // Pico Máximo
    { duration: "3m", target: 500 },

    // Ramp-Down
    { duration: "1m", target: 0 },
  ],
};

// Dados para os testes (evita conflitos de CPF/Telefone)
function generateCustomerPayload(vu, iteration) {
  const vuPadded = String(vu).padStart(4, "0");
  const iterPadded = String(iteration).padStart(6, "0");

  return {
    name: `Stress Test VU${vuPadded} IT${iterPadded}`,
    // E-mail único: garantido único por VU+iteração
    email: `stress.vu${vuPadded}.it${iterPadded}@k6.test`,
    // Documento CPF fictício único baseado em VU+iteração (11 dígitos)
    document: generateFakeCpf(vu, iteration),
    // Telefone fictício único (11 dígitos, formato celular brasileiro)
    phone: generateFakePhone(vu, iteration),
  };
}

function generateFakeCpf(vu, iteration) {
  // Gera um número de 11 dígitos baseado em vu+iteration para unicidade.
  // Não passa na validação de CPF real — apenas para simular carga estrutural.
  const base = (vu * 1000 + (iteration % 1000)) % 100000;
  return String(base).padStart(5, "0") + String(iteration % 1000000).padStart(6, "0");
}

function generateFakePhone(vu, iteration) {
  const areaCode = 10 + (vu % 90);
  const number = 900000000 + ((vu * 1000 + iteration) % 99999999);
  return `${areaCode}${number}`;
}

// Fluxo principal do teste
export default function () {
  // IDs de customers criados nesta iteração (para reutilizar em GET/PUT/DELETE)
  const createdIds = [];

  group("Health Check", function () {
    const res = http.get(`${BASE_URL}/healthz`, {
      tags: { endpoint: "healthz" },
    });
    check(res, {
      "healthz retorna 200": (r) => r.status === 200,
      "healthz body contém status healthy": (r) =>
        r.json("status") === "healthy",
    });
  });

  sleep(0.1);

  group("POST /api/customers", function () {
    const payload = JSON.stringify(
      generateCustomerPayload(__VU, __ITER)
    );

    const params = {
      headers: { "Content-Type": "application/json" },
      tags: { endpoint: "create_customer" },
    };

    const res = http.post(`${API}/customers`, payload, params);
    createLatency.add(res.timings.duration);

    const created = check(res, {
      "POST /api/customers retorna 201": (r) => r.status === 201,
      "response contém id": (r) => r.json("id") !== undefined,
    });

    customerCreationErrors.add(!created);

    if (created && res.status === 201) {
      customerCreatedCount.add(1);
      const body = res.json();
      if (body && body.id) {
        createdIds.push(body.id);
      }
    }
  });

  sleep(0.2);

  group("GET /api/customers", function () {
    const res = http.get(`${API}/customers`, {
      tags: { endpoint: "list_customers" },
    });
    getLatency.add(res.timings.duration);

    check(res, {
      "GET /api/customers retorna 200": (r) => r.status === 200,
      "response é um array": (r) => Array.isArray(r.json()),
    });
  });

  sleep(0.1);

  group("GET /api/customers/{id}", function () {
    const id = createdIds[0];
    const res = http.get(`${API}/customers/${id}`, {
      tags: { endpoint: "get_customer_by_id" },
    });

    check(res, {
      "GET por ID retorna 200": (r) => r.status === 200,
      "response contém o id correto": (r) => r.json("id") === id,
    });
  });

  sleep(0.1);

  group("PUT /api/customers/{id}", function () {
    const id = createdIds[0];
    const updatedPayload = JSON.stringify({
      id: id,
      name: `Updated VU${__VU} IT${__ITER}`,
      email: `updated.vu${__VU}.it${__ITER}@k6.test`,
      document: generateFakeCpf(__VU, __ITER + 500000),
      phone: generateFakePhone(__VU, __ITER + 500000),
    });

    const res = http.put(`${API}/customers/${id}`, updatedPayload, {
      headers: { "Content-Type": "application/json" },
      tags: { endpoint: "update_customer" },
    });

    check(res, {
      "PUT retorna 200": (r) => r.status === 200,
    });
  });

  sleep(0.1);

  group("DELETE /api/customers/{id}", function () {
    const id = createdIds[0];
    const res = http.del(`${API}/customers/${id}`, null, {
      tags: { endpoint: "delete_customer" },
    });

    check(res, {
      "DELETE retorna 204": (r) => r.status === 204,
    });
  });

  // Pausa entre iterações: simula "think time" do usuário real.
// Reduzir para 0 em testes de carga máxima.
sleep(0.5);
}
