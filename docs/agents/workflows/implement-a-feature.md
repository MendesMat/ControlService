# Workflow: implement a feature

Use this workflow for any slice of the roadmap, for example "permission profiles" or "user deactivation". A slice goes from the business rule to a pull request, in small steps that each leave the build green.

## Sessions

An issue is done in three conversations, so each one starts with a small, focused context. A long conversation is paid again on every reply, and a fresh reviewer is not biased by code it wrote.

| Session | Sections | Ends with |
|---|---|---|
| **Plan** | 1 and 2 | The owner's answers and the approved test list, posted on the issue as the *plan comment* |
| **Build** | 3 to 8 | The pull request open and CI green, following the plan comment |
| **Review** | 9 | The findings fixed on the same branch; the owner merges |

The owner chooses the model and effort of each session ([working with agents](../trabalhando-com-agentes.md)). If a conversation does not say which session it is, ask.

**The plan comment** is the hand-over between sessions. It is written in English and posted with `gh issue comment <number> --body-file <file>` once the owner approves the test list. It contains:

- the owner's decisions, each with the rule ID or ADR it settles;
- the new messages and rule IDs to add to `docs/`;
- the test list, in order: test name, rule ID and the **expected result** (the status, code, message or value the test asserts), so the build session does not have to decide it;
- the files to create or change, and what is out of scope.

Check every decision against the rule IDs it touches before posting. A vague decision ("an unmapped code becomes a 500") is filled in by guesswork later, and the guess can contradict a rule (API-12 in issue #6).

The build session starts from the issue and its plan comment instead of repeating the survey, and reads the documents the plan cites. When the plan is wrong or incomplete, it stops and asks; it never replaces a planned approach silently.

## 1. Understand the rule

1. Read the feature document in `docs/product/features/`, plus `docs/product/conventions.md` and `docs/api/conventions.md`. For a new screen without a document, create it from `docs/product/features/template.md` and have the owner confirm the rules before coding.
2. Read the ADRs listed at the top of the feature document (the index is in `docs/adr/README.md`).
3. Check `docs/product/open-questions.md`: if the slice depends on an open question, ask the owner before coding.
4. List the rule IDs the slice implements (for example USR-01 to USR-13): they become your checklist of tests. If anything is ambiguous or contradictory, ask the owner (see the [communication guide](../guides/communication.md)).
5. For every rejection or error case in the slice (an invalid value, a value outside a closed list, a missing record), confirm the feature document gives its exact message. List the ones without a message and ask the owner **before** showing the test list, so the gaps are settled in one conversation instead of interrupting a Red or Green phase. A new message becomes a new rule ID in the feature document, in the same pull request.
6. Check that each rule ID you will cite exists once in its document. A duplicated ID is a documentation bug: report it with a proposal (the later rule takes the next free number).
7. Report the survey to the owner before the test list: the documents you read, the gaps from steps 4 and 5, and any inconsistency you found, each with your recommendation. Wait for the answers; do not fill a gap with an assumption.

## 2. Plan the slice

- Split it into steps that can each be committed with passing tests, inside out: **domain → application → infrastructure → API**.
- Write the **test list** of the [TDD workflow](test-driven-development.md): one-line test names, simplest first, each with its rule ID and expected result. Show it to the owner in Portuguese, together with the files you expect to create, and wait for approval. Then post the [plan comment](#sessions).
- Scope every type to what the slice's tests demand. An aggregate gets a field only when a test of this slice needs it; the fields of later issues come with those issues, driven by their tests. Tests create such objects through one helper, so a factory that grows later changes one place.
- Create the branch ([git and pull requests](git-and-pull-requests.md), steps 1 and 2).

Every step below follows the [TDD workflow](test-driven-development.md): one failing test, the smallest code that passes, refactor, and **one pause at the end of each cycle** in pair mode (ADR-0033).

## 3. Domain

For each rule: a failing unit test in `ControlService.Domain.Tests`, then the value object, aggregate method or domain service that makes it pass, then refactor. Commit after green cycles.

Validation messages come verbatim from the feature document. Each rule ID must be covered by at least one test; cite the ID in a comment when the test name does not make it obvious.

## 4. Application

1. Start from a failing handler or validator test in `ControlService.Application.Tests`: the success path, each expected failure (`Result` errors) and the observable outcome (what is saved, what is sent).
2. Let the test drive the use case folder (command or query, handler, validator) and the interfaces the handler needs (repository, clock, e-mail) in the Application project.
3. Use hand-written in-memory fakes for those interfaces (ADR-0033). Commit after green cycles.

## 5. Infrastructure

1. Start from a failing integration test against real PostgreSQL (Testcontainers): the repository saves and reads the aggregate, a unique index refuses a duplicate, a concurrency conflict is detected.
2. Implement the EF Core configuration and repository that make it pass. Generate migrations with the EF Core CLI and review them before committing (the migration itself has no Red: say so).
3. Register services in the Infrastructure's service registration method; the integration tests cover this wiring. Commit.

## 6. API

1. Start from a failing integration test with `WebApplicationFactory` for each route in the *Operations* section of the feature document: the status code, body and error codes of `docs/api/conventions.md`.
2. Add or extend `{Feature}Endpoints.cs` until it passes, declaring the permission of each endpoint (`.RequireScreenAccess(...)`) with the minimum level in the same table (PERM-03) and mapping results as in ADR-0009.
3. Cover the allowed path, the denied path for the minimum level, validation errors and the concurrency conflict where applicable. Commit.

## 7. Verify end to end

1. Build and run all tests.
2. Start the AppHost in the background, exercise the endpoints (Scalar or the `.http` file), check the e-mails in Mailpit if the slice sends any, and stop the AppHost.

## 8. Document and deliver

1. Update `docs/`, the ADRs and the README roadmap as described in the [documentation guide](../guides/documentation.md).
2. Open the pull request and hand it over to the owner ([git and pull requests](git-and-pull-requests.md), steps 5 to 7). The build session ends here.

## 9. Review before merge

The review session starts with a new conversation, before the owner merges. It reads the pull request (`gh pr view <number>`, `gh pr diff <number>`), the issue with its plan comment, and the documents they cite. It checks:

1. Every rule ID of the plan is covered by a test, and the code does what the rule says, not only what the test asserts.
2. The code matches `docs/` and the accepted ADRs. A mismatch is fixed in the code, or reported to the owner when the document may be the wrong one.
3. What the pull request and the tests claim is true: a comment such as "covered by `ApiStartupTests`" names a test that really exercises that code.
4. The scope of the issue is complete, and nothing outside it slipped in.
5. The delivery rules: commits per green cycle, the author identity, no attribution lines in commits or in the pull request body.

Fixes are new commits on the same branch, with the same TDD rules. Problems that belong to another issue are reported, not fixed. Then hand the pull request over again: the owner merges.
