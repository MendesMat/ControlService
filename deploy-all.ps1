# deploy-all.ps1
# Script de automacao para deploy do ControlService e Observabilidade
# Monorepo - Estrutura descentralizada:
#   infra-shared/k8s/ -> Redis, Prometheus, Grafana (infraestrutura compartilhada)
#   backend/k8s/      -> ControlService API

Write-Host "Iniciando Deployment do ControlService no Kubernetes..." -ForegroundColor Cyan

# 1. Infraestrutura Compartilhada (infra-shared/k8s/)
Write-Host "`n[1/2] Aplicando Infraestrutura Compartilhada (Redis + Prometheus + Grafana)..." -ForegroundColor Yellow
kubectl apply -f infra-shared/k8s/01-redis-deployment.yaml
kubectl apply -f infra-shared/k8s/02-redis-service.yaml
kubectl apply -f infra-shared/k8s/05-prometheus-pvc.yaml
kubectl apply -f infra-shared/k8s/06-prometheus-configmap.yaml
kubectl apply -f infra-shared/k8s/07-prometheus-deployment.yaml
kubectl apply -f infra-shared/k8s/08-prometheus-service.yaml
kubectl apply -f infra-shared/k8s/09-grafana-deployment.yaml
kubectl apply -f infra-shared/k8s/10-grafana-service.yaml
kubectl apply -f infra-shared/k8s/11-grafana-datasource-config.yaml

# 2. Backend - ControlService API (backend/k8s/)
Write-Host "`n[2/2] Aplicando Backend - ControlService API..." -ForegroundColor Yellow
kubectl apply -f backend/k8s/03-app-deployment.yaml
kubectl apply -f backend/k8s/04-app-service.yaml

# 3. Aguardar Pods
Write-Host "`nAguardando Pods ficarem prontos (Running)..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l project=controlservice --timeout=120s

Write-Host "`nAmbiente Deployment concluido com sucesso!" -ForegroundColor Green

# 4. Instrucoes de Proximos Passos
Write-Host "`nProximos Passos:" -ForegroundColor Cyan
Write-Host "1. Abra o Port-Forward do Grafana em um novo terminal:"
Write-Host "   kubectl port-forward service/grafana-service 30030:3000"
Write-Host "2. Importe o Dashboard (arquivo movido para infra-shared/k8s/):"
Write-Host "   Dashboard JSON: .\infra-shared\k8s\grafana-dashboard.json"
Write-Host "3. Acesse: http://localhost:30030 (admin / controlservice2024)"
