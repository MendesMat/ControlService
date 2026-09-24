# ADR-0015: Record audit fields automatically

- **Status:** Accepted
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Records currently only have `updatedAt`, set by the browser. There is no record of when something was created or who changed it (`docs/07-pendencias.md`). An ERP needs at least basic traceability.

## Decision

- Every aggregate has `CreatedAt`, `CreatedBy`, `UpdatedAt` and `UpdatedBy`.
- The values are filled by an EF Core **`SaveChangesInterceptor`**, never by the client and never by handlers.
- The current user comes from an `ICurrentUser` abstraction backed by the authenticated principal (ADR-0019).
- Timestamps come from the injected **`TimeProvider`**, stored in UTC, so tests can control time.
- Read responses include the display names of `CreatedBy` and `UpdatedBy`, so the front-end can show a footer such as *"Criado por Admin em 12/09/2026 às 09:58 · Última alteração por Bruno Lima em 20/09/2026 às 14:41"*, in Brasília time.

## Alternatives considered

- **Set the fields in each handler.** Easy to forget.
- **A full audit log table or event sourcing.** Records every change, field by field, but is more than the project needs now. It is listed as future work in `docs/07-pendencias.md` and can be added later without changing this decision.

## Consequences

- Audit data is consistent across all features with no per-feature code.
- The front-end stops sending `updatedAt`.
