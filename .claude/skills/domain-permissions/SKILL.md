---
name: domain-permissions
description: Business rules of permissions in Control Service (rule IDs PERM) - access levels (negado, leitor, editor, gerenciador), permission profiles, effective access across several profiles, the Gerenciador system profile, screen keys and the screen catalog, the minimum level of each operation, and deleting profiles. Use when working on PermissionProfile, AccessLevel, ScreenKey, ScreenKeys, EffectiveAccess, the permission-profiles or screens endpoints, when declaring the permission of any endpoint, or when adding, renaming or removing a screen.
---

# Permissions

## Purpose

Decide who may do what, on which screen, and keep stored permissions valid when profiles and screens change.

## Source of truth

The canonical text is `docs/product/features/permission-profiles.md` (PERM-01 to PERM-25) and the "Screen keys" section of `docs/product/overview.md` (paths are from the repository root). This skill gives the decision rules and the traps, each with its rule ID. Before writing a message, a wire value or a level in code or in a test, open the rule and copy from there (CNV-16). If this skill and the documents disagree, the documents win: stop and tell the owner. A pull request that changes a PERM rule or the screen catalog updates this file too.

## When to use

- Changing `Domain/Access`, `Domain/PermissionProfiles` or the profile and screen endpoints.
- Declaring the screen and minimum level of **any** endpoint, in any feature.
- Computing or returning someone's access (`me().levels`).
- Adding, renaming or retiring a screen.

Load `domain-record-contract` with it.

## Core concepts

| Concept | Meaning | In code |
|---|---|---|
| Screen ("tela") | A submenu item | `ScreenKey`, constants in `ScreenKeys` |
| Area ("área") | A menu group. Presentation only, **not** a code boundary and not a permission unit | — |
| Access level | Ordered: `Denied < Reader < Editor < Manager` | `AccessLevel` |
| Permission profile | A named set of one level per screen | `PermissionProfile`, `ScreenLevel` |
| Effective access | What a user can really do on a screen | `EffectiveAccess` (domain service) |

Wire values are Portuguese and fixed: `negado`, `leitor`, `editor`, `gerenciador`. Code names are English.

## Decision rules

### Which level an operation needs (PERM-03)

| Operation | Minimum level on its screen |
|---|---|
| Open the screen, list and view records | Reader |
| Create, change, resend access | Editor |
| Delete a profile, deactivate or reactivate a user | Manager |

Each level includes the previous ones (PERM-02). A screen with exceptions documents them in its own "Operations" table: take the level from there, never by analogy.

**Exception (PERM-04):** listing profiles is also allowed with Reader on **Usuários**, even without access to Permissões, because the user form needs the list.

### Computing effective access (PERM-05 to PERM-07)

For each screen, independently:

1. The user has the Gerenciador profile → `Manager`, whatever the other profiles say, including on screens created later (PERM-06).
2. Otherwise → the **highest** level among the user's profiles for that screen (PERM-05).
3. A screen missing from a profile counts as `Denied`. No profiles → `Denied` everywhere. A profile id that matches no profile is ignored (PERM-05).

`Denied` is the **absence** of permission, not a prohibition: a profile can never take away what another grants (PERM-07). Never implement "deny wins".

### Saving a profile

- `name`: required (PERM-16); unique among all profiles **including the Gerenciador**, with the normalized comparison of CNV-09 (PERM-17).
- `levels`: every `screen` must be a key of the catalog. An unknown key **refuses the whole save** (PERM-24). Do not drop it silently.
- A new profile starts with every screen at `Denied` (PERM-18).
- Changing a profile affects everyone who has it, from each person's next request, without signing out (PERM-13).

### Deleting a profile (PERM-20)

1. It is the Gerenciador profile → refuse, 409 `system_record` (PERM-22, PERM-25).
2. Any user has it, whether active, pending **or deactivated** → refuse, 409 `profile_in_use`, with `details.userNames`.
3. Otherwise → truly delete. A database constraint guarantees rule 2 even under a concurrent assignment.

### The Gerenciador system profile

- Always exists, fixed id `SystemIds.ManagerProfile`, `isSystem: true`; cannot be changed or deleted (PERM-22, PERM-25).
- Its levels are **not stored**. The server computes `gerenciador` for every catalog screen and returns the full `levels` list like any other profile (PERM-23).
- It can be duplicated like any profile (PERM-19).

### Screen keys (ADR-0021)

- A key is written once, in `ScreenKeys`, and **never** changes, is never derived from the screen name, and is never reused.
- Renaming a screen changes only its name. If the key followed the name, stored permissions would point to the old key and the screen would vanish for everyone but the Gerenciador profile.
- Adding a screen: update `ScreenKeys`, `docs/product/screen-catalog.json` and the tables in `docs/product/overview.md` together (`docs/agents/guides/documentation.md`). It appears as `Denied` in every existing profile, with **no data migration**; only the Gerenciador profile sees it right away (PERM-15).
- A screen that gets its rules specified also gets a feature document from `docs/product/features/template.md`, with a new rule ID prefix, confirmed by the owner before coding.
- Removing a screen: retire the key and clean up its stored levels with a migration (ask the owner first: migrations need approval).

### What the server returns

- `me().levels` and a profile's `levels` have the same shape: `[{ screen, level }]`. `me` returns **every** screen of the catalog, in catalog order, including `negado`.
- `listProfiles` returns the whole list, not paged (API-08), with `userCount`, `grantedCount` and `screenCount`.
- `listScreens` needs only a signed-in person.

## Invariants

1. The server refuses any request above the person's level; hiding a button is not protection (PERM-12, CNV-01).
2. Adding a profile to a user never reduces their access (PERM-07).
3. The Gerenciador profile is `Manager` on every screen, present and future (PERM-06).
4. A screen key is immutable and never reused (ADR-0021).
5. A profile in use by any user cannot be deleted (PERM-20).

## Forbidden

- Writing `Gerenciador` as an identifier in code. Use `AccessLevel.Manager` for the level and `SystemIds.ManagerProfile` for the profile.
- Storing levels for the Gerenciador profile, or special-casing a new screen for it.
- Computing a key from a screen name, or changing a key value.
- An endpoint without a declared screen and level (API-11).
- A "duplicate profile" endpoint: duplication is done by the front-end, which opens a prefilled new record (PERM-19).
- Authorizing by area, or by ASP.NET Core roles: profiles are not roles, and Identity is configured without roles.

## Exceptions

- PERM-04 (profiles listed with Reader on Usuários).
- PERM-08 to PERM-11, PERM-14 and PERM-21 describe what the front-end shows or hides. The back-end's part is refusing the request (PERM-12) and returning the levels.
- Asymmetry to remember: an unknown **profile id** in a user's `profileIds` is discarded (USR-13); an unknown **screen key** in a profile's `levels` refuses the save (PERM-24).

## Terminology

| Term (pt-BR) | Code | Do not confuse with |
|---|---|---|
| Gerenciador (nível) | `AccessLevel.Manager`, wire `gerenciador` | Gerenciador (perfil) |
| Gerenciador (perfil) | System profile, `SystemIds.ManagerProfile` | The Admin user; the Manager level |
| Negado | `AccessLevel.Denied`, wire `negado` | A prohibition, or an error |
| Chave da tela | `ScreenKey`, e.g. `gerenciamento/usuarios` | The screen name ("Usuários"), which may change |
| Área | Menu group | A module, a bounded context or a permission scope |
| Perfil de permissão | `PermissionProfile` | A role; "Perfis CNPJ", an unrelated future screen |
| Acesso efetivo | `EffectiveAccess` | The level stored in one profile |

In conversation and documents, always say "o nível Gerenciador" or "o perfil Gerenciador" (`docs/product/glossary.md`).

## Cross-domain dependencies

| Topic | Owner |
|---|---|
| Which profiles a user has; a user without profiles; the Admin always has the Gerenciador profile | `domain-users` (USR-13, USR-21, USR-24) |
| Checking the account on every request; `me` and sessions | `domain-authentication` (AUTH-18) |
| Uniqueness comparison, version, 403 `forbidden` and the error shape | `domain-record-contract` |

Every feature depends on this skill for the minimum level of its operations.

## Known gaps (ask the owner, do not decide)

Found on 2026-10-04:

- The "Errors" table of the feature document lists PERM-16 and PERM-17 under 400 `validation_failed`, but not PERM-24.
- PERM-04 mentions only **listing** profiles. Whether a Reader on Usuários without access to Permissões may also read one profile (`getProfile`) is not stated.

## How to verify

- `EffectiveAccess` has unit tests for: no profiles, one profile, several profiles (highest wins per screen), the Gerenciador profile, an unknown profile id.
- Each endpoint has an integration test for the allowed path and for the denied path one level below its minimum.
- The screen keys in `ScreenKeys` equal the keys in `docs/product/screen-catalog.json`.

## References

- `docs/product/features/permission-profiles.md`
- `docs/product/overview.md` (Screen keys and the screen tables)
- `docs/product/screen-catalog.json`
- `docs/product/glossary.md` ("Access and permissions")
- ADR-0006, ADR-0020, ADR-0021, ADR-0022, ADR-0032, in `docs/adr/`
- `docs/agents/guides/architecture.md` (API layer: declaring the permission of an endpoint)
