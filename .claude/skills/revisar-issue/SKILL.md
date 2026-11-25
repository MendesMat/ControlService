---
name: revisar-issue
description: Review the implementation of a Control Service issue before the merge - compare the pull request with the issue, the acceptance criteria, the numbered tests, the business rules and the project decisions, and report the problems by severity, explained for a junior developer, as a pull request comment. It changes no code. Use when the owner asks for /revisar-issue or says "revise a issue" or "revisar a issue", preferably in a new conversation.
argument-hint: "<issue number>"
---

# /revisar-issue

Third of the three commands (`AGENTS.md`, "Workflow: three commands"). It works best in a **new conversation**: whoever wrote the code tends to defend it.

**The review only reports.** It changes no code, makes no commit and pushes nothing. The fixes the owner chooses are made by `/executar-issue`, in re-execution, in a new conversation, and then reviewed again.

Issue: `$ARGUMENTS`

Talk to the owner in Portuguese. The review report is written in Portuguese, from the template below.

## Steps

1. **Read the issue** (`gh issue view <n> --comments`) and find the pull request that closes it (`gh pr list --state all --search "<n> in:body"`; confirm the `Closes #<n>`). Without a pull request, stop and say so. If the pull request is already merged, say so in the first line of the report and review `main` at the merge commit: in step 4, update `main` instead of checking out the pull request.
2. **Check the entry state** (`AGENTS.md`, "Issue states"): the issue must be in `status:in-review`. In any other state, say which state it is in and which command fits, and stop; go on only on the owner's explicit order in the conversation, recorded first in a comment on the issue (`gh issue comment <n> --body-file <file>`). A pull request that is already merged is reviewed whatever the label.
3. **Read the pull request:** `gh pr view <pr> --comments`, `gh pr diff <pr>` and the CI status. The comments hold the execution report and any earlier review.
4. **Read the documents** the issue cites. The review is against the written rule, not against a memory of it.
5. **Bring the code:** `gh pr checkout <pr>`, then run `powershell.exe -NoProfile -File check.ps1` in `backend/ControlService`.
6. **Check** the list below.
7. **Publish the report** on the pull request: write it to a file outside the repository and run `gh pr comment <pr> --body-file <file>`.
8. **Mark the issue** from the verdict: "Pode ir para a main" → `gh issue edit <n> --remove-label status:in-review --add-label status:approved`; "Corrigir antes do merge" → `--add-label status:changes-requested` instead. For a pull request already merged, change no label.
9. **Deliver the report** in the conversation, with the link to the comment and the new label, and stop. The next step goes in the last line of the report:
   - `status:approved`: the owner merges.
   - `status:changes-requested`: the owner says which findings to apply, in a pull request comment or in the next conversation, and runs `/executar-issue <n>` in a new conversation.
   - pull request already merged: the findings the owner chooses become a new issue, through `/levantar-issue`.

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

Estado da issue: `<status:approved ou status:changes-requested>`. Próximo passo: <o merge, feito por você; ou "diga quais achados aplicar e rode `/executar-issue <n>` numa conversa nova"; ou, com o pull request já mesclado, "os achados escolhidos viram uma issue nova, por `/levantar-issue`">.
```

With no finding in a severity, say "nenhum". Never invent a problem to fill the report.
