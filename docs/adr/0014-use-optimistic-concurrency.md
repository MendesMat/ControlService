# ADR-0014: Use optimistic concurrency control

- **Status:** Accepted
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Today, if two people edit the same record and both save, the last save silently overwrites the first (`docs/07-pendencias.md`). The ERP will be used by several people at once, so lost updates are a real risk.

## Decision

- Every aggregate has a concurrency token. In PostgreSQL we use the `xmin` system column, mapped by Npgsql as a row version, so no extra column is needed.
- Read responses include the current version. Update and delete requests must send it back, either in an `If-Match` header or in the request body.
- If the record changed in the meantime, EF Core raises a concurrency exception, which the API returns as **409 Conflict** with the code `concurrency_conflict`. The response includes the display name of the person who made the latest change, taken from the record's `UpdatedBy` (ADR-0015), so the front-end can show: *"Este cadastro foi alterado por {nome} enquanto você editava. Recarregue para ver a versão atual."*
- Nothing is written in that case. The front-end keeps the user's input on screen and offers "Recarregar" or "Continuar aqui" (`docs/03-regras-de-negocio.md`).

## Alternatives considered

- **Last write wins (current behavior).** Simplest, but loses data without warning.
- **Pessimistic locking.** Prevents conflicts, but locks rows while someone has a form open, which does not suit a web application.

## Consequences

- The front-end must keep the version of each open record and handle the 409 response with a clear dialog.
- Integration tests must cover the conflict path.
