# Justificativas Técnicas e Arquiteturais — Projeto ControlService

Este documento detalha o racional técnico por trás de cada componente da infraestrutura, seguindo princípios de **Cloud-Native**, **Continuous Delivery** e **SRE**.

---

## FASE 1: Conteinerização Imutável (`docker/`)

O foco inicial foi transformar o código-fonte em um artefato imutável, seguro e portátil.

### [Dockerfile](file:///c:/Projetos/.NET/ControlService/docker/Dockerfile)
- **Multi-Stage Build**: Utilizamos duas imagens distintas. A `sdk:10.0-alpine` (pesada) apenas para compilar o código e a `aspnet:10.0-alpine` (leve) para rodar. Isso garante que ferramentas de build e segredos de compilação não vazem para a imagem final.
- **Alpine Linux**: Escolhida por ser extremamente leve (~5MB de base), reduzindo a latência de transferência e a superfície de ataque.
- **Segurança Non-Root**: Inserimos o comando `USER app`. Isso impede que, em caso de invasão do contêiner, o atacante tenha privilégios de `root` no Host.

### [build-and-push.sh](file:///c:/Projetos/.NET/ControlService/docker/build-and-push.sh)
- **Versionamento Semântico**: O script automatiza a criação de tags baseadas em argumentos, prevenindo o uso da tag nebulosa `latest` em produção.
- **Automação de Push**: Garante que a imagem local seja imediatamente disponibilizada no Docker Registry (DockerHub), tornando-a visível para o cluster Kubernetes.

---

## FASE 2: Infraestrutura de Aplicação (`k8s/`)

Migramos de contêineres isolados para uma malha orquestrada com alta disponibilidade.

### [01-redis-deployment.yaml](file:///c:/Projetos/.NET/ControlService/k8s/01-redis-deployment.yaml)
- **Cache de Baixa Latência**: Implantamos o Redis para suportar operações de alta performance da API, desacoplando o estado da aplicação.

### [02-redis-service.yaml](file:///c:/Projetos/.NET/ControlService/k8s/02-redis-service.yaml)
- **ClusterIP**: Define um IP interno estável. A API se conecta ao Redis via DNS interno (`redis-service`), eliminando a fragilidade de IPs variáveis.

### [03-app-deployment.yaml](file:///c:/Projetos/.NET/ControlService/k8s/03-app-deployment.yaml)
- **Réplicas e Escalabilidade**: Definimos 4 réplicas para garantir que a aplicação suporte carga e tenha redundância.
- **RollingUpdate**: Configurado para que, ao atualizar a versão, o Kubernetes troque os Pods um a um, mantendo o serviço disponível 100% do tempo.
- **Resource Limits**: Limitamos CPU ($500m$) e RAM ($512Mi$) para evitar que um vazamento de memória na aplicação comprometa o nó inteiro do cluster.
- **Probes (Health Checks)**: Usamos `Liveness` e `Readiness` no endpoint `/healthz`. O Kubernetes reinicia o Pod automaticamente se ele travar ou parar de responder.

### [04-app-service.yaml](file:///c:/Projetos/.NET/ControlService/k8s/04-app-service.yaml)
- **NodePort 30080**: Expõe a API para o mundo externo. Escolhemos uma porta fixa para facilitar a integração com ferramentas de teste de carga e o front-end.

---

## FASE 3: Observabilidade e Monitoramento (`k8s/`)

Implementamos o monitoramento baseando-nos nos **Golden Signals** (RED).

### [05-prometheus-pvc.yaml](file:///c:/Projetos/.NET/ControlService/k8s/05-prometheus-pvc.yaml)
- **Persistência**: Define um volume dedicado para que, mesmo que o Prometheus reinicie, o histórico de métricas não seja perdido.

### [06-prometheus-configmap.yaml](file:///c:/Projetos/.NET/ControlService/k8s/06-prometheus-configmap.yaml)
- **Target Scraping**: Configura o Prometheus para buscar métricas na API a cada 15 segundos. É o "cérebro" que ensina ao monitor onde os dados estão.

### [07-prometheus-deployment.yaml](file:///c:/Projetos/.NET/ControlService/k8s/07-prometheus-deployment.yaml)
- **Engine Analítico**: Gerencia o binário do Prometheus com montagem de volumes para o banco de dados de séries temporais.

### [08-prometheus-service.yaml](file:///c:/Projetos/.NET/ControlService/k8s/08-prometheus-service.yaml)
- **ClusterIP 9090**: Permite que o Grafana acesse os dados do Prometheus internamente.

### [09-grafana-deployment.yaml](file:///c:/Projetos/.NET/ControlService/k8s/09-grafana-deployment.yaml)
- **Camada de Visualização**: Injeta credenciais administrativas via env vars e configura o acesso automático ao datasource.

### [10-grafana-service.yaml](file:///c:/Projetos/.NET/ControlService/k8s/10-grafana-service.yaml)
- **Dashboard Port (30030)**: Porta externa para acesso ao painel visual de métricas.

### [grafana-dashboard.json](file:///c:/Projetos/.NET/ControlService/k8s/grafana-dashboard.json)
- **Dashboard as Code**: Contém a definição visual de todos os gráficos.
- **Tratamento "No Data"**: Justificativa técnica para o uso de `or vector(0)` nas queries: Evita que o painel pareça quebrado quando a taxa de erro é zero, exibindo um status de "0%" profissional e confiável.

### [deploy.sh](file:///c:/Projetos/.NET/ControlService/k8s/deploy.sh)
- **Orquestração de Deploy**: Garante que os recursos sejam criados na ordem lógica (Volumes -> Configs -> Deploys -> Services), evitando erros de dependência.

---

## FASE 4: Continuous Delivery (`ci/`)

Automatizamos o ciclo de vida do software para remover a falha humana.

### [Jenkinsfile](file:///c:/Projetos/.NET/ControlService/ci/Jenkinsfile)
- **Pipeline Declarativo**: Define estágios como Build, Push e Deploy.
- **Quality Gate**: O pipeline agora integra o teste de estresse como um estágio de aprovação. Se os requisitos de performance falharem, o deploy é interrompido (**Stop the Line**).

---

## FASE 5: Engenharia de Performance (`stress/`)

Validamos os limites do sistema antes de qualquer lançamento.

### [k6-stress-test.js](file:///c:/Projetos/.NET/ControlService/stress/k6-stress-test.js)
- **Cenários de Carga**: Simula 500 usuários simultâneos realizando operações de CRUD.
- **Thresholds**: Define que 95% das requisições devem responder em menos de 1s e erros devem ser inferiores a 5%.

### [run-stress-test.sh](file:///c:/Projetos/.NET/ControlService/stress/run-stress-test.sh)
- **Modo Headless**: Removidos os inputs manuais para permitir que o k6 rode de forma autônoma dentro do Jenkins.

### [README.md (stress)](file:///c:/Projetos/.NET/ControlService/stress/README.md)
- **Guia Operacional**: Documentação de como executar os testes de forma isolada para debug.

---

## Workflow Geral da Arquitetura (Fluxo de Funcionamento)

A arquitetura adotada consolida um ecossistema nativo em nuvem focado em **Integração Constante, Execução Orquestrada e Observabilidade**. O circuito fim-a-fim da aplicação e da infraestrutura opera da seguinte forma:

1. **Desenvolvimento e Integração Contínua (Pipeline via Jenkins)**
   - As alterações de código são registradas no repositório. O Jenkins intercepta e orquestra a jornada por meio do `ci/Jenkinsfile`.
   - A versão atualizada da aplicação `.NET` (previamente adaptada no `Program.cs` e `.csproj` para expor o provedor de telemetria `prometheus-net`) é submetida a um build otimizado Multi-Stage a partir das definições do `docker/Dockerfile`. O ambiente expõe nativamente a porta `/metrics`.
   - Após as validações primárias, a imagem sofre empacotamento semântico de tags e é despachada para o contêiner registry (`Docker Hub`), tornando-se uma versão imutável.

2. **Orquestração e Execução (Kubernetes)**
   - Os manifestos Kubernetes são aplicados instanciando a infraestrutura completa de uma só vez. O Banco Redis é implantado primeiro, mantendo sua camada isolada do exetior através do *networking* restrito `ClusterIP`.
   - A Aplicação .NET sobe de forma descentralizada baseada na imagem recém publicada. Para garantir segurança contra falhas singulares, estipulou-se a ativação de **4 réplicas** (`Deployments`) controladas. Após validadas pelo `Readiness` e `Liveness` probes, o componente lógico interceptador do tipo `NodePort` recebe permissão para direcionar e trafegar as requisições puras via sua interface de rede externa (porta `30080`).

3. **Monitoramento e Visualização (Prometheus e Grafana)**
   - Enquanto a Aplicação manipula dados, o componente Prometheus (reforçado para não ter perdas de séries temporais via `PVC`) atua baseando-se na tática de monitoria de coleta (*pull*). Orientado pelo `ConfigMap`, acessa proativamente o K8s consultando dados dos *endpoints* expostos.
   - O Grafana, hospedado independentemente em porta de exposição externa pública correspondente (`NodePort 30030`), interage de maneira nativa com seu Prometheus vizinho configurado. Em decorrido do sistema (*Dashboard as Code* via `grafana-dashboard.json`), os acumuladores brutos de resiliência e saúde (*CPU*, *RAM*, tráfego dinâmico RED - *Rate, Errors, Duration*) convertem-se e materializam em visões limpas.

4. **Testes e Limites Arquiteturais Operacionais (Stress Testing)**
   - Pondo o sistema à prova definitiva e exigindo sua alta latência imposta em SLA, as automações estressantes (via simulações como o `k6-stress-test.js` ou scripts pontuais dentro da pasta `stress`) intensificam carga concentrada interativa contra a respectiva rota de APIs liberada aos usuários exposta via NodePort.
   - O Kubernetes bloqueia agressivamente o abuso via travas predefinidas (*Resource e Memory Limits*), salvaguardando a estabilidade e assegurando que clusters inteiros não sucumbam por exaustão. Todas as reações operacionais e flutuações ficam registradas para as bancadas de administradores no Grafana, certificando assim a imutabilidade tolerante a falhas requisitada via implantação orquestrada.

---

## Referências Bibliográficas

- **Docker Deep Dive** – Nigel Poulton
- **Kubernetes Book** – Nigel Poulton
- **Continuous Delivery** – Jez Humble e David Farley
- **Jenkins 2: Up and Running** – Brent Laster
- **Prometheus: Up & Running** – Brian Brazil
