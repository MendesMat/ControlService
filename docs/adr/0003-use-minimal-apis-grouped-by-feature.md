---
status: accepted
date: 2026-09-23
accepted: 2026-09-24
scope: back-end
tags: [platform, api]
---

# ADR-0003: Use Minimal APIs grouped by feature

## Context

ASP.NET Core offers two ways to expose HTTP endpoints: MVC Controllers and Minimal APIs. Controllers are still common in existing enterprise systems. Minimal APIs are the default in current project templates and have received most of the recent investment (endpoint filters, typed results, native OpenAPI support).

The front-end is organized by screens (Usuários, Permissões, and 18 more to come). Each screen maps naturally to one back-end feature.

## Decision

We will use **Minimal APIs**, organized **by feature**. Each feature (for example `Users` or `PermissionProfiles`) lives in its own folder in the API project and registers its endpoints through a route group created with `MapGroup`.

- All routes share the `/api/v1` prefix. The version segment exists from day one so a future breaking change can live under `/api/v2`.
- Endpoints return **typed results** (`Results<Ok<T>, NotFound, ValidationProblem>`), which keeps responses explicit and feeds the OpenAPI document.
- Cross-cutting behavior (validation, authorization) is applied per group through **endpoint filters** and metadata, not repeated in each endpoint.

## Alternatives considered

- **MVC Controllers.** Familiar and well documented, and worth knowing for existing codebases. They add attributes and base classes that Minimal APIs no longer need.
- **A third-party endpoint library (for example FastEndpoints or Carter).** Adds structure, but also a dependency that the built-in `MapGroup` already covers.

## Consequences

- The API code mirrors the screens of the front-end, which makes it easy to find where a behavior lives.
- The project demonstrates the current ASP.NET Core style; the team should still be able to explain how the same thing is done with Controllers.
- A small convention (an extension method per feature, such as `MapUserEndpoints`) is needed to keep `Program.cs` short.
