---
name: executar-issue
description: Execute a Control Service issue that was already surveyed - implement only the scope of the issue, test-first, without pausing for approval, run every numbered test and the whole suite, open the pull request and deliver the final report with the result of each test. In re-execution, apply the review findings the owner chose. Use when the owner asks for /executar-issue or says "execute a issue" or "executar a issue".
argument-hint: "<issue number>"
---

# /executar-issue

Second of the three commands (`AGENTS.md`, "Workflow: three commands"). Execution is **continuous**: never ask for approval test by test. The owner reviews afterwards, from the final report, so the report must let them check each test on its own.

Issue: `$ARGUMENTS`

Talk to the owner in Portuguese. The final report is written in Portuguese, from the template below.

## Before starting

The states and their rules are in `AGENTS.md`, "Issue states".

1. **Check the entry state:** `gh issue view <n> --json state,labels`.
   - `status:ready`: a first execution; follow "Steps".
   - `status:changes-requested`: a re-execution; follow "Re-execution".
   - Any other state: say which state the issue is in and which command fits, and stop. Go on only on the owner's explicit order in the conversation, and record it first: `gh issue comment <n> --body-file <file>`, quoting the order.
2. **Check that no other issue is in flight:** `gh issue list --state open --search 'label:"status:in-progress","status:in-review","status:changes-requested","status:approved"'`. If an issue other than `<n>` appears, remind the owner that one issue goes to `main` at a time, and ask whether to go on. This is the only question before the execution starts.
3. **Check the dependencies:** every issue listed under "Dependências" must be closed (`gh issue view <m> --json state`). If one is open, stop and say which; the label stays as it is.
4. **Check the test list:** if the issue body has no **Testes** section, stop and say so: `/levantar-issue` comes first.

## Steps

1. **Read the issue** (`gh issue view <n> --comments`) and the documents it cites.
2. **Read the related code again** before changing it: it may have changed since the survey.
3. **Prepare git:** the identity, an updated `main` and a new branch (`docs/agents/workflows/git-and-pull-requests.md`, steps 1 and 2). Then mark the issue: `gh issue edit <n> --remove-label status:ready --add-label status:in-progress`.
4. **For each test of the list, in order,** one full cycle:
   - **Red:** write the test, with the comment `// #<n>-T01` on the line above it. Run it filtered and confirm that it fails **for the expected reason**. A compile error for a type that does not exist yet counts; a typo in the test does not.
   - **Green:** the smallest code that makes the test pass.
   - **Refactor:** with the tests green, remove duplication and improve names. "Nothing to refactor" is a valid result.
   - Commit at the end of a green cycle, or of a few cycles of the same behavior.
   - Note for the report: the test method, the file, and whether there was a Red.
5. **Run everything:** `powershell.exe -NoProfile -File check.ps1` in `backend/ControlService`. Fix what breaks, including older tests.
6. **Check end to end** when the issue delivers an endpoint or a screen: start the AppHost, exercise it and stop it.
7. **Update `docs/`** in the same pull request (`docs/agents/guides/documentation.md`).
8. **Open the pull request** with `Closes #<n>`, the title in English in the Conventional Commits format, and the test list with the result of each test. Wait for CI. If CI stays red after you looked for the root cause, publish the report with "CI: vermelho" (step 9), set `status:blocked` instead of `status:in-review`, and say why.
9. **Publish the final report** on the pull request: write it to a file outside the repository and run `gh pr comment <pr> --body-file <file>`.
10. **Mark the issue** `gh issue edit <n> --remove-label status:in-progress --add-label status:in-review`, **deliver the final report** in the conversation with the link to the comment and the new label, and stop. The owner merges after `/revisar-issue`.

## When to stop in the middle

Only in these cases, explaining in the six points:

- A business rule that neither the issue nor `docs/` answers.
- The solution would need something the issue does not authorize: a package, abstraction, layer, pattern, migration, contract change, or a file to delete.
- The current architecture makes the requirement needlessly complex.
- The real code contradicts the issue in a way that changes the scope.

To stop: commit and push the partial work to the branch, **without opening a pull request** (an open pull request suggests work ready for review); comment on the issue with the reason in the six points and the branch name (`gh issue comment <n> --body-file <file>`); then `gh issue edit <n> --remove-label status:in-progress --add-label status:blocked`, and tell the owner in the conversation, with the new label.

**A failing test is not a reason to stop:** fix it. After two failed attempts at the same problem, stop trying by trial and error and look for the root cause. If it stays unsolved, go on with the other tests and report that one as FALHOU, with what you tried.

## Re-execution

When the issue is in `status:changes-requested`, the pull request is already open and a review asked for changes.

1. **Find the pull request** (`gh pr list --state open --search "<n> in:body"`, confirming the `Closes #<n>`) and bring its branch: `gh pr checkout <pr>`, then `git pull`.
2. **Read the latest review report:** `gh pr view <pr> --comments`. A pull request may have several reviews; use the **last** comment that starts with `# Issue #<n> — <título>: revisão`, not the first.
3. **Read the owner's choices:** the pull request comments posted after that report, or the request in this conversation. If neither says which findings to apply, ask. **A finding the owner did not choose is not applied.**
4. **Mark the issue:** `gh issue edit <n> --remove-label status:changes-requested --add-label status:in-progress`.
5. **Fix each chosen finding** on the same branch, test-first, with the rules below. A new test takes the next free ID of the issue list. Commit per finding.
6. **Run `check.ps1`**, push and wait for CI.
7. **Publish the re-execution report** on the pull request (`gh pr comment <pr> --body-file <file>`), from the template below.
8. **Mark the issue** `gh issue edit <n> --remove-label status:in-progress --add-label status:in-review`, deliver the report in the conversation with the new label, and stop.

## Rules for the tests

- One test of the list is one test method (or a `[Theory]`, for several examples of the same behavior), with the ID in the comment.
- The method name is in English and describes the behavior: `Wrong_password_is_refused`.
- Messages are copied from the feature document, never written from memory.
- Use hand-written fakes instead of mocks, a fake `TimeProvider` instead of the real clock, and fictitious data only.
- **A test that passes at once** (another cycle already covered the rule): check that it would fail if the rule broke, keep it and mark it "sem Red" in the report. Never invent an artificial Red.
- **A new test discovered during the execution** takes the next free number and is reported as "adicionado na execução", with the reason.
- **A test of the issue that had to change** keeps its ID, and the report says what changed and why.
- **A test that makes no sense or could not be written** is PULADO, with the reason. Never delete one silently, and never weaken a test to make it pass.
- **Bug fix:** the first test reproduces the bug and fails for the reason the report describes.

## Final report

In this format and order. The IDs and the sentences of the tests are the ones of the issue, unchanged.

```markdown
# Issue #<n> — <título>: relatório de execução

Pull request: <link> · CI: <verde ou vermelho> · Branch: `<nome>`

## Implementação

<3 a 6 tópicos: o que mudou, por camada ou por arquivo.>

## Testes

Resumo: <x> de <y> passaram, <f> falharam, <p> pulados.

| ID | Resultado | Teste | Método de teste |
|---|---|---|---|
| T01 | PASSOU | Deve rejeitar login com senha incorreta | `SignInTests.Wrong_password_is_refused` |
| T02 | FALHOU | ... | ... |
| T03 | PULADO | ... | — |

- **Falhas:** <ID: o motivo e o que foi tentado>, ou "nenhuma".
- **Pulados:** <ID: o motivo>, ou "nenhum".
- **Correções feitas depois de uma falha:** <ID: o que estava errado e o que mudou>, ou "nenhuma".
- **Testes alterados em relação à issue:** <ID: o que mudou e por quê>, ou "nenhum".
- **Testes adicionados na execução:** <ID: o motivo>, ou "nenhum".
- **Sem Red:** <IDs>, ou "nenhum".

## Regressões

Suíte completa: <total> testes, <resultado>. Testes antigos alterados: <quais e por quê>, ou "nenhum".

## Arquitetura

Classes, interfaces, camadas, padrões ou pacotes novos: <cada um com o porquê>, ou "nenhum: seguiu o padrão de <funcionalidade>".

## Observações

Problemas fora do escopo, que não foram alterados: <lista>, ou "nenhum".

Estado da issue: `status:in-review`. Próximo passo: `/revisar-issue <n>`, numa conversa nova, **antes do merge**.
```

## Re-execution report

```markdown
# Issue #<n> — <título>: reexecução

Pull request: <link> · CI: <verde ou vermelho> · Revisão respondida: <link do comentário da revisão>

## Achados aplicados

| Achado | O que mudou | Testes afetados | Resultado |
|---|---|---|---|
| 1. <título do achado> | <arquivo e mudança> | T03; T12 (adicionado na reexecução) | PASSOU |

Achados não aplicados, porque o dono não os escolheu: <números>, ou "nenhum".

## Regressões

Suíte completa: <total> testes, <resultado>. Testes antigos alterados: <quais e por quê>, ou "nenhum".

Estado da issue: `status:in-review`. Próximo passo: `/revisar-issue <n>`, numa conversa nova, **antes do merge**.
```
