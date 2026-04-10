# deploy-all.ps1
# Script de automação para deploy do ControlService e Observabilidade

Write-Host "Iniciando Deployment do ControlService no Kubernetes..." -ForegroundColor Cyan

# 1. Aplicar manifests em ordem
Write-Host "`nAplicando manifestos..." -ForegroundColor Yellow
kubectl apply -f k8s/01-redis-deployment.yaml
kubectl apply -f k8s/02-redis-service.yaml
kubectl apply -f k8s/03-app-deployment.yaml
kubectl apply -f k8s/04-app-service.yaml
kubectl apply -f k8s/05-prometheus-pvc.yaml
kubectl apply -f k8s/06-prometheus-configmap.yaml
kubectl apply -f k8s/07-prometheus-deployment.yaml
kubectl apply -f k8s/08-prometheus-service.yaml
kubectl apply -f k8s/09-grafana-deployment.yaml
kubectl apply -f k8s/10-grafana-service.yaml
kubectl apply -f k8s/11-grafana-datasource-config.yaml

# 2. Aguardar Pods
Write-Host "`nAguardando Pods ficarem prontos (Running)..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l project=controlservice --timeout=120s

Write-Host "`nAmbiente Deployment concluido com sucesso!" -ForegroundColor Green

# 3. Instruções de Próximos Passos
Write-Host "`nProximos Passos:" -ForegroundColor Cyan
Write-Host "1. Abra o Port-Forward do Grafana em um novo terminal:"
Write-Host "   kubectl port-forward service/grafana-service 30030:3000"
Write-Host "2. Importe o Dashboard:"
Write-Host "   python push_pro_dash.py"
Write-Host "3. Acesse: http://localhost:30030 (admin / admin)"
