# Roteiro – Apresentação Arquitetural: Control Service ERP
**Duração estimada:** ~5 minutos  
**Ritmo de fala:** ~140 palavras/minuto → ~700 palavras no total  
**Fio condutor:** cada bloco *gera* o próximo, como uma cadeia de decisões

---

## 🎬 BLOCO 1 — O Sistema e Seus Drivers de Negócio
**⏱ Tempo estimado: 50 segundos**

> O sistema que vou apresentar é o **Control Service ERP**: uma plataforma de gestão para empresas prestadoras de serviços especializados — dedetização, higienização de reservatórios e impermeabilização. Pode parecer um domínio simples, mas esconde três forças que condicionaram cada decisão arquitetural que tomei.
>
> **Primeiro driver: conformidade regulatória.** A empresa é auditada pelo INEA — o Instituto Estadual do Ambiente — e precisa gerar mensalmente o relatório RAAE. Qualquer inconsistência nos dados de insumos químicos e execução de serviços é um risco jurídico real.
>
> **Segundo driver: múltiplos CNPJs.** A empresa opera como uma entidade única de gestão, mas juridicamente se fragmenta em dois CNPJs por razões fiscais e regulatórias. Isso parece um detalhe administrativo, mas vou mostrar que ele dita o design do banco de dados, da autenticação e dos documentos gerados.
>
> **Terceiro driver: equipe de um.** Sou o único desenvolvedor. Isso não é uma limitação menor — é o driver mais determinístico de todas as decisões que vou apresentar. Qualquer arquitetura que eu adote precisa ser operável por uma pessoa.

---

## 🎬 BLOCO 2 — Atributos de Qualidade e o Trade-off Real
**⏱ Tempo estimado: 65 segundos**

> Com esses drivers em mente, priorizei três atributos de qualidade. **Confiabilidade** — os dados coletados no campo nunca podem se perder. **Conformidade Legal** — as regras do INEA precisam estar embutidas na estrutura de dados, não ser validadas só na UI. E **Manutenibilidade** — precisa ser operável por uma pessoa.
>
> Mas esses atributos entram em conflito. Vou falar do trade-off mais honesto que enfrentei.
>
> A existência dos múltiplos perfis CNPJs exige isolamento rigoroso de dados: contratos, alíquotas tributárias e relatórios RAAE pertencem a um CNPJ específico e não podem vazar para outro. A solução mais segura seria um banco de dados separado por CNPJ. Isso me daria isolamento físico total.
>
> O problema: eu teria que gerenciar múltiplos pipelines de migração e múltiplos backups — e relatórios consolidados da empresa inteira se tornariam consultas entre bancos de dados distintos. Para um desenvolvedor solo, isso é inviável.
>
> A resolução foi o **isolamento lógico**: um banco de dados único, com uma chave `tenant_id` em todo documento que exige segregação. Assim ganho uma operação simples e documentos consolidados. O risco é claro: se alguma consulta ao banco de dados esquecer de perguntar "para qual CNPJ estou buscando?", os dados de um CNPJ apareceriam misturados com os do outro. Para eliminar esse risco humano, configurei um filtro global e obrigatório — toda consulta já sai com esse critério aplicado automaticamente, sem depender da memória do desenvolvedor.
>
> Esse é o trade-off real: segurança de infraestrutura trocada por disciplina de código. E aceitei com segurança.

---

## 🎬 BLOCO 3 — ADR: A Decisão Mais Difícil (Offline-First)
**⏱ Tempo estimado: 80 segundos**

> Agora quero apresentar a decisão arquitetural que considero a mais impactante no design do sistema: a estratégia do aplicativo móvel para os operadores de campo.
>
> **Contexto:** Os técnicos executam serviços em subsolos de condomínios, reservatórios industriais, áreas rurais — locais onde o sinal de celular é muito instável. E é exatamente nesses locais que eles precisam registrar os insumos químicos utilizados, fotografar o ambiente, e dar baixa no serviço.
>
> Se a execução do trabalho depender de internet, o fluxo de trabalho trava.
>
> A decisão foi projetar um aplicativo sob arquitetura **Offline-First** com sincronização explícita e bidirecional, em três fases claras:
>
> — Na **Fase 1**, com internet disponível no início do turno, o operador sincroniza e baixa o roteiro do dia. A partir daí, não precisa mais de rede.
>
> — Na **Fase 2**, todo o trabalho acontece offline: insumos, fotos, não conformidades, status de cada serviço — tudo persistido localmente em SQLite no dispositivo.
>
> — Na **Fase 3**, ao retornar a uma área com sinal, o operador sincroniza de volta. O backend processa as baixas, gera as Ordens de Serviço e os devidos relatórios.
>
> O custo dessa decisão é real: lógica de sincronização bidirecional, resolução de conflitos e dois ciclos de deploy independentes. Mas a alternativa seria travar o fluxo de trabalho DIÁRIO dos operadores por ausência de sinal, o que seria impraticável.

---

## 🎬 BLOCO 4 — Os Diagramas C4
**⏱ Tempo estimado: 75 segundos**

> Vamos visualizar tudo isso nos três níveis do modelo C4, do mais amplo para o mais detalhado.
>
> **Nível 1 — Contexto.** [*exibir diagrama de Contexto*] Aqui vemos o sistema como uma caixa preta. Os atores externos são: os usuários administrativos do escritório, os operadores de campo e a API externa de notas fiscais, que é quem faz essa integração por mim. No centro, o Control Service ERP.
>
> **Nível 2 — Contêiner.** [*exibir diagrama de Contêiner*] Agora abrimos o sistema. Temos três contêineres independentes: a **SPA Web** para o escritório, o **App Móvel** offline-first para o campo, e a **API Backend** — um Monólito Modular em .NET. Os dois clientes falam com a API via REST. O banco de dados é um único PostgreSQL, compartilhado pela API com isolamento lógico abordado anteriormente, por `tenant_id`. Perceba que o App Móvel também tem seu próprio banco local (SQLite) — isso é o offline-first materializado no diagrama.
>
> **Nível 3 — Componentes.** [*exibir diagrama de Componentes da API*] Dentro da API, vemos os cinco módulos de domínio: Gerenciamento, Comercial, Operacional, Financeiro e Relatórios. Cada módulo é uma fatia vertical com seus próprios casos de uso e acesso à persistência. A comunicação entre eles é via interfaces — nunca via referências concretas. Há dois adaptadores de infraestrutura conectados para fora: o gateway de notas fiscais e o motor de templates de documentos. Ambos implementam interfaces do domínio — os módulos internos nunca sabem qual SaaS ou qual biblioteca de PDF está em uso.
>
> Os três níveis são consistentes: o que aparece como "API Backend" no Nível 2 é exatamente o que está detalhado como módulos no Nível 3. Cada elemento tem um único dono e uma única responsabilidade.

---

## 🎬 BLOCO 5 — O Estilo Arquitetural: Por Que o Monólito Modular
**⏱ Tempo estimado: 50 segundos**

> Com todo esse contexto construído, a justificativa do estilo arquitetural fica evidente
>
> Microsserviços foram considerados e descartados. Não por dogma, mas por análise de custo real: o faturamento envolve dados do Comercial, do Operacional e do Financeiro numa mesma transação. Distribuir esses módulos em serviços separados exigiria transações compensatórias via padrão Saga, observabilidade distribuída e múltiplos pipelines de deploy, ou seja, uma sobrecarga operacional que uma equipe de um não consegue sustentar sem comprometer o produto.
>
> O **Monólito Modular** me dá o que preciso: garantias ACID entre módulos, um único artefato para implantar, e módulos particionados por domínio, o que preserva a coesão e evita o conceito de Big Ball of Mud.
>
> E o mais importante: o particionamento interno por domínio é o que tornará possível, se a empresa crescer, extrair um módulo, como o Operacional por exemplo, para um serviço independente no futuro. Começo com o Monólito Modular como ponto de partida estratégico, não como limitação.

---

## 📋 Resumo Cronológico (para revisão do apresentador)

| # | Bloco | Tempo | Conecta para... |
|---|---|---|---|
| 1 | Sistema e drivers | 50s | Os drivers explicam *por que* existem conflitos de atributo |
| 2 | Atributos + trade-off (Multitenancy) | 65s | O trade-off resolvido *leva à necessidade* do estilo arquitetural |
| 3 | ADR-003 (Offline-First) | 80s | A decisão mais diferenciada — o campo como fronteira arquitetural |
| 4 | Diagramas C4 (3 níveis) | 75s | Visualiza tudo que foi descrito nos blocos anteriores |
| 5 | Estilo Arquitetural (Monólito Modular) | 50s | Culminância: todas as forças convergem para essa escolha |
| **Total** | | **~320s (~5min20s)** | |

> **Nota de ritmo:** Fala em ritmo ligeiramente acelerado nos Blocos 1 e 2 (contexto) para preservar tempo nos Blocos 3 e 4, que são os mais densos e técnicos. O Bloco 5 pode ser levemente desacelerado — é a conclusão e deve soar como uma síntese natural, não como uma informação nova.
