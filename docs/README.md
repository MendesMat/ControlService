# Control Service — Documentação

Esta pasta descreve o ERP Control Service: um front-end de página única (`control-service-erp.html`), com a estrutura de navegação completa e duas telas funcionais, **Usuários** e **Permissões**, e as decisões tomadas para o back-end em C# com .NET.

Os documentos descrevem o sistema **como foi decidido**, e o front-end já segue essas decisões. Enquanto o back-end está em construção, o front-end traz um **servidor simulado**, que implementa as mesmas operações e regras que o back-end deverá implementar (ver [05-integracao-com-o-front.md](05-integracao-com-o-front.md)). O que ainda não foi decidido está reunido em [07-pendencias.md](07-pendencias.md).

## Como ler

| Arquivo | Para que serve |
|---|---|
| [01-visao-geral.md](01-visao-geral.md) | O produto, o público, os princípios de design e o mapa de menus e telas. |
| [02-modelo-de-dados.md](02-modelo-de-dados.md) | Os registros que o front-end grava e lê, campo por campo, com exemplos em JSON. |
| [03-regras-de-negocio.md](03-regras-de-negocio.md) | Validações, acesso ao sistema, desativação, autoria, concorrência e todas as mensagens exibidas. |
| [04-permissoes.md](04-permissoes.md) | Níveis de acesso, chaves das telas e como o acesso efetivo é calculado. |
| [05-integracao-com-o-front.md](05-integracao-com-o-front.md) | O servidor simulado, as operações que o back-end precisa oferecer e os erros que o front-end trata. |
| [06-front-end.md](06-front-end.md) | Organização do código, navegação por abas, acesso e sessão, rotas, temas e como adicionar uma tela nova. |
| [07-pendencias.md](07-pendencias.md) | Decisões em aberto que afetam o back-end. |
| [adr/](adr/README.md) | Registros de decisões de arquitetura do back-end (ADRs), em inglês: tecnologias escolhidas, alternativas e consequências. |
| [catalogo-de-telas.json](catalogo-de-telas.json) | Lista das áreas, telas e níveis de acesso em formato de dados, com as mesmas chaves usadas pelo front-end. |

## Convenções

Os nomes de campos, chaves e identificadores estão em inglês, exatamente como aparecem no código e nos dados gravados (`fullName`, `profileIds`, `levels`). Os textos exibidos para as pessoas, as mensagens de erro e esta documentação estão em português.

Datas seguem o formato ISO 8601: `AAAA-MM-DD` para datas simples, como nascimento, e data e hora completas com fuso UTC para carimbos de tempo, como `updatedAt`.

## Situação atual

| Parte | Situação |
|---|---|
| Menu lateral, abas, temas claro e escuro | Pronto |
| Tela Usuários (lista por página, cadastro, edição, desativação, reativação, reenvio de acesso) | Pronta |
| Tela Permissões (lista, cadastro, edição, duplicação, exclusão) | Pronta |
| As outras 18 telas do menu | Estrutura pronta, conteúdo em construção |
| Login por link de ativação, troca de senha e bloqueio por tentativas | Pronto no front-end, com servidor simulado |
| Permissões aplicadas: menu, telas e botões conforme o nível | Pronto no front-end, com servidor simulado |
| Desativação de usuários, autoria, controle de versão, listas por página | Pronto no front-end, com servidor simulado |
| Back-end | Em construção, em [`backend/ControlService`](../backend/ControlService/README.md). A estrutura está pronta; a primeira etapa é login, usuários e permissões. Até lá, o front-end usa o servidor simulado, que guarda os dados no banco do artifact do Claude ou no navegador (ver [05](05-integracao-com-o-front.md)). As decisões para o back-end real estão em [adr/](adr/README.md). |
