---
status: accepted
date: 2026-09-23
accepted: 2026-09-24
scope: back-end
tags: [architecture]
---

# ADR-0005: Adopt Clean Architecture with four projects

## Context

The ERP will grow from two screens to more than twenty, with business rules that must not depend on the web framework or the database. Clean Architecture is also one of the most requested architectural styles in .NET job descriptions, which makes it a good fit for a portfolio.

## Decision

The solution will have four production projects, with dependencies pointing inward only:

| Project | Responsibility | May reference |
|---|---|---|
| `ControlService.Domain` | Entities, value objects, domain rules and domain errors. No framework dependencies. | Nothing |
| `ControlService.Application` | Use cases (commands and queries), validation, interfaces for infrastructure (repositories, storage, current user). | Domain |
| `ControlService.Infrastructure` | EF Core, Dapper, object storage, identity, caching: implementations of Application interfaces. | Application, Domain |
| `ControlService.API` | HTTP endpoints, authentication setup, composition root. | Application, Infrastructure (for DI registration only) |

Test projects mirror these (`ControlService.Domain.Tests`, `ControlService.Application.Tests`, `ControlService.Api.IntegrationTests`, `ControlService.ArchitectureTests`). Local orchestration adds the Aspire projects described in ADR-0027.

Inside every project, code is organized **by feature** (`Access`, `Auth`, `Users`, `PermissionProfiles`, `Screens`), not by technical type. The folder names tell what the system does, while the project boundaries enforce the dependency rule.

## Alternatives considered

- **Vertical Slice Architecture in a single project.** Less ceremony and increasingly popular; it would be a valid choice. Clean Architecture was preferred because the layering itself is part of what the portfolio should demonstrate.
- **A single project with folders.** Simplest, but nothing prevents the domain from depending on EF Core or ASP.NET Core.
- **One project per menu area (Management, Commercial, Finance, Reporting).** Tried first. It puts walls between business areas instead of between layers, so nothing stops a domain class from using EF Core. The areas also depend on each other constantly (Finance needs Commercial clients, Reporting reads everything), sign-in and permissions belong to no single area, and the menu grouping is a presentation concern that may change (see ADR-0021). Discarded before any code was written.

## Consequences

- Business rules can be tested without a database or web server.
- Adding a feature touches several projects; the by-feature folders keep related files easy to find.
- The dependency rules are enforced automatically by architecture tests (ADR-0025).
