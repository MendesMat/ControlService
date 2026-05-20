#!/usr/bin/env bash
# =============================================================================
# run-stress-test.sh — Fase 5.1 + 5.2: Execução do Estresse e Coleta de Evidências
#
# Orquestra o teste de carga k6, abre os painéis do Grafana para captura
# de evidências visuais e valida os resultados ao final.
#
# Uso:
#   ./stress/run-stress-test.sh                          # detecta URL automaticamente
#   ./stress/run-stress-test.sh http://192.168.49.2:30080  # URL explícita
#
# Pré-requisitos:
#   - k6 instalado: https://k6.io/docs/getting-started/installation/
#     Windows: winget install k6 --source winget
#     Linux:   sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg \
#                --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69 && \
#              echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" \
#                | sudo tee /etc/apt/sources.list.d/k6.list && sudo apt-get update && sudo apt-get install k6
#   - kubectl configurado e cluster ativo
#   - Deploy das Fases 2 e 3 realizado (./k8s/deploy.sh)
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
RESULTS_DIR="${SCRIPT_DIR}/results"
K6_SCRIPT="${SCRIPT_DIR}/k6-stress-test.js"
TIMESTAMP=$(date +"%Y-%m-%d_%H-%M-%S")

# ── Detectar URL da aplicação automaticamente ──────────────────────────────────
if [ -n "${1:-}" ]; then
    BASE_URL="$1"
else
    if command -v minikube &> /dev/null && minikube status &> /dev/null 2>&1; then
        NODE_IP=$(minikube ip 2>/dev/null)
    else
        NODE_IP=$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[?(@.type=="InternalIP")].address}' 2>/dev/null || echo "")
    fi

    if [ -z "${NODE_IP}" ]; then
        echo "[ERRO] Não foi possível detectar o IP do nó K8s."
        echo "       Passe a URL explicitamente: ./stress/run-stress-test.sh http://<IP>:30080"
        exit 1
    fi
    BASE_URL="http://${NODE_IP}:30080"
fi

GRAFANA_URL="${BASE_URL%:30080}:30030"

# ── Preparação ─────────────────────────────────────────────────────────────────
mkdir -p "${RESULTS_DIR}"

echo "═══════════════════════════════════════════════════════"
echo "  ControlService — Fase 5: Estresse em Produção"
echo "═══════════════════════════════════════════════════════"
echo "  Aplicação (NodePort) : ${BASE_URL}"
echo "  Grafana  (NodePort)  : ${GRAFANA_URL}"
echo "  Health Check         : ${BASE_URL}/healthz"
echo "  Métricas (Prometheus): ${BASE_URL}/metrics"
echo "  Resultados k6        : ${RESULTS_DIR}/"
echo "  Timestamp            : ${TIMESTAMP}"
echo "═══════════════════════════════════════════════════════"

# ── Verifica pré-requisitos ────────────────────────────────────────────────────
echo ""
echo "[PRÉ-REQUISITO] Verificando k6..."
if ! command -v k6 &> /dev/null; then
    echo ""
    echo "[ERRO] k6 não encontrado. Instale antes de prosseguir:"
    echo ""
    echo "  Windows : winget install k6 --source winget"
    echo "  Linux   : sudo apt-get install k6  (após adicionar o repositório oficial)"
    echo "  macOS   : brew install k6"
    echo "  Docs    : https://k6.io/docs/get-started/installation/"
    echo ""
    exit 1
fi
echo "  k6 $(k6 version | head -1) — OK"

# ── Verifica se a aplicação está respondendo ───────────────────────────────────
echo ""
echo "[PRÉ-REQUISITO] Verificando conectividade com a aplicação..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 5 "${BASE_URL}/healthz" 2>/dev/null || echo "000")
if [ "${HTTP_STATUS}" != "200" ]; then
    echo ""
    echo "[ERRO] A aplicação não está respondendo em ${BASE_URL}/healthz (HTTP ${HTTP_STATUS})"
    echo "       Verifique se o deploy K8s foi realizado: ./k8s/deploy.sh"
    echo "       Verifique se o NodePort 30080 está acessível."
    exit 1
fi
echo "  Aplicação respondendo (HTTP ${HTTP_STATUS}) — OK"

# ── Instruções para captura de evidências no Grafana ──────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════"
echo "  FASE 5.2: INSTRUÇÕES DE CAPTURA (antes de iniciar)"
echo "═══════════════════════════════════════════════════════"
echo ""
echo "  1. Abra o Grafana em: ${GRAFANA_URL}"
echo "     Login: admin / controlservice2024"
echo ""
echo "  2. Importe o dashboard (se não importou ainda):"
echo "     Dashboards → Import → Upload JSON"
echo "     Arquivo: ${REPO_ROOT}/k8s/grafana-dashboard.json"
echo ""
echo "  3. Configure o datasource Prometheus:"
echo "     Connections → Data Sources → Add → Prometheus"
echo "     URL: http://prometheus-service:9090"
echo ""
echo "  4. Abra o dashboard: 'ControlService — Observabilidade'"
echo "     Defina o período: Last 30 minutes (atualiza a cada 30s)"
echo ""
echo "  5. DURANTE o teste (Stage 4 — pico de 500 VUs por 3 min),"
echo "     capture screenshots dos seguintes painéis para evidência:"
echo ""
echo "     📸 Painel A: 'Consumo de Memória RAM por Réplica'"
echo "        Mostrará: process_working_set_bytes > 300MB sob pressão"
echo ""
echo "     📸 Painel B: 'CPU Total das 4 Réplicas (Gauge)'"
echo "        Mostrará: rate(process_cpu_seconds_total) próximo ao limit de 500m"
echo ""
echo "     📸 Painel C: 'R — Taxa de Requisições por Status HTTP'"
echo "        Mostrará: pico de req/s com possível aparecimento de erros 5xx"
echo ""
echo "     📸 Painel D: 'D — Latência HTTP (P50 · P95 · P99)'"
echo "        Mostrará: P99 subindo durante o stage de 500 VUs"
echo ""
echo "═══════════════════════════════════════════════════════"
echo ""

# ── Modo Autônomo / Continuous Delivery (Headless) ────────────────────────────
echo "  [Headless Mode] Iniciando automaticamente (sem pausa interativa)."
sleep 3

echo ""
echo "═══════════════════════════════════════════════════════"
echo "  INICIANDO TESTE DE ESTRESSE k6"
echo "  Duração total estimada: ~8 min 30s"
echo "  Stages:"
echo "    [0:00]  Stage 1: Ramp-Up  0 → 50  VUs (1 min)"
echo "    [1:00]  Stage 2: Carga Sustentada 50  VUs (3 min)"
echo "    [4:00]  Stage 3: Spike   50 → 500 VUs (30s)"
echo "    [4:30]  Stage 4: Pico  500     VUs (3 min) ← CAPTURE AQUI"
echo "    [7:30]  Stage 5: Ramp-Down 500 → 0 VUs (1 min)"
echo "═══════════════════════════════════════════════════════"
echo ""

# ── Execução do k6 ────────────────────────────────────────────────────────────
K6_OUTPUT_FILE="${RESULTS_DIR}/k6-output-${TIMESTAMP}.log"
K6_SUMMARY_FILE="${RESULTS_DIR}/k6-summary-${TIMESTAMP}.json"

# Executa o k6 salvando output completo em log e summary JSON
k6 run \
    --env BASE_URL="${BASE_URL}" \
    --summary-export="${K6_SUMMARY_FILE}" \
    --out "json=${RESULTS_DIR}/k6-raw-${TIMESTAMP}.json" \
    "${K6_SCRIPT}" \
    2>&1 | tee "${K6_OUTPUT_FILE}"

K6_EXIT_CODE=${PIPESTATUS[0]}

# ── Resultado Final ────────────────────────────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════"
echo "  FASE 5: RESULTADOS"
echo "═══════════════════════════════════════════════════════"
echo ""

if [ "${K6_EXIT_CODE}" -eq 0 ]; then
    echo "  ✅ Thresholds: TODOS APROVADOS"
    echo "     P95 < 1000ms e taxa de erros < 5%"
else
    echo "  ⚠️  Thresholds: VIOLADOS (código de saída: ${K6_EXIT_CODE})"
    echo "     Verifique o log: ${K6_OUTPUT_FILE}"
fi

echo ""
echo "  Arquivos de evidência gerados:"
echo "    Log completo  : ${K6_OUTPUT_FILE}"
echo "    Summary JSON  : ${K6_SUMMARY_FILE}"
echo "    Raw metrics   : ${RESULTS_DIR}/k6-raw-${TIMESTAMP}.json"
echo ""
echo "  Evidência visual (Grafana):"
echo "    Acesse ${GRAFANA_URL} e exporte os painéis como PNG/PDF."
echo "    Use Grafana → Share → Export → Save as image."
echo "═══════════════════════════════════════════════════════"

exit "${K6_EXIT_CODE}"
