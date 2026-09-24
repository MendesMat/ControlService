# ADR-0011: Use PostgreSQL as the primary database

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

The ERP stores relational data with clear relationships: users, profiles, the profiles of each user, and the level of each profile for each screen. Future screens (clients, accounts payable and receivable) are also relational. SQL Server is very common in .NET jobs; PostgreSQL is free, light to run in Docker and widely used in newer companies.

## Decision

We will use **PostgreSQL** (a currently supported major version, 17 or later) through the Npgsql provider for EF Core.

- Table and column names use `snake_case`, following PostgreSQL conventions.
- **Uniqueness lives in the database** as the last line of defense:
  - A unique index on the user's login, stored in lowercase.
  - A unique index on a normalized copy of the user's display name.
  - A unique index on a normalized copy of the profile name.
  - "Normalized" means trimmed, lowercase and without accents, matching the rules in `docs/03-regras-de-negocio.md`. The normalized column is filled by the application, so the rule is identical in C# and in the database.
- The many-to-many relationship between users and profiles is a join table, not an array column.
- Profile levels are stored as rows (`profile_id`, `screen_key`, `level`), with a missing row meaning `Denied` (see `docs/04-permissoes.md`).

## Alternatives considered

- **SQL Server.** Excellent tooling and very common in .NET jobs; would be equally valid. It requires a heavier container locally and a paid license in production.
- **A document database.** Would match the current JSON records, but the domain is relational.

## Consequences

- Local development and CI run the same database engine in containers (ADR-0024, ADR-0027).
- The project can switch to SQL Server later with limited effort because data access goes through EF Core and a small Dapper layer.
