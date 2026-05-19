### Requisitos da Tarefa

O projeto deve atender **obrigatoriamente** aos seguintes requisitos:

1\. **Contexto e Drivers**

* Descrever claramente o contexto do sistema  
* Identificar os principais drivers de negócio  
* Explicitar restrições técnicas, organizacionais ou regulatórias

2\. **Atributos de Qualidade**

* Identificar e classificar os atributos de qualidade do sistema (Operacionais, Estruturais e Transversais)  
* Justificar como esses atributos influenciam as decisões arquiteturais  
* Apontar possíveis conflitos e trade-offs entre atributos

3\. **Decisões Arquiteturais**

* Registrar formalmente **no mínimo três ADRs (Architectural Decision Records)** relevantes.  
* Cada ADR deve conter: Contexto, Decisão, Alternativas consideradas e Trade-offs, riscos e impactos.

4\. **Estilos e Padrões Arquiteturais**

* Avaliar e justificar a adoção de um ou mais estilos arquiteturais, como: Monólito modular, Microsserviços, Arquitetura baseada em eventos, Arquitetura em pipeline, Microkernel ou Arquitetura baseada em serviços.  
* Explicar por que alternativas foram descartadas

5\. **Design Arquitetural**

* Definir claramente: Escopo arquitetural, Granularidade dos componentes, Estratégias de modularidade e Gerenciamento de acoplamento e dependências.  
* Aplicar princípios de arquitetura limpa e camadas quando pertinente

6\. **Comunicação da Arquitetura \- Modelo C4**

É obrigatório apresentar os diagramas do **Modelo C4**, contemplando:

* Diagrama de Contexto  
* Diagrama de Contêiner  
* Diagrama de Componentes (ao menos para um contêiner relevante)

Os diagramas devem ser claros, consistentes entre si e coerentes com as decisões documentadas.

7\. **APIs e Sistemas Distribuídos (quando aplicável)**

Caso o sistema seja distribuído:

* Definir contratos de serviço (APIs)  
* Explicar estratégias de versionamento e governança  
* Aplicar conceitos de consumer-driven contracts  
* Descrever abordagem para consistência eventual (Sagas, transações compensatórias)  
* Explicitar estratégia de propriedade e acesso aos dados

---

### Formato de Entrega

Você deverá entregar **um único relatório técnico em formato PDF**, com nome no seguinte padrão: **nome\_sobrenome\_dr2\_fundamentos\_design\_sistemas\_c4.pdf**

O relatório deve conter, no mínimo:

* Contexto do sistema e drivers de negócio  
* Identificação e análise dos atributos de qualidade  
* Decisões arquiteturais documentadas (ADRs)  
* Justificativa dos estilos e padrões escolhidos  
* Diagramas C4 (Contexto, Contêiner e Componentes)  
* Discussão sobre trade-offs, riscos e evolução futura  
* Considerações sobre impacto da arquitetura na efetividade dos times

O relatório deve ser técnico, claro, objetivo e bem estruturado.

Como parte da entrega, o aluno deve incluir no PDF o link para um vídeo de apresentação hospedado no Youtube, não listado ou público, com permissão de acesso público (qualquer pessoa com o link pode assistir). Duração máxima: 5 minutos. Durante o vídeo, o aluno deve exibir na tela os próprios artefatos entregues no PDF e percorrer, com suas palavras, os seguintes aspectos técnicos:

* **O sistema escolhido e seus drivers de negócio:** explique brevemente o contexto e por que esses drivers condicionaram as decisões que você tomou.  
* **Os atributos de qualidade priorizados:** mencione ao menos um conflito de trade-off real que você enfrentou e como o resolveu.  
* **Um ADR de sua escolha:** apresente o raciocínio por trás da decisão: quais alternativas foram consideradas e por que foram descartadas.  
* **Os diagramas C4:** navegue pelos três níveis (Contexto, Contêiner e Componentes), explicando o que cada elemento representa e como os diagramas são consistentes entre si.  
* **O estilo arquitetural adotado:** justifique a escolha com base no contexto do sistema, não apenas na definição teórica do estilo.

### Avaliação do Aluno

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno definiu o papel do Arquiteto de Software e justificou a importância da arquitetura no sistema proposto, demonstrando como as decisões arquiteturais foram diretamente motivadas pelos drivers de negócio e pelos requisitos de escalabilidade identificados?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno identificou e classificou os Atributos de Qualidade do sistema nas categorias Operacionais, Estruturais e Transversais, explicando como cada atributo influenciou concretamente as decisões de design arquitetural documentadas?

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno registrou formalmente as decisões arquiteturais por meio de ADRs, estruturando cada registro com contexto, decisão tomada, alternativas consideradas e os impactos esperados sobre o sistema?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno avaliou os trade-offs e riscos das decisões arquiteturais tomadas, documentando as justificativas de cada escolha e os impactos técnicos e organizacionais decorrentes sobre o sistema proposto?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno distinguiu as preocupações de design das preocupações de arquitetura no sistema proposto, aplicando os valores da arquitetura limpa para reduzir o esforço de manutenção e justificando como essa separação aumentou a produtividade esperada dos times?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno detectou as características arquiteturais relevantes do sistema a partir dos requisitos funcionais e dos drivers de negócio, distinguindo as características explícitas das implícitas e justificando sua relevância para o contexto proposto?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno definiu métricas objetivas para as características arquiteturais prioritárias do sistema, especificando os indicadores e os limiares que permitiram avaliar se cada característica foi atendida na arquitetura proposta?

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno definiu o escopo e a granularidade arquitetural do sistema, justificando o nível de decomposição adotado para cada componente com base nos requisitos de autonomia, coesão e manutenibilidade esperados?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno comunicou a arquitetura do sistema por meio dos diagramas de Contexto, Contêiner e Componente do Modelo C4, garantindo consistência entre os níveis e coerência com as decisões arquiteturais documentadas nos ADRs?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno aplicou a arquitetura em camadas ao sistema proposto, definindo a topologia de cada camada e demonstrando como as regras de dependência foram respeitadas para garantir o desacoplamento entre as responsabilidades do sistema?	

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno distinguiu os princípios de Modularidade e Granularidade e os aplicou no design do sistema, demonstrando como a decomposição adotada reduziu a complexidade acidental e facilitou a evolução independente dos módulos?

1\. Arquitetar software considerando modelagem de software e tomada de decisão, alinhando os drivers de negócio, avaliando atributos arquiteturais, aplicando princípios de arquitetura limpa e comunicando decisões.

O aluno tomou decisões arquiteturais relevantes, registrou-as formalmente e identificou os antipadrões que foram deliberadamente evitados, justificando como cada decisão contribuiu para a qualidade estrutural da arquitetura proposta?	

2\. Liderar times efetivos segundo preceitos da Arquitetura de Software, aumentando produtividade, negociando requisitos e gerenciando acoplamento e dependências.

O aluno explicou como as decisões arquiteturais tomadas contribuíram para aumentar a efetividade dos times de engenharia, especificando os mecanismos — como redução de acoplamento, autonomia de módulos ou clareza de contratos — que permitiram times trabalharem de forma mais independente e produtiva?	

2\. Liderar times efetivos segundo preceitos da Arquitetura de Software, aumentando produtividade, negociando requisitos e gerenciando acoplamento e dependências.

O aluno demonstrou atuação como líder técnico no projeto, evidenciando como negociou requisitos com stakeholders, gerenciou expectativas técnicas e estruturou a comunicação da arquitetura para públicos com diferentes níveis de conhecimento?	

2\. Liderar times efetivos segundo preceitos da Arquitetura de Software, aumentando produtividade, negociando requisitos e gerenciando acoplamento e dependências.

O aluno gerenciou o acoplamento e as dependências entre os componentes do sistema, aplicando os princípios de coesão e dependência acíclica e demonstrando como a estrutura resultante evitou ciclos de dependência e isolou pontos de variação?	

2\. Liderar times efetivos segundo preceitos da Arquitetura de Software, aumentando produtividade, negociando requisitos e gerenciando acoplamento e dependências.

O aluno relacionou as preocupações de Arquitetura de Software às práticas de Engenharia de Software no contexto do sistema proposto, explicando como as decisões arquiteturais criaram as condições para que práticas de engenharia — como testes, refatoração e integração contínua — fossem sustentáveis ao longo do tempo?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno avaliou os estilos arquiteturais candidatos e justificou a escolha entre particionamento monolítico e distribuído, fundamentando a decisão nos atributos de qualidade prioritários, nos drivers de negócio e nas capacidades operacionais da organização?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura em Monolito Modular ao sistema proposto, demonstrando como os módulos foram delimitados por domínio, como as dependências internas foram controladas e por que esse estilo foi mais adequado do que alternativas distribuídas para o contexto descrito?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura em Pipeline ao sistema proposto, especificando os filtros, os canais de comunicação entre etapas e justificando como esse estilo atendeu aos requisitos de processamento sequencial e transformação de dados identificados?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura em Microkernel ao sistema proposto, definindo o núcleo central de funcionalidades e os plug-ins de extensão, e justificando como esse estilo proporcionou a flexibilidade e a extensibilidade exigidas pelo contexto do sistema?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura baseada em Serviços ao sistema proposto, delimitando os serviços por capacidade de negócio, definindo seus contratos de comunicação e justificando como esse estilo equilibrou autonomia e reutilização em relação a alternativas de maior ou menor granularidade?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura orientada a eventos ao sistema proposto, especificando os produtores, consumidores e canais de eventos, e justificando como esse estilo reduziu o acoplamento temporal entre os componentes e habilitou a escalabilidade assíncrona requerida?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura baseada em espaço ao sistema proposto, descrevendo como o espaço de tuplas ou grid distribuído foi utilizado para compartilhar estado entre componentes e justificando como esse estilo atendeu aos requisitos de escalabilidade elástica do contexto?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura baseada em orquestração ao sistema proposto, especificando o papel do orquestrador na coordenação dos serviços, os fluxos gerenciados centralmente e justificando por que a orquestração foi preferida à coreografia no contexto descrito?	

3\. Aplicar estilos e padrões para Arquiteturas Distribuídas, avaliando arquiteturas monolíticas e distribuídas, adotando eventos e microsserviços conforme requisitos de escalabilidade, acoplamento e evolução.

O aluno aplicou a arquitetura de microsserviços ao sistema proposto, delimitando os serviços por bounded context, definindo suas estratégias de comunicação e implantação independente, e justificando como o estilo atendeu aos requisitos de escalabilidade e evolução autônoma dos times?	

4\. Projetar sistemas e contratos de serviço, elaborando e gerenciando APIs com consumer-driven contracts, aplicando Sagas para consistência eventual e gerenciando propriedade e acesso a dados.

O aluno considerou os aspectos determinantes — incluindo características de negócio, capacidade operacional do time, atributos de qualidade prioritários e custo de implantação — na justificativa do padrão arquitetural selecionado, demonstrando que a escolha resultou de uma análise comparativa e não de uma preferência tecnológica isolada?	

4\. Projetar sistemas e contratos de serviço, elaborando e gerenciando APIs com consumer-driven contracts, aplicando Sagas para consistência eventual e gerenciando propriedade e acesso a dados.

O aluno elaborou os contratos de serviço da solução distribuída aplicando práticas de consumer-driven contracts, especificando como os contratos foram definidos a partir das necessidades dos consumidores, versionados e validados para garantir fidelidade e compatibilidade entre produtores e consumidores?	

4\. Projetar sistemas e contratos de serviço, elaborando e gerenciando APIs com consumer-driven contracts, aplicando Sagas para consistência eventual e gerenciando propriedade e acesso a dados.

O aluno aplicou padrões de Sagas e transações compensatórias para garantir a consistência eventual dos dados no sistema distribuído proposto, especificando os passos da saga, as condições de falha que acionaram cada transação compensatória e os mecanismos de rastreabilidade do estado da saga em execução?	

4\. Projetar sistemas e contratos de serviço, elaborando e gerenciando APIs com consumer-driven contracts, aplicando Sagas para consistência eventual e gerenciando propriedade e acesso a dados.

O aluno definiu a propriedade dos dados entre os serviços do sistema distribuído, especificando qual serviço detém a fonte de verdade de cada entidade e projetando os padrões de leitura para que consumidores externos acessassem os dados sem violar os limites de responsabilidade dos serviços proprietários?	

