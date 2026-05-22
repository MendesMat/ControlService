# ADR-001: Adoção do Estilo Arquitetural Monolítico Modular

**Data:** 15/05/2025
**Autor:** Matheus Mendes
**Status:** Aceito

---

## Contexto

O Control Service ERP abrange cinco domínios de negócio fortemente interdependentes — Gerenciamento, Comercial, Operacional, Financeiro e Relatórios — que compartilham dados transacionais e precisam de garantias ACID para manter a integridade referencial (ex.: uma Ordem de Serviço gerada pelo Comercial é a base para o realtório RAAE no Módulo de Relatórios).

O sistema será construído e mantido por **um único desenvolvedor**. Essa restrição organizacional é o driver mais crítico da decisão: qualquer estilo arquitetural adotado deve maximizar a produtividade individual e minimizar a sobrecarga operacional, sem comprometer os atributos de qualidade essenciais do sistema (Confiabilidade, Conformidade Legal e Manutenibilidade).

Dois estilos foram colocados em análise:

1. **Monólito Modular** — um único processo com domínios particionados internamente.
2. **Arquitetura Baseada em Serviços (Service-Based)** ou **Microsserviços** — múltiplos processos implantados e gerenciados de forma independente.

---

## Decisão

**Adotarei o Monólito Modular com Particionamento por Domínio** como estilo arquitetural da camada de backend do Control Service ERP.

O sistema será estruturado internamente como um conjunto de módulos fortemente coesos e fracamente acoplados, organizados em torno dos domínios de negócio (e não por função técnica, como na arquitetura em camadas clássica). A comunicação entre módulos ocorrerá exclusivamente via chamadas de método síncronas dentro do mesmo processo, sustentada por **Conascência Estática** (contratos de tipos e nomes compartilhados via interfaces e DTOs puros), nunca por referências diretas a implementações concretas.

A adoção do **Particionamento por Domínio** (em oposição ao Particionamento Técnico) é deliberada: cada módulo concentra, verticalmente, suas responsabilidades de domínio, de caso de uso e de persistência. Isso evita o espalhamento de lógica de negócio por camadas horizontais genéricas e preserva o **Princípio de Fechamento Comum (CCP)** — classes que mudam pelos mesmos motivos ficam juntas.

As regras de negócio puras (cálculos regulatórios do INEA, validações de alíquota por CNPJ, regras de faturamento) residirão no núcleo interno do sistema, completamente independentes de frameworks, ORM e transporte HTTP. Detalhes como o Entity Framework Core e os conectores de notas fiscais serão injetados de fora para dentro, respeitando a **Regra da Dependência** da Clean Architecture: as dependências de código-fonte apontam **apenas** para dentro, em direção às políticas de maior nível de abstração.

---

## Alternativas Consideradas e Motivos de Descarte

### Microsserviços
Descartado. A adoção de Microsserviços implica aceitar um conjunto de custos que não se justificam para este cenário:
- **Transações distribuídas:** A integridade entre a geração de O.S.s (Comercial), o faturamento (Financeiro) e o registro no RAAE (Relatórios) exigiria transações compensatórias via padrão Saga — adicionando uma camada de complexidade de orquestração desproporcional ao tamanho da equipe.
- **Observabilidade distribuída:** Rastrear um fluxo de negócio que cruza múltiplos serviços exigiria investimento em tracing distribuído (OpenTelemetry, Jaeger) antes mesmo de a funcionalidade principal existir.
- **Overhead operacional:** Deploy segmentado, múltiplos pipelines de CI/CD, gerenciamento de service discovery e latência de rede entre serviços são sobrecarga inaceitável para um desenvolvedor solo.

### Arquitetura Baseada em Serviços (Service-Based)
Descartado pelos mesmos vetores de risco dos Microsserviços, porém em escala menor. Ainda assim, introduziria a complexidade de múltiplos deployments e a ausência de transações ACID unificadas entre os serviços, sem oferecer um benefício concreto que a modularização interna do monólito já não entregue.

### Arquitetura em Camadas (Layered)
Descartado como particionamento de topo. A organização por camadas técnicas (Apresentação → Negócio → Dados) espalharia os domínios de negócio transversalmente, dificultando a localização e a manutenção de uma funcionalidade específica. O risco de *Architecture Sinkhole* — onde requisições transitam por camadas sem aplicar lógica alguma — também é elevado neste modelo.

---

## Trade-offs, Riscos e Impactos

### Vantagens
- **Simplicidade operacional:** Um único artefato para implantar, monitorar e depurar.
- **Garantias transacionais locais (ACID):** Operações críticas como o fechamento de faturamento, que envolve múltiplos módulos, são atômicas por natureza.
- **Produtividade individual máxima:** Eliminação do overhead de rede, serialização e contratos de API interna, permitindo foco total na lógica de domínio.
- **Caminho seguro para evolução:** O particionamento por domínio preserva a capacidade de extrair um módulo (ex.: Operacional) para um serviço independente no futuro, caso o crescimento da empresa justifique. Essa é a estratégia do *Strangler Fig Pattern*, e o Monólito Modular é seu ponto de partida natural.

### Desvantagens e Riscos
- **Elasticidade indivisível:** Se o módulo de Relatórios (geração de PDFs, RAAE) consumir recursos intensivos sob carga, isso impacta os demais módulos que rodam no mesmo processo. Mitigação: tarefas computacionalmente pesadas serão executadas de forma assíncrona (background jobs) para isolar o impacto.
- **Risco de erosão arquitetural (Big Ball of Mud):** Sem fronteiras de módulo enforçadas por ferramentas, o acoplamento tende a crescer silenciosamente. A mitigação está na seção de Compliance.

---

## Conformidade e Governança

Para impedir que as fronteiras de módulo sejam violadas ao longo do tempo, a arquitetura será governada por **Fitness Functions** automatizadas integradas ao pipeline de CI/CD:

- **NetArchTest** ou equivalente será configurado para negar qualquer referência direta de um módulo de domínio para a implementação concreta de outro módulo (apenas interfaces são permitidas).
- Testes de integração entre módulos serão escritos exclusivamente contra as interfaces publicadas, nunca contra implementações internas.
- Revisões de Pull Request incluirão verificação explícita do grafo de dependências de namespaces (princípio ADP — Acyclic Dependencies Principle).
