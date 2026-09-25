# Workflow: implement a feature

Use this workflow for any slice of the roadmap, for example "permission profiles" or "user deactivation". A slice goes from the business rule to a pull request, in small steps that each leave the build green.

## 1. Understand the rule

1. Read the relevant parts of `docs/02-modelo-de-dados.md`, `docs/03-regras-de-negocio.md`, `docs/04-permissoes.md` and `docs/05-integracao-com-o-front.md`.
2. Read the ADRs that apply (the index is in `docs/adr/README.md`).
3. Check `docs/07-pendencias.md`: if the slice depends on an open question, ask the owner before coding.
4. Write down, for yourself, the rules, validations, messages, routes, status codes and error codes the slice must implement. If anything is ambiguous or contradictory, ask the owner (see the [communication guide](../guides/communication.md)).

## 2. Plan the slice

- Split it into steps that can each be committed with passing tests, inside out: **domain → application → infrastructure → API**.
- For a large slice, share the plan with the owner before starting, in Portuguese, with the files you expect to create.
- Create the branch ([git and pull requests](git-and-pull-requests.md), steps 1 and 2).

## 3. Domain, test first

For each rule:

1. Write a failing test in `ControlService.Domain.Tests` named after the behavior.
2. Write the smallest domain code that makes it pass: a value object, an aggregate method or a domain service.
3. Refactor with the tests green. Commit.

Validation messages come verbatim from `docs/03-regras-de-negocio.md`.

## 4. Application

1. Create the use case folder: command or query, handler, validator.
2. Declare the interfaces the handler needs (repository, clock, e-mail) in the Application project.
3. Test the handler in `ControlService.Application.Tests` with NSubstitute fakes: the success path, each expected failure (`Result` errors) and the orchestration (what is saved, what is sent). Commit.

## 5. Infrastructure

1. Implement the interfaces: EF Core configuration, repository, migration, e-mail sender.
2. Generate migrations with the EF Core CLI and review the generated SQL-relevant code before committing.
3. Register services in the Infrastructure's service registration method. Commit.

## 6. API

1. Add or extend `{Feature}Endpoints.cs` with the routes from `docs/05-integracao-com-o-front.md`.
2. Declare the permission of each endpoint (`.RequireScreenAccess(...)`), following `docs/04-permissoes.md`.
3. Map results to the status codes and error codes of ADR-0009.
4. Write integration tests: allowed path, denied path for the minimum level, validation errors, concurrency conflict where applicable. Commit.

## 7. Verify end to end

1. Build and run all tests.
2. Start the AppHost in the background, exercise the endpoints (Scalar or the `.http` file), check the e-mails in Mailpit if the slice sends any, and stop the AppHost.

## 8. Document and deliver

1. Update `docs/`, the ADRs and the README roadmap as described in the [documentation guide](../guides/documentation.md).
2. If the first test was added to a test project, remove its `TEMPORARY` exit-code line ([testing guide](../guides/testing.md)).
3. Open the pull request and hand it over to the owner ([git and pull requests](git-and-pull-requests.md), steps 5 to 7).
