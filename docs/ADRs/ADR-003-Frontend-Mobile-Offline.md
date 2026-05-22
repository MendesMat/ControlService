# ADR-003: Separação do Quantum Frontend com Estratégia Offline-First para Operadores de Campo

**Data:** 15/05/2025
**Autor:** Matheus Mendes
**Status:** Aceito

---

## Contexto

O Control Service ERP serve dois perfis de usuário com necessidades radicalmente distintas, que configuram **quanta arquiteturais diferentes** na camada de frontend:

**Perfil Administrativo (escritório):**
- Ambiente controlado, com rede estável e confiável.
- Fluxos de trabalho: aprovação de propostas, agendamento de roteiros, faturamento, emissão de documentos.
- Latência tolerável; a interatividade e a riqueza de interface são prioritárias.

**Perfil Operacional (campo):**
- Operadores executam serviços em endereços de clientes: subsolos de condomínios, áreas industriais, reservatórios de água e locais remotos — onde a cobertura de rede de celular é inexistente ou extremamente instável.
- Fluxos de trabalho: receber o roteiro diário de serviços, executar as atividades (registrar insumos químicos utilizados, fotografar o ambiente, registrar não conformidades e dar baixa no serviço como concluído ou cancelado) e sincronizar os resultados com o ERP ao retornar a uma área com conectividade.
- **A indisponibilidade de rede não pode jamais bloquear a execução operacional.** Um técnico impedido de registrar um serviço porque não há internet equivale a faturamento travado e inadimplência operacional.

A ausência de uma estratégia de conectividade para o perfil de campo não é um risco técnico — é uma **falha de negócio direta**, pois impede a conclusão do ciclo que culmina em "Pronto para Faturar".

Adicionalmente, existe uma assimetria clara de responsabilidade de dados entre os dois módulos: o **Módulo Comercial** é o proprietário autoritativo das Ordens de Serviço e da documentação final. O **Módulo Operacional** é responsável pela coleta do relato bruto de campo. Essa separação de responsabilidades deve ser respeitada na arquitetura do frontend.

---

## Decisão

**Adotarei dois clientes frontend distintos e independentes**, cada um projetado para o seu contexto de uso, ambos consumindo a mesma API REST do backend:

### 1. SPA Administrativa (Web Application)
Aplicação web de página única (SPA) para uso no escritório. Conectividade contínua pressuposta. Foco em riqueza de interface e interatividade para fluxos comerciais, financeiros e de relatórios.

### 2. App Móvel Operacional (Offline-First)
Aplicativo móvel para os operadores de campo, projetado sob a arquitetura **Offline-First** com um modelo de **sincronização explícita e bidirecional**.

O ciclo de sincronização do app é composto por duas fases distintas:

**Fase 1 — Obtenção do Roteiro (Sincronização de Entrada):**
Com conectividade disponível (geralmente no início do turno), o operador aciona a sincronização para baixar o roteiro de serviços agendado pelo Módulo Comercial. O roteiro é persistido em um banco de dados local no dispositivo (SQLite). A partir desse momento, o operador não depende mais de internet para operar.

> **Invariante de consistência:** Se o Módulo Comercial realizar alterações no roteiro após a sincronização inicial (reagendamentos, inserção de novos serviços), o operador afetado deverá sincronizar novamente antes de iniciar os trabalhos. Não há atualização em tempo real — esse é um trade-off deliberado e aceito.

**Fase 2 — Execução Totalmente Offline:**
Com o roteiro local, o operador realiza o trabalho em campo sem nenhuma dependência de rede. O app registra localmente: insumos químicos consumidos (com código e quantidade para o RAAE), fotografias do ambiente pré/pós serviço, não conformidades identificadas, e o status final de cada serviço (concluído, cancelado ou pendente de revisão).

**Fase 3 — Sincronização de Retorno (Sincronização de Saída):**
Ao concluir o roteiro ou ao retornar a uma área com conectividade, o operador aciona a sincronização de retorno. O app envia as baixas consolidadas para o backend. O Módulo Comercial processa esses dados para gerar as Ordens de Serviço definitivas e atualizar o status dos roteiros. O dado bruto coletado no campo (o "relato de baixa") nunca é deletado do dispositivo antes da confirmação de recebimento pelo servidor.

---

## Separação de Responsabilidades e Propriedade dos Dados

A fronteira entre os dois módulos é governada pelo **Princípio da Responsabilidade Única (SRP)** e respeita o fluxo de dados definido na arquitetura:

- O Módulo Operacional **conhece** a interface de agendamento do Módulo Comercial (para obter o roteiro).
- O Módulo Comercial **desconhece** como o app gerencia localmente a coleta de dados de campo.
- Após a sincronização de retorno, a **responsabilidade do Operacional termina**. Qualquer auditoria, correção ou ajuste posterior nas baixas é responsabilidade do Módulo Comercial — sem necessidade de devolver edições ao app.

Essa assimetria proposital é o que garante o **baixo acoplamento** entre os dois módulos (Baixa Conascência de Execução), preservando a independência evolutiva de cada cliente.

---

## Alternativas Consideradas e Motivos de Descarte

### PWA Responsivo Online-Only
Descartado sumariamente. Uma Progressive Web App sem estratégia offline robusta travaria completamente as atividades dos técnicos em subsolos ou áreas sem sinal. O impacto seria direto no faturamento diário da empresa — um risco inaceitável para o negócio.

### App Móvel com Sincronização em Tempo Real (WebSockets/Push)
Descartado. Em ambientes com conectividade intermitente, manter uma conexão WebSocket aberta é impraticável. Além disso, o modelo de sincronização em tempo real introduziria complexidade de resolução de conflitos em tempo de execução (race conditions entre atualizações do escritório e ações do campo) — complexidade que o modelo de sincronização explícita com fases bem definidas evita por design.

### Sincronização Automática em Background (Sem Controle Explícito do Operador)
Descartado. A sincronização em background sem controle do usuário aumentaria o risco de envio de baixas parciais (um serviço ainda não concluído sendo sincronizado prematuramente). O modelo de sincronização explícita garante que o operador só envia dados quando deliberadamente decide que estão prontos.

---

## Trade-offs, Riscos e Impactos

### Vantagens
- **Confiabilidade máxima na operação de campo:** A ausência de rede nunca bloqueia a coleta de dados — o atributo de qualidade mais crítico do sistema.
- **Integridade dos dados coletados:** O relato de baixa é persistido localmente antes de qualquer tentativa de envio, eliminando o risco de perda de dados por falha de rede durante a sincronização.
- **Separação clara de quanta:** Cada cliente evolui de forma independente, com seu próprio ciclo de deploy e suas próprias dependências técnicas.

### Desvantagens e Riscos
- **Custo elevado de desenvolvimento:** A lógica de sincronização bidirecional, resolução de conflitos e gerenciamento de banco de dados local (SQLite) no dispositivo aumentam substancialmente o escopo de implementação do App Móvel.
- **Consistência eventual entre roteiro agendado e roteiro executado:** Existe uma janela de tempo em que o operador pode estar trabalhando com uma versão desatualizada do roteiro (caso mudanças tenham sido feitas no escritório após a sincronização inicial). Esse risco é aceito e mitigado por processo operacional (o operador deve sempre sincronizar no início do turno).
- **Dois ciclos de deploy independentes:** O app móvel possui seu próprio ciclo de versionamento e distribuição (stores), desacoplado do backend. Mudanças de contrato da API devem ser retrocompatíveis ou versionadas.

---

## Conformidade e Governança

- O contrato da API de sincronização (endpoints de obtenção de roteiro e de envio de baixas) será documentado formalmente e versionado via URL (`/api/v1/sync/...`), garantindo que versões antigas do app continuem funcionando durante períodos de transição.
- O app móvel implementará **idempotência** nas operações de sincronização de saída: o reenvio de uma baixa já processada pelo servidor deve ser detectado e ignorado sem erro, protegendo contra retries em caso de timeout de rede.
- Testes de integração cobrirão os cenários de borda da sincronização: roteiro não encontrado, baixa duplicada, e conflito de status entre campo e retaguarda.
