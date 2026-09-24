# ADR-0025: Enforce architecture rules with tests

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Clean Architecture (ADR-0005) only works if the dependency rules are respected. Over time it is easy to reference EF Core from the Domain, or an Infrastructure class from an endpoint, by accident.

## Decision

A `ControlService.ArchitectureTests` project uses **NetArchTest** to verify, on every build:

- Domain does not depend on Application, Infrastructure, Api, EF Core or ASP.NET Core.
- Application does not depend on Infrastructure, Api or EF Core.
- Api does not use Infrastructure types outside the composition root.
- Handlers, validators and endpoints follow the naming conventions (`...Handler`, `...Validator`, `...Endpoints`).

## Alternatives considered

- **Code review only.** Relies on attention; violations slip through.
- **ArchUnitNET.** More expressive, with a steeper learning curve; either would work.

## Consequences

- Architectural violations fail the build instead of being discovered months later.
- Legitimate exceptions must be made explicit in the tests.
