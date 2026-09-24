# ADR-0017: Paginate, sort and search on the server

- **Status:** Accepted
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

The front-end loads whole collections into memory and filters them in the browser (`docs/05-integracao-com-o-front.md`). This works for a few dozen users, but not for clients, accounts or reports, which can reach thousands of rows. It also sends the browser data that the user may not be allowed to see, which conflicts with per-screen permissions (ADR-0020).

## Decision

- List endpoints accept `page`, `pageSize` (default 10, maximum 100), `sort` and `search` query parameters. The front-end uses 10 rows per page.
- Responses have a standard envelope: `items`, `page`, `pageSize`, `totalCount`.
- Search ignores accents and letter case, matching the current front-end behavior. User search covers display name, full name and CPF digits.
- Offset pagination is the default. Keyset pagination can be adopted for specific large tables later.
- **Small reference lists** that change rarely and fit on one screen, such as permission profiles, the screen catalog and, later, payment methods, are returned whole, without pagination.
- Users are filtered with a `status` parameter: `current` (active and pending, the default), `pending`, `inactive` or `all`. Inactive users are therefore excluded unless requested.

## Alternatives considered

- **Return everything (current behavior).** Simple, but does not scale.
- **Keyset pagination everywhere.** Faster on large tables, but makes "go to page N" impossible and adds complexity where it is not needed.

## Consequences

- The front-end lists must request pages and show pagination controls.
- Uniqueness checks move entirely to the server, since the client no longer has all records.
