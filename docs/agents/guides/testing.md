# Testing guide

The strategy is in ADR-0024, and all development is test-first. Tests are the executable version of the business rules, so they are part of every change, not an extra. Each issue carries a numbered test list ([AGENTS.md](../../../AGENTS.md#tests)); **how** to write the tests, one failing test at a time, is in the [`/executar-issue` skill](../../../.claude/skills/executar-issue/SKILL.md).

## Test projects

| Project | Tests | Doubles |
|---|---|---|
| `ControlService.Domain.Tests` | Value objects, aggregates, effective access | None: the domain has no dependencies |
| `ControlService.Application.Tests` | Handlers and validators | Hand-written in-memory fakes of the Application interfaces; NSubstitute only when a fake would be clearly heavier |
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
- **Every behavior starts as a failing test,** and is tested in one layer only: the cheapest one that proves the rule ([AGENTS.md](../../../AGENTS.md#tests)).
- **Each test of an issue carries its ID** in a comment on the line above it (`// #10-T03`), so the owner can find it from the issue.
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

## Persistence tests

Since `Program` needs a database to start, **every** `WebApplicationFactory` in `ControlService.Api.IntegrationTests` shares one PostgreSQL container: `Common/PostgresContainerFixture.cs` is an xUnit v3 **assembly fixture** (`[assembly: AssemblyFixture(...)]`), started once and stopped after the whole assembly runs. `Common/ApiFactory.cs` is the shared base every factory derives from (including `ErrorEndpointsFactory`); it wires the container's connection string as `ConnectionStrings:controlservice` and `Admin:Email` = `admin@example.com`, and forces `Development` so migrations and seeding run (ADR-0013). This needs **Docker running locally and in CI**.

Because every factory targets the same database, test collections run **sequentially** (`xunit.runner.json`, `parallelizeTestCollections: false`): two hosts racing to seed the system records at the same time would violate `pk_users`. `Xunit.CollectionBehaviorAttribute.DisableTestParallelization` is obsolete in xunit v3; use the JSON setting instead.

- **No cleanup package.** Each test creates its own data with unique logins, e-mails and display names, so a uniqueness test violates only the index under test. Reusing a name another test already inserted (`"Ana Souza"`, `"Bruno Lima"`) fails on the wrong constraint. When a scenario needs a specific name the plan calls for (D5's `"Bruno Lima"`), get-or-create it by a query instead of inserting it again from every test.
- **Starting the host before mutating a fake.** `WebApplicationFactory.Services` builds and starts the host lazily on first access, which is also when migrations and seeding run. A test that sets `PersistenceApiFactory.CurrentUser` to `null` or an unknown id *before* touching `.Services` makes seeding itself fail (it also needs a signed-in "user", the Admin) instead of the save the test means to target. Access `.Services` (or open a scope) once under the default Admin first.
- `Persistence/PersistenceApiFactory.cs` replaces `ICurrentUser` and `TimeProvider` with `FakeCurrentUser` and `FixedTimeProvider`, so authorship and timestamps are asserted precisely instead of against the real clock.
- Assert database errors through `DbUpdateException.InnerException` as `Npgsql.PostgresException` (`SqlState`, `ConstraintName`). PostgreSQL reports an immediate `ON DELETE RESTRICT` violation as `23001` (`restrict_violation`), not `23503` (`foreign_key_violation`, which `NO ACTION` raises instead) — check which delete behavior a foreign key uses before asserting a code.

## Authentication tests

- **`Auth/AuthApiFactory`** is the API with a controllable clock (`Clock`, a `FixedTimeProvider` with `Advance`), so sessions and access tokens expire when the test says. `CreateHttpsClient()` talks to `https://localhost` with cookie handling off: the refresh cookie is `Secure`, so it never comes back over `http`, and tests read `Set-Cookie` and send each session's `Cookie` header themselves (`Auth/AuthHttp.cs`).
- **`Auth/RateLimitedApiFactory`** lowers both rate limits to 2 requests. `ApiFactory` sets them to 1000 so no other test hits them.
- `ApiFactory` also wires fictitious values for `Admin:InitialPassword` and `Auth:SigningKey`. The real ones live in user secrets and never in source.
- **The Admin is one row shared by the whole test assembly**, so a test that signs in as the Admin or changes its password starts with `AuthTestSupport.ResetAdminAsync` (initial password, mandatory change, no sessions, no activation time). `AuthTestSupport.CreateUserAsync` creates an active user with a credential under a unique login; `GrantAsync` and `DeactivateUserAsync` change what that user may do.
- **Any other authenticated endpoint** is stood in for by `/api/v1/test-only/protected`, mapped through `MapApiV1()` by a startup filter (that filter needs its own `UseRouting`, `UseAuthentication` and `UseAuthorization`).
- `Auth/CapturingLoggerProvider` keeps every log line, prefixed by its level, so a test can assert that no password, token or cookie value was written. To capture below `Information`, add a filter rule for the provider (`AddFilter<CapturingLoggerProvider>(null, LogLevel.Trace)`): `SetMinimumLevel` applies only when no rule matches, and `appsettings.json` always matches with `Default: Information`.
- In `ControlService.Application.Tests`, `Auth/AuthTestBed` builds the handlers over the hand-written fakes in `Fakes/` (in-memory repositories, `FakeCredentialStore`, `InMemorySessionStore`, `FakeAccessTokenIssuer`, `FakeUnitOfWork`, `FixedTimeProvider`). Lockout counting is Identity's job, so the fake store scripts the outcome and the real counting is tested against PostgreSQL (`CredentialStoreTests`).

