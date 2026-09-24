# ADR-0016: Deactivate users instead of deleting them

- **Status:** Accepted
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Today, deleting a user removes the record permanently. In an ERP, users will be referenced by other records (created by, responsible for a route, signed a document). Removing them breaks that history.

## Decision

- **Users** are deactivated, not deleted. The `User` status becomes `inactive` (ADR-0019), with `DeactivatedAt` and `DeactivatedBy` recorded.
- Deactivated users cannot sign in. Their refresh tokens are revoked, and any pending activation or password-reset link stops working.
- An EF Core **global query filter** hides deactivated users from default queries. Screens that need them, such as history or the status filter of the Users list, disable the filter explicitly.
- A deactivated user can be reactivated. If they had already activated their account, they return to `active` with the same password. If they were still `pending`, they return to `pending` and a new activation link is sent.
- Nobody can deactivate their own account, to avoid locking themselves out by mistake.
- The login and the display name of a deactivated user remain reserved, so old log entries and audit fields never point to two different people with the same name.
- The Admin user cannot be deactivated (ADR-0022).
- **Permission profiles** keep the current rule: a profile can be permanently deleted only when no user (active or inactive) has it.

## Alternatives considered

- **Hard delete everywhere (current behavior).** Simple, but loses history.
- **Soft delete for every entity.** Consistent, but adds filtering complexity where it brings no value.

## Consequences

- The front-end shows "Desativar usuário" and "Reativar usuário" instead of "Excluir usuário", and the Users list has a status filter ("Ativos e pendentes", "Só pendentes", "Só desativados", "Todos").
- Every query must be written with the global filter in mind.
