# Control Service ERP \- Documentação de Arquitetura

## Visão Geral

O **Control Service ERP** é um sistema de gestão integrado para empresas prestadoras de serviços especializados, com foco em segurança regulatória e automação de processos repetitivos e sensíveis a erros.

### Escopo de Serviços

**Serviços Regulamentados pelo INEA:**

* Dedetização  
* Higienização e desinfecção de reservatórios de água

**Serviços Complementares:**

* Impermeabilização  
* Obras gerais

### Arquitetura do Sistema

O sistema foi estruturado em **cinco setores funcionais** independentes, organizados por domínio de negócio:

1. **Gerenciamento** \- Configuração e controle centralizado  
2. **Comercial** \- Operações de vendas e relacionamento com clientes  
3. **Operacional** \- Execução de itinerários e serviços  
4. **Financeiro** \- Fluxo de caixa e obrigações  
5. **Relatórios** \- Conformidade regulatória e análise de desempenho

---

## 1\. Setor de Gerenciamento

Responsável pela configuração centralizada do sistema e controle de acesso.

### 1.1 Perfis de CNPJ

Cadastro e administração de entidades CNPJ com configurações isoladas por pessoa jurídica (multitenancy).

**Funcionalidades:**

* Cadastro e edição de múltiplos CNPJs  
* Associação de naturezas de serviço por competência  
* Personalização de documentos por CNPJ  
* Configuração de alíquotas tributárias específicas  
* Vinculação de certificado digital A1

### 1.2 Perfis de Permissões

Modelo de controle de acesso granular baseado em papéis.

**Funcionalidades:**

* Criação de perfis de usuário por setor  
* Definição de permissões por componente do sistema  
* Níveis de acesso personalizáveis:  
  * **Leitura** \- Visualização apenas  
  * **Edição** \- Modificação de registros (em conjunto com leitura)  
  * **Criação e Exclusão** \- Operações estruturantes (em conjunto com edição)

### 1.3 Cadastro de Usuários

Gerenciamento de contas de acesso ao sistema.

**Funcionalidades:**

* Criação de usuários com identificação nominal  
* Atribuição de perfil de permissões

### 1.4 Cadastro de Operadores

Usuários com escopo limitado ao aplicativo móvel/de campo.

**Funcionalidades:**

* Gerenciamento exclusivo via aplicativo operacional  
* Registros de consumo de produtos/serviços em itinerários

### 1.5 Naturezas de Serviço e Objetos

Definição da taxonomia de serviços e seus alvos de tratamento.

**Natureza de Serviço:**

* Classificação do tipo de serviço (ex.: dedetização, higienização)

**Objetos:**

* Alvos específicos do tratamento (ex.: baratas e roedores para dedetização; caixa d'água e cisternas para higienização)  
* Tempo de garantia pós-serviço (configurável por objeto)  
* Percentual de comissão (parametrizável para controle de distribuição de comissões)

### 1.6 Garantias

Catálogo de períodos de garantia reutilizáveis, associados aos objetos de serviço.

### 1.7 Produtos Químicos

Cadastro de insumos químicos utilizados na execução de serviços regulamentados pelo INEA.

**Propósito:**

* Alimentar geração de Ordens de Serviço  
* Conformidade com relatórios RAAE (Relatório de Acompanhamento das Atividades de Empresas)

### 1.8 Documentos Personalizáveis

Templates dinâmicos de documentos por natureza de serviço e CNPJ.

**Funcionalidades:**

* Estrutura modular com variáveis predefinidas  
* Exemplo de variável: `[[NOME_CLIENTE]]`  
* Customização isolada por combinação CNPJ \+ Natureza de Serviço

---

## 2\. Setor Comercial

Gerenciamento de clientes, propostas e itinerários operacionais.

### 2.1 Gerenciamento de Clientes

CRUD completo com histórico integrado.

**Funcionalidades:**

* Cadastro, leitura, edição e exclusão de clientes  
* Histórico completo de serviços executados  
* Operações sobre serviços:  
  * Emissão, edição e exclusão de serviços  
  * Geração de novos serviços baseados em padrões anteriores  
* Atribuição de vendedor responsável (para fins de comissão e relatórios)

### 2.2 Renovação de Serviços

Visualização prospectiva de vencimentos de garantia.

**Funcionalidades:**

* Agenda mensal de términos de garantias  
* Facilita renovação proativa de serviços

### 2.3 Itinerário de Serviços

Núcleo operacional do sistema com visualização diária de serviços agendados.

**Funcionalidades:**

* Navegação por data (visualização diária)  
* Filtros avançados:  
  * Natureza de serviço  
  * Objeto de serviço  
  * Vendedor responsável  
  * Operador de campo  
  * Bairro/localização  
  * Horário  
  * Identificadores: pedido, itinerário, ordem de serviço  
* Visualização de status de execução  
* Geração de documentação pertinente

---

## 3\. Setor Financeiro

Controle de fluxo de caixa operacional.

### 3.1 Contas a Receber

Posição de crédito a receber de clientes.

**Critério:**

* Inclui apenas serviços com status financeiro "Pronto para Faturar"  
* Visualização mensal agrupada

### 3.2 Contas a Pagar

Posição de obrigações da empresa.

**Composição:**

* Obrigações operacionais gerais  
* Comissões a vendedores (segregadas em visualização, mas consolidadas na posição total)

---

## 4\. Setor de Relatórios

Conformidade regulatória e análise de desempenho.

### 4.1 Relatórios RAAE

Conformidade obrigatória com o INEA.

**Relatórios:**

* **RAAE** \- Relatório de Acompanhamento das Atividades de Empresas 

**Funcionalidades:**

* Geração mensal automática  
* Conformidade garantida pela arquitetura de dados (naturezas, objetos e produtos pré-formatados)  
* Validação de regras impostas pelo INEA integrada ao sistema

### 4.2 Balanço de Comissões

Resumo de distribuição de comissões com perspectiva de custo.

**Funcionalidades:**

* Visualização mensal segregada por vendedor  
* Filtro por natureza de serviço  
* Reconciliação com Contas a Pagar (aparece como custo registrado, mantendo balanço financeiro real)

### 4.3 Relatório de Vendas

Análise de desempenho comercial.

**Funcionalidades:**

* Período mensal  
* Filtros: vendedor, natureza de serviço, objeto de serviço

### 4.4 Relatório de Divergências

Auditoria de inconsistências de dados.

**Cenários Detectados:**

* Propostas aprovadas sem serviços agendados  
* Propostas com valor zerado (mesmo que intencionais)  
* Propostas faturadas sem serviços agendados ou executados

---

## 5\. Fluxo de Dados Principal

Serviço Criado (Comercial)

    ↓

Agendado em Itinerário (Comercial)

    ↓

Executado no Campo (Operacional)

    ↓

Status: Pronto para Faturar

    ↓

Faturamento (Financeiro)

    ↓

Relatórios & Análise (Relatórios)

---

## 6\. Classificação dos Atributos de Qualidade

**Naturezas de Serviço, Objetos de Serviço e Relatórios RAAE**  
**Atributo:** Conformidade Legal **| Categoria:** Transversal  
**Descrição:** Garante que o sistema opere dentro das normas operacionais do INEA. Influencia a estrutura de dados para validação obrigatória.

**Isolamento por Perfil CNPJ**  
**Atributo:** Isolamento (multitenancy) **| Categoria:** Transversal  
**Descrição:** Garante que dados de um perfil CNPJ não vazem para outro, ou seja, documentação personalizada por perfil. Influencia também o design do banco de dados (filtros por tenant).

**Templates Dinâmicos**  
**Atributo:** Configurabilidade | **Categoria:** Estrutural  
**Descrição:** Permite que usuários modifiquem documentos sem alterar código. Influencia a adoção de um motor de renderização de templates.

**Rastreabilidade**  
**Atributo:** Auditabilidade | **Categoria:** Transversal  
**Descrição:** Permite reconstruir o histórico de serviços, comissões e relatórios de performance. Influencia a criação de logs e tabelas de auditoria.  
**Segregação de Responsabilidades**  
**Atributo:** Segurança (autorização) | **Categoria:** Transversal  
**Descrição:** Diferença clara entre usuários (acesso web) e operadores (acesso via aplicativo). Garante que operadores de campo não acessem dados comerciais sensíveis. Além da hierarquia de funções entre perfis dos usuários web.  
Influencia o controle de acesso granular baseado em papéis.

**Execução de Itinerários**  
**Atributo:** Confiabilidade | **Categoria:** Operacional  
**Descrição:** Essencial na execução de itinerários de campo para garantir a integridade dos dados coletados pelos operadores. Influência de estratégias de persistência offline/sincronização.

---

## 7\. Estilo Arquitetural

**Estilo Adotado:** Monólito Modular  
Dado que o projeto será desenvolvido por um único desenvolvedor, o Monólito Modular oferece o menor custo de implantação e complexidade operacional.   
Ele permite a separação clara de domínios (Gerenciamento, Comercial, Operacional, Financeiro e Relatórios) sem os riscos de rede e latência de microserviços.

### 7.1 Design Arquitetural e Modularidade

O sistema utiliza Particionamento por Domínio (Domain Partitioning) no nível superior:

**Escopo e Granularidade:** O sistema é dividido em 5 módulos (quanta lógico) que compartilham uma base de dados única, mas com isolamento lógico via ID dos perfis CNPJ.

**Acoplamento:** Os módulos comunicam-se via chamadas de método síncronas (dentro do mesmo processo), mas seguindo o fluxo de dados definido: Comercial → Operacional → Financeiro → Relatórios.

**Gerenciamento de Dados:** Cada entidade possui um "dono" claro (ex: Financeiro detém a regra de "Pronto para Faturar"), reduzindo conflitos de integridade.  
