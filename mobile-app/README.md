# Mobile App — Módulo Operacional (Offline-First)

> Espaço reservado para o desenvolvimento do Módulo Operacional do Control Service ERP.

## Objetivo

Este aplicativo é destinado exclusivamente aos **operadores de campo** da empresa. Ele opera sob o padrão **Offline-First** com sincronização explícita conforme definido no [ADR-003](../docs/ADRs/ADR-003-Frontend-Mobile-Offline.md).

## Fluxo de Sincronização

1. **Obtenção do Roteiro:** O operador sincroniza manualmente para baixar a rota de serviços do dia agendada no backend.
2. **Execução Offline:** O trabalho é executado sem necessidade de conexão, com dados salvos localmente (SQLite).
3. **Sincronização de Retorno:** Ao finalizar, o operador sincroniza novamente para enviar as baixas de volta ao ERP.

## Stack (A Definir)

A tecnologia do app mobile ainda será definida. Candidatos:
- **.NET MAUI** — Para aproveitar o ecossistema C# já existente.
- **React Native** — Para compartilhamento de lógica com o `frontend-web/`.
- **Flutter** — Para máximo desempenho nativo e UI consistente entre plataformas.

## Contato

Módulo gerenciado pelo mesmo desenvolvedor responsável pelo backend em `../backend/`.
