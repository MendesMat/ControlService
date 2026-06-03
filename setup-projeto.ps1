# setup-projeto.ps1
# =============================================================================
# Script de avaliacao do projeto ControlService
# Executa TODO o ambiente do zero: deploy K8s + dashboard Grafana
# =============================================================================
# PRE-REQUISITOS:
#   - Docker Desktop instalado e rodando
#   - kubectl configurado (kubectl get nodes deve funcionar)
#   - Python 3 instalado
#   - Conexao com a internet (para baixar a imagem Docker Hub se necessario)
#
# USO (PowerShell como Administrador):
#   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
#   ./setup-projeto.ps1
# =============================================================================

$ErrorActionPreference = "Stop"

function Write-Step($n, $total, $msg) {
    Write-Host ""
    Write-Host "[$n/$total] $msg" -ForegroundColor Cyan
    Write-Host ("-" * 60) -ForegroundColor DarkGray
}

function Assert-Command($cmd) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Host "ERRO: '$cmd' nao encontrado. Instale antes de continuar." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  ControlService - Setup para Avaliacao     " -ForegroundColor Magenta
Write-Host "  Disciplina: DevOps / Computacao em Nuvem  " -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host ""

# ------------------------------------------------------------------
# ETAPA 0 - Verificar pre-requisitos
# ------------------------------------------------------------------
Write-Step 0 5 "Verificando pre-requisitos..."

Assert-Command "kubectl"
Assert-Command "python"
Assert-Command "docker"

$nodes = kubectl get nodes 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: kubectl nao consegue conectar ao cluster." -ForegroundColor Red
    Write-Host "Verifique se o Docker Desktop esta rodando com Kubernetes ativado." -ForegroundColor Yellow
    exit 1
}
Write-Host "kubectl OK - cluster acessivel" -ForegroundColor Green
Write-Host $nodes

# ------------------------------------------------------------------
# ETAPA 1 - Deploy da infraestrutura no Kubernetes
# ------------------------------------------------------------------
Write-Step 1 5 "Implantando infraestrutura no Kubernetes..."

kubectl apply -f infra-shared/k8s/01-redis-deployment.yaml
kubectl apply -f infra-shared/k8s/02-redis-service.yaml
kubectl apply -f infra-shared/k8s/05-prometheus-pvc.yaml
kubectl apply -f infra-shared/k8s/06-prometheus-configmap.yaml
kubectl apply -f infra-shared/k8s/07-prometheus-deployment.yaml
kubectl apply -f infra-shared/k8s/08-prometheus-service.yaml
kubectl apply -f infra-shared/k8s/09-grafana-deployment.yaml
kubectl apply -f infra-shared/k8s/10-grafana-service.yaml
kubectl apply -f infra-shared/k8s/11-grafana-datasource-config.yaml
kubectl apply -f backend/k8s/03-app-deployment.yaml
kubectl apply -f backend/k8s/04-app-service.yaml

Write-Host "Manifestos aplicados. Aguardando pods ficarem prontos (ate 3 minutos)..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=controlservice --timeout=180s
kubectl wait --for=condition=ready pod -l app=prometheus    --timeout=120s
kubectl wait --for=condition=ready pod -l app=grafana       --timeout=120s

Write-Host "Todos os pods estao Running!" -ForegroundColor Green
kubectl get pods

# ------------------------------------------------------------------
# ETAPA 2 - Verificar API
# ------------------------------------------------------------------
Write-Step 2 5 "Verificando API (health check)..."

$pfAPI = Start-Process -FilePath "kubectl" `
    -ArgumentList "port-forward", "service/controlservice-service", "8080:80" `
    -PassThru -WindowStyle Hidden

Start-Sleep 5

try {
    $resp = Invoke-WebRequest -Uri "http://localhost:8080/healthz" -UseBasicParsing -TimeoutSec 8
    Write-Host "API respondeu: $($resp.Content)" -ForegroundColor Green
    Write-Host "Endpoints disponiveis:" -ForegroundColor Green
    Write-Host "  http://localhost:8080/healthz         (Health Check)" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:8080/metrics          (Metricas Prometheus)" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:8080/api/customers    (CRUD de clientes)" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:8080/docs             (Documentacao OpenAPI)" -ForegroundColor DarkGreen
}
catch {
    Write-Host "AVISO: nao foi possivel confirmar o health check agora. Continue mesmo assim." -ForegroundColor Yellow
}
finally {
    $pfAPI | Stop-Process -Force -ErrorAction SilentlyContinue
}

# ------------------------------------------------------------------
# ETAPA 3 - Importar Dashboard do Grafana
# ------------------------------------------------------------------
Write-Step 3 5 "Importando Dashboard no Grafana..."

$pfGrafana = Start-Process -FilePath "kubectl" `
    -ArgumentList "port-forward", "service/grafana-service", "3000:3000" `
    -PassThru -WindowStyle Hidden

Start-Sleep 6

try {
    python push_pro_dash.py
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Dashboard importado com sucesso!" -ForegroundColor Green
    }
    else {
        Write-Host "AVISO: falha ao importar o dashboard automaticamente." -ForegroundColor Yellow
        Write-Host "Importe manualmente: Grafana > Dashboards > Import > Upload JSON" -ForegroundColor Yellow
        Write-Host "Arquivo: infra-shared\k8s\grafana-dashboard.json" -ForegroundColor Yellow
    }
}
finally {
    $pfGrafana | Stop-Process -Force -ErrorAction SilentlyContinue
}

# ------------------------------------------------------------------
# ETAPA 4 - Resumo final
# ------------------------------------------------------------------
Write-Step 4 5 "Verificando estado final do cluster..."
kubectl get pods
Write-Host ""
kubectl get services

# ------------------------------------------------------------------
# ETAPA 5 - Instrucoes para o professor
# ------------------------------------------------------------------
Write-Step 5 5 "Setup concluido!"

Write-Host ""
Write-Host "============================================================" -ForegroundColor Magenta
Write-Host "  AMBIENTE PRONTO PARA AVALIACAO                            " -ForegroundColor Magenta
Write-Host "============================================================" -ForegroundColor Magenta
Write-Host ""
Write-Host "Para verificar cada criterio, abra novos terminais:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  API + Metricas:" -ForegroundColor Yellow
Write-Host "    kubectl port-forward service/controlservice-service 8080:80"
Write-Host "    # http://localhost:8080/healthz"
Write-Host "    # http://localhost:8080/metrics"
Write-Host "    # http://localhost:8080/api/customers"
Write-Host ""
Write-Host "  Prometheus:" -ForegroundColor Yellow
Write-Host "    kubectl port-forward service/prometheus-service 9090:9090"
Write-Host "    # http://localhost:9090/targets  (controlservice-api deve estar UP)"
Write-Host ""
Write-Host "  Grafana (dashboard com CPU, RAM e RED):" -ForegroundColor Yellow
Write-Host "    kubectl port-forward service/grafana-service 3000:3000"
Write-Host "    # http://localhost:3000  (admin / admin)"
Write-Host ""
Write-Host "  Stress Test (k6 - requer k6 instalado):" -ForegroundColor Yellow
Write-Host "    # Instalar: winget install k6 --source winget"
Write-Host "    ./run-stress-windows.ps1"
Write-Host "    # Acompanhe o Grafana em tempo real durante o teste!"
Write-Host ""
Write-Host "  Stress Test UI (Locust - requer Python):" -ForegroundColor Yellow
Write-Host "    kubectl port-forward service/controlservice-service 8080:80"
Write-Host "    pip install locust"
Write-Host "    locust -f backend/stress/locustfile.py --host http://localhost:8080"
Write-Host "    # http://localhost:8089"
Write-Host ""
