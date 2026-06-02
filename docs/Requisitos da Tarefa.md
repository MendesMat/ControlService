# Modelagem de Domínios com Domain-Driven Design [26E2_3]

## Projeto da Disciplina [Obrigatório]

Olá, Matheus.

Este Projeto de Disciplina tem como objetivo consolidar, na prática, os fundamentos do Domain-Driven Design (DDD), explorando tanto o design estratégico quanto o design tático do domínio. A proposta é que você atue como arquiteto(a) ou designer de domínio, conduzindo a modelagem de um domínio real ou altamente plausível a partir de regras de negócio, linguagem ubíqua e decisões conscientes de isolamento e estruturação do modelo.

Ao longo da sua atuação profissional, sistemas bem-sucedidos não nascem apenas de boas tecnologias, mas de bons modelos de domínio, que refletem com fidelidade o negócio, facilitam a comunicação entre stakeholders e permitem evolução contínua com menor custo. Este projeto busca desenvolver exatamente essa competência.

## Contexto

Durante a disciplina, você estudou os fundamentos do Domain-Driven Design, a importância da exploração do domínio com especialistas, o papel da linguagem ubíqua, a definição de bounded contexts, o isolamento do domínio em relação a detalhes técnicos e a modelagem tática por meio de entidades, value objects, agregados, repositórios e domain services.

Esses conceitos são aplicados por arquitetos e times de engenharia para:

- Reduzir ambiguidades de negócio
- Criar modelos expressivos e alinhados à realidade organizacional
- Evitar acoplamentos indesejados
- Sustentar a evolução do software ao longo do tempo

Neste projeto, você deverá escolher um domínio relevante do seu contexto profissional ou um domínio fictício, porém realista e conduzir sua modelagem de forma estruturada, documentando decisões e demonstrando clareza conceitual.

Caso você não tenha um domínio claro para trabalhar, procure o professor da disciplina, que irá auxiliá-lo na definição de um contexto adequado, viável e alinhado às competências do curso.

## Tarefa

Sua tarefa é modelar um domínio utilizando Domain-Driven Design, contemplando tanto o design estratégico quanto o design tático, com foco na clareza conceitual, isolamento adequado e capacidade de tradução do modelo para código.

Você deverá:

- Explorar o domínio e explicitar conceitos de negócio
- Definir linguagem ubíqua
- Delimitar fronteiras claras de contexto
- Estruturar o modelo tático respeitando regras de negócio e invariantes
- Demonstrar como o modelo pode ser implementado e evoluído

O objetivo não é escrever código funcional, mas demonstrar domínio conceitual, capacidade de modelagem e entendimento profundo das decisões tomadas.

## Requisitos da Tarefa

O projeto deve atender obrigatoriamente aos seguintes requisitos:

### Parte 1 — Exploração do Domínio e Linguagem Ubíqua

- Descrever o domínio escolhido e seu contexto organizacional
- Identificar especialistas de domínio (reais ou representados)
- Explicitar conceitos-chave, termos e regras de negócio
- Definir e apresentar uma Linguagem Ubíqua, garantindo consistência terminológica

### Parte 2 — Design Estratégico - Bounded Contexts

- Identificar e definir Bounded Contexts relevantes
- Justificar as fronteiras escolhidas
- Elaborar um Context Map, analisando:
  - Interações entre contextos
  - Padrões de relacionamento (por exemplo, Partnership, Customer/Supplier, Conformist)
- Identificar claramente o Core Domain

### Parte 3 — Isolamento do Domínio

- Demonstrar como o domínio é isolado de detalhes técnicos e de infraestrutura
- Explicar a aderência aos princípios de Arquitetura Limpa
- Discutir estratégias para manter a maleabilidade do design ao longo do tempo

### Parte 4 — Modelagem Tática do Domínio

- Modelar:
  - Entidades, com identidade contínua e regras de negócio encapsuladas
  - Value Objects, imutáveis e semanticamente ricos
- Definir Agregados, explicitando:
  - Raiz do agregado
  - Limites de consistência transacional
  - Invariantes e regras de negócio críticas

### Parte 5 — Comportamento e Serviços de Domínio

- Identificar e definir Domain Services quando o comportamento não pertencer claramente a uma entidade ou value object
- Aplicar padrões de análise, como Specification ou Policy, quando pertinente
- Demonstrar como as regras de negócio são aplicadas e protegidas no modelo

### Parte 6 — Persistência e Ciclo de Vida

- Definir Repositórios como abstrações de persistência dos agregados
- Explicar o ciclo de vida dos agregados:
  - Criação
  - Transições de estado
  - Exclusão ou arquivamento
- Discutir implicações de persistência sem acoplar o domínio a tecnologias específicas

### Parte 7 — Tradução do Modelo para Código

- Apresentar o modelo de domínio em uma forma que possa ser traduzida diretamente em código
  - Diagramas conceituais
  - Pseudocódigo
  - Estruturas de classes
- Demonstrar coerência entre conceitos, linguagem ubíqua e estrutura do modelo

## Formato de Entrega

Você deverá entregar um único relatório técnico em formato PDF, com nome no seguinte padrão:

`nome_sobrenome_dr2_modelagem_dominios_ddd.pdf`

O relatório deve conter, no mínimo:

- Contexto do domínio e problema de negócio
- Linguagem Ubíqua definida
- Bounded Contexts e Context Map
- Identificação do Core Domain
- Modelo tático (Entidades, Value Objects, Agregados)
- Domain Services e Repositórios
- Discussão sobre regras, invariantes e evolução do modelo
- Considerações sobre maleabilidade e impacto arquitetural

O relatório deve ser claro, técnico e bem estruturado, demonstrando maturidade na aplicação do DDD.

Como parte da entrega, o aluno deve incluir no PDF o link para um vídeo de apresentação hospedado no Youtube, não listado ou público, com permissão de acesso público (qualquer pessoa com o link pode assistir).

Duração máxima: 5 minutos.

Durante o vídeo, o aluno deve exibir na tela os próprios artefatos entregues no PDF e percorrer, com suas palavras, os seguintes aspectos técnicos:

- **O domínio escolhido e seu contexto de negócio:** explique brevemente o problema que o domínio resolve e quem são os especialistas de domínio considerados.
- **A Linguagem Ubíqua:** apresente ao menos três termos-chave definidos e demonstre como eles aparecem de forma consistente no modelo.
- **Os Bounded Contexts e o Context Map:** explique as fronteiras que você delimitou, por que as delimitou assim e qual padrão de relacionamento foi adotado entre eles.
- **O modelo tático:** percorra ao menos um Agregado, identificando sua raiz, suas invariantes e como as regras de negócio estão encapsuladas nele.
- **A tradução do modelo para código:** mostre como os conceitos do domínio se refletem nas estruturas de classes, pseudocódigo ou diagramas entregues, evidenciando a coerência entre linguagem ubíqua e implementação.

Espera-se que o aluno apareça no vídeo, por webcam. Caso opte por não aparecer ou não possa, deve justificar explicitamente no início do vídeo, de forma clara e fundamentada, por que não o faz.

A ausência sem justificativa será considerada na avaliação. O objetivo deste vídeo é evidenciar que as decisões de modelagem foram compreendidas, raciocínio próprio foi exercido e os artefatos são de autoria genuína do aluno.

## Status da Entrega

- **Número da tentativa:** Esta é a tentativa 1 (2 tentativas permitidas).
- **Status da entrega:** Nenhuma tentativa
- **Status da avaliação:** Não avaliado
- **Data de entrega:** segunda, 29 jun 2026, 23:59
- **Tempo restante:** 28 dias 1 hora

---

## Rubrica

Template de Rubrica para ser utilizado com a extensão Rubricator

### 1. Arquitetar o design estratégico do domínio e da comunicação, explorando o domínio com especialistas, definindo linguagem ubíqua e formalizando o modelo para tradução em código.

- O aluno explicou os conceitos fundamentais do Domain-Driven Design e justificou sua relevância para o design do software proposto, demonstrando como a abordagem orientada ao domínio foi determinante para as decisões de modelagem tomadas ao longo do projeto?

- O aluno conduziu a exploração do domínio junto aos especialistas identificados, documentando os conceitos, as regras de negócio e as ambiguidades extraídas nesse processo e demonstrando como esses insumos influenciaram as escolhas de modelagem?

- O aluno definiu a Linguagem Ubíqua do domínio e a utilizou de forma consistente ao longo de todo o modelo, garantindo que os mesmos termos fossem empregados nas descrições, nos diagramas e nas estruturas de código ou pseudocódigo apresentados?

- O aluno formalizou o modelo de domínio em uma representação — diagrama conceitual, pseudocódigo ou estrutura de classes — que permitiu sua tradução direta em código, mantendo coerência entre os conceitos da Linguagem Ubíqua e as estruturas do modelo?

- O aluno identificou e tornou explícitos os conceitos tácitos do negócio — regras implícitas, restrições não documentadas ou termos usados informalmente pelos especialistas — incorporando-os ao modelo de domínio e à Linguagem Ubíqua de forma rastreável?

### 2. Definir fronteiras arquiteturais e isolamento do modelo, aplicando bounded contexts, analisando interações, isolando o domínio de detalhes técnicos e promovendo design maleável.

- O aluno aplicou o padrão Bounded Context para delimitar as fronteiras dos módulos ou microsserviços do sistema, justificando cada fronteira com base na coesão conceitual do domínio, na autonomia de evolução esperada e nas regras de negócio que cada contexto encapsula?

- O aluno elaborou o Context Map do domínio, analisando as interações entre os Bounded Contexts identificados e classificando cada relacionamento com o padrão correspondente — como Partnership, Customer/Supplier, Conformist ou Anti-Corruption Layer — e justificando a escolha de cada padrão?

- O aluno isolou o Core Domain de detalhes técnicos e de infraestrutura, demonstrando como as dependências foram direcionadas para o domínio e como camadas externas — como persistência, apresentação e integração — foram impedidas de contaminar o modelo de negócio?

- O aluno projetou o modelo de domínio com práticas que favorecem a maleabilidade, especificando as decisões de design — como uso de abstrações, evitação de acoplamento prematuro e separação de responsabilidades — que permitirão refatorar e evoluir o modelo sem ruptura dos contratos existentes?

### 3. Modelar estruturas táticas do domínio, projetando value objects ricos, definindo responsabilidades de agregados e garantindo invariantes e regras transacionais.

- O aluno modelou as Entidades do domínio com identidade contínua bem definida, demonstrando como cada entidade encapsula as regras de negócio que lhe pertencem e impede que estados inválidos sejam representados no modelo?

- O aluno projetou Value Objects imutáveis para os conceitos de domínio que não requerem identidade própria, justificando a escolha entre Value Object e Entidade em cada caso e demonstrando como a imutabilidade foi garantida no modelo proposto?

- O aluno agrupou Entidades e Value Objects em Agregados, definindo a raiz de cada agregado, os objetos internos que somente podem ser acessados por meio dela e justificando como os limites estabelecidos refletem as unidades de consistência transacional do domínio?

- O aluno garantiu as invariantes e as regras transacionais dos Agregados, especificando quais regras de negócio foram protegidas pela raiz do agregado, como mutações foram controladas e como o acesso direto a entidades internas foi prevenido no modelo?

### 4. Implementar comportamento, persistência e evolução do domínio, gerenciando o ciclo de vida dos agregados, projetando domain services e aplicando padrões táticos.

- O aluno definiu os Repositórios como abstrações de persistência dos Agregados, especificando as interfaces de recuperação e armazenamento de cada agregado e demonstrando como essa camada isolou o domínio de qualquer tecnologia de persistência específica?

- O aluno gerenciou o ciclo de vida dos Agregados do domínio, especificando os mecanismos de criação, as transições de estado válidas, as regras que governam a exclusão ou o arquivamento e como essas regras foram protegidas pelo modelo?

- O aluno identificou e definiu Domain Services para as operações de domínio que não pertenciam naturalmente a nenhuma Entidade ou Value Object, justificando a criação de cada serviço com base na ausência de um dono conceitual claro e demonstrando como o serviço foi mantido sem estado?

- O aluno aplicou padrões de análise — como Specification ou Policy — para resolver problemas de modelagem identificados no domínio, justificando a escolha de cada padrão com base no problema específico que ele resolveu e demonstrando como sua aplicação tornou as regras de negócio mais explícitas e combináveis no modelo?
