#!/usr/bin/env bash
# =============================================================================
# deploy.sh — Script de implantação GLOBAL do ControlService no Kubernetes
#
# Orquestra a aplicação de manifestos de DOIS componentes em ordem correta:
#   1. infra-shared/k8s/ — Redis, Prometheus, Grafana (infraestrutura compartilhada)
#   2. backend/k8s/      — API ControlService (Deployment + Service)
#
# Uso:
#   ./deploy.sh              # Deploy completo
#   ./deploy.sh --dry-run    # Valida manifestos sem aplicar (server-side)
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRA_DIR="${SCRIPT_DIR}/infra-shared/k8s"
BACKEND_DIR="${SCRIPT_DIR}/backend/k8s"
DRY_RUN="${1:-}"

echo "============================================================"
echo "  ControlService — Deploy Kubernetes (Monorepo)"
echo "============================================================"
echo "  Contexto ativo : $(kubectl config current-context 2>/dev/null || echo 'N/A')"
echo "  Namespace       : default"
echo "  Infra Shared    : ${INFRA_DIR}"
echo "  Backend K8s     : ${BACKEND_DIR}"
echo "============================================================"

# ── Modo dry-run (validação) ───────────────────────────────────────────────────
if [ "${DRY_RUN}" = "--dry-run" ]; then
    echo ""
    echo "[DRY-RUN] Validando todos os manifestos (server-side)..."
    kubectl apply --dry-run=server -f "${INFRA_DIR}/"
    kubectl apply --dry-run=server -f "${BACKEND_DIR}/"
    echo ""
    echo "[DRY-RUN] Validação concluída. Nenhum recurso foi criado."
    exit 0
fi

# ── 1. Infraestrutura Compartilhada ───────────────────────────────────────────
echo ""
echo "[1/2] Aplicando Infraestrutura Compartilhada (Redis + Prometheus + Grafana)..."
kubectl apply -f "${INFRA_DIR}/01-redis-deployment.yaml"
kubectl apply -f "${INFRA_DIR}/02-redis-service.yaml"
echo "      Aguardando Redis ficar disponível..."
kubectl rollout status deployment/redis-deployment --timeout=60s

kubectl apply -f "${INFRA_DIR}/05-prometheus-pvc.yaml"
kubectl apply -f "${INFRA_DIR}/06-prometheus-configmap.yaml"
kubectl apply -f "${INFRA_DIR}/07-prometheus-deployment.yaml"
kubectl apply -f "${INFRA_DIR}/08-prometheus-service.yaml"
kubectl apply -f "${INFRA_DIR}/09-grafana-deployment.yaml"
kubectl apply -f "${INFRA_DIR}/10-grafana-service.yaml"
kubectl apply -f "${INFRA_DIR}/11-grafana-datasource-config.yaml"

echo "      Aguardando Prometheus e Grafana ficarem disponíveis..."
kubectl rollout status deployment/prometheus-deployment --timeout=120s
kubectl rollout status deployment/grafana-deployment --timeout=120s

# ── 2. Backend — ControlService API ──────────────────────────────────────────
echo ""
echo "[2/2] Aplicando Backend — ControlService API (4 réplicas, RollingUpdate)..."
kubectl apply -f "${BACKEND_DIR}/03-app-deployment.yaml"
kubectl apply -f "${BACKEND_DIR}/04-app-service.yaml"
echo "      Monitorando rollout da API (timeout: 120s)..."
kubectl rollout status deployment/controlservice-deployment --timeout=120s

# ── Resumo do estado do cluster ───────────────────────────────────────────────
echo ""
echo "============================================================"
echo "  Estado do cluster após o deploy:"
echo "============================================================"
echo ""
echo "  Deployments:"
kubectl get deployments -l project=controlservice
echo ""
echo "  Pods:"
kubectl get pods -l project=controlservice -o wide
echo ""
echo "  Services:"
kubectl get services -l project=controlservice
echo ""
echo "============================================================"
echo "  Acesso externo:"
echo ""

if command -v minikube &> /dev/null && minikube status &> /dev/null 2>&1; then
    MINIKUBE_IP=$(minikube ip 2>/dev/null)
    echo "    Aplicação : http://${MINIKUBE_IP}:30080"
    echo "    Grafana   : http://${MINIKUBE_IP}:30030  (admin / controlservice2024)"
    echo "    Dashboard : ${INFRA_DIR}/grafana-dashboard.json"
else
    NODE_IP=$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[?(@.type=="InternalIP")].address}' 2>/dev/null || echo "<IP-DO-NÓ>")
    echo "    Aplicação : http://${NODE_IP}:30080"
    echo "    Grafana   : http://${NODE_IP}:30030  (admin / controlservice2024)"
    echo "    Dashboard : ${INFRA_DIR}/grafana-dashboard.json"
fi
echo "============================================================"
