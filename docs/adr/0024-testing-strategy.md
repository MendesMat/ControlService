---
status: accepted
date: 2026-09-23
accepted: 2026-09-24
scope: back-end
tags: [quality, testing]
amended-by: ADR-0033 (test doubles and when tests are written)
---

# ADR-0024: Testing strategy

## Context

The business rules (CPF validation, uniqueness, effective permissions, protection of system records) are the core of the project. Tests are also one of the first things reviewers look at in a portfolio. Some popular test libraries changed licenses recently: FluentAssertions was licensed under Apache 2.0 up to version 7, and later versions require a paid license for commercial use.

## Decision

| Layer | What is tested | Tools |
|---|---|---|
| Domain | Value objects, aggregates, effective permission | xUnit, Shouldly |
| Application | Handlers and validators with fake dependencies | xUnit, Shouldly, NSubstitute |
| API integration | Real HTTP calls, authentication, authorization, persistence | `WebApplicationFactory`, Testcontainers (PostgreSQL, Mailpit; MinIO when ADR-0018 is implemented) |
| Architecture | Dependency rules between projects | NetArchTest (ADR-0025) |

- **xUnit v3** is the test framework, running on **Microsoft.Testing.Platform** (enabled in `global.json`). Coverage uses `Microsoft.Testing.Extensions.CodeCoverage`.
- **Shouldly** is used for assertions. AwesomeAssertions, the community fork of FluentAssertions 7, is an acceptable alternative.
- Integration tests run against **real containers**, not the EF Core in-memory provider, so SQL behavior, indexes and concurrency tokens are exercised.
- Test names describe behavior (`Cpf_with_all_equal_digits_is_rejected`).
- The account flows have integration tests that read the Mailpit inbox (ADR-0030): activation link sent on creation, link expired after 72 hours (using a fake `TimeProvider`), link invalid after use or after "resend access", identical response to password-reset requests for existing and unknown logins.
- The effective-permission rule has dedicated domain tests covering at least: `Denied` in one profile and `Editor` in another results in `Editor`; a screen missing from a profile counts as `Denied`; the Gerenciador profile results in `Manager` on every screen, including screens added later; an unknown profile id is ignored; a user with no profiles results in `Denied` on every screen.
- Code coverage is collected in CI and reported, without a hard threshold at first.

## Alternatives considered

- **FluentAssertions 8.** Rich API, but no longer free for commercial use.
- **EF Core in-memory provider or SQLite for integration tests.** Faster, but behaves differently from PostgreSQL.
- **Moq for mocks.** Popular; NSubstitute was chosen for its simpler syntax.

## Consequences

- Integration tests need Docker locally and in CI; GitHub-hosted Linux runners provide it.
- The test suite documents the business rules as executable examples.

## References

- [AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions)
