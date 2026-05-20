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
- **Equipe Enxuta:** O desenvolvimento e a manutenção são realizados por um único desenvolvedor. Essa restrição inviabiliza abordagens de infraestrutura complexas que exigiria uma equipe maior e bem estabelecida. Além disso, direciona a terceirização de integrações caóticas, como a emissão de notas fiscais, visto que construir emissores fiscais internamente aumentaria o tempo de entrega e a complexidade arquitetural em um nível absurdo e impraticável para um escopo individual.
- **Capacitação e Localização dos Usuários de Campo:** Operadores da empresa que executam serviços diretamente no endereço dos clientes frequentemente enfrentam ausência de sinal de internet. Portanto, o fluxo móvel é baseado em um modelo de sincronização explícita: o roteiro de serviços agendado pelo Módulo Comercial só é refletido no app móvel (Módulo Operacional) quando o operador realiza a sincronização ativa. Havendo atualizações posteriores no roteiro, os operadores afetados devem sincronizar o aplicativo novamente. Ao concluir um serviço ou toda a rota, o operador realiza uma última sincronização para enviar as baixas físicas dadas nos serviços (executados ou não).

---

## 2. Atributos de Qualidade (As "Ilities")
Com base nos princípios de que a arquitetura deve priorizar um subconjunto enxuto de características explícitas e implícitas, identifiquei os atributos centrais, focando nos trade-offs gerados:

### 2.1 Classificação
- **Operacionais:**
  - *Confiabilidade:* Crítico para o módulo operacional. A coleta de dados no campo em redes offline não pode resultar em perda de dados dos serviços concluídos.
- **Estruturais:**
  - *Configurabilidade:* Capacidade de formatar contratos, relatórios e certificados dinamicamente em tempo de execução. Adicionalmente, teremos as configurações de alíquotas tributárias isoladas por perfil CNPJ para que a emissão das notas fiscais siga um padrão já estipulado no banco de dados, mas ainda facilmente configurável caso haja mudanças fiscais na empresa.
  - *Manutenibilidade:* Atributo vital devido ao tamanho da equipe, uma pessoa.
- **Transversais (Cross-cutting):**
  - *Conformidade Legal (Explícito):* O core estrutural precisa validar regras do INEA de forma nativa.
  - *Isolamento e Segurança (Multitenancy e Autorização):* Isolamento rigoroso para evitar o cruzamento de dados de CNPJs.
  - *Auditabilidade:* Garantia de histórico contra inconsistências.

### 2.2 Conflitos e Trade-offs Arquiteturais
- **Isolamento de Tenant vs. Manutenibilidade de Código:** 
  Para garantir a segregação adequada dos documentos e alíquotas entre os perfis CNPJ, optei pelo uso de um *Isolamento Lógico* num banco de dados unificado. O *trade-off* envolvido nesta decisão é muito explícito: mantenho uma excelente facilidade operacional de infraestrutura (pois administra-se um único banco de dados), mas aumenta-se brutalmente o esforço e a responsabilidade de manutenção no código.
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
  - O meu Módulo Operacional (utilizado pelos operadores de campo) conhece a interface de agendamento do Módulo Comercial para sincronizar e obter as atividades planejadas do roteiro. O Módulo Comercial **desconhece** como o aplicativo móvel realiza e gerencia localmente (offline) a coleta de produtos, não conformidades, fotos e o status final de cada serviço, recebendo esses dados consolidados apenas quando o operador executa a sincronização de retorno das baixas (Baixa Conascência de Execução).
- **Preocupações de Design vs Arquitetura Limpa:** 
  - Centralizei as regras puras de negócio (como os cálculos e validações do INEA) no centro ("*Core*"). Detalhes como o *Entity Framework* ou os conectores de notas fiscais terceirizadas são injetados de fora para dentro (*Dependency Inversion Principle*).
  - Essa abstração me salva um tempo enorme para testar o *core* de faturamento, onde apenas simulo (mock) a API de notas e valido a matemática da regra com segurança.

---

## 5. APIs e Sistemas Distribuídos (Considerações Futuras)
Atualmente, o sistema é um Monólito Modular (tudo roda junto). Mas, se a empresa crescesse muito e precisássemos separar o **Módulo Operacional** em um sistema independente, adotaríamos as seguintes estratégias para lidar com a complexidade de um sistema distribuído. Nessas estratégias, respeitaríamos a regra de que **o coração do sistema é o Comercial** (o Financeiro e o Operacional nunca se comunicariam diretamente, ambos falariam apenas com o Comercial):

- **Propriedade dos Dados (Quem manda em qual dado):** Hoje, o banco de dados é um só. Se separássemos os sistemas, o Módulo Comercial continuaria sendo o absoluto "Dono da Verdade" sobre as Ordens de Serviço (O.S.s) e toda a documentação. O Módulo Operacional gerenciaria apenas a coleta do "relato de baixa" bruto no campo. Uma vez que o Operacional sincronizasse essas informações, a responsabilidade do operador terminaria. O Comercial usaria as baixas para gerar as O.S.s definitivas. Qualquer alteração, auditoria ou correção posterior nos relatos seria feita exclusivamente no Comercial, sem qualquer necessidade de devolver essas edições ao aplicativo mobile.
- **Contratos de Serviço (Consumer-Driven Contracts):** Para evitar que uma mudança na estrutura de dados enviada pelo Módulo Operacional quebrasse o Módulo Comercial de surpresa, usaríamos "Contratos Direcionados pelo Consumidor" (com ferramentas como o Pact). Isso garantiria que o formato das baixas fosse testado automaticamente para manter a compatibilidade de integração.
- **Consistência Eventual e Sagas (Lidando com falhas sem travar tudo):** Como os sistemas estariam separados, não teríamos a garantia de transações ACID unificadas. Usaríamos o padrão **Saga** com eventos assíncronos para o recebimento das baixas:
  1. O operador realizaria a sincronização final no mobile e o Operacional publicaria um evento assíncrono contendo as informações brutas das baixas (relatos, produtos, fotos).
  2. O Comercial escutaria esse evento e o utilizaria para **gerar as O.S.s** e atualizar o status dos roteiros e pedidos associados.
  3. Se houvesse falha durante a geração das O.S.s ou consolidação no Comercial, aplicaríamos uma **"Transação Compensatória"** via orquestração: o processamento no Comercial seria revertido e o registro entraria em "Falha de Sincronização/Aguardando Correção". O evento de baixa original gerado no Operacional não sofreria deleção, garantindo que o dado valioso coletado no campo não se perdesse enquanto o Comercial atuasse para resolver o problema.
  *Nota sobre o Financeiro:* O Módulo Financeiro ficaria totalmente isolado dessa Saga. Como o faturamento (lançamento de contas a receber ou notas fiscais) depende de regras de negócio estritas dos clientes (ex: bloqueio de notas entre o dia 25 e 31) e exige intervenção humana, a ação de faturar só seria disparada manualmente por um usuário a partir dos pedidos já consolidados no Comercial.