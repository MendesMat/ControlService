# Mobile App — Módulo Operacional (Offline-First)

Este é o aplicativo móvel do **ControlService ERP**, desenvolvido especificamente para apoiar as atividades externas dos **operadores de campo** da empresa. Ele foi projetado sob a filosofia **Offline-First**, garantindo que as operações de dedetização, higienização e impermeabilização sejam executadas sem qualquer interrupção, mesmo em locais totalmente sem sinal de internet (como subsolos, garagens ou condomínios remotos).

---

## 📱 Stack de Tecnologias

O aplicativo móvel é construído com foco em consistência multiplataforma e alto desempenho local:

*   **Framework:** React Native (Versão LTS mais atual)
*   **Linguagem:** TypeScript para tipagem segura, facilitando a robustez da lógica de sincronização
*   **Banco de Dados Local:** SQLite (via biblioteca nativa de persistência robusta) para armazenamento seguro dos dados offline
*   **Comunicação de Rede:** Axios com suporte a detecção ativa de conectividade (`@react-native-community/netinfo`) e enfileiramento inteligente de requisições de sincronização

---

## 🔒 Segregação de Responsabilidades e Segurança

O aplicativo móvel segue o princípio de **Segurança por Segregação de Funções**:
*   Ele é exclusivo para o escopo operacional dos **operadores de campo**.
*   Diferente do painel administrativo web (`frontend-web/`), o operador **não tem acesso** a dados comerciais sensíveis, propostas financeiras, comissões de vendedores ou faturamento da empresa.
*   O operador visualiza unicamente o seu roteiro diário designado, as ordens de serviço correspondentes e os insumos que deve utilizar ou relatar na execução física.

---

## 🔄 Fluxo de Sincronização Explícita (ADR-003)

Para lidar com a instabilidade crônica de rede móvel em campo, o aplicativo adota um modelo de **sincronização ativa e explícita** projetado no [ADR-003](../docs/ADRs/ADR-003-Frontend-Mobile-Offline.md):

```
 [Escritório/Web] ──(Agenda o Roteiro)──> [Backend C# API]
                                                 │
                                         (Sincronização Ativa)
                                                 ▼
 [Operador/Mobile] <──(Baixa Rota Local)─────── [Internet]
         │
  (Execução Offline) ───[Registra Insumos / Fotos / SQLite]
         │
         ▼
 [Sincronização de Retorno] ──(Envia Relato de Baixa)──> [Backend C# API] ──> [Faturamento]
```

### 1. Obtenção do Roteiro (Download)
*   **Ação:** O operador, antes de sair a campo ou sob uma rede estável (Wi-Fi/4G), aciona ativamente a sincronização no app.
*   **Resultado:** O aplicativo consome a API REST (.NET 10) e baixa todo o roteiro de tarefas agendado pelo Módulo Comercial para o seu dispositivo, salvando as informações estruturadas no banco de dados local **SQLite**.
*   **Atualizações:** Se houver alteração administrativa na retaguarda posterior, o operador deve acionar a sincronização novamente para obter a rota atualizada.

### 2. Execução Offline
*   **Ação:** O operador realiza as vistorias técnicas e aplicações no endereço do cliente de forma 100% desconectada.
*   **Registro Local:** Todas as informações físicas da execução são registradas diretamente no SQLite do aparelho:
    *   Status do serviço (Executado / Não Executado / Cancelado / Pendente).
    *   Uso de insumos químicos (necessário para a posterior conformidade legal com o RAAE do INEA).
    *   Registro de fotos das caixas d'água, cisternas ou pragas, servindo como prova de execução.
    *   Descrição de não conformidades encontradas.

### 3. Sincronização de Retorno (Upload das Baixas)
*   **Ação:** Ao concluir uma atividade ou o roteiro do dia, o operador ativa o upload de retorno sob uma conexão de internet estável.
*   **Resultado:** Os relatos brutos contidos no SQLite são encapsulados e enviados de volta ao backend. O backend processa essas baixas, atualiza o status definitivo das Ordens de Serviço no Módulo Comercial e as disponibiliza como "Pronto para Faturar" para o Módulo Financeiro, além de registrar as dosagens na base de dados para o relatório RAAE do INEA.

---

## 🏗️ Governança e Evolução Distribuída (Saga & Consistência Eventual)

Caso o sistema cresça e o **Módulo Operacional** seja migrado para uma infraestrutura de microsserviço independente no futuro, a comunicação de sincronização seguirá as seguintes diretrizes:

*   **Propriedade de Dados Focada:** O Módulo Comercial continuará sendo o dono definitivo das Ordens de Serviço (O.S.). O Mobile atua puramente como um coletor de relatos. Uma vez que as baixas do app são sincronizadas e entregues ao servidor, a responsabilidade do dispositivo móvel cessa. Modificações ou auditorias históricas subsequentes ocorrem exclusivamente no backend/painel web administrativo.
*   **Transações Compensatórias (Sagas):** Em caso de falha de consistência no processamento do relato recebido no backend, uma transação compensatória preservará o relato original intacto no histórico de falhas da sincronização. O dado coletado em campo nunca será deletado ou corrompido, permitindo a correção administrativa na retaguarda sem necessidade de retrabalho do operador externo.

---

## ⚙️ Configuração e Execução Local (Futuro)

Assim que o desenvolvimento do app for iniciado, o projeto utilizará os comandos padrões do ecossistema React Native/TypeScript:

```bash
# Instalar dependências do projeto
npm install

# Instalar dependências nativas (iOS apenas)
npx pod-install ios

# Executar emulador Android
npm run android

# Executar emulador iOS
npm run ios
```
