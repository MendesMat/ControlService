# ControlService

ControlService é uma API RESTful desenvolvida com **.NET 10** para gestão de clientes, projetada com arquitetura **Clean Architecture** em camadas e infraestrutura **Cloud-Native** completa. O projeto contempla conteinerização, orquestração Kubernetes, observabilidade com Prometheus e Grafana, pipeline de entrega contínua via Jenkins e testes de capacidade com k6 e Locust.

---

## Stack de Tecnologias

| Camada | Tecnologia |
|---|---|
| **API** | .NET 10, ASP.NET Core, Scalar (OpenAPI) |
| **Cache** | Redis 7 Alpine |
| **Conteinerização** | Docker 29 (Multi-Stage Build, Alpine Linux) |
| **Orquestração** | Kubernetes (Docker Desktop / Minikube) |
| **Observabilidade** | Prometheus, Grafana 10, `prometheus-net.AspNetCore` |
| **CI/CD** | Jenkins 2 (Pipeline Declarativo) |
| **Stress Testing** | k6 (headless/CLI), Locust (interface gráfica) |
| **Arquitetura** | Clean Architecture, CQRS, MediatR |

---

## Pré-Requisitos

| Dependência | Verificação | Obrigatório |
|---|---|---|
| Docker Desktop (com K8s ativado) | `kubectl get nodes` | ✅ Sim |
| Autenticação Docker Hub | `docker login` | ✅ Sim |
| k6 | `k6 version` | ⚠️ Stress test CLI |
| Python 3 + Locust | `locust --version` | ⚠️ Stress test UI |

---

## Workflow de Implantação

O projeto é estruturado em 5 fases sequenciais que refletem um ciclo de entrega profissional real: do código-fonte ao sistema monitorado em produção.

```
[Código-Fonte] → [Imagem Docker] → [Cluster K8s] → [Observabilidade] → [Testes de Carga]
      │                │                  │                  │                   │
  Dockerfile     DockerHub         4 réplicas        Prometheus            k6 / Locust
  Unit Tests     build-and-push    RollingUpdate      Grafana               thresholds
```

---

## Fase 1 — Conteinerização

A aplicação é empacotada como uma imagem Docker imutável usando **Multi-Stage Build**: o estágio de build (`sdk:10.0-alpine`) executa os testes de unidade e compila a aplicação; apenas o artefato publicado é copiado para o estágio de runtime (`aspnet:10.0-alpine`), mantendo a imagem final enxuta e sem ferramentas de desenvolvimento.

**Imagem publicada:** [`mendesmat/controlservice:1.0.0`](https://hub.docker.com/r/mendesmat/controlservice)

```bash
# Construir e publicar uma nova versão
./docker/build-and-push.sh 1.0.0
```

### Ambiente Local com Docker Compose

Para desenvolvimento local, o `docker-compose.yml` orquestra a API e o Redis com recursos de armazenamento explícitos:

- **Volume nomeado** (`redis-data:/data`): persiste os dados do Redis entre reinicializações do container.
- **Bind Mount** (`./logs:/app/logs`): expõe os logs da aplicação diretamente no sistema de arquivos do host.
- **Bind Mount** (`./src/ControlService.API/appsettings.json:/app/appsettings.json`): permite alterar configurações sem reconstruir a imagem.

```bash
docker-compose up -d
```

| Serviço | URL Local |
|---|---|
| API REST | `http://localhost:8080/api/customers` |
| Health Check | `http://localhost:8080/healthz` |
| Métricas | `http://localhost:8080/metrics` |
| Documentação (Scalar) | `http://localhost:8080/docs` |

---

## Fase 2 — Orquestração com Kubernetes

Os manifestos declarativos em [`k8s/`](k8s/) constroem a infraestrutura completa do cluster na ordem correta via script de deploy.

```bash
# Deploy completo (Windows)
./deploy-all.ps1

# Deploy via script Bash (Linux/macOS/WSL)
./k8s/deploy.sh

# Validação sem aplicar (dry-run)
./k8s/deploy.sh --dry-run
```

### Recursos Kubernetes Utilizados

| Recurso | Manifesto | Descrição |
|---|---|---|
| `Deployment` | `01-redis-deployment.yaml` | Pod do Redis com resource limits |
| `Service` (ClusterIP) | `02-redis-service.yaml` | DNS interno `redis-service:6379` |
| `Deployment` | `03-app-deployment.yaml` | 4 réplicas da API com RollingUpdate |
| `Service` (NodePort) | `04-app-service.yaml` | Exposição externa na porta `30080` |
| `PersistentVolumeClaim` | `05-prometheus-pvc.yaml` | Volume de 5Gi para o banco TSDB do Prometheus |
| `ConfigMap` | `06-prometheus-configmap.yaml` | Configuração de scraping do Prometheus |
| `Deployment` | `07-prometheus-deployment.yaml` | Pod do Prometheus |
| `Service` (ClusterIP) | `08-prometheus-service.yaml` | DNS interno `prometheus-service:9090` |
| `Deployment` | `09-grafana-deployment.yaml` | Pod do Grafana |
| `Service` (NodePort) | `10-grafana-service.yaml` | Dashboard exposto na porta `30030` |

### Alta Disponibilidade

A API opera com **4 réplicas** sob estratégia `RollingUpdate`. Durante uma atualização, o Kubernetes substitui os Pods um a um (`maxUnavailable: 1`, `maxSurge: 1`), garantindo disponibilidade contínua do serviço. O kube-proxy distribui o tráfego entre as réplicas via round-robin interno.

### Health Checks (Probes)

Ambas as probes consultam o endpoint `/healthz` e são fundamentais para a resiliência automática do cluster:

**Readiness Probe** — controla o ingresso de tráfego. Um Pod só entra no pool de balanceamento do Service após retornar `HTTP 200` neste endpoint. Durante a inicialização do runtime .NET, nenhuma requisição externa é roteada para o Pod.

```yaml
readinessProbe:
  httpGet:
    path: /healthz
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 10
  failureThreshold: 3
```

**Liveness Probe** — monitora a saúde contínua. Se a aplicação entrar em deadlock ou parar de responder (ex.: esgotamento de threads), o Kubernetes reinicia o Pod automaticamente após 3 falhas consecutivas, sem intervenção manual.

```yaml
livenessProbe:
  httpGet:
    path: /healthz
    port: 8080
  initialDelaySeconds: 15
  periodSeconds: 20
  failureThreshold: 3
```

### Estado do Cluster (Produção)

```
NAME                                         READY   STATUS    AGE
controlservice-deployment-7c6b66778c-gk8qm   1/1     Running   19d
controlservice-deployment-7c6b66778c-rwqsm   1/1     Running   19d
controlservice-deployment-7c6b66778c-sghwt   1/1     Running   19d
controlservice-deployment-7c6b66778c-zs2p2   1/1     Running   19d
grafana-deployment-576f7f4958-x6rlp          1/1     Running   19d
prometheus-deployment-5dcfc86d88-tpz78       1/1     Running   20d
redis-deployment-5fb684db45-9dl2r            1/1     Running   20d
```

---

## Fase 3 — Observabilidade

A stack de monitoramento implementa o padrão **RED** (Rate, Errors, Duration) com coleta no modelo *pull* e visualização em tempo real.

### Exportação de Métricas

A API exporta métricas nativamente no formato Prometheus via `prometheus-net.AspNetCore`. A instrumentação é configurada em `Program.cs`:

```csharp
app.UseHttpMetrics(); // Instrumenta todas as rotas HTTP automaticamente
app.MapMetrics();     // Expõe as métricas em GET /metrics
```

As métricas geradas incluem histogramas de latência por rota e método (`http_request_duration_seconds`), contadores de requisições por código de status e métricas de processo .NET (`process_*`, `dotnet_*`).

### Prometheus — Scraping

O Prometheus coleta métricas da API a cada 15 segundos via DNS interno do cluster. A configuração está em [`k8s/06-prometheus-configmap.yaml`](k8s/06-prometheus-configmap.yaml):

```yaml
scrape_configs:
  - job_name: "controlservice-api"
    static_configs:
      - targets: ["controlservice-service:80"]
```

O banco de dados TSDB é persistido em um `PersistentVolume` de 5Gi, garantindo que o histórico de métricas não seja perdido em caso de reinicialização do Pod.

```bash
# Acessar a interface do Prometheus
kubectl port-forward service/prometheus-service 9090:9090
# http://localhost:9090 → Status → Targets → "controlservice-api" deve estar UP
```

### Grafana — Dashboard

O Grafana está disponível na porta `30030` com datasource do Prometheus provisionado automaticamente via ConfigMap ([`k8s/11-grafana-datasource-config.yaml`](k8s/11-grafana-datasource-config.yaml)).

| Acesso | URL | Credenciais |
|---|---|---|
| NodePort (Docker Desktop) | `http://localhost:30030` | `admin` / `admin` |
| Port-forward | `kubectl port-forward service/grafana-service 3000:3000` → `http://localhost:3000` | `admin` / `admin` |

**Dashboard pré-configurado** ([`k8s/grafana-dashboard.json`](k8s/grafana-dashboard.json)):

| Painel | Métrica | Formato |
|---|---|---|
| Taxa de Requisições por Status HTTP | `http_request_duration_seconds_count` | Time Series |
| Latência HTTP — P50 · P95 · P99 | `http_request_duration_seconds` (histogram) | Time Series |
| Taxa de Erros (%) | `http_requests_total` | Gauge |
| Memória RAM por Réplica | `process_working_set_bytes` | Time Series |
| CPU Total das 4 Réplicas | `process_cpu_seconds_total` | Gauge |

Para importar: **Dashboards → Import → Upload JSON file** → selecione `k8s/grafana-dashboard.json`.

---

## Fase 4 — Entrega Contínua

O pipeline declarativo em [`ci/Jenkinsfile`](ci/Jenkinsfile) automatiza o ciclo completo:

```
Build → Unit Tests → Docker Push → K8s Deploy → Stress Test (Quality Gate)
```

O estágio de stress test atua como **Quality Gate**: se os thresholds de performance falharem (P95 > 1s ou erros > 5%), o pipeline retorna `exit 1` e bloqueia a entrega, implementando o princípio *Stop the Line* de Continuous Delivery.

---

## Fase 5 — Testes de Capacidade

Os testes de stress validam o comportamento do sistema sob carga real, usando duas ferramentas complementares.

### k6 — Teste via Script (Headless)

Ferramenta CLI escrita em Go, integrada ao pipeline Jenkins para execução autônoma.

**Cenário de carga (`stress/k6-stress-test.js`):**

| Stage | Duração | Usuários (VUs) | Objetivo |
|---|---|---|---|
| Ramp-Up | 1 min | 0 → 50 | Aquecimento do JIT .NET |
| Baseline | 3 min | 50 | Comportamento em steady-state |
| Spike | 30s | 50 → 500 | Simular pico abrupto de tráfego |
| Pico | 3 min | 500 | Evidenciar gargalo e limites |
| Ramp-Down | 1 min | 500 → 0 | Validar resiliência pós-carga |

**Thresholds configurados:**
- `http_req_duration` P95 < 1000ms
- `http_req_failed` < 5%

```bash
# Execução completa automatizada (detecta a URL do cluster)
./stress/run-stress-test.sh

# Execução direta com URL explícita
k6 run --env BASE_URL=http://localhost:30080 stress/k6-stress-test.js

# Com exportação de resultados para análise
k6 run \
  --env BASE_URL=http://localhost:30080 \
  --out json=stress/results/raw.json \
  --summary-export=stress/results/summary.json \
  stress/k6-stress-test.js
```

### Locust — Teste via Interface Gráfica

Ferramenta Python com dashboard Web nativo para controle interativo de carga.

```bash
pip install locust
locust -f stress/locustfile.py
# Acessar: http://localhost:8089
# Host: http://localhost:30080
```

A interface do Locust permite ajustar dinamicamente o número de usuários e a taxa de criação (*spawn rate*), exibindo gráficos de requisições por segundo, latência e falhas em tempo real.

---

## Endpoints da API

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/healthz` | Health check (usado pelas Probes K8s) |
| `GET` | `/metrics` | Métricas no formato Prometheus |
| `GET` | `/api/customers` | Listar todos os clientes |
| `POST` | `/api/customers` | Criar novo cliente |
| `GET` | `/api/customers/{id}` | Buscar cliente por ID |
| `PUT` | `/api/customers/{id}` | Atualizar cliente |
| `DELETE` | `/api/customers/{id}` | Remover cliente |
| `GET` | `/docs` | Documentação interativa (Scalar/OpenAPI) |

---

## Solução de Problemas

**Grafana sem dados nos gráficos:**
Verifique se o Prometheus está coletando:
```bash
kubectl port-forward service/prometheus-service 9090:9090
# http://localhost:9090 → Status → Targets → "controlservice-api" deve estar UP
```
O Prometheus aguarda até 15s entre coletas. Após o primeiro scrape, os dados aparecem automaticamente.

**Pods em `CrashLoopBackOff`:**
```bash
kubectl logs -l app=controlservice --previous
```

**Porta já em uso:**
```bash
kubectl port-forward service/grafana-service 3001:3000
```

---

## Referências

- **Docker Deep Dive** — Nigel Poulton
- **The Kubernetes Book** — Nigel Poulton
- **Continuous Delivery** — Jez Humble e David Farley
- **Jenkins 2: Up and Running** — Brent Laster
- **Prometheus: Up & Running** — Brian Brazil
