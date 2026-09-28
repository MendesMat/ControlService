# Testing guide

The strategy is in ADR-0024, and ADR-0033 makes all development test-first. Tests are the executable version of the business rules, so they are part of every change, not an extra. **How** to write them, one failing test at a time, is in the [test-driven development workflow](../workflows/test-driven-development.md).

## Test projects

| Project | Tests | Doubles |
|---|---|---|
| `ControlService.Domain.Tests` | Value objects, aggregates, effective access | None: the domain has no dependencies |
| `ControlService.Application.Tests` | Handlers and validators | Hand-written in-memory fakes of the Application interfaces (ADR-0033); NSubstitute only when a fake would be clearly heavier |
| `ControlService.Api.IntegrationTests` | Real HTTP calls, authentication, authorization, persistence; also unit tests of the API's own helpers, such as the `Error` → Problem Details table (`Common/ErrorResultsTests.cs`), since there is no API unit test project | `WebApplicationFactory`, Testcontainers (PostgreSQL, Mailpit) |
| `ControlService.ArchitectureTests` | Dependency rules between layers | None |

Framework: xUnit v3 on Microsoft.Testing.Platform, assertions with Shouldly. Common packages come from `tests/Directory.Build.props`; do not repeat them in each project.

## Commands

Run from `backend/ControlService`. `check.ps1` formats, builds and tests, printing only problems and summaries; a failing run keeps the whole output, as evidence of the Red:

```bash
powershell.exe -NoProfile -File check.ps1 -Project tests/ControlService.Domain.Tests -Filter '*Cpf*'
```

The plain commands, when you need their full output:

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
- **Every behavior starts as a failing test,** in every layer ([TDD workflow](../workflows/test-driven-development.md)).
- **Test behavior, not implementation:** assert on outcomes through the public API. Fakes live in the test project (for example `Fakes/InMemoryUserRepository.cs`) and are reused across tests.
- Arrange, act, assert, in that order, one behavior per test. Use `[Theory]` with `[InlineData]` for tables of cases, such as valid and invalid CPFs.
- Control time with a fake `TimeProvider`; never depend on the real clock.
- Integration tests run against real containers, never the EF Core in-memory provider.
- Pass `TestContext.Current.CancellationToken` to async calls (xUnit analyzer rule).

## Mandatory cases (ADR-0024)

Effective permission: `Denied` in one profile and `Editor` in another gives `Editor`; a screen missing from a profile counts as `Denied`; the Gerenciador profile gives `Manager` on every screen, including new ones; an unknown profile id is ignored; no profiles gives `Denied` everywhere.

Account flows: activation link sent on creation; link expired after 72 hours; link invalid after use or after "resend access"; identical response to password-reset requests for existing and unknown logins.

Every endpoint: one allowed and one denied path for its minimum level; the 409 concurrency path for updates.

## API tests without a feature endpoint

- **Test-only endpoints.** To exercise the real pipeline before a feature exists, register an `IStartupFilter` in a `WebApplicationFactory` (see `Common/ErrorEndpointsFactory.cs`). The filter receives a plain `ApplicationBuilder`, not an `IEndpointRouteBuilder`: call `next(app)`, then `app.UseRouting()` and `app.UseEndpoints(...)`. Those endpoints run after `UseExceptionHandler`, so a thrown exception gets the real 500 response.
- **Executing an `IResult` without a server.** A bare `DefaultHttpContext` has no services; give it `RequestServices` built from `new ServiceCollection().AddLogging().AddProblemDetails()` and a `MemoryStream` as `Response.Body`.
- A new test project with no tests yet fails with exit code 8 ("zero tests ran"). Add the first test in the same pull request that creates the project.
