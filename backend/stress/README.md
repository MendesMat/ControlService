# Fase 5: Estresse em Produção — Guia Operacional

## Visão Geral

Esta pasta contém o script de estresse da aplicação ControlService, utilizado para gerar evidência visual de consumo de **Memória RAM** e **CPU** sob carga real no cluster Kubernetes, com resultados visualizados no Grafana.

**Ferramenta:** [k6](https://k6.io) — driver de estresse HTTP moderno via CLI, escrito em Go.

---

## Pré-requisitos

| Requisito | Verificação |
|---|---|
| Cluster K8s ativo | `kubectl get nodes` |
| Fases 2 e 3 implantadas | `./k8s/deploy.sh` |
| k6 instalado | `k6 version` |

### Instalação do k6

```bash
# Windows (via winget)
winget install k6 --source winget

# macOS
brew install k6

# Linux (Debian/Ubuntu)
sudo gpg --no-default-keyring \
  --keyring /usr/share/keyrings/k6-archive-keyring.gpg \
  --keyserver hkp://keyserver.ubuntu.com:80 \
  --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69

echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] \
  https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list

sudo apt-get update && sudo apt-get install k6
```

---

## Cenário de Carga (Stages)

```
VUs
500 │                        ████████████████
    │                       █                █
 50 │            ████████████                 █
    │           █                              █
  0 │──────────█──────────────────────────────────▶ tempo
    │   1 min  3 min         30s     3 min    1 min
    │  ramp-up sustentado  spike   pico      ramp-down
```

| Stage | Duração | VUs | Propósito |
|---|---|---|---|
| 1 — Ramp-Up | 1 min | 0 → 50 | Aquecimento JIT .NET |
| 2 — Baseline | 3 min | 50 | Comportamento steady-state |
| 3 — Spike | 30s | 50 → 500 | Simular pico de tráfego |
| 4 — Pico | 3 min | 500 | **Evidência de gargalo** |
| 5 — Ramp-Down | 1 min | 500 → 0 | Evidência de resiliência |

**Duração total estimada:** ~8 min 30s

---

## Endpoints Exercitados

Todos os endpoints do `CustomersController` são cobertos em cada iteração de VU:

| Ordem | Método | Endpoint | Propósito |
|---|---|---|---|
| 1 | `GET` | `/healthz` | Warm-up da iteração |
| 2 | `POST` | `/api/customers` | Operação de escrita (cria dados únicos) |
| 3 | `GET` | `/api/customers` | Leitura em massa (lista todos) |
| 4 | `GET` | `/api/customers/{id}` | Leitura pontual (busca criado) |
| 5 | `PUT` | `/api/customers/{id}` | Escrita com ID (atualiza criado) |
| 6 | `DELETE` | `/api/customers/{id}` | Limpeza (remove criado) |

---

## Thresholds de Aprovação

| Threshold | Critério | Descrição |
|---|---|---|
| `http_req_duration` | P95 < 1000ms | 95% das requisições respondem em menos de 1s |
| `http_req_failed` | < 5% | Taxa de erros HTTP abaixo de 5% |
| `customer_creation_error_rate` | < 5% | Criações com falha abaixo de 5% |

> Se qualquer threshold for violado, k6 retorna código de saída `!= 0`.

---

## Como Executar

### Execução via CI/CD (Pipeline-as-Code) — Recomendado
Na Fase 4 da implantação, incluímos o *Stage 5* (`Capacity & Performance Test`) no **`ci/Jenkinsfile`**. A aprovação da sua "Release" agora está diretamente vinculada ao sucesso dos testes da infraestrutura em produção de modo totalmente Autossustentado (`Shift-Right testing`). O script foi reconfigurado de "Modo Interativo/Operacional" parar o **"Modo Autônomo (Headless)"**. O teste morre (Exit 1) se os níveis críticos (*Thresholds*) não baterem e o Pipeline bloqueia a entrega final.

### Execução Completa Direta (com URL automática)
```bash
./stress/run-stress-test.sh
```

### Execução com URL Explícita
```bash
./stress/run-stress-test.sh http://192.168.49.2:30080
```

### Execução Direta do k6 (sem automação)
```bash
# Com URL via variável de ambiente
k6 run --env BASE_URL=http://192.168.49.2:30080 stress/k6-stress-test.js

# Com output JSON para análise (Minikube)
k6 run \
  --env BASE_URL=http://$(minikube ip):30080 \
  --out json=stress/results/raw.json \
  --summary-export=stress/results/summary.json \
  stress/k6-stress-test.js
```

---

## Captura de Evidências no Grafana (Observabilidade Secundária)

Sendo guiado pelas regras de *Continuous Delivery*, as estatísticas mais precisas são coletadas e logadas através de relatórios JSON (e do `EXIT CODE` do K6 retornado para o Jenkins). Contudo, você pode aferir esses dados visualmente: **Durante o Stage 4 do K6 (minutos 4:30–7:30)**, você pode verificar os seguintes painéis do seu serviço no k8s (via URL `http://<NODE_IP>:30030`):

### 📸 Painel 1 — Memória RAM em Uso
- **Título:** "Consumo de Memória RAM por Réplica"
- **O que mostrar:** `process_working_set_bytes` acima de 300MB por réplica
- **Evidencia:** pressão de RAM passiva crescente sob 500 VUs

### 📸 Painel 2 — CPU Total
- **Título:** "CPU Total das 4 Réplicas (Gauge)"
- **O que mostrar:** gauge se aproximando do `limits.cpu` de 500m por réplica
- **Evidencia:** consumo acumulado de CPU proporcional à carga

### 📸 Painel 3 — Taxa de Requisições
- **Título:** "R — Taxa de Requisições por Status HTTP"
- **O que mostrar:** pico de req/s, breakdown por código de status
- **Evidencia:** volume de tráfego gerado vs. capacidade das 4 réplicas

### 📸 Painel 4 — Latência P50/P95/P99
- **Título:** "D — Latência HTTP (P50 · P95 · P99)"
- **O que mostrar:** P99 subindo durante o spike de 500 VUs
- **Evidencia:** degradação visível de latência sob carga extrema

### Como exportar pelo Grafana
1. Abra o painel desejado em modo de tela cheia (ícone de expansão)
2. Menu (⋮) → **Share** → **Export** → **Save as image** (PNG)
3. Guarde com nome descritivo: `evidencia-ram-stage4.png`

---

## Estrutura de Arquivos

```
stress/
├── k6-stress-test.js       ← Script principal do k6
├── run-stress-test.sh      ← Orquestrador (detecta URL, valida, executa)
├── README.md               ← Este documento
└── results/
    ├── .gitignore          ← Exclui arquivos de resultado do Git
    ├── k6-output-*.log     ← Log completo da execução (gerado)
    ├── k6-summary-*.json   ← Summary JSON do k6 (gerado)
    └── k6-raw-*.json       ← Métricas brutas por requisição (gerado)
```

---

## Interpretação dos Resultados

Após a execução, o k6 exibe um sumário similar a:

```
✓ healthz retorna 200
✓ POST /api/customers retorna 201
✓ GET /api/customers retorna 200

checks.........................: 99.12%  ✓ 89212  ✗ 788
data_received..................: 245 MB  480 kB/s
data_sent......................: 78 MB   153 kB/s
http_req_duration..............: avg=124ms min=2ms  med=89ms max=4.2s p(90)=312ms p(95)=489ms p(99)=1.1s
http_req_failed................: 0.88%   ✓ 788    ✗ 89212
http_reqs......................: 90000   176/s
```

> Valores de P99 acima de 1s no Stage 4 são **esperados e desejados** para demonstrar o gargalo sob pressão — é justamente isso que constitui a evidência do trabalho.
