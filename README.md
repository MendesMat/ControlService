# ControlService

ControlService é um ERP desenvolvido com ecossistema **.NET**, voltado para **prestadores de serviços**. A solução oferece uma gestão robusta e integrada, simplificando os processos operacionais.

---

## Guia de Inicialização Rápida

Este guia orienta sobre o fluxo automatizado para subir o ambiente Kubernetes e visualizar o dashboard no Grafana.

### Pré-Requisitos
- **Docker Desktop** (com Kubernetes ativado) ou **Minikube**.
- Autenticação no DockerHub (`docker login`) ativada no terminal.

### 1. Deploy Automatizado
Abra o terminal na raiz do projeto e execute o script de automação:
```powershell
./deploy-all.ps1
```
O script irá aplicar os manifestos (Redis, API, Prometheus, Grafana), configurar o Data Source do Prometheus e aguardar a prontidão dos Pods.

### 2. Acesso ao Grafana (Port-Forward)
Para acessar o Grafana, é necessário criar um túnel. Execute em uma nova janela do terminal:
```powershell
kubectl port-forward service/grafana-service 30030:3000
```
**Mantenha esta janela aberta** durante o uso.

### 3. Importação do Dashboard
Em outra janela (na raiz do projeto), execute o script de importação:
```powershell
python push_pro_dash.py
```
Acesse no navegador: **[http://localhost:30030](http://localhost:30030)**
- **Usuário:** `admin`
- **Senha:** `admin`
- Menu lateral -> Dashboards -> **ControlService — Observabilidade**.

### 4. Geração de Dados (Teste de Stress)
Para visualizar métricas ativas, gere tráfego na API:
```powershell
docker run --rm -v "${PWD}/stress:/stress" -e BASE_URL=http://host.docker.internal:30080 grafana/k6 run /stress/k6-stress-test.js
```
Os gráficos de CPU, Memória e Requisições começarão a subir em poucos segundos.

---

## Arquitetura e Detalhes Técnicos

Para detalhes sobre as decisões arquiteturais, consulte o arquivo [JUSTIFICATIVAS.md](JUSTIFICATIVAS.md).

### Fases do Projeto
1. **Conteinerização**: Implantação de política Multi-Stage focada em segurança com Alpine Linux.
2. **Orquestração**: Utilização de manifestos K8s declarativos com RollingUpdates entre 4 réplicas.
3. **Observabilidade**: Implementação do padrão RED (Rate, Errors, Duration) via Prometheus e Grafana.
4. **Continuous Delivery**: Pipeline as Code gerenciado via Jenkins (`ci/Jenkinsfile`).
5. **Capacity Testing**: Engenharia de performance com k6 para validação de carga.

## Solução de Problemas

### Gráficos sem dados
- Verifique os Alvos: No Prometheus (`kubectl port-forward service/prometheus-service 9090:9090`), acesse `Status -> Targets`. O target `controlservice-api` deve estar **UP**.
- Coleta: O Prometheus pode levar até 30 segundos para a primeira coleta de dados.

### Erro no Port-Forward
- Se a porta `30030` estiver ocupada, utilize outra: `kubectl port-forward service/grafana-service 30031:3000` (ajustando a URL no navegador).
