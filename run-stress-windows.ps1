# run-stress-windows.ps1
# Mantém kubectl port-forward ativo durante toda a execução do k6
param(
    [string]$LocalPort = "18080"
)

$env:PATH = $env:PATH + ";C:\Program Files\k6"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

Write-Host "==> Iniciando port-forward service/controlservice-service $($LocalPort):80 ..." -ForegroundColor Cyan

# Inicia o port-forward como processo filho (não como Job, para sobreviver)
$pf = Start-Process -FilePath "kubectl" `
    -ArgumentList "port-forward", "service/controlservice-service", "${LocalPort}:80" `
    -PassThru -WindowStyle Hidden

Start-Sleep 4

# Valida conectividade antes de disparar o teste
try {
    $health = Invoke-WebRequest -Uri "http://localhost:$LocalPort/healthz" -UseBasicParsing -TimeoutSec 5
    Write-Host "==> Health check OK: $($health.Content)" -ForegroundColor Green
} catch {
    Write-Host "ERRO: port-forward não está respondendo. Abortando." -ForegroundColor Red
    $pf | Stop-Process -Force
    exit 1
}

Write-Host "==> Disparando k6 stress test via localhost:$LocalPort ..." -ForegroundColor Cyan
Write-Host "    Duração total: ~8m30s | Pico: 500 VUs" -ForegroundColor Yellow
Write-Host "    Abra o Grafana em http://localhost:3000 para acompanhar (port-forward ativo em outro terminal)!" -ForegroundColor Yellow

# Executa o k6 (bloqueante — aguarda conclusão)
$k6Exit = 0
try {
    k6 run `
        --env BASE_URL=http://localhost:$LocalPort `
        --out "json=backend\stress\results\k6-raw-$timestamp.json" `
        --summary-export "backend\stress\results\k6-summary-$timestamp.json" `
        backend\stress\k6-stress-test.js
    $k6Exit = $LASTEXITCODE
} finally {
    Write-Host "==> Encerrando port-forward (PID $($pf.Id))..." -ForegroundColor Cyan
    $pf | Stop-Process -Force -ErrorAction SilentlyContinue
}

Write-Host "==> k6 encerrado com exit code: $k6Exit" -ForegroundColor $(if ($k6Exit -eq 0) { "Green" } else { "Red" })
Write-Host "==> Resultados salvos em backend\stress\results\" -ForegroundColor Cyan

exit $k6Exit
