# ADR-0022: Seed and protect system records

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Two records must always exist and can never be changed: the **Gerenciador** profile, with the highest level on every screen, and the **Admin** user, who has that profile. Today they are hard-coded in the front-end and never stored (`docs/02-modelo-de-dados.md`).

## Decision

- Both records are stored in the database and created by **EF Core seeding** with fixed, well-known GUIDs (ADR-0012), exposed as constants.
- Both carry an `IsSystem` flag. The aggregates reject any change or deletion of a system record with a domain error, returned as 409 Conflict.
- The Gerenciador profile has no stored levels. The effective-access service treats it as Manager on every screen, including screens added in the future.
- The Admin signs in with the fixed login `admin`. Its e-mail and initial password come from configuration (user secrets locally, environment variables in deployment) and are never committed. This bootstrap password is the only exception to the activation-link flow (ADR-0019), because the first administrator has nobody to invite them. The Admin must change it on first sign-in.
- System records cannot be deactivated, and the Admin cannot lose the Gerenciador profile.

## Alternatives considered

- **Keep system records only in code.** Avoids seeding, but joins and foreign keys to them become special cases.
- **Store the Gerenciador levels explicitly.** Uniform with other profiles, but new screens would need a data migration to stay fully accessible.

## Consequences

- Queries and foreign keys treat system records like any other record.
- The front-end no longer adds these records to lists itself; it receives them from the API with an `isSystem` flag.
