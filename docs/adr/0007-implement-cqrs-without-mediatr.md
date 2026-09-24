# ADR-0007: Implement CQRS with in-house handlers instead of MediatR

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Separating commands (changes) from queries (reads) keeps use cases small and makes read paths easy to optimize. In .NET this was usually done with MediatR. MediatR and AutoMapper launched commercial editions on July 2, 2025: version 12 and earlier keep their open-source licenses, while version 13 onward uses a dual license with a free community tier for individuals and non-commercial use.

## Decision

We will implement a lightweight CQRS structure **without MediatR**:

- The Application project defines `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>`.
- Handlers are registered in dependency injection and injected directly into the endpoints that use them.
- Cross-cutting behavior (validation, logging, unit of work) is added with **decorators** around the handler interfaces, registered with Scrutor or by hand.
- Commands go through the domain model and EF Core. Queries may skip the domain and project straight to response DTOs (`AsNoTracking`, or Dapper for reports; see ADR-0013).

## Alternatives considered

- **MediatR 12.** Free, but frozen on an older version with no future updates.
- **MediatR 13+ community license.** Allowed for a personal portfolio, but ties the project to a commercial license model.
- **Another mediator library.** Solves the same problem with a new dependency.

## Consequences

- The dispatch mechanism is a few dozen lines of code owned by the project, with no license concerns.
- The portfolio shows an understanding of the pattern rather than just the use of a library.
- "Go to implementation" from an endpoint leads straight to the handler, which keeps navigation simple.

## References

- [MediatR repository and licensing](https://github.com/LuckyPennySoftware/MediatR)
