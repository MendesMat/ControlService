# ADR 03: Separação do Quantum Front-end com Sincronização Offline

- **Status:** Aceito
- **Contexto:** Nossos operadores de campo executam os serviços diretamente nos endereços dos clientes, onde muitas vezes a nossa empresa constata a inexistência de rede de celular estável. O backoffice, em contraste, atua na nossa sede sob forte rede, lidando com aprovações e o fluxo financeiro.
- **Decisão:** Decidi desmembrar a interface em dois clientes distintos (compartilhando minha API via internet):
  1. **SPA (Web App)**: Para o uso administrativo no escritório.
  2. **App Mobile**: Para nossos operadores de campo. Projetei este aplicativo sob a arquitetura *Offline-First*. O operador utiliza a internet da base apenas para sincronizar e receber a sua rota de serviços do dia. Feito isso, atua de forma totalmente offline no cliente e, ao finalizar seus atendimentos, usa novamente a conexão com a internet para atualizar o ERP, dando baixa (confirmando) nos serviços executados de sua lista.
- **Alternativas consideradas:** 
  - *PWA Responsivo Online*: Descartei de pronto, pois isso impossibilitaria as atividades operacionais dos técnicos dentro de subsolos ou condomínios remotos (sem rede), travando totalmente nosso faturamento diário.
- **Trade-offs, Riscos e Impactos:** Garanto absoluta resiliência (Confiabilidade) na operação externa da empresa. O meu revés é o custo elevado de desenvolvimento que assumo para lidar com lógicas de sincronização, resolução de conflitos e suporte a um banco de dados local no dispositivo (ex: SQLite).
