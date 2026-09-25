# Workflow: implement a feature

Use this workflow for any slice of the roadmap, for example "permission profiles" or "user deactivation". A slice goes from the business rule to a pull request, in small steps that each leave the build green.

## 1. Understand the rule

1. Read the feature document in `docs/product/features/`, plus `docs/product/conventions.md` and `docs/api/conventions.md`. For a new screen without a document, create it from `docs/product/features/template.md` and have the owner confirm the rules before coding.
2. Read the ADRs listed at the top of the feature document (the index is in `docs/adr/README.md`).
3. Check `docs/product/open-questions.md`: if the slice depends on an open question, ask the owner before coding.
4. List the rule IDs the slice implements (for example USR-01 to USR-13): they become your checklist of tests. If anything is ambiguous or contradictory, ask the owner (see the [communication guide](../guides/communication.md)).

## 2. Plan the slice

- Split it into steps that can each be committed with passing tests, inside out: **domain → application → infrastructure → API**.
- Write the **test list** of the [TDD workflow](test-driven-development.md): one-line test names, simplest first, each with its rule ID. Show it to the owner in Portuguese, together with the files you expect to create, and wait for approval.
- Create the branch ([git and pull requests](git-and-pull-requests.md), steps 1 and 2).

Every step below follows the [TDD workflow](test-driven-development.md): one failing test, the smallest code that passes, refactor, **pause after each phase** in pair mode.

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
2. If the first test was added to a test project, remove its `TEMPORARY` exit-code line ([testing guide](../guides/testing.md)).
3. Open the pull request and hand it over to the owner ([git and pull requests](git-and-pull-requests.md), steps 5 to 7).
