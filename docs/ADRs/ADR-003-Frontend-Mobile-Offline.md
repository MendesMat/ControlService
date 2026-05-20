# ADR 03: Separação do Quantum Front-end com Sincronização Offline

## Status:
Aceito

## Contexto:
Os operadores de campo executam os serviços diretamente nos endereços dos clientes, onde muitas vezes a empresa constata a inexistência de rede de celular estável. O escritório, em contraste, atua na sede sob forte rede, lidando com aprovações e o fluxo financeiro.

## Decisão:
Decidi desmembrar a interface em dois clientes distintos (compartilhando minha API via internet):
1. SPA (Web App): Para o uso administrativo no escritório.
2. App Mobile: Para os operadores de campo. Projetei este aplicativo sob a arquitetura *Offline-First* e com base em um modelo de sincronização explícita:
   - **Obtenção do Roteiro:** O operador utiliza a internet para sincronizar e carregar em seu dispositivo a rota de serviços agendada pelo Módulo Comercial. 
   - **Atualizações de Rota:** Se o roteiro sofrer alterações administrativas posteriores na retaguarda, os operadores afetados devem sincronizar o aplicativo novamente para atualizar sua lista de tarefas local.
   - **Execução Offline:** Com a rota sincronizada, o operador realiza o trabalho em campo de forma totalmente offline, registrando as baixas dos serviços (insumos utilizados, fotos, não conformidades e status da execução).
   - **Sincronização de Retorno:** Ao concluir um serviço ou toda a rota, o operador realiza uma sincronização online para enviar ao ERP as baixas dadas nos serviços executados (concluídos) ou não executados (cancelados/pendentes).

## Alternativas consideradas:
PWA Responsivo Online: Descartei de pronto, pois isso impossibilitaria as atividades operacionais dos técnicos dentro de subsolos ou condomínios remotos (sem rede), travando totalmente o faturamento diário.

## Trade-offs, Riscos e Impactos:
Garanto absoluta resiliência (Confiabilidade) na operação externa da empresa. O meu revés é o custo elevado de desenvolvimento que assumo para lidar com lógicas de sincronização, resolução de conflitos e suporte a um banco de dados local no dispositivo (ex: SQLite).
