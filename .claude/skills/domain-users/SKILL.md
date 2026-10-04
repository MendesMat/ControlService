---
name: domain-users
description: Business rules of the Users feature of Control Service (rule IDs USR) - the user record and its validation, login and display name uniqueness, login suggestion, the pending, active and inactive account states, deactivation and reactivation, users without a profile, the Admin system user, signature, and personal data under the LGPD. Use when working on the User aggregate, its value objects (Login, EmailAddress, Cpf, PhoneNumber, Cep, BloodType), the users endpoints, or any test or message of that feature.
---

# Users

## Purpose

Implement and change the Users feature without breaking its rules: who can be registered, what is validated, and how an account moves between states.

## Source of truth

The canonical text is `docs/product/features/users.md` (USR-01 to USR-34; paths are from the repository root). This skill gives the decision rules and the traps, each with its rule ID. Before writing a message, a field or a status in code or in a test, open the rule and copy from there (CNV-16). If this skill and the document disagree, the document wins: stop and tell the owner. A pull request that changes a USR rule updates this file too.

## When to use

- Changing `Domain/Users`, `Application/Users` or the users endpoints.
- Validating a user field or deciding which error a refusal returns.
- Deactivating, reactivating, or anything that depends on the account status.
- Touching the Admin user or the seeder.

Load `domain-record-contract` with it (fields, versions, errors). Signing in, links and passwords are `domain-authentication`; what a user may access is `domain-permissions`.

## Domain context

`User` is an aggregate root. It references permission profiles by id only (`profileIds`). Credentials (password, links, lockout) are **not** part of it (AUTH-19). The aggregate grows slice by slice: a documented field that is still missing in code belongs to a later issue, and is not a discrepancy.

## State transitions

`status` is controlled by the system and never edited directly by a client.

| From | Event | To | Rules |
|---|---|---|---|
| (none) | User registered | `pending` | AUTH-02; saved before the e-mail is sent (AUTH-05) |
| `pending` | Person creates the password through the activation link | `active`, `activatedAt` set | AUTH-02 (owned by `domain-authentication`) |
| `pending` or `active` | Deactivate | `inactive`, with date and author | USR-17 |
| `inactive` | Deactivate again | `inactive`, **first** date and author kept, success | USR-30 |
| `inactive` | Reactivate, password already created | `active`, same password | USR-18 |
| `inactive` | Reactivate, never activated | `pending`, new activation link sent | USR-18 |
| `pending` or `active` | Reactivate | Refused, 409 `not_inactive` | USR-29 |
| any | Delete | **Does not exist** | USR-16 |

In code: `User.Activate`, `User.Deactivate`, `User.Reactivate`.

## Decision rules

### Deactivating

Evaluate in this order (the order implemented in `User.Deactivate`; the document does not rank the refusals):

1. Target is the Admin → refuse, 409 `system_record` (USR-23, USR-28).
2. Target is the person acting → refuse, 409 `self_deactivation` (USR-19, USR-27).
3. Target already `inactive` → succeed and change nothing (USR-30).
4. Otherwise → `inactive`, record date and author (USR-17).

Deactivation requires the Manager level on Usuários. Its effects on sessions and links are owned by `domain-authentication` (AUTH-09, AUTH-18, AUTH-21). The login and the display name stay reserved (USR-17).

### Validating a save

The rules, field paths and verbatim messages are the table "Validation on save" (USR-01 to USR-13, USR-26). Easy to get wrong:

- `login`: unique among **all** users, including the Admin and deactivated ones, ignoring case; stored lowercase (USR-05, USR-06).
- `displayName`: unique among **all** users, including deactivated ones, with the normalized comparison of CNV-09 (USR-03).
- `email`: required, valid format, **not unique** (USR-07). Never add a unique index or a duplicate check.
- `cpf`: optional, check digits validated, all-equal digits refused, **not unique** (USR-08).
- `profileIds`: may be empty; ids of profiles that do not exist are **discarded silently** on save (USR-13). Do not answer an error for them.
- `address` and `emergencyContact`: always present objects, every inner field optional.
- `rg`: free text, no validation.
- Duplicates are field validation errors, 400 (API-13).

### Suggesting a login

Rules USR-14, USR-15 and USR-31 (first and last name, no accents, numeric suffix when taken, 30-character cut including the suffix, no suggestion under 3 characters, name suffixes taken literally). In code: `LoginSuggestion`. The full name goes in the request body, never in the URL (API-04).

### Users without a profile

Allowed. It equals `negado` on every screen: the person signs in and sees no screen (USR-21). Never refuse a save for an empty `profileIds`; the confirmation of USR-22 is shown by the front-end.

### The Admin

- Always exists, fixed id `SystemIds.AdminUser`, `isSystem: true`; cannot be changed, deactivated or deleted (USR-23, USR-28).
- Always has the Gerenciador profile (USR-24).
- Created with `activatedAt` empty (USR-32); it is filled when the Admin replaces the initial password (USR-34). That is not an edit of the record.
- Its e-mail is read from configuration only at first creation (USR-33).
- Until OQ-06 is decided, it is a technical account without personal data.

### Minimum levels

Reader lists and reads; Editor creates, changes, suggests a login and resends access; Manager deactivates and reactivates. The exact table is "Operations" in the feature document; the level rule is owned by `domain-permissions` (PERM-03).

## Invariants

1. A user is never deleted (USR-16, ADR-0016).
2. Nobody deactivates themselves; the Admin is never deactivated (USR-19).
3. Login and display name are unique forever, including across deactivated users (USR-03, USR-06, USR-17).
4. `status` changes only through activation, deactivation and reactivation.

## Forbidden

- A delete operation, hard or soft, for users.
- Uniqueness on e-mail or CPF.
- Returning or storing credentials in the user record (AUTH-19).
- Logging CPF, phone, address, emergency contact or blood type, or using real personal data in tests. Blood type is sensitive health data (`AGENTS.md`, Security and privacy).
- Implementing the signature. It is out of the first back-end slice until OQ-01 and ADR-0018 are decided (USR-25).

## Terminology

| Term (pt-BR) | Code | Do not confuse with |
|---|---|---|
| Login | `Login` | E-mail: nobody signs in with the e-mail (AUTH-07) |
| Nome de exibição | `DisplayName`, unique | Nome completo (`FullName`), not unique |
| Pendente / Ativo / Desativado | `UserStatus.Pending` / `Active` / `Inactive` | A locked-out login (lockout is not a status) |
| Desativar | `Deactivate` | Delete; and deleting a **profile**, which does exist (PERM-20) |
| Reenviar acesso | `ResendAccess`, new activation link | "Esqueci minha senha" (password reset) |
| Admin | System user, `SystemIds.AdminUser` | The Gerenciador profile, or the Manager access level |

Full table: `docs/product/glossary.md` ("Users and accounts").

## Cross-domain dependencies

| Topic | Owner |
|---|---|
| Saving the user before sending the activation e-mail, `emailSent`, resend access | `domain-authentication` (AUTH-04 to AUTH-06) |
| What deactivation does to sessions, refresh tokens and links | `domain-authentication` (AUTH-09, AUTH-18, AUTH-21) |
| The Admin's password and first access | `domain-authentication` (AUTH-13 to AUTH-15) |
| Effective access of a user, a user without profiles | `domain-permissions` (PERM-05) |
| A profile cannot be deleted while a user of any status has it | `domain-permissions` (PERM-20) |
| Field format, version, audit, error shape | `domain-record-contract` |

## Known gaps (ask the owner, do not decide)

Found on 2026-10-04:

- The error codes `not_pending` and `email_missing` (resend access) have no rule ID and no verbatim message in the feature document.
- `email_missing` supposes a user without an e-mail, but USR-07 makes the e-mail required.

## How to verify

- Each USR rule cited has a test; the test asserts the verbatim message.
- The deactivation order above has one test per branch.
- Uniqueness of login and display name has an integration test against the unique index.

## References

- `docs/product/features/users.md`
- `docs/product/conventions.md`, `docs/api/conventions.md`
- `docs/product/glossary.md`
- `docs/product/open-questions.md` (OQ-01, OQ-06)
- ADR-0006, ADR-0012, ADR-0016, ADR-0017, ADR-0018 (proposed), ADR-0022, in `docs/adr/`
