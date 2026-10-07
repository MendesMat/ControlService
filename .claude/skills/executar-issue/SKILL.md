---
name: executar-issue
description: Execute a Control Service issue that was already surveyed - implement only the scope of the issue, test-first, without pausing for approval, run every numbered test and the whole suite, open the pull request and deliver the final report with the result of each test. Use when the owner asks for /executar-issue or says "execute a issue" or "executar a issue".
argument-hint: "<issue number>"
---

# /executar-issue

Second of the three commands (`AGENTS.md`, "Workflow: three commands"). Execution is **continuous**: never ask for approval test by test. The owner reviews afterwards, from the final report, so the report must let them check each test on its own.

Issue: `$ARGUMENTS`

If the issue body has no **Testes** section, stop and say so: `/levantar-issue` comes first.

Talk to the owner in Portuguese. The final report is written in Portuguese, from the template below.

## Steps

1. **Read the issue** (`gh issue view <n> --comments`) and the documents it cites.
2. **Read the related code again** before changing it: it may have changed since the survey.
3. **Prepare git:** the identity, an updated `main` and a new branch (`docs/agents/workflows/git-and-pull-requests.md`, steps 1 and 2).
4. **For each test of the list, in order,** one full cycle:
   - **Red:** write the test, with the comment `// #<n>-T01` on the line above it. Run it filtered and confirm that it fails **for the expected reason**. A compile error for a type that does not exist yet counts; a typo in the test does not.
   - **Green:** the smallest code that makes the test pass.
   - **Refactor:** with the tests green, remove duplication and improve names. "Nothing to refactor" is a valid result.
   - Commit at the end of a green cycle, or of a few cycles of the same behavior.
   - Note for the report: the test method, the file, and whether there was a Red.
5. **Run everything:** `powershell.exe -NoProfile -File check.ps1` in `backend/ControlService`. Fix what breaks, including older tests.
6. **Check end to end** when the issue delivers an endpoint or a screen: start the AppHost, exercise it and stop it.
7. **Update `docs/`** in the same pull request (`docs/agents/guides/documentation.md`).
8. **Open the pull request** with `Closes #<n>`, the title in English in the Conventional Commits format, and the test list with the result of each test. Wait for CI.
9. **Deliver the final report** and stop. The owner merges after `/revisar-issue`.

## When to stop in the middle

Only in these cases, explaining in the six points:

- A business rule that neither the issue nor `docs/` answers.
- The solution would need something the issue does not authorize: a package, abstraction, layer, pattern, migration, contract change, or a file to delete.
- The current architecture makes the requirement needlessly complex.
- The real code contradicts the issue in a way that changes the scope.

**A failing test is not a reason to stop:** fix it. After two failed attempts at the same problem, stop trying by trial and error and look for the root cause. If it stays unsolved, go on with the other tests and report that one as FALHOU, with what you tried.

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

Próximo passo: `/revisar-issue <n>`, numa conversa nova, **antes do merge**.
```
