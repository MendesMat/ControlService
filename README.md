# ControlService — Projeto de Pós-Graduação

**Disciplina:** Integração Contínua, DevOps e Computação em Nuvem [26E1_3]  
**Aluno:** Matheus Mendes  
**Imagem Docker Hub:** [`mendesmat/controlservice:1.0.0`](https://hub.docker.com/r/mendesmat/controlservice)

---

## 🚀 Início Rápido

> Execute **um único script** para implantar tudo e receber as instruções:

```powershell
# 1. Abra o PowerShell na raiz do projeto
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# 2. Execute o setup completo (deploy + dashboard Grafana + guia)
./setup-projeto.ps1
```

O script faz automaticamente:
- ✅ Valida pré-requisitos (kubectl, python, docker)
- ✅ Aplica todos os manifests Kubernetes (Redis, API, Prometheus, Grafana)
- ✅ Aguarda todos os pods ficarem `Running`
- ✅ Verifica o health check da API
- ✅ Importa o dashboard no Grafana
- ✅ Exibe as URLs e comandos para avaliar cada critério

**Pré-requisito mínimo:** Docker Desktop com Kubernetes ativado (Settings → Kubernetes → Enable Kubernetes).

---

## Pré-Requisitos para Avaliação

| Dependência | Verificação | Necessário para |
|---|---|---|
| kubectl configurado | `kubectl get nodes` | Verificar K8s |
| Docker Desktop | `docker info` | Build/Compose local |
| Python 3 | `python --version` | Importar dashboard Grafana |
| k6 | `k6 version` | Stress test via script |
| Locust | `locust --version` | Stress test via interface gráfica |

**Instalar k6 (Windows):**
```bash
winget install k6 --source winget
```

**Instalar Locust:**
```bash
pip install locust
```

---

## Critério 1 — Docker: Imagem personalizada + Docker Hub

A aplicação é empacotada como imagem Docker com **Multi-Stage Build**:
- **Estágio 1 (builder):** SDK Alpine → restaura pacotes, executa testes unitários, publica o binário
- **Estágio 2 (runtime):** ASP.NET Alpine → copia apenas o binário publicado (sem SDK, sem código-fonte)

**Imagem publicada no Docker Hub:**
```bash
docker pull mendesmat/controlservice:1.0.0
docker inspect mendesmat/controlservice:1.0.0 | findstr "Labels"
```

**Reconstruir e publicar (opcional):**
```bash
./backend/build-and-push.sh 1.0.0
```

---

## Critério 2 — Docker: Volumes e Binds

O `docker-compose.yml` demonstra ambos os tipos de armazenamento Docker:

| Tipo | Declaração | Finalidade |
|---|---|---|
| **Bind Mount** | `./logs:/app/logs` | Logs da API acessíveis no host |
| **Bind Mount** | `./backend/src/.../appsettings.json:/app/appsettings.json` | Configuração sem rebuild |
| **Named Volume** | `redis-data:/data` | Persistência do Redis entre reinicializações |

**Rodar localmente com Docker Compose:**
```bash
docker-compose up -d
```

| Serviço | URL |
|---|---|
| API REST | `http://localhost:8080/api/customers` |
| Health Check | `http://localhost:8080/healthz` |
| Métricas | `http://localhost:8080/metrics` |
| Documentação OpenAPI | `http://localhost:8080/docs` |

---

## Critério 3 — Kubernetes: Deployment com 4 Réplicas

**Verificar estado atual do cluster:**
```bash
kubectl get deployments
kubectl get pods -l app=controlservice
```

**Resultado esperado:**
```
NAME                             READY   UP-TO-DATE   AVAILABLE
controlservice-deployment        4/4     4            4
```

**Deploy completo (caso necessário recriar do zero):**
```bash
# Windows (PowerShell)
./setup-projeto.ps1

# Linux / macOS / WSL
./deploy.sh
```

O deploy aplica os manifests em ordem numérica: Redis → API → Prometheus → Grafana.

---

## Critério 4 — Kubernetes: NodePort e ClusterIP

```bash
kubectl get services
```

| Service | Tipo | Porta | Finalidade |
|---|---|---|---|
| `controlservice-service` | **NodePort** | `30080` | API acessível externamente |
| `redis-service` | **ClusterIP** | `6379` | Redis interno ao cluster |
| `prometheus-service` | **ClusterIP** | `9090` | Prometheus interno ao cluster |
| `grafana-service` | **NodePort** | `30030` | Grafana acessível externamente |

**Testar acesso externo à API:**
```bash
kubectl port-forward service/controlservice-service 8080:80
# Em outro terminal:
curl http://localhost:8080/healthz
# Resposta esperada: {"status":"healthy"}
```

---

## Critério 5 — Kubernetes: Liveness e Readiness Probes

```bash
kubectl describe deployment controlservice-deployment | findstr -i "liveness\|readiness"
```

**Resultado esperado:**
```
Liveness:   http-get http://:8080/healthz delay=15s timeout=5s period=20s #failure=3
Readiness:  http-get http://:8080/healthz delay=10s timeout=5s period=10s #failure=3
```

Ambas as probes consultam `/healthz`. A **Readiness** impede que tráfego chegue ao Pod antes do runtime .NET inicializar. A **Liveness** reinicia o Pod automaticamente em caso de deadlock.

---

## Critério 6 — Prometheus: Exportação e Scrape de Métricas

A API exporta métricas no formato Prometheus via `prometheus-net.AspNetCore`:

```bash
kubectl port-forward service/controlservice-service 8080:80
curl http://localhost:8080/metrics
# Retorna centenas de métricas: process_*, dotnet_gc_*, http_request_duration_seconds, etc.
```

**Verificar scrape ativo no Prometheus:**
```bash
kubectl port-forward service/prometheus-service 9090:9090
```
Acesse `http://localhost:9090/targets` → o target `controlservice-api` deve estar **UP**.

---

## Critério 7 — Prometheus: PVC para Persistência

```bash
kubectl get pvc
```

**Resultado esperado:**
```
NAME              STATUS   CAPACITY   ACCESS MODES
prometheus-pvc    Bound    5Gi        RWO
```

O TSDB do Prometheus é persistido em 5Gi, garantindo histórico de métricas mesmo após reinicialização do Pod.

---

## Critério 8 — Grafana: Acesso externo, Datasource e Dashboard

### Acessar o Grafana

```bash
kubectl port-forward service/grafana-service 3000:3000
```

Acesse: **`http://localhost:3000`** | Login: `admin` / `admin`

> **Alternativa (se docker-desktop):** `http://localhost:30030`

### Verificar Datasource

O datasource Prometheus é provisionado automaticamente via ConfigMap (`11-grafana-datasource-config.yaml`).

**Verificar via terminal:**
```bash
kubectl port-forward service/grafana-service 3000:3000
# Em outro terminal:
python -c "import urllib.request,base64,json; h={'Authorization':'Basic '+base64.b64encode(b'admin:admin').decode()}; r=urllib.request.Request('http://localhost:3000/api/datasources',headers=h); print(urllib.request.urlopen(r).read().decode())"
```

### Importar / Restaurar o Dashboard

O dashboard com 11 painéis (RAM, CPU, RED) está em `infra-shared/k8s/grafana-dashboard.json`.

**Para importar via script (recomendado):**
```bash
# Com port-forward ativo na porta 3000:
kubectl port-forward service/grafana-service 3000:3000

# Em outro terminal, a partir da raiz do projeto:
cd infra-shared
python ../push_pro_dash.py
```

Resposta esperada:
```json
{"status":"success","uid":"controlservice-obs-v1","url":"/d/controlservice-obs-v1/..."}
```

**Alternativa (importação manual via UI):**  
Grafana → Dashboards → Import → Upload JSON → selecione `infra-shared/k8s/grafana-dashboard.json`

### Painéis disponíveis

| Seção | Painel | Métrica |
|---|---|---|
| Visão Geral | Requisições/s | `rate(http_requests_received_total[5m])` |
| Visão Geral | Taxa de Erros (4xx+5xx) | por status code |
| **Memória RAM** | **Consumo por Réplica** | `process_working_set_bytes` |
| Memória RAM | Coleções do GC .NET | `dotnet_collection_count_total` |
| **CPU** | **Consumo por Réplica** | `rate(process_cpu_seconds_total[5m])` |
| **CPU** | **CPU Total (Gauge)** | soma das 4 réplicas |
| RED | Taxa de requisições | por código HTTP |
| RED | Latência P50/P95/P99 | `histogram_quantile` |

---

## Critério 9 — Pipeline Jenkins CI/CD

O pipeline declarativo em [`ci/Jenkinsfile`](ci/Jenkinsfile) implementa 5 stages:

```
Checkout → Test & Build → Docker Build & Push → Deploy K8s → Stress Test (Quality Gate)
```

| Stage | Descrição |
|---|---|
| **Checkout** | `checkout scm` |
| **Test & Build** | `dotnet test` nas 2 suites + `dotnet publish` |
| **Docker Build & Push** | Build imutável + push para Docker Hub com credenciais protegidas |
| **Deploy to Production K8s** | `kubectl set image` + `kubectl rollout status` |
| **Capacity & Performance Test** | k6 como Quality Gate — bloqueia a entrega se P95 > 1s |

O pipeline usa `post { always { cleanWs() } }` para limpeza de workspace e implementa *Stop the Line*: falhas no stress test impedem a entrega.

---

## Critério 10 — Stress Test

### Via Script — k6 (headless, integrado ao Jenkins)

**Pré-requisito:** porta `8080` disponível via port-forward.

```bash
# Terminal 1 — manter o port-forward ativo
kubectl port-forward service/controlservice-service 8080:80

# Terminal 2 — executar o teste
k6 run --env BASE_URL=http://localhost:8080 backend/stress/k6-stress-test.js
```

**Cenário de carga:**

| Stage | Duração | VUs | Objetivo |
|---|---|---|---|
| Ramp-Up | 1 min | 0 → 50 | Aquecimento JIT .NET |
| Baseline | 3 min | 50 | Comportamento steady-state |
| Spike | 30s | 50 → 500 | Pico abrupto de tráfego |
| Pico Máximo | 3 min | 500 | **← capturar prints do Grafana aqui** |
| Ramp-Down | 1 min | 500 → 0 | Resiliência pós-carga |

**Duração total:** ~8 minutos 30 segundos

**Thresholds:**
- `http_req_duration` P95 < 1.000ms
- `http_req_failed` < 5%

### Via Interface Gráfica — Locust

```bash
# Terminal 1 — port-forward
kubectl port-forward service/controlservice-service 8080:80

# Terminal 2 — Locust
pip install locust
locust -f backend/stress/locustfile.py --host http://localhost:8080
```

Acesse: **`http://localhost:8089`** → defina usuários e spawn rate → Start

---

## Verificação Completa — Checklist Rápido

```bash
# 1. Cluster saudável
kubectl get all

# 2. 4 réplicas rodando
kubectl get pods -l app=controlservice

# 3. Serviços expostos corretamente
kubectl get services

# 4. PVC do Prometheus vinculado
kubectl get pvc

# 5. Probes ativas
kubectl describe deployment controlservice-deployment | findstr -i "liveness\|readiness"

# 6. API respondendo
kubectl port-forward service/controlservice-service 8080:80
curl http://localhost:8080/healthz

# 7. Métricas exportadas
curl http://localhost:8080/metrics | findstr "process_working_set"

# 8. Prometheus com scrape ativo
kubectl port-forward service/prometheus-service 9090:9090
# → http://localhost:9090/targets → controlservice-api = UP

# 9. Grafana acessível
kubectl port-forward service/grafana-service 3000:3000
# → http://localhost:3000 (admin/admin)
```

---

## Solução de Problemas

**Dashboard sem painéis no Grafana:**
```bash
# Re-importar o dashboard
kubectl port-forward service/grafana-service 3000:3000
cd infra-shared && python ../push_pro_dash.py
```

**Prometheus sem dados:**
```bash
kubectl port-forward service/prometheus-service 9090:9090
# Acesse http://localhost:9090/targets
# O target "controlservice-api" deve estar UP
# Se DOWN, verificar se a API está running:
kubectl get pods -l app=controlservice
```

**Pod em CrashLoopBackOff:**
```bash
kubectl logs -l app=controlservice --previous
```

**Recriar toda a infraestrutura do zero:**
```bash
kubectl delete -f backend/k8s/ -f infra-shared/k8s/
./setup-projeto.ps1   # Windows
# ou: ./deploy.sh  # Linux/WSL
```

---

## Estrutura do Repositório

```
ControlService/
├── backend/
│   ├── Dockerfile                    ← Multi-stage build (testes + publicação)
│   ├── k8s/
│   │   ├── 03-app-deployment.yaml    ← 4 réplicas, Liveness + Readiness probes
│   │   └── 04-app-service.yaml       ← NodePort 30080
│   └── stress/
│       ├── k6-stress-test.js         ← Stress test via script (CLI/headless)
│       └── locustfile.py             ← Stress test via interface gráfica
├── infra-shared/
│   └── k8s/
│       ├── 01-redis-deployment.yaml  ← Pod Redis
│       ├── 02-redis-service.yaml     ← ClusterIP redis-service:6379
│       ├── 05-prometheus-pvc.yaml    ← PVC 5Gi para TSDB
│       ├── 06-prometheus-configmap.yaml ← Configuração de scraping
│       ├── 07-prometheus-deployment.yaml
│       ├── 08-prometheus-service.yaml ← ClusterIP prometheus-service:9090
│       ├── 09-grafana-deployment.yaml
│       ├── 10-grafana-service.yaml   ← NodePort 30030
│       ├── 11-grafana-datasource-config.yaml ← Provisioning automático
│       └── grafana-dashboard.json   ← Dashboard com 11 painéis
├── ci/
│   └── Jenkinsfile                   ← Pipeline 5-stages
├── docker-compose.yml               ← Ambiente local (volumes + binds)
├── setup-projeto.ps1                ← Setup completo para avaliação (Windows)
├── deploy.sh                        ← Deploy completo (Linux/WSL)
└── push_pro_dash.py                 ← Importar dashboard no Grafana
```

---

## Referências

- **Docker Deep Dive** — Nigel Poulton
- **The Kubernetes Book** — Nigel Poulton
- **Continuous Delivery** — Jez Humble e David Farley
- **Jenkins 2: Up and Running** — Brent Laster
- **Prometheus: Up & Running** — Brian Brazil
