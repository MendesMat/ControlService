---
name: revisar-issue
description: Review the implementation of a Control Service issue before the merge - compare the pull request with the issue, the acceptance criteria, the numbered tests, the business rules and the project decisions, and report the problems by severity, explained for a junior developer. Use when the owner asks for /revisar-issue or says "revise a issue" or "revisar a issue", preferably in a new conversation.
argument-hint: "<issue number>"
---

# /revisar-issue

Third of the three commands (`AGENTS.md`, "Workflow: three commands"). It works best in a **new conversation**: whoever wrote the code tends to defend it.

Issue: `$ARGUMENTS`

Talk to the owner in Portuguese. The review report is written in Portuguese, from the template below.

## Steps

1. **Read the issue** (`gh issue view <n> --comments`) and find the pull request that closes it (`gh pr list --state all --search "<n> in:body"`; confirm the `Closes #<n>`). Without a pull request, stop and say so.
2. **Read the pull request:** `gh pr view <pr>`, `gh pr diff <pr>` and the CI status.
3. **Read the documents** the issue cites. The review is against the written rule, not against a memory of it.
4. **Bring the code:** `gh pr checkout <pr>`, then run `powershell.exe -NoProfile -File check.ps1` in `backend/ControlService`.
5. **Check** the list below.
6. **Deliver the report** in the format below and stop. Change no code during the review.
7. **After the owner chooses** what to apply: new commits on the same branch, test-first; run `check.ps1`, push, wait for CI and report only what changed, with the result of the tests affected.

## What to check

- **Requirements:** the code does what each item of Escopo asks, and nothing beyond it.
- **Acceptance criteria:** one by one, met or not.
- **Tests:** each ID of the issue has a method with the comment `// #<n>-Txx`, and the test **asserts the expected result of the issue**, not only "no error". Check that what the execution report said is true.
- **Business rules:** the behavior matches the rule cited, including the error cases. Messages equal the ones in the document, character by character.
- **Bugs:** null and empty values, limits, the order of the checks, concurrency, the error path of each call.
- **Security:** the permission declared on each endpoint, personal data or passwords in logs and responses, unvalidated input, information that reveals whether a login exists.
- **Architecture:** a dependency in the wrong direction between layers, a business rule outside the domain, a departure from the pattern of the neighboring feature without a reason.
- **Needless complexity:** an abstraction, interface, layer or pattern the issue did not ask for and that `AGENTS.md` ("Proportionality") does not justify.
- **Duplication** and **maintenance:** repeated code, misleading names, comments that point to something the reader cannot open.
- **Code and documentation:** mismatches. When the document may be the wrong side, ask; never adjust `docs/` silently.
- **Delivery:** the commit identity, no attribution lines, one pull request per issue.

A more sophisticated solution is not better for being more sophisticated.

## Severity

| Severity | When |
|---|---|
| **Bloqueante** | A bug, a security flaw, a business rule violated, an acceptance criterion not met, a test that does not test what it says. It must not reach `main` |
| **Importante** | Nothing breaks today, but it costs later: needless complexity, duplication, a mismatch between code and documentation, an error path without a test |
| **Sugestão** | An optional improvement. It may become a future issue |

## Report

Most severe first.

```markdown
# Issue #<n> — <título>: revisão

Pull request: <link> · CI: <estado> · `check.ps1` local: <resultado>

**Veredito:** <"Pode ir para a main" ou "Corrigir antes do merge">, em uma frase.

## Critérios de aceite

| Critério | Atendido? |
|---|---|
| <critério da issue> | Sim / Não: <motivo> |

## Testes

| ID | Confere com a issue? | Observação |
|---|---|---|
| T01 | Sim | |
| T02 | Não | Afirma só o status; a issue pede também a mensagem |

## Achados

### 1. [Bloqueante] <título curto>

`<arquivo>:<linha>` · <ID da regra, se houver>

**Problema.** <O que está errado.>

**Por que isso importa.** <A consequência prática.>

**Opções.** A) <...> B) <...>

**Recomendação.** <Qual opção, e por quê, para este projeto.>

**Impacto.** <O que muda no código, na arquitetura ou na manutenção.>

## O que está bom

<2 ou 3 pontos concretos, para o dono saber o que repetir.>

## Decisões para você

<Lista numerada: os achados que dependem de uma escolha sua.>
```

With no finding in a severity, say "nenhum". Never invent a problem to fill the report.
