---
name: levantar-issue
description: Survey a Control Service issue before any code is written - read the documentation and the code, define scope, dependencies, risks, the numbered test list and the acceptance criteria, and write them to the issue body on GitHub after the owner approves the draft. Use when the owner asks for /levantar-issue, says "levante a issue" or "levantar a issue", or wants to plan an issue.
argument-hint: "<issue number, or the description of a new issue>"
---

# /levantar-issue

First of the three commands (`AGENTS.md`, "Workflow: three commands"). The result is an issue that someone else could execute without asking anything: **what will be done, why, and how we will know it is finished**.

Issue: `$ARGUMENTS`

- No argument: the lowest open issue of the current milestone.
- Text instead of a number: the description of a new issue, created in step 8.

Talk to the owner in Portuguese. The issue body is written in Portuguese, from the template below.

## Before starting

The states and their rules are in `AGENTS.md`, "Issue states".

1. **Check the entry state** of an existing issue: `gh issue view <n> --json state,labels`. It must be open, in `status:survey` or with no `status:*` label. In any other state, say which state the issue is in and which command fits, and stop. Go on only on the owner's explicit order in the conversation, and record it first: `gh issue comment <n> --body-file <file>`, quoting the order. A new issue (text instead of a number) has no state to check.
2. **Check that no other issue is in flight:** `gh issue list --state open --search 'label:"status:in-progress","status:in-review","status:changes-requested","status:approved"'`. If one appears, remind the owner that one issue goes to `main` at a time, and ask whether to go on.

## Steps

1. **Read the issue** and its comments: `gh issue view <n> --comments`.
2. **Read the documentation it touches:** the feature document in `docs/product/features/`, `docs/product/conventions.md`, `docs/api/conventions.md`, the decisions that apply in `docs/decisoes-de-arquitetura.md` and `docs/product/open-questions.md`. The scope decisions in `AGENTS.md` override older issues.
3. **Read the related code:** the files the issue will touch, and the nearest similar feature already built, which is the pattern to follow.
4. **Survey:**
   - the requirement or the problem, in one sentence;
   - the smallest scope that solves it;
   - the dependencies (other issues, decisions, documents), and whether each issue among them is still open (`gh issue view <m> --json state`);
   - the risks (what may surprise the execution);
   - the gaps: a rule `docs/` does not answer, an error case without a verbatim message, a contradiction between documents or between a document and the code, a duplicated rule ID.
5. **Apply proportionality** (`AGENTS.md`). Anything that is an abstraction, layer, pattern, package, migration or contract change enters the draft as a decision explained in the six points. An improvement the issue does not need goes to "Recomendações futuras".
6. **Build the test list** (rules below).
7. **Present to the owner and wait:**
   - what you read;
   - each dependency issue that is still open: `/executar-issue` will refuse to start until it is closed;
   - each gap and each decision, in the six points, with your recommendation;
   - the complete draft of the issue, in the template below.
8. **After the approval, write it and mark it:** `gh issue edit <n> --body-file <file> --remove-label status:survey --add-label status:ready` (or `gh issue create --title ... --body-file <file> --milestone ... --label status:ready` for a new issue). Confirm with the link and the new label, and stop.

**If the survey stops with questions unanswered** (the owner leaves them for later, or ends the conversation before approving the draft): publish the open questions, each with what you already know about it, in a comment on the issue (`gh issue comment <n> --body-file <file>`), and set `gh issue edit <n> --remove-label status:survey --add-label status:blocked`. Tell the owner the new label. When they answer, they set the label of the next step.

## Test list

- **Stable IDs:** `T01`, `T02`, ... inside the issue; `#<n>-T01` outside it. Once approved, the list is never renumbered.
- **One behavior per test**, described in Portuguese and starting with "Deve": *Deve rejeitar login com senha incorreta*.
- **Order:** simplest first, inside out (the domain, then the operation through its endpoint).
- **Exact expected result:** the value, the status, the `code` and the verbatim message. A vague expectation becomes guesswork during the execution.
- **One layer per behavior**, the cheapest one that proves the rule:

  | Layer | What goes there |
  |---|---|
  | Domínio | A rule of a value object or an aggregate: validation, state transition, calculation |
  | API (integration, real PostgreSQL) | Each operation: success, validation errors, one allowed and one denied path for the minimum level, the version conflict where it applies, business refusals, uniqueness in the database |
  | Aplicação (handler with fakes) | Only when the logic cannot be reached reasonably through HTTP: the passage of time, a failing dependency |

- **Never repeat** the same behavior in two layers.
- **Pieces without a test of their own** (service registration, configuration, a migration): list each one with the ID of the test that covers it.

## Issue body template

The title is in English, short and imperative, because the pull request title comes from it. The body is in Portuguese.

```markdown
## Contexto

<2 a 4 frases: o requisito ou o problema, e por que agora.>

## Escopo

- <item a implementar, com o ID da regra: USR-06>
- <pacote, migration ou mudança de contrato que esta issue autoriza, se houver>

## Fora do escopo

- <item> — <onde fica: issue #N ou "recomendação futura">

## Dependências

- <issue, decisão ou documento de que esta issue depende>

## Riscos

- <o que pode surpreender> — <como perceber>

## Decisões do levantamento

- <decisão tomada pelo dono> — <motivo, em uma frase>

## Testes

| ID | Teste | Camada | Regra | Resultado esperado |
|---|---|---|---|---|
| T01 | Deve rejeitar login com senha incorreta | API | AUTH-08 | 401 `invalid_credentials`, "Login ou senha incorretos." |
| T02 | ... | | | |

Sem teste próprio: <peça> — coberta por <ID>.

## Critérios de aceite

- [ ] Os testes T01 a Tnn passam.
- [ ] `check.ps1` termina sem avisos, com todos os testes do projeto verdes.
- [ ] <critério observável, específico desta issue>
- [ ] `docs/` atualizado no mesmo pull request: <arquivos>.

## Recomendações futuras

- <melhoria que não entra nesta issue, e por quê>
```

## Rules

- Write no code and no tests in this command.
- Never write the issue body to GitHub before the owner approves the draft. The `status:*` labels and the comments above are part of the normal flow.
- Never invent a business rule or a message: ask. A new message is approved by the owner with its exact text and enters the feature document, with a new rule ID, in the pull request of the execution.
- Never widen the scope without a justification.
- Whatever `AGENTS.md` lists under "Ask the owner first" must appear in **Escopo**: it is the authorization `/executar-issue` relies on to work without stopping.
