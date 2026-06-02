# Relatório Técnico: Fundamentos de Arquitetura de Software e Design de Sistemas

**Projeto:** Control Service ERP  
**Links de Referência:** [ADR’s](https://github.com/MendesMat/ControlService/tree/transicao-arquitetural/docs/ADRs) | [C4 Model](https://github.com/MendesMat/ControlService/tree/transicao-arquitetural/docs/C4%20Model) | Vídeo de Apresentação

---

## 1\. Contexto e Drivers de Negócio

### 1.1 Contexto do Sistema

O Control Service ERP é um sistema de gestão integrado para empresas prestadoras de serviços especializados de: dedetização, higienização de reservatórios e impermeabilização.  
O sistema controla o ciclo completo do serviço, do agendamento comercial à emissão de relatórios regulatórios, com execução móvel em campo e controle financeiro. A emissão de notas fiscais é delegada a um SaaS especializado via Anti-Corruption Layer (ver ADR-005).

O sistema é composto por **três contêineres independentes**: uma **SPA Web** para o escritório, um **App Móvel Offline-First** para operadores de campo, e uma **API Backend** que atende ambos via requisições REST. Cada cliente possuirá um ciclo de deploy próprio.

### 1.2 Drivers de Negócio

- **Conformidade Regulatória:** Geração confiável e automática do relatório RAAE (Relatório de Acompanhamento das Atividades de Empresas), obrigação jurídica crítica para a operação da empresa, regulamentada pelo INEA (Instituto Estadual do Ambiente).  
- **Automação de Processos:** Eliminação de inconsistências manuais na geração de contratos, certificados, ordens de serviço e relatórios RAAE.  
- **Operação Multi-CNPJ:** A empresa opera como uma entidade única, mas juridicamente fragmentada em múltiplos CNPJs por razões fiscais e regulatórias, exigindo isolamento de dados.

### 1.3 Restrições Organizacionais e Técnicas

- **Equipe de Um:** Desenvolvedor único. Inviabiliza infraestrutura distribuída complexa e direciona a terceirização de integrações caóticas (como a emissão de notas fiscais).  
- **Operação Offline de Campo:** Os operadores de campo atuam em subsolos, reservatórios e áreas rurais, com sinal instável de rede móvel. O App Móvel adota arquitetura “*Offline-First*” com sincronização explícita em três fases (ver ADR-003): **Fase 1** — download do itinerário no início do turno (SQLite local); **Fase 2** — execução totalmente offline com persistência local de insumos, fotos e status; **Fase 3** — upload das baixas ao retornar a área com sinal, com confirmação de recebimento antes de qualquer exclusão local.

---

## 2\. Atributos de Qualidade

### 2.1 Classificação

| Categoria | Atributo | Justificativa Arquitetural |
| :---- | :---- | :---- |
| Operacional | **Confiabilidade** | Dados de campo persistidos no SQLite antes do envio; O dados nunca se perdem por falhas de rede |
| Estrutural | **Configurabilidade** | Motor de Templates com marcações `[[VARIAVEL]]` no banco de dados. A gestão da empresa atualiza documentos sem deploy (ver ADR-004) |
| Estrutural | **Manutenibilidade** | Equipe de um exige particionamento por domínio e fronteiras claras |
| Transversal | **Conformidade Legal** | Regras do INEA residem no Core, validadas nativamente, sem depender da UI |
| Transversal | **Isolamento Multi-Tenant** | `tenant_id` em todas os documentos segregados por CNPJ; resolução automática via claim JWT (`ITenantContext`) |
| Transversal | **Auditabilidade** | Rastreabilidade completa do ciclo fiscal: payload, resposta, timestamp e ID do pedido por cada tentativa |

### 2.2 Trade-offs Arquiteturais

- **Isolamento de Tenant vs. Manutenibilidade:** Banco de dados único com isolamento lógico (ver ADR-002) preserva a operação de forma simples e relatórios consolidados, mas transfere a responsabilidade do isolamento para o código. Mitigação: **Global Query Filters do EF Core** garantem `WHERE tenant_id = @currentTenantId` em toda query, de forma declarativa e centralizada.  
- **Configurabilidade vs. Complexidade Estrutural:** Os templates dinâmicos eliminam deploys para mudanças jurídicas, ao custo de uma camada de parsing de strings e validação proativa de variáveis ao salvar templates.  
- **Confiabilidade Offline vs. Custo de Desenvolvimento:** Offline-First garante que ausência de rede nunca paralise o fluxo de trabalho em campo, ao custo de sincronização bidirecional, resolução de conflitos, SQLite no dispositivo e ciclos de deploy independentes.

### 2.3 Métricas Objetivas dos Atributos Prioritários

| Atributo | Indicador | Limiar Aceitável |
| :---- | :---- | :---- |
| Confiabilidade | Taxa de baixas perdidas por falha de rede | 0% — toda baixa persiste localmente antes do envio |
| Manutenibilidade | Violações de fronteira de módulo detectadas no CI | 0 violações em build verde (via NetArchTest) |
| Conformidade Legal | Relatórios RAAE rejeitados por inconsistência de dados | 0 rejeições — validação no Core antes da geração |
| Isolamento Multi-Tenant | Queries sem filtro de `tenant_id` em produção | 0 — Global Query Filter garante aplicação universal |
| Configurabilidade | Deploys necessários para atualizar texto de contrato | 0 — templates vivem no banco de dados |

---

## 3\. Decisões Arquiteturais (ADRs)

As decisões arquiteturais foram formalmente registradas em seis ADRs ([link das ADRs](https://github.com/MendesMat/ControlService/tree/transicao-arquitetural/docs)). Resumo das decisões e seus motivadores:

| ADR | Decisão | Principal motivador do descarte das alternativas |
| :---- | :---- | :---- |
| ADR-001 | **Monólito Modular** como estilo do backend | Microsserviços exigem Sagas, observabilidade distribuída e múltiplos pipelines de CI/CD, uma sobrecarga inaceitável para equipe de um |
| ADR-002 | **Multitenancy Lógico** com `tenant_id` e **Global Query Filter** | Database-per-tenant exigiria múltiplos pipelines de migração e complicaria relatórios consolidados |
| ADR-003 | **App Móvel Offline-First** com sincronização explícita em 3 fases | PWA online-only travaria o fluxo diário dos operadores; sincronização em tempo real não sobrevive a conectividade intermitente |
| ADR-004 | **Motor Interno de Templates** com marcações `[[VARIAVEL]]` e catálogo fechado | SaaS de documentos: custo e ponto de falha externo; hardcode: deploy a cada ajuste |
| ADR-005 | **Terceirização fiscal** para SaaS via `INotaFiscalGateway` (ACL) | Integração direta com webservices municipais é caótica, instável e equivale a manter um produto separado |
| ADR-006 | **PostgreSQL** como banco relacional | SQL Server Express: limite de 10 GB e restrições de CPU; SQL Server licenciado: OPEX injustificável |

---

## 4\. Estilos e Padrões Arquiteturais

O **Monólito Modular** com **Particionamento por Domínio** foi adotado como estilo central (ver ADR-001). O sistema é particionado em cinco módulos verticais: **Gerenciamento, Comercial, Operacional, Financeiro e Relatórios**. Cada módulo tem responsabilidades de domínio, caso de uso e persistência coesas. Esse particionamento respeita o **Princípio de Fechamento Comum (CCP)**: classes que mudam pelos mesmos motivos ficam juntas.

Estilos descartados e motivadores:

- **Microsserviços / Service-Based:** Transações entre Comercial, Operacional e Financeiro exigiriam Sagas compensatórias, tracing distribuído e múltiplas pipelines de CI/CD, incabível para uma equipe de um.  
- **Arquitetura em Camadas (Layered):** Particionamento técnico espalharia domínios transversalmente, elevando o risco de “*Architecture Sinkhole”* e dificultando a localização de funcionalidades.  
- **Pipeline (Pipes and Filters):** O fluxo de agendamento exige retroalimentação síncrona, incompatível com processamento sequencial unidirecional.

O Monólito Modular também é o ponto de partida estratégico para eventual extração de módulos via “*Strangler Fig Pattern”*, se o crescimento da empresa e da equipe justificarem.

---

## 5\. Design Arquitetural

### 5.1 Topologia de Camadas (Clean Architecture)

As dependências de código-fonte apontam exclusivamente para dentro, em direção às políticas de maior abstração:

- **Domain (Core):** Entidades, regras do INEA, cálculos de alíquota. Zero dependências externas.  
- **Application:** Casos de uso (ex.: `EmitirCertificadoDeGarantia`). Depende apenas de interfaces do domínio.  
- **Infrastructure:** Implementações concretas. EF Core, PostgreSQL, `IDocumentRenderer` (QuestPDF), `INotaFiscalGateway` (SaaS de notas fiscais). Injetadas de fora para dentro via DIP.  
- **Presentation:** Controllers ASP.NET Core e endpoints de sincronização do App Móvel.

**\[ Presentation \]  →  \[ Application (Casos de Uso) \]  →  \[ Domain (Core) \]**

      **↑                         ↑**

**\[ Infrastructure (EF Core, QuestPDF, INotaFiscalGateway, ITenantContext) \]**

### 5.2 Modularidade e Granularidade

**Modularidade** refere-se ao particionamento lógico do sistema em unidades coesas com fronteiras bem definidas. Cada um dos cinco módulos de domínio representa uma fatia vertical independente, qualquer funcionalidade nova é localizada em um único módulo, sem espalhamento transversal.

**Granularidade** refere-se ao nível de decomposição interno de cada módulo. Adotei granularidade de componente (não de microsserviços): cada módulo é um conjunto de casos de uso e repositórios coesos, sem decomposição excessiva, que criaria complexidade acidental. O nível de granularidade foi pensado para maximizar a autonomia de cada domínio sem introduzir sobrecarga de coordenação.

### 5.3 Acoplamento e Gerenciamento de Dependências

- **Conascência Estática** entre módulos: contratos via interfaces e DTOs puros, nunca referências a implementações concretas.  
- **Baixa Conascência de Execução** entre Módulo Operacional e Módulo Comercial. O Módulo Comercial desconhece o gerenciamento local offline do app, recebendo apenas as baixas consolidadas na Fase 3 (sincronização de baixas).  
- **Princípio de Dependência Acíclica (ADP):** O grafo de dependências entre módulos é acíclico e verificado por testes automatizados no CI.  
- **Antipadrões evitados:** *Big Ball of Mud* (prevenido por fronteiras enforçadas via NetArchTest); *Architecture Sinkhole* (prevenido pelo particionamento por domínio em vez de camadas técnicas); *Vendor Lock-in* (mitigado pelos ACLs `IDocumentRenderer` e `INotaFiscalGateway`).

### 5.4 Quanta Arquiteturais e Tecnologias

| Quantum | Tecnologia | Ciclo de Deploy |
| :---- | :---- | :---- |
| API Backend \+ PostgreSQL | .NET 10 (C\#) / EF Core 10 / Npgsql | Único artefato implantável |
| SPA Web Administrativo | React 18 (LTS) / TypeScript | Independente do backend |
| App Móvel Operacional | React Native / Expo \+ SQLite | Via stores |

---

## 6\. Comunicação da Arquitetura — Modelo C4

A arquitetura é comunicada em três níveis do Modelo C4 ([link do modelo C4](https://github.com/MendesMat/ControlService/tree/transicao-arquitetural/docs/C4%20Model)):

**Nível 1 \- Contexto:** O Control Service ERP de forma ampla, com dois atores externos (Escritório/Admin e Operador de Campo) e um sistema externo (API de Notas Fiscais SaaS). Evidencia a fronteira entre o sistema e o mundo externo.

**Nível 2 \- Contêineres:** Abre o sistema nos três contêineres independentes: SPA Web, App Móvel e API Backend. Os dois clientes consomem a API via REST/JSON. A API acessa o PostgreSQL unificado. O App Móvel persiste o itinerário e as baixas no SQLite.

**Nível 3 — Componentes (API Backend):** Detalha os cinco módulos de domínio e o Motor de Templates como componentes internos. Evidencia as regras de comunicação: o App Móvel conecta exclusivamente ao Módulo Operacional; a SPA conecta aos módulos Gerenciamento, Comercial, Financeiro e Relatórios; o Módulo Financeiro é o único que acessa o SaaS externo de notas fiscais. Os três níveis são consistentes entre si, o que aparece como "API Backend" no Nível 2 é exatamente o que está decomposto como cinco módulos no Nível 3\.

---

## 7\. Impacto na Efetividade dos Times

Embora o sistema seja desenvolvido por um único engenheiro, as decisões arquiteturais foram tomadas com a premissa de que a estrutura deve suportar um possível crescimento da equipe:

- **Autonomia por Módulo:** Cada módulo de domínio é uma unidade de trabalho independente. Em um cenário hipotético com múltiplos desenvolvedores, cada módulo pode ser atribuído a um engenheiro ou sub-time sem risco de conflito estrutural, pois as interfaces de comunicação entre módulos são o único ponto de contrato.  
- **Contratos Explícitos como Fronteira de Trabalho:** A obrigatoriedade de comunicação via interfaces (`IDocumentRenderer`, `INotaFiscalGateway`, DTOs entre módulos) elimina dependências implícitas. Mudanças internas a um módulo não quebram outros, reduzindo o custo de coordenação e revisão.  
- **Fitness Functions no CI:** Testes arquiteturais automatizados (NetArchTest) verificam fronteiras de módulo a cada build, eliminando a dependência de atenção humana constante para manter a integridade. Isso libera o desenvolvedor para focar em produto, não em questões estruturais.  
- **Deploy de Conteúdo Desacoplado do Deploy de Código:** O Motor de Templates permite que mudanças textuais (contratos, cláusulas jurídicas) sejam aplicadas pela gestão da empresa, sem envolver o time de desenvolvimento. Isso reduz o volume de demandas técnicas operacionais e aumenta a velocidade de resposta ao negócio.  
- **Testabilidade por Inversão de Dependência:** As abstrações `INotaFiscalGateway` e `IDocumentRenderer` permitem mockar integrações externas em qualquer nível de teste. O core de faturamento e o relatório RAAE são validados sem chamadas reais ao SaaS, acelerando o ciclo de feedback e reduzindo o custo de testes de regressão.

---

## 8\. Evolução Futura e Sistemas Distribuídos

**Nota:** Esta seção é estritamente prospectiva e descreve caminhos de evolução, caso o crescimento da empresa justifique a extração de módulos em serviços independentes.

O Monólito Modular é o ponto de partida natural para o “*Strangler Fig Pattern”*. O Módulo Operacional, por ter fronteiras já bem delimitadas, seria o primeiro candidato à extração. Nesse cenário distribuído, o **Módulo Comercial permaneceria o "Dono da Verdade"** sobre as Ordens de Serviço. O Módulo Financeiro e Operacional nunca se comunicariam diretamente, apenas com o Comercial.

Três padrões guiariam a transição:

- **Consumer-Driven Contracts (ex.: Pact):** Para garantir que mudanças no formato das baixas enviadas pelo Operacional não quebrem o Comercial de surpresa.  
- **Saga com Eventos Assíncronos:** Para substituir as transações ACID unificadas, o Módulo Operacional publicaria um evento de baixas; o Módulo Comercial consomiria e geraria as O.S.s; falhas no Módulo Comercial disparariam transações compensatórias sem deletar o dado bruto de campo.  
- **Propriedade de Dados por Serviço:** Auditoria e correção de baixas seriam responsabilidades exclusivas do Módulo Comercial após a sincronização, o app nunca receberia edições de volta.

O Módulo Financeiro permaneceria isolado dessas Sagas, pois o faturamento depende de regras de negócio estritas (ex.: clientes com bloqueio de notas fiscais geradas entre os dias 25–31) e sempre exige intervenção humana explícita, a partir dos pedidos consolidados no Módulo Comercial.