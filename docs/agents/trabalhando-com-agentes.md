# Trabalhando com agentes de IA

Guia para humanos tirarem o máximo dos agentes de IA como *pair programmers*: um parceiro que programa enquanto você orienta, revisa e aprende. As regras que os agentes seguem estão no [AGENTS.md](../../AGENTS.md), em inglês. Este guia é o outro lado: como **você** conduz o trabalho.

## A ideia central

O agente é rápido, mas não lê pensamentos, e para quando o trabalho *parece* pronto. Duas coisas mudam a qualidade do resultado:

1. **Contexto preciso no pedido:** o que fazer, onde, e o que conta como pronto.
2. **Uma forma de verificar:** testes, build ou uma chamada à API. Com isso, o próprio agente confere e corrige antes de te entregar.

## Como pedir uma tarefa

Um bom pedido tem quatro partes: **o quê**, **onde**, **restrições** e **como saber que ficou pronto**.

| Pedido vago | Pedido bom |
|---|---|
| "Faz a validação de CPF." | "Crie o objeto de valor `Cpf` em `Domain/Users`, seguindo a regra USR-08 e a CNV-07 (dígitos verificadores, sequências repetidas recusadas, gravado só com dígitos). Escreva os testes primeiro, incluindo `52998224725` como válido e `11111111111` como inválido. Pronto quando `dotnet test` passar." |
| "Arruma o erro do login." | "Depois de 5 senhas erradas, o bloqueio não acontece (regra AUTH-08). Veja o fluxo em `Application/Auth`. Escreva um teste que reproduza o problema e depois corrija." |
| "Melhora a tela de perfis." | "Leia a seção *Operations* de `docs/product/features/permission-profiles.md` e me diga o que falta no back-end para `listProfiles` devolver `userCount`. Não altere nada ainda." |

Dicas:
- **Aponte arquivos, seções e IDs de regra** (USR-06, PERM-05) em vez de descrever onde as coisas estão. Cada regra de negócio tem um ID estável; a lista de prefixos está no [índice da documentação](../README.md#rule-ids).
- **Diga o que não fazer**, quando importa ("sem pacote novo", "não mude as rotas").
- **Perguntas abertas também valem**, quando você quer explorar: "o que você melhoraria neste arquivo?"

## Começando a próxima tarefa

O trabalho está organizado em issues no [milestone M1](https://github.com/MendesMat/ControlService/milestone/1), na ordem em que devem ser feitas. Numa conversa nova, cole:

> "Implemente a próxima issue aberta do milestone M1. Antes de programar, leia a issue e os documentos que ela cita, e me mostre um plano curto em português: quais arquivos vai criar e quais IDs de regra cada teste vai cobrir. Siga o AGENTS.md e pare quando o PR estiver aberto com o CI verde."

Para uma issue específica, troque o começo por "Implemente a issue #4". O agente abre o PR com `Closes #4`, e a issue fecha sozinha quando você fizer o merge.

## Funcionalidades grandes: peça para ser entrevistado

Antes de uma fatia inteira (por exemplo, "desativação de usuários"), peça:

> "Quero implementar [funcionalidade]. Me entreviste antes: pergunte sobre regras, casos de borda e decisões que eu talvez não tenha pensado. Depois escreva um plano com os arquivos que vai criar e como vamos verificar."

Quando o plano estiver bom, **comece uma conversa nova** para implementar. A conversa nova começa limpa, focada só no plano.

## Durante o trabalho

- **Corrija cedo.** Se o agente for para um caminho errado, interrompa e redirecione na hora.
- **Duas correções sem sucesso?** Comece uma conversa nova, com um pedido melhor que inclua o que você aprendeu. Uma conversa cheia de tentativas erradas atrapalha o agente.
- **Uma conversa por tarefa.** Misturar assuntos (implementar, depois uma dúvida solta, depois voltar) enche a conversa de informação irrelevante.
- **Peça explicações.** "Por que você escolheu isso?" e "qual a alternativa?" são perguntas que o agente deve responder bem. Você está aprendendo; use isso.

## Revisando um pull request

O agente abre o PR e para. **Quem faz o merge é você.** Na revisão, olhe nesta ordem:

1. **A descrição do PR:** o que mudou, por quê, e como foi testado. Se não der para entender, peça para reescrever.
2. **Regras de negócio:** o comportamento bate com o documento da funcionalidade em `docs/product/features/`? O PR deve citar os IDs das regras que implementa. Esse é o seu ponto forte, e a parte que os testes não pegam sozinhos.
3. **Mensagens para o usuário:** estão iguais às do documento da funcionalidade?
4. **Testes:** existe um teste para cada regra nova? Os nomes dos testes descrevem o comportamento?
5. **Tamanho:** um PR grande demais é difícil de revisar. Peça para dividir.

Para pedir ajustes, comente no próprio PR ou diga ao agente: "No PR #N, a mensagem de bloqueio está diferente da AUTH-08. Corrija." Para aprovar: "pode juntar o PR #N".

## Revisão cruzada

Um agente é tendencioso em relação ao código que ele mesmo escreveu. Para mudanças importantes (segurança, permissões, banco), abra uma **conversa nova** e peça:

> "Revise o PR #N contra o documento da funcionalidade em `docs/product/features/` e o AGENTS.md. Aponte só problemas de correção ou regras não cumpridas (citando os IDs), não preferências de estilo."

## O que o agente nunca faz, e por quê

| O agente não... | Porque |
|---|---|
| Envia direto para a `main` nem faz merge sozinho | Toda mudança passa pela sua revisão e pelo CI |
| Marca certificados como confiáveis | É uma configuração de segurança do seu Windows: só você decide |
| Inventa regra de negócio | Você é quem conhece o negócio; ele pergunta |
| Apaga o volume `controlservice-postgres-data` | São os dados do banco |
| Usa o seu e-mail pessoal nos commits | Privacidade: o repositório é público |

No Claude Code, algumas dessas regras são **travas de verdade**, configuradas em `.claude/settings.json`: mesmo que o agente tente, o comando é bloqueado.

## Quando o agente erra algo que vale para o projeto todo

Corrija e diga: "isso vale sempre, atualize o AGENTS.md". O agente propõe a mudança no mesmo PR. É assim que o AGENTS.md melhora com o tempo, em vez de envelhecer.
