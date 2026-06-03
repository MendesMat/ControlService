# Integração Contínua, DevOps e Computação em Nuvem [26E1_3]

## Projeto da Disciplina - ENTREGA FINAL [Obrigatório]

Olá, Matheus,

Chegamos no momento mais importante! Você veio ao longo das aulas se preparando e agora chegou a hora do seu Projeto de Disciplina.

## Tarefa

Agora é hora de colocarmos tudo que aprendemos em prática! Portanto, você deve executar as tarefas abaixo:

1. Utilize o Docker para criar uma imagem personalizada de alguma aplicação previamente feita por você.
   - Publique a sua imagem no Docker Hub.
2. Suba sua imagem em algum cluster kubernetes, seguindo as seguintes especificações:
   - Utilize Deployment para subir sua aplicação com 4 réplicas.
   - Exponha sua aplicação de modo que ela fique acessível fora do Cluster (NODEPORT).
   - Se sua aplicação fizer uso de banco de dados, crie um POD com o mesmo e deixe-o acessível através do ClusterIP. Se sua aplicação não fizer uso de um BD suba uma imagem do Redis e crie um ClusterIP para o mesmo.
   - Crie algum probe para sua aplicação (Readness ou Liveness.)
3. Crie a estrutura para monitorar sua aplicação com o Prometheus e o Grafana (ou qualquer ferramenta a sua escolha: você deve ter um servidor de métricas e alguma ferramenta para dashboards).
   - Apenas o Grafana deverá ficar acessível para fora do Cluster.
   - Utilize um PVC para escrever os dados do Prometheus de maneira persistente.
   - Crie dashboards do Grafana que exponha dados sensíveis da sua aplicação (memória, cpu, etc.)
4. Utilize o Jenkins (ou qualquer ferramenta) para criar um pipeline de entrega do seu projeto.
5. Execute um stress test do seu projeto e tire print do Dashboard sofrendo alterações.

### 1. Projetar e implementar software para integração e entrega contínua em nuvem

- O aluno utilizou docker para criar containers com imagens da sua aplicação?
- O aluno utilizou os recursos básicos do Docker (Binds, volumes)?
- O aluno utilizou o K8s para rodar seu projeto de forma a conseguir alta disponibilidade?
- O aluno utulizou os recursos primitivos do K8s(Pods, services, volumes)?

### 2. Automatizar testes contínuos em nuvem

- O aluno utilizou Readness probe?
- O aluno utilizou Liveness Probe?
- O aluno desenvolveu stress test via interface gráfica?
- O aluno desenvolveu stress test via script?

### 3. Monitorar proativamente software em nuvem

- O aluno exportou as métricas do seu projeto?
- O aluno utilizou o Prometheus para fazer o Scrape das métricas do seu projeto?
- O aluno instanciou o grafana no seu Cluster?
- O aluno criou Dashboards no Grafana?

