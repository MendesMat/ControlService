# Testing guide

The strategy is in ADR-0024. Tests are the executable version of the business rules, so they are part of every change, not an extra.

## Test projects

| Project | Tests | Doubles |
|---|---|---|
| `ControlService.Domain.Tests` | Value objects, aggregates, effective access | None: the domain has no dependencies |
| `ControlService.Application.Tests` | Handlers and validators | NSubstitute fakes for the Application interfaces |
| `ControlService.Api.IntegrationTests` | Real HTTP calls, authentication, authorization, persistence | `WebApplicationFactory`, Testcontainers (PostgreSQL, Mailpit) |
| `ControlService.ArchitectureTests` | Dependency rules between layers | None |

Framework: xUnit v3 on Microsoft.Testing.Platform, assertions with Shouldly. Common packages come from `tests/Directory.Build.props`; do not repeat them in each project.

## Commands

Run from `backend/ControlService`:

```bash
dotnet test --solution ControlService.slnx
```

One project:

```bash
dotnet test --project tests/ControlService.Domain.Tests
```

With coverage and reports, as in CI:

```bash
dotnet test --solution ControlService.slnx --coverage --coverage-output-format cobertura --report-xunit-trx
```

`dotnet test` uses Microsoft.Testing.Platform (enabled in `global.json`). VSTest options such as `--logger` or `--collect` do not work; use the options above. Exit code 8 means "zero tests ran".

## Writing tests

- **Name tests after the behavior**, with underscores: `Cpf_with_all_equal_digits_is_rejected`, `Denied_in_one_profile_and_editor_in_another_results_in_editor`.
- **Domain rules are written test-first:** a failing test for the rule, then the smallest code that passes, then refactor.
- Arrange, act, assert, in that order, one behavior per test. Use `[Theory]` with `[InlineData]` for tables of cases, such as valid and invalid CPFs.
- Control time with a fake `TimeProvider`; never depend on the real clock.
- Integration tests run against real containers, never the EF Core in-memory provider.
- Pass `TestContext.Current.CancellationToken` to async calls (xUnit analyzer rule).

## Mandatory cases (ADR-0024)

Effective permission: `Denied` in one profile and `Editor` in another gives `Editor`; a screen missing from a profile counts as `Denied`; the Gerenciador profile gives `Manager` on every screen, including new ones; an unknown profile id is ignored; no profiles gives `Denied` everywhere.

Account flows: activation link sent on creation; link expired after 72 hours; link invalid after use or after "resend access"; identical response to password-reset requests for existing and unknown logins.

Every endpoint: one allowed and one denied path for its minimum level; the 409 concurrency path for updates.

## Temporary settings to remove

`ControlService.Domain.Tests.csproj` and `ControlService.Application.Tests.csproj` contain a line marked `TEMPORARY` that accepts exit code 8 (no tests yet). Delete that line in the pull request that adds the first test to each project.
