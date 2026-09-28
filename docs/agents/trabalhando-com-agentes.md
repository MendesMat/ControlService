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

## O fluxo de uma issue: três conversas

O trabalho está organizado em issues no [milestone M1](https://github.com/MendesMat/ControlService/milestone/1), na ordem em que devem ser feitas. Cada issue passa por **três conversas separadas**, cada uma com o modelo certo para o tipo de trabalho:

| Conversa | Modelo e esforço | O que você faz | Termina com |
|---|---|---|---|
| **1. Plano** | O mais forte (hoje, Opus 5.5), esforço **alto** | Responde às lacunas e aprova a lista de testes | O *comentário de plano* publicado na issue |
| **2. Construção** | Intermediário (hoje, Sonnet 5), esforço **médio** | Navega os ciclos de TDD | O PR aberto, com o CI verde |
| **3. Revisão** | O mais forte, esforço **alto**, numa conversa **nova** | Lê os achados e decide os que forem seus | Os achados corrigidos no mesmo PR; aí você faz o merge |

Por que três conversas, e não uma só:

- **Custo.** A cada resposta, o agente relê a conversa inteira. Uma conversa que chega a centenas de milhares de tokens fica cara em cada "segue". Conversas curtas e focadas gastam menos.
- **Cada modelo onde rende mais.** O plano e a revisão exigem julgamento: achar lacunas, conferir decisões contra as regras. A construção é trabalho repetitivo, guiado por uma lista de testes detalhada. Por isso o modelo mais forte fica nas pontas.
- **Olhos frescos.** Um agente é tendencioso em relação ao código que ele mesmo escreveu. A revisão numa conversa nova pega o que a construção deixou passar.
- **A revisão vem antes do merge.** No issue #6 ela veio depois, e as correções precisaram de um segundo PR.

### O comentário de plano

É a passagem de bastão entre as conversas. No fim do plano, o agente publica na issue as suas decisões, as mensagens novas, a lista de testes com o **resultado esperado** de cada teste, e o que fica fora do escopo. A conversa de construção lê esse comentário em vez de reler toda a documentação, e não precisa improvisar decisões. Detalhes para os agentes: [sessions](workflows/implement-a-feature.md#sessions).

Revise o plano com atenção. No issue #6, uma decisão vaga do plano ("código fora da tabela vira 500") foi completada na construção de um jeito que contrariava a regra API-12.

### O que colar em cada conversa

**1. Plano** (Opus, alto):

> "Sessão de plano da próxima issue aberta do milestone M1. Leia a issue e os documentos que ela cita, me conte o que leu e as lacunas que encontrou, e espere as minhas respostas. Depois me mostre a lista de testes, do mais simples ao mais complexo, com o ID da regra e o resultado esperado de cada um. Quando eu aprovar, publique o comentário de plano na issue e pare."

**2. Construção** (Sonnet, médio):

> "Sessão de construção da issue #N. Leia a issue e o comentário de plano, e siga o AGENTS.md em TDD no modo par, pausando ao fim de cada ciclo, até o PR estar aberto com o CI verde. Se algo do plano não funcionar, pare e me diga."

Troque "no modo par" por "no modo autônomo" quando quiser menos pausas (veja abaixo quando vale a pena).

**3. Revisão** (Opus, alto, conversa nova):

> "Sessão de revisão do PR #N, antes do merge. Siga a etapa 9 do workflow implement-a-feature: confira o PR contra a issue, o comentário de plano, os documentos que eles citam e o AGENTS.md. Corrija no mesmo PR o que for correção, e me pergunte o que for decisão minha."

Para uma issue específica, cite o número dela. O agente abre o PR com `Closes #N`, e a issue fecha sozinha quando você faz o merge.

### Modo par ou modo autônomo

- **Modo par** nas regras de negócio (domínio, validações, permissões). É onde você aprende TDD e onde as suas decisões mudam o resultado.
- **Modo autônomo** na infraestrutura e na fiação (tratamento de erros, persistência, configuração), desde que o plano tenha o resultado esperado de cada teste. No issue #6, quase todas as pausas foram um "siga" sem decisão nenhuma.

### Trocando de modelo no meio

Se, na construção, uma abordagem do plano falhar ou surgir uma decisão de desenho, troque para o modelo mais forte **na mesma conversa**, pelo seletor de modelo, só para aquele passo. Depois, volte ao intermediário. Isso sai mais barato do que deixar o modelo intermediário improvisar, e mais barato do que uma revisão corrigindo depois.

### Tarefas mecânicas

Para sincronizar o git depois de um merge, revisar atualizações do Dependabot ou corrigir um erro de digitação na documentação, use o modelo mais leve (hoje, Haiku 4.5) ou o intermediário com esforço **baixo**.

### Cuidando da cota

- **Referência:** o issue #6 inteiro (plano, 13 ciclos, PR, revisão e dois PRs de correção) usou cerca de 5 pontos percentuais da cota semanal do plano Pro. Use isso para estimar se uma issue cabe na cota que sobrou; o agente consegue ler o seu uso atual se você pedir.
- **Saída curta:** o agente usa o `check.ps1`, que formata, compila e testa mostrando só os problemas e o resumo. Saídas longas ficam na conversa e são pagas de novo a cada resposta; no issue #6, só o ruído de formatação encheu uma boa parte da conversa.
- **Uma conversa por fase**, e uma fase por conversa: não junte perguntas soltas no meio da construção.

## TDD no modo par: o seu papel

Todo o desenvolvimento segue TDD (*Test-Driven Development*, ADR-0033). Cada comportamento nasce de um teste que falha, e o código cresce só o necessário para ele passar. No **modo par**, que é o padrão, você é o **navegador** e o agente é o **motorista**: ele escreve, você decide a direção.

O ciclo tem três fases. O agente faz as três para um teste e **para uma vez, no fim do ciclo**, mostrando a saída real de cada fase. Ele só para no meio do ciclo se um teste falhar por um motivo inesperado, se passar quando deveria falhar, ou se aparecer uma decisão que é sua.

| Fase | O que o agente mostra | O que você confere |
|---|---|---|
| **Red** (vermelho) | O teste novo e a falha dele | O teste descreve o comportamento certo? Falhou **pelo motivo esperado**, e não por um erro de digitação? |
| **Green** (verde) | O código mínimo e o teste passando | A estratégia faz sentido? Às vezes ele "trapaceia" de propósito (*Fake It*, devolvendo um valor fixo); o próximo teste vai forçar o código de verdade |
| **Refactor** | O que ficou mais limpo, ou "nada a refatorar", e os testes ainda verdes | Nomes claros, sem duplicação. Depois ele sugere os 2 ou 3 próximos testes: **você escolhe** |

Como responder nas pausas:

- **"ok"** ou **"segue"**: continua com o teste que o agente sugeriu.
- **"próximo: o teste X"**: escolhe o próximo teste da lista.
- **"por que assim?"**: peça a explicação. Entender o motivo de cada passo é o objetivo do modo par.
- **"pause a cada fase"**: o agente para depois do Red, do Green e do Refactor, para você acompanhar um ciclo mais de perto.
- **"modo autônomo nesta tarefa"**: o agente faz os ciclos sozinho e te entrega o registro de cada fase para você auditar. Use em tarefas repetitivas.
- **"passo a passo"**: volta ao modo par a qualquer momento.

Antes do primeiro ciclo, o agente mostra a **lista de testes** da tarefa. Revise com atenção, porque é ali que você garante que nenhuma regra ficou de fora. Se um caso estiver faltando, diga "acrescente um teste para...".

## Funcionalidades grandes: peça para ser entrevistado

Antes de uma fatia inteira (por exemplo, "desativação de usuários"), peça:

> "Quero implementar [funcionalidade]. Me entreviste antes: pergunte sobre regras, casos de borda e decisões que eu talvez não tenha pensado. Depois escreva um plano com os arquivos que vai criar e como vamos verificar."

Essa entrevista é a conversa de plano. Quando o plano estiver bom, **comece uma conversa nova** para implementar: ela começa limpa, focada só no plano.

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

A terceira conversa do [fluxo de uma issue](#o-fluxo-de-uma-issue-três-conversas) é uma revisão cruzada. Faça uma também quando um PR fora do milestone mexer em segurança, permissões ou banco: abra uma conversa nova e cole o texto da revisão.

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
