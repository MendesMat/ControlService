---
status: accepted
date: 2026-09-23
accepted: 2026-09-24
scope: back-end
tags: [data]
---

# ADR-0013: Use EF Core 10 for writes and Dapper for reporting queries

## Context

Most screens are forms and lists that fit an ORM well. The Relatórios area (for example Custo x Faturamento) will need aggregated queries across many tables, where hand-written SQL is clearer and faster than LINQ.

## Decision

- **EF Core 10** is the default for all commands and for simple queries. Migrations are created with the EF Core CLI and committed to the repository.
- Read-only queries use `AsNoTracking` and project directly to DTOs with `Select`.
- **Dapper** is used for reporting queries, with SQL kept next to the query handler and sharing the same Npgsql data source. It is added together with the first report; until then, EF Core is the only data access library.
- Migrations are applied by a dedicated step in deployment, not automatically at application startup in production.

## Alternatives considered

- **EF Core only.** Possible, but complex reports become hard-to-read LINQ.
- **Dapper only.** Full control, but a lot of repetitive code for simple forms.

## Consequences

- The portfolio shows when to use an ORM and when to leave it.
- Two data access styles must be tested; integration tests run both against a real PostgreSQL (ADR-0024).
