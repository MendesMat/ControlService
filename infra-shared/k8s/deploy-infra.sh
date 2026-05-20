#!/usr/bin/env bash
# =============================================================================
# deploy.sh — Script de implantação no Kubernetes
# Aplica os manifestos na ordem correta e monitora o rollout.
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DRY_RUN="${1:-}"

echo "============================================================"
echo "  ControlService — Deploy Kubernetes"
echo "============================================================"
echo "  Contexto ativo : $(kubectl config current-context 2>/dev/null || echo 'N/A')"
echo "  Namespace       : default"
echo "  Manifests       : ${SCRIPT_DIR}"
echo "============================================================"

# ── Modo dry-run (validação) ───────────────────────────────────────────────────
if [ "${DRY_RUN}" = "--dry-run" ]; then
    echo ""
    echo "[DRY-RUN] Validando manifestos (server-side)..."
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/01-redis-deployment.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/02-redis-service.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/03-app-deployment.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/04-app-service.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/05-prometheus-pvc.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/06-prometheus-configmap.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/07-prometheus-deployment.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/08-prometheus-service.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/09-grafana-deployment.yaml"
    kubectl apply --dry-run=server -f "${SCRIPT_DIR}/10-grafana-service.yaml"
    echo ""
    echo "[DRY-RUN] Validação concluída. Nenhum recurso foi criado."
    exit 0
fi

# ── Redis ──────────────────────────────────────────────────────────────────────
echo ""
echo "[1/4] Aplicando Redis Deployment (ClusterIP — acesso interno apenas)..."
kubectl apply -f "${SCRIPT_DIR}/01-redis-deployment.yaml"

echo "[2/4] Aplicando Redis Service (ClusterIP)..."
kubectl apply -f "${SCRIPT_DIR}/02-redis-service.yaml"

echo "      Aguardando Redis ficar disponível..."
kubectl rollout status deployment/redis-deployment --timeout=60s

# ── Aplicação ─────────────────────────────────────────────────────────────────
echo ""
echo "[3/6] Aplicando ControlService Deployment (4 réplicas, RollingUpdate)..."
kubectl apply -f "${SCRIPT_DIR}/03-app-deployment.yaml"

echo "[4/6] Aplicando ControlService Service (NodePort :30080)..."
kubectl apply -f "${SCRIPT_DIR}/04-app-service.yaml"

# ── Observabilidade (Prometheus + Grafana) ─────────────────────────────────────
echo ""
echo "[5/6] Aplicando stack de Observabilidade (Prometheus + Grafana)..."

echo "      [Prometheus] PersistentVolumeClaim (5Gi para TSDB)..."
kubectl apply -f "${SCRIPT_DIR}/05-prometheus-pvc.yaml"

echo "      [Prometheus] ConfigMap (prometheus.yml — targets de scraping)..."
kubectl apply -f "${SCRIPT_DIR}/06-prometheus-configmap.yaml"

echo "      [Prometheus] Deployment (Pull model, retenção 15d)..."
kubectl apply -f "${SCRIPT_DIR}/07-prometheus-deployment.yaml"

echo "      [Prometheus] Service (ClusterIP :9090 — interno)..."
kubectl apply -f "${SCRIPT_DIR}/08-prometheus-service.yaml"

# ── Grafana ───────────────────────────────────────────────────────────────────
echo "      [Grafana] Deployment..."
kubectl apply -f "${SCRIPT_DIR}/09-grafana-deployment.yaml"

echo "      [Grafana] Service (NodePort :30030 — externo)..."
kubectl apply -f "${SCRIPT_DIR}/10-grafana-service.yaml"

echo ""
echo "[6/6] Aguardando Prometheus e Grafana ficarem disponíveis..."
kubectl rollout status deployment/prometheus-deployment --timeout=120s
kubectl rollout status deployment/grafana-deployment --timeout=120s

# ── Monitoramento do Rollout ──────────────────────────────────────────────────
echo ""
echo "Monitorando rollout do ControlService (timeout: 120s)..."
kubectl rollout status deployment/controlservice-deployment --timeout=120s

# ── Resumo do estado do cluster ────────────────────────────────────────────────
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
echo "  Acesso externo à aplicação:"
echo ""

if command -v minikube &> /dev/null && minikube status &> /dev/null 2>&1; then
    MINIKUBE_IP=$(minikube ip 2>/dev/null)
    echo "    Minikube detectado! IP: ${MINIKUBE_IP}"
    echo ""
    echo "    ── Aplicação ──────────────────────────────────────"
    echo "    URL da aplicação : http://${MINIKUBE_IP}:30080"
    echo "    Métricas (/metrics): http://${MINIKUBE_IP}:30080/metrics"
    echo "    Health Check     : http://${MINIKUBE_IP}:30080/healthz"
    echo ""
    echo "    ── Observabilidade ────────────────────────────────"
    echo "    Grafana (dashboard) : http://${MINIKUBE_IP}:30030"
    echo "    Grafana login       : admin / controlservice2024"
    echo "    Prometheus datasource (interno): http://prometheus-service:9090"
else
    NODE_IP=$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[?(@.type=="InternalIP")].address}' 2>/dev/null || echo "<IP-DO-NÓ>")
    echo ""
    echo "    ── Aplicação ──────────────────────────────────────"
    echo "    URL da aplicação : http://${NODE_IP}:30080"
    echo "    Métricas (/metrics): http://${NODE_IP}:30080/metrics"
    echo "    Health Check     : http://${NODE_IP}:30080/healthz"
    echo ""
    echo "    ── Observabilidade ────────────────────────────────"
    echo "    Grafana (dashboard) : http://${NODE_IP}:30030"
    echo "    Grafana login       : admin / controlservice2024"
    echo "    Prometheus datasource (interno): http://prometheus-service:9090"
fi
echo ""
echo "  Dashboard JSON para importar no Grafana:"
echo "    ${SCRIPT_DIR}/grafana-dashboard.json"
echo "============================================================"
