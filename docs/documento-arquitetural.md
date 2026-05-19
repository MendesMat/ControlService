# Relatório Técnico: Fundamentos de Arquitetura de Software e Design de Sistemas
**Projeto:** Control Service ERP

## 1. Contexto e Drivers de Negócio

### 1.1 Contexto do Sistema
O Control Service ERP é um sistema de gestão integrado desenvolvido para empresas prestadoras de serviços especializados de: dedetização, higienização de reservatórios de água e impermeabilização. O sistema atua como o núcleo operacional e gerencial da empresa, controlando o ciclo completo de vida do serviço: desde o agendamento comercial até a emissão de relatórios regulatórios governamentais, perpassando pela execução móvel em campo e o controle financeiro (sendo a emissão de notas fiscais delegada a um sistema terceiro especializado).

### 1.2 Drivers de Negócio
- **Conformidade Regulatória Rigorosa:** O sistema deve garantir aderência contínua às normas do INEA (Instituto Estadual do Ambiente), gerando de forma confiável e automática relatórios como o RAAE (Relatório de Acompanhamento das Atividades de Empresas), que são uma obrigatoriedade crítica para a empresa.
- **Automação de Processos Sensíveis a Erros:** Redução drástica do esforço manual em tarefas repetitivas e eliminação de inconsistências na geração de contratos, certificados de garantia, consolidação do faturamento, ordens de serviço e relatórios RAAE.
- **Operações Multiestruturais (Múltiplos CNPJs):** Apesar de funcionar como uma única empresa integrada, o sistema operará sob perfis de múltiplos CNPJs. Esses perfis distintos existem exclusivamente por conta de alíquotas tributárias diferentes, certificados A1, e porque os serviços de dedetização e higienização precisam seguir normas operacionais muito específicas do INEA, exigindo tratamentos especiais. O sistema precisa sustentar a emissão correta sob o CNPJ apropriado, mantendo a gestão de toda a empresa centralizada.

### 1.3 Restrições Organizacionais e Técnicas
- **Equipe Enxuta:** O desenvolvimento e a manutenção são realizados por um único desenvolvedor. Essa restrição inviabiliza abordagens de infraestrutura complexas que exigiriam equipes maduras de DevOps. Além disso, direciona a terceirização de integrações caóticas, como a emissão de notas fiscais, visto que construir emissores fiscais internamente aumentaria o tempo de entrega e a complexidade arquitetural em um nível absurdo e impraticável para um escopo individual.
- **Capacitação e Localização dos Usuários de Campo:** Operadores da empresa que executam serviços diretamente no endereço dos clientes frequentemente enfrentam ausência de sinal de internet e demandam fluxos focados em dispositivos móveis que operem de maneira primordialmente offline.

---

## 2. Atributos de Qualidade (As "Ilities")
Com base nos princípios de que a arquitetura deve priorizar um subconjunto enxuto de características explícitas e implícitas, identifiquei os atributos centrais, focando nos trade-offs gerados:

### 2.1 Classificação
- **Operacionais:**
  - *Confiabilidade (Reliability):* Crítico para o módulo operacional. A coleta de dados no campo em redes offline não pode resultar em perda de dados dos serviços concluídos.
- **Estruturais:**
  - *Configurabilidade (Configurability):* Capacidade de formatar contratos, relatórios e certificados dinamicamente em tempo de execução. Adicionalmente, teremos as configurações de alíquotas tributárias isoladas por perfil CNPJ para que a emissão das notas fiscais siga um padrão já estipulado no banco de dados, mas ainda facilmente configurável e personalizável caso haja mudanças fiscais na empresa.
  - *Manutenibilidade (Maintainability):* Atributo vital devido ao tamanho da equipe.
- **Transversais (Cross-cutting):**
  - *Conformidade Legal (Legal Compliance - Explícito):* O core estrutural precisa validar regras do INEA de forma nativa.
  - *Isolamento e Segurança (Multitenancy e Autorização):* Isolamento rigoroso para evitar o cruzamento de dados de CNPJs.
  - *Auditabilidade (Auditability):* Garantia de histórico contra inconsistências.

### 2.2 Conflitos e Trade-offs Arquiteturais
- **Isolamento de Tenant vs. Manutenibilidade de Código:** 
  Para garantir a segregação adequada dos documentos e alíquotas entre os nossos diversos CNPJs, optei pelo uso de um *Isolamento Lógico* num banco de dados unificado. O *trade-off* envolvido nesta decisão é muito explícito: mantenho uma excelente facilidade operacional de infraestrutura (pois administra-se um único banco de dados), mas aumenta-se brutalmente o esforço e a responsabilidade de manutenção no código.
- **Configurabilidade vs. Complexidade Estrutural:** Ao decidir transferir o esforço de atualização de contratos para o usuário através de Templates Dinâmicos, ganho imensa agilidade corporativa, pois não precisarei fazer *deploys* para cada correção. Porém, o *trade-off* é a introdução de uma pesada camada de *parsing* de *strings* no servidor, elevando a complexidade estrutural do código.

---

## 3. Estilos e Padrões Arquiteturais

Conforme estipulado nas minhas decisões arquiteturais (ver pasta `docs/ADRs/`), recusei para o meu projeto estilos altamente distribuídos (como *Service-Based* ou *Microservices*). 
O **Monólito Modular** me permite dividir o sistema em fatias orientadas ao negócio (Domínios de Comercial, Operacional, Relatórios, etc). Isso possibilita alta flexibilidade porque previne o emaranhado letal de código ("*Big Ball of Mud*"), mantendo o meu esforço de implantação no nível mínimo tolerável. O estilo de *Pipeline (Pipes and Filters)* também não utilizei, posto que nosso fluxo de agendamento exige retroalimentação síncrona.

---

## 4. Design Arquitetural, Granularidade e Acoplamento

- **Escopo e Quantum Arquitetural:** Modelei o sistema base (API Backend e Banco de Dados) como *um único Quantum Arquitetural*. Eles serão implantados juntos, escalarão juntos e partilharão da mesma dependência (Alta Conascência de Plataforma/Identidade).
- **Acoplamento e Modularidade:**
  - Adoto estrita topologia limitando as fronteiras dos namespaces no meu código. 
  - O meu Módulo Comercial conhece a interface de agendamento do Módulo Operacional, mas **desconhece** como ele salva no banco ou lida com o offline (Baixa Conascência de Execução). 
- **Preocupações de Design vs Arquitetura Limpa:** 
  - Centralizei as regras puras de negócio (como os cálculos e validações do INEA) no centro ("*Core*"). Detalhes como o *Entity Framework* ou os conectores de notas fiscais terceirizadas são injetados de fora para dentro (*Dependency Inversion Principle*).
  - Essa abstração me salva um tempo enorme para testar o *core* de faturamento, onde apenas simulo (mock) a API de notas e valido a matemática da regra com segurança.

---

## 5. APIs e Sistemas Distribuídos (Considerações Futuras)
Minhas diretrizes atuais ditam uma construção coesa. Contudo, num cenário futuro de crescimento agressivo da organização, que me force a separar o *Módulo Operacional* num contêiner físico independente:

- **Propriedade dos Dados:** O sistema de faturamento perderia a chave primária relacional. O Módulo Operacional passaria a ser o "Dono da Verdade" sobre execução.
- **Consumer-Driven Contracts (Contratos de Serviço):** Mudanças que eu fizesse na API Operacional não poderiam estourar o App Financeiro silenciosamente. Eu adotaria ferramentas como o Pact para auditar compatibilidade entre eles.
- **Consistência Eventual via Sagas:** Eu abandonaria as transações ACID do meu banco monolítico. Se um operador confirmasse uma O.S. no mobile, um evento assíncrono seria publicado. O Módulo Financeiro escutaria e tentaria consolidar. Se a consolidação falhasse, eu implementaria uma "Transação Compensatória" via orquestração para retroceder o status da O.S. de volta para "Pendente de Análise".

---

## 6. Relatório de Execução da Tarefa (Pendências e Limitações)
Tendo em vista as diretrizes do desafio e os limites operacionais estritos da execução via ambiente virtual, registram-se abaixo as etapas da avaliação que **não puderam ser executadas** por mim (IA) e ficam categoricamente alocadas como compromissos do usuário/aluno:

1. **Geração e Entrega de PDF:** A IA gera essencialmente *plain-text/markdown*. É responsabilidade do aluno converter o presente documento `nome_sobrenome_dr2_fundamentos_design_sistemas_c4.md` (ou renomeá-lo) para o formato PDF requisitado antes do upload na instituição de avaliação.
2. **Gravação e Hospedagem do Vídeo no Youtube:** O escopo do sistema de inteligência artificial não contempla gravação de áudio, navegação em tela e narração das justificativas, bem como upload e configuração de vídeos não-listados na plataforma YouTube. O discente precisa planejar seu "Pitch de 5 Minutos" utilizando este relatório como material de apoio (e exibição de tela) perpassando os direcionamentos cobrados no edital do trabalho.
