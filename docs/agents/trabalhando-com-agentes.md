# Trabalhando com agentes de IA

Guia para você, dono do projeto. As regras que os agentes seguem estão no [AGENTS.md](../../AGENTS.md), em inglês; este é o outro lado: como **você** conduz o trabalho.

## A ideia central

O agente é rápido, mas não lê pensamentos, e para quando o trabalho *parece* pronto. Duas coisas definem a qualidade do resultado:

1. **Uma issue precisa:** o que fazer, o que fica de fora e o que conta como pronto.
2. **Uma forma de verificar:** a lista numerada de testes. Com ela, o agente confere o próprio trabalho, e você confere o do agente.

## Os três comandos

Todo trabalho em uma issue passa por eles, nesta ordem. O procedimento completo de cada um está em `.claude/skills/<comando>/SKILL.md`, em inglês como as demais instruções para agentes. As issues e os relatórios que eles produzem são em português.

| Comando | O que o agente faz | O que você faz | Termina com |
|---|---|---|---|
| `/levantar-issue <n>` | Lê a documentação e o código, aponta as lacunas, propõe o escopo e a lista de testes | Responde às perguntas e aprova o rascunho | A issue reescrita no GitHub, em `status:ready` |
| `/executar-issue <n>` | Implementa tudo sem parar: teste antes do código, um teste por vez | Nada, até o relatório chegar | O pull request aberto, com o CI verde, o relatório final publicado no pull request, e a issue em `status:in-review` |
| `/revisar-issue <n>` | Compara o pull request com a issue e com as regras, **sem alterar código** | Lê os achados e decide quais aplicar | O relatório de revisão publicado no pull request, e a issue em `status:approved` ou `status:changes-requested` |

**O merge vem depois da revisão.** Com o CI verde, o pull request já parece pronto, mas é a revisão que confere se ele faz o que a issue pede.

Use uma **conversa nova para cada comando**. A conversa fica curta, o que gasta menos cota, e a revisão é feita por um agente que não escreveu o código. Como cada conversa começa sem memória, tudo o que um comando conclui fica no GitHub: o estado na label da issue, e os relatórios em comentários do pull request.

## Os estados da issue

Cada issue tem no máximo uma label `status:*`, que diz em que passo ela está. Cada comando confere a label antes de começar e a troca no fim, dizendo a nova label na mensagem final.

| Label | Significa | Quem põe |
|---|---|---|
| `status:survey` | Aguarda `/levantar-issue` | Você, ao priorizar uma issue |
| `status:ready` | Levantada e aprovada; aguarda `/executar-issue` | `/levantar-issue`, depois que você aprova o rascunho |
| `status:in-progress` | Em execução | `/executar-issue`, ao começar |
| `status:in-review` | Pull request aberto, com CI verde; aguarda `/revisar-issue` | `/executar-issue`, ao entregar |
| `status:changes-requested` | A revisão pediu mudanças; aguarda `/executar-issue` | `/revisar-issue` |
| `status:approved` | Revisão aprovada; aguarda o seu merge | `/revisar-issue` |
| `status:blocked` | Parada à espera de uma resposta sua | O comando que parou; você, ao responder, põe a label do passo seguinte |

O caminho normal é `survey` → `ready` → `in-progress` → `in-review` → `approved` → merge. Quando a revisão pede mudanças, a issue volta: `in-review` → `changes-requested` → `in-progress` → `in-review`, até a revisão aprovar.

- **Comando na fase errada.** Se a issue não está na label de entrada do comando, ele diz o estado atual e qual comando cabe, e para. Ele só segue se você mandar explicitamente na conversa, e registra a ordem num comentário na issue.
- **Uma issue por vez.** Se outra issue tem trabalho fora da `main` (`in-progress`, `in-review`, `changes-requested` ou `approved`), `/levantar-issue` e `/executar-issue` lembram a regra e perguntam se você quer seguir.
- **Dependências.** Uma issue citada em "Dependências" precisa estar fechada: `/levantar-issue` avisa, e `/executar-issue` se recusa a começar.
- **Parada.** Quando o agente para à espera de uma resposta sua, ele explica o motivo num comentário na issue (`/executar-issue` também deixa o trabalho parcial na branch, sem abrir pull request) e põe `status:blocked`. Responda e troque a label para a do passo seguinte.
- **Label errada.** Se um comando recusa começar por causa de uma label esquecida, corrija a label no GitHub e rode o comando de novo.

## A lista de testes

Cada issue tem uma lista numerada: `T01`, `T02`, ... Fora da issue, o teste 3 da issue 10 é o `#10-T03`.

- A mesma lista, com os mesmos números, aparece na issue, no pull request e no relatório final.
- No código, cada teste tem um comentário com o seu ID (`// #10-T03`). Para achar um teste, procure por esse texto.
- Os números não mudam depois que você aprova a issue. Um teste descoberto durante a execução entra no fim da lista.

É no `/levantar-issue` que você garante que nenhuma regra ficou de fora. Se faltar um caso, diga "acrescente um teste para...".

## Como ler o relatório de execução

O relatório chega na conversa e fica publicado como comentário no pull request. Ele tem sempre as mesmas cinco partes:

| Parte | O que conferir |
|---|---|
| **Implementação** | O resumo bate com o escopo da issue? |
| **Testes** | Cada ID aparece com PASSOU, FALHOU ou PULADO. Leia os motivos das falhas e de qualquer teste alterado, adicionado ou pulado |
| **Regressões** | Algum teste antigo precisou mudar? Por quê? |
| **Arquitetura** | Surgiu alguma classe, interface, camada ou padrão novo? A justificativa convence? |
| **Observações** | Problemas fora do escopo. Decida se algum vira issue |

Para conferir um teste, abra o método indicado na tabela e compare o que ele afirma com o "Resultado esperado" da issue.

## Como as decisões são explicadas

Sempre que houver uma decisão técnica relevante, o agente a explica em seis pontos:

1. O que está sendo proposto.
2. Que problema isso resolve.
3. O que acontece se não fizermos.
4. Qual é o custo.
5. Quais são as alternativas mais simples.
6. O que ele recomenda para este projeto, e por quê.

Se alguma explicação não ficar clara, peça outra, com um exemplo do próprio projeto. Você não deve aceitar uma recomendação que não consegue explicar com as suas palavras: o objetivo é que você saiba defender cada decisão numa entrevista.

O agente também não deve acrescentar complexidade "porque é boa prática". Se aparecer algo que você não pediu, pergunte: "o projeto precisa disso agora?".

## Fazendo o merge

O agente abre o pull request e para. **Quem faz o merge é você**, quando a revisão deixa a issue em `status:approved`.

A revisão só relata: ela não altera código. Se ela deixou a issue em `status:changes-requested`:

1. Diga quais achados aplicar, num comentário do pull request ou no começo da próxima conversa. Por exemplo: "Aplique os achados 1 e 3; o 2 fica para uma issue futura."
2. Rode `/executar-issue <n>` numa conversa nova. O agente lê o último relatório de revisão e as suas escolhas, corrige na mesma branch, teste antes do código, e comenta no pull request o que mudou, achado por achado. Achado que você não escolheu não é aplicado.
3. Rode `/revisar-issue <n>` de novo, numa conversa nova. A correção também passa por um segundo olhar antes de chegar à `main`.

Se o pull request já foi mesclado quando a revisão rodou, o relatório fica no pull request e os achados que você escolher viram uma issue nova, pelo `/levantar-issue`.

## Modelos e cota

| Comando | Modelo indicado | Motivo |
|---|---|---|
| `/levantar-issue` | O mais forte | Exige julgamento: achar lacunas e medir o escopo |
| `/executar-issue` | O intermediário | É trabalho guiado por uma lista de testes detalhada |
| `/revisar-issue` | O mais forte, em conversa nova | Exige julgamento, com olhos frescos |

Se a execução travar numa decisão de desenho, troque para o modelo mais forte na mesma conversa, só para aquele passo.

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

Corrija e diga: "isso vale sempre, atualize o AGENTS.md". O agente propõe a mudança no mesmo pull request.
