# ADR-0006: Apply tactical DDD in the domain

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

The front-end currently validates CPF, phone and CEP and stores them **with formatting masks** (`529.982.247-25`). Every rule lives only in the browser. The permission model relies on an ordered set of access levels, and the effective level of a user is the highest level among their profiles. These rules belong to the business, not to any particular screen. See `docs/02-modelo-de-dados.md`, `docs/03-regras-de-negocio.md` and `docs/04-permissoes.md`.

## Decision

The Domain project will use tactical Domain-Driven Design patterns.

**Aggregates.** `User` and `PermissionProfile` are aggregate roots. Each exposes behavior methods (for example `User.AssignProfiles`, `PermissionProfile.SetLevel`) instead of public setters, and protects its own invariants, such as: a system record cannot be changed. A user **may have no profiles at all**; this is a valid state that means `Denied` on every screen, and the project owner explicitly confirmed it.

**Value objects.** Immutable types with validation in their factory methods:

| Value object | Stored as | Rule |
|---|---|---|
| `Cpf` | 11 digits | Valid check digits; all-equal sequences rejected. Informational only and **not unique**. |
| `Login` | Lowercase text | 3 to 30 characters: unaccented letters, digits, `.`, `-`, `_`. Unique (checked by the application and the database). |
| `EmailAddress` | Text | Valid e-mail format. Not unique. |
| `PhoneNumber` | 10 or 11 digits | Must include area code. |
| `Cep` | 8 digits | Exactly 8 digits. |
| `BloodType` | One of eight values | Closed list. |
| `ScreenKey` | Text | Must exist in the screen catalog (ADR-0021). |
| `AccessLevel` | Ordered enumeration | `Denied < Reader < Editor < Manager`. |

**Normalized storage.** CPF, phone and CEP are **informational data**: no other rule depends on them. They are stored and returned as **digits only**. The API accepts both masked and unmasked input. Formatting is a presentation concern handled by the front-end. This was confirmed by the project owner.

**Wire format of access levels.** In C#, `AccessLevel` uses English names. On the wire and in the database it keeps the existing identifiers `negado`, `leitor`, `editor` and `gerenciador`, so the current front-end contract does not change.

**Effective access.** A domain service computes the effective level of a user for a screen as the **maximum level among the user's profiles**, treating a missing entry as `Denied`. With no profiles, the maximum of an empty set is `Denied`. The calculation is done independently for each screen (each submenu item). `Denied` means "no permission granted by this profile", not an explicit prohibition: it never overrides a higher level granted by another profile. This business rule is described in `docs/04-permissoes.md`.

## Alternatives considered

- **Anemic entities with validation in services.** Simpler at first, but rules end up duplicated across handlers.
- **Keep masked values in storage.** Matches the current front-end, but makes uniqueness checks and searches depend on formatting.
- **Most restrictive level wins, or explicit deny overrides.** Common in security products, where a single deny must always block. Rejected for this ERP: profiles describe job roles, and a person who accumulates roles is expected to accumulate access. Restricting someone is done by removing the profile that grants access.

## Consequences

- The rules in `docs/03-regras-de-negocio.md` become unit-testable domain code, independent of the front-end.
- The front-end adapter must format CPF, phone and CEP for display and send them unmasked or masked; either is accepted.
- EF Core needs value converters for the value objects.
- Adding a profile to a user can only increase their access; reducing access requires removing a profile or lowering a level inside one. The user interface must make this clear when profiles are assigned.
