# Roteiro de Pitch e Apresentação em Vídeo: Control Service ERP
**Duração Alvo:** ~4 a 5 minutos (Máximo de 5 minutos)
**Objetivo:** Demonstrar de forma cronológica o contexto, as decisões arquiteturais (ADRs), o estilo adotado e o Modelo C4 do projeto, garantindo conformidade total com todos os critérios de avaliação técnica da disciplina.

---

## 🎬 Visão Geral do Fluxo do Vídeo (Cronograma)
*   **0:00 - 1:00 (1 min):** Introdução, Contexto, Drivers de Negócio e Atributos de Qualidade (Trade-offs).
*   **1:00 - 2:45 (1 min 45s):** As Tomadas de Decisões e a Jornada das ADRs (ADR 01 a 06).
*   **2:45 - 4:15 (1 min 30s):** Navegação e Consistência dos Diagramas C4 (Contexto, Contêiner e Componente).
*   **4:15 - 5:00 (45s):** Conclusão: Arquitetura Limpa, Produtividade de Times e Práticas Sustentáveis.

---

## 📝 Roteiro Detalhado: Cena a Cena

### 🎬 Parte 1: Introdução, Contexto, Drivers de Negócio e Atributos de Qualidade
⏱️ **Duração:** 0:00 - 1:00  
📺 **Visual na Tela:** Mostrar o título do [documento-arquitetural.md](file:///c:/Projetos/.NET/ControlService/docs/documento-arquitetural.md) e rolar até as seções **1. Contexto** e **2. Atributos de Qualidade**.

🎙️ **Fala do Apresentador (Você):**
> *"Olá! Me chamo [Seu Nome] e hoje vou apresentar a arquitetura do **Control Service ERP**, um sistema de gestão integrada projetado especificamente para empresas prestadoras de serviços de saneamento ambiental, como dedetização e higienização de reservatórios.
>
> Para entender o design do sistema, precisamos olhar para os nossos **Drivers de Negócio**. O primeiro é a **Conformidade Regulatória Rigorosa**, pois o sistema precisa gerar automaticamente o relatório RAAE, exigido de forma obrigatória pelo INEA. O segundo é a **Operação Multiestrutural**, pois gerenciaríamos múltiplos CNPJs com regras fiscais distintas em uma única estrutura unificada. E o terceiro, nossa restrição crítica: a operação deve rodar perfeitamente em campo, onde frequentemente **não há sinal de internet**.
>
> Diante disso, priorizamos atributos de qualidade vitais: a **Conformidade Legal** e a **Confiabilidade** de dados operacionais contra perdas, equilibrando-os com a **Manutenibilidade** estrutural, já que o time de desenvolvimento sou apenas eu.
>
> O maior trade-off que enfrentei foi o **Isolamento de Tenants vs. Manutenibilidade de Código**. Para isolar os dados dos múltiplos CNPJs de forma barata e simples, optei pelo isolamento lógico em um único banco de dados. O impacto negativo é o risco de vazamento de dados caso um filtro seja esquecido na query, risco este que mitigamos na codificação usando Global Query Filters na camada de ORM."*

---

### 🎬 Parte 2: A Jornada Cronológica das Tomadas de Decisão (ADRs)
⏱️ **Duração:** 1:00 - 2:45  
📺 **Visual na Tela:** Mostrar a pasta de [ADRs](file:///c:/Projetos/.NET/ControlService/docs/ADRs) e ir abrindo ou passando rapidamente por cada ADR conforme fala delas.

🎙️ **Fala do Apresentador (Você):**
> *"Para estruturar cada detalhe desse ecossistema, tomamos 6 decisões arquiteturais formais, registradas cronologicamente em nossas ADRs:
>
> 1. **ADR 01 - Monólito Modular:** Avaliamos microsserviços, mas os descartamos sumariamente pela complexidade de rede e observabilidade para um único desenvolvedor. O Monólito Modular particionado por domínio nos garante a simplicidade de implantação com forte separação lógica e controle rigoroso de dependências com testes do NetArchTest.
> 2. **ADR 02 - Multitenancy Lógico por CNPJ:** Escolhemos adicionar um ID de Tenant e filtros globais em banco único. Descartamos a abordagem de 'banco por cliente' (Isolamento Físico), pois traria um custo operacional insustentável de migrations.
> 3. **ADR 03 - Quantum Frontend Separado com Sincronização Offline:** Para resolver a restrição de rede no campo, separamos o Frontend administrativo em um Web App do aplicativo móvel do operador. O app móvel foi projetado como Offline-First: ele sincroniza o roteiro do Comercial ativamente, opera 100% offline salvando os dados em SQLite local e realiza a sincronização ativa de retorno com o backend para consolidar as baixas de serviços.
> 4. **ADR 04 - Motor Interno de Templates:** Embutimos um parser de strings no servidor para renderizar contratos dinamicamente. Isso evita deploys constantes a cada mudança jurídica de layout ou cláusula, repassando o controle para os gerentes através de templates salvos em banco.
> 5. **ADR 05 - Terceirização da Emissão de Notas Fiscais:** Decidimos delegar a integração fiscal para uma API SaaS parceira. Construir um emissor próprio de notas municipais direto com prefeituras atrasaria o time-to-market e geraria uma manutenção eterna e caótica.
> 6. **ADR 06 - PostgreSQL como Banco de Dados:** Adotamos o PostgreSQL no servidor central devido a ser open-source, gratuito e possuir excelente performance com EF Core 10 via Npgsql, reduzindo custos de licenciamento e contornando os limites de 10GB da versão gratuita do SQL Server Express."*

---

### 🎬 Parte 3: Navegação Prática nos Diagramas C4
⏱️ **Duração:** 2:45 - 4:15  
📺 **Visual na Tela:** Abrir o arquivo de especificação do C4 [workspace.dsl](file:///c:/Projetos/.NET/ControlService/docs/C4%20Model/workspace.dsl) ou os diagramas renderizados (se aplicável), e ir apontando para cada nível (Contexto, Contêiner e Componente).

🎙️ **Fala do Apresentador (Você):**
> *"Para garantir que essa arquitetura seja compreensível para todos os stakeholders, adotamos o **Modelo C4**.
>
> *   **Nível 1 - Contexto do Sistema:** Aqui vemos o ecossistema macro. Temos duas personas centrais: o **Administrador (Backoffice)**, que opera a retaguarda, e o **Operador de Campo**, que executa os serviços. O nosso **Control Service ERP** interage externamente apenas com a **API de Notas Fiscais SaaS**, que centraliza a comunicação com os órgãos municipais e fiscais.
> *   **Nível 2 - Contêineres:** Detalhando o sistema, mostramos a divisão de quanta arquitetural. O Administrador interage via protocolo HTTPS com a **SPA Admin Web (React/TypeScript)**. O operador utiliza o **App Mobile (React Native/Expo)** que possui um banco local **SQLite** para resiliência offline. Ambos os Frontends consomem a nossa **Backend Monolítico API (.NET 10)**, que realiza leitura e escrita em nosso **Banco de Dados Unificado (PostgreSQL)**, particionado logicamente pelo tenant ID.
> *   **Nível 3 - Componentes:** Entrando no contêiner da nossa API Backend, demonstramos a alta coesão e modularidade do sistema. Ele é dividido em fatias verticais orientadas por domínios de negócio específicos: o **Módulo de Gerenciamento** (que cuida de tenants e permissões), o **Módulo Comercial** (que gerencia contratos e gera o roteiro diário), o **Módulo Operacional** (que lida com o recebimento de baixas offline e produtos químicos do INEA), o **Módulo Financeiro** (que processa comissões e dispara a API fiscal) e o **Módulo de Relatórios** (que gera os PDFs e o RAAE). A consistência de dados flui de forma unidirecional e livre de ciclos."*

---

### 🎬 Parte 4: Conclusão, Arquitetura Limpa e Práticas Sustentáveis
⏱️ **Duração:** 4:15 - 5:00  
📺 **Visual na Tela:** Mostrar o final do [documento-arquitetural.md](file:///c:/Projetos/.NET/ControlService/docs/documento-arquitetural.md) (Seção 4: Preocupações de Design vs Arquitetura Limpa) ou a tela de código do projeto real.

🎙️ **Fala do Apresentador (Você):**
> *"Por fim, gostaria de destacar como as escolhas de design e a **Arquitetura Limpa** impactam positivamente a nossa produtividade e sustentabilidade a longo prazo.
>
> Ao isolar o núcleo das regras de negócio — especialmente as regras complexas de conformidade de saneamento e INEA — do acesso direto a frameworks e drivers de infraestrutura (como Entity Framework ou serviços externos), conseguimos criar uma estrutura altamente testável. Conseguimos mockar a API de Notas Fiscais ou a persistência física com facilidade extrema, reduzindo drasticamente o esforço manual de testes e garantindo que refatorações futuras não quebrem o core do faturamento e agendamento.
>
> Isso dá a um único desenvolvedor a autonomia de evoluir o sistema de forma segura, com baixo acoplamento e excelente manutenibilidade.
>
> Agradeço a atenção de todos e estou aberto a dúvidas sobre a arquitetura do Control Service ERP!"*

---

## 💡 Dicas de Ouro para Gravação do Vídeo:
1.  **Compartilhamento de Tela:** Deixe a tela cheia no VS Code mostrando a documentação em Markdown ou abra o PDF gerado. Mantenha os diagramas C4 em abas prontas no seu navegador para alternar rapidamente e sem engasgos na Parte 3.
2.  **Ritmo:** Fale com firmeza e sem pressa, mas mantenha a dinâmica para não estourar o tempo limite de **5 minutos**. Se necessário, faça um ensaio rápido de voz usando o roteiro acima para ajustar seu tempo.
3.  **Qualidade de Áudio:** Grave em um local silencioso. Um áudio limpo passa maior autoridade na apresentação técnica para a banca avaliadora.
