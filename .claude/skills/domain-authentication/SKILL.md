---
name: domain-authentication
description: Business rules of authentication in Control Service (rule IDs AUTH) - sign-in with login and password, lockout and rate limiting, sessions with access and refresh tokens, account activation and password reset links, the Admin's first access and mandatory password change, password rules, activation e-mails, and the 401, 403, 410 and 429 error codes. Use when working on Application/Auth, Infrastructure/Auth, API/Auth, the auth endpoints or /me, credentials, sessions, links, or anything that happens when an account is deactivated.
---

# Authentication

## Purpose

Implement and change sign-in, sessions, links and passwords without leaking information and without breaking what the front-end expects from each error code.

## Source of truth

The canonical text is `docs/product/features/authentication.md` (AUTH-01 to AUTH-28; paths are from the repository root). This skill gives the decision rules and the traps, each with its rule ID. Before writing a message, a duration or a status in code or in a test, open the rule and copy from there (CNV-16). If this skill and the document disagree, the document wins: stop and tell the owner. A pull request that changes an AUTH rule updates this file too.

## When to use

- Changing sign-in, refresh, sign-out, change-password, `me`, activation or password reset.
- Deciding which status and code an authentication failure returns.
- Sending or resending an access e-mail.
- Implementing the consequences of deactivating a user on sessions and links.

Load `domain-record-contract` with it. The account data and status belong to `domain-users`; the `levels` returned by `me` belong to `domain-permissions`.

## Core concepts

| Concept | Meaning |
|---|---|
| Credential | Password hash, mandatory-change flag and lockout counter. Stored apart from the user record, never returned (AUTH-19) |
| Access link | A random single-use token with a purpose: `activation` (72 hours) or `reset` (2 hours) |
| Session | One refresh token, valid 8 hours from the last refresh, rotated on each use; plus a 15-minute access token (AUTH-17) |
| Lockout | Per **login**, after consecutive wrong passwords (AUTH-08) |
| Rate limit | Per **client address**, on too many requests (AUTH-26) |

Every duration and limit is configurable (AUTH-22), and a message that states one shows the configured value (AUTH-28). Never hard-code "15 minutos" in a message.

## Decision rules

### Registering and activating

- Nobody creates their own account, and nobody ever sees or types another person's password (AUTH-01).
- Save the user as `pending` **first**; send the activation e-mail right after, in the same operation; answer `emailSent` (AUTH-05).
- If sending fails, the user stays registered and pending. Never roll the user back and never ask to register again (AUTH-06).
- Resend access: only for a pending user; sends to the e-mail saved at that moment and invalidates the previous link (AUTH-04).
- Activating an account that is not `pending` is refused as an invalid link, 410 `link_invalid` (AUTH-23).

### Signing in

Answer in this way:

| Situation | Answer | Rule |
|---|---|---|
| Wrong login or wrong password | 401 `invalid_credentials`, one message that never says which was wrong | AUTH-08 |
| The 5th consecutive wrong password | Already answers 429 `locked_out` | AUTH-08 |
| Any attempt while locked, even with the right password | 429 `locked_out`; the attempt is not counted | AUTH-08 |
| Correct password, account deactivated | **403** `account_inactive` | AUTH-09 |
| Too many requests from the same address | 429 `locked_out`, with the rate-limit message | AUTH-26 |
| Success | Resets the failure count; 200 with `accessToken`, `expiresIn` in **seconds**, `mustChangePassword`; sets the refresh cookie | AUTH-08, Operations |

People sign in with the login, never with the e-mail: e-mails are shared (AUTH-07).

### Forgot password

- The person gives the **login**, not the e-mail (AUTH-10).
- The response is identical in content **and duration** whether or not the login exists. That is why reset e-mails are sent in the background, unlike activation e-mails (AUTH-11, ADR-0031).
- Creating a new password through the link clears an active lockout (AUTH-12).

### The Admin's first access

- The Admin has no activation link; its initial password comes from configuration, never from code (AUTH-13).
- Until the initial password is replaced, every operation except `me`, sign-out and change-password answers 401 `password_change_required` (AUTH-14).
- The Admin may change its own password and use "Esqueci minha senha" although it is a system record: credentials are not part of the record (AUTH-15).
- Change-password is **only** for an account with a mandatory change; any other account gets 403 `forbidden` (AUTH-24).
- The new password must differ from the initial one (AUTH-25).

### Passwords

At least 8 characters, no composition requirement, typed twice, **never trimmed** (AUTH-16). Error fields are `password` and `passwordConfirmation`.

### Sessions

- The server checks on **every request** that the account is still active; a valid access token is not enough (AUTH-18).
- Refresh is anonymous, so it makes the same check itself: a non-active account gets `account_inactive` and its session is removed (AUTH-18).
- A missing, invalid or expired access token answers `session_expired` (AUTH-27).
- Sign-out ends only the session named by the token's `sid` claim; the person's other sessions stay open. Change-password ends every session of the person and starts a new one (Operations table).
- A profile change takes effect on the person's next request, without signing out (PERM-13, owned by `domain-permissions`).

### Deactivated accounts

Deactivating a user invalidates their pending links and revokes their refresh tokens (AUTH-21); the person is rejected on the next request (AUTH-18) and cannot sign in again (AUTH-09). The deactivation itself is owned by `domain-users` (USR-17).

## Error codes that are easy to get wrong

| Code | Trap |
|---|---|
| `account_inactive` | **403 at sign-in**, **401 during a session** and at refresh. `ErrorStatusCodes` maps it to 401; the sign-in endpoint returns the 403 explicitly |
| `locked_out` | One code, two causes, two messages: lockout (AUTH-08) and rate limit (AUTH-26). Both 429 with `Retry-After` and `details.retryAfterSeconds` |
| `link_invalid` | 410, also for a valid token whose account is no longer pending (AUTH-23). Activation and reset have different messages |
| `forbidden` | Also used by change-password without a mandatory change (AUTH-24), with its own message |

The full tables are "Errors" and "Messages" in the feature document.

## Invariants

1. Credentials and tokens are stored only hashed and never returned by any operation (AUTH-19).
2. Link tokens travel in the request body, never in the URL, and are never logged (AUTH-20).
3. No response reveals whether a login exists, or which of login and password was wrong (AUTH-08, AUTH-11).
4. A deactivated account can do nothing, whatever token it holds (AUTH-09, AUTH-18).

## Forbidden

- Signing in, or resetting a password, by e-mail.
- Writing a password, token, activation link or reset link to a log, an exception or test output.
- Sending the reset e-mail inside the request, or answering "login not found".
- Putting credentials, links or the lockout counter in the `User` aggregate or in a user response.
- Trusting the access token alone for the account status.

## Exceptions

- **Known limitation, documented:** a login that does not exist is never locked, so five attempts reveal whether it exists. The per-address rate limit makes scanning slow. Do not "fix" it without the owner.
- The Admin is the only account without an activation link (AUTH-13).

## Terminology

| Term (pt-BR) | Code | Do not confuse with |
|---|---|---|
| Bloqueio por tentativas | Lockout, per login | Rate limit, per address (same code `locked_out`); the `inactive` status |
| Link de ativação | Access link, purpose `activation` | Link de troca de senha, purpose `reset` |
| Reenviar acesso | `ResendAccess`: new activation link for a pending user | "Esqueci minha senha": reset link for the person themselves |
| Conta ativa | `UserStatus.Active` | Having an open session |
| `expiresIn` | Seconds until the access token expires | A timestamp |
| `mustChangePassword` | Mandatory change of the initial password | A generic "change my password" feature (there is none: AUTH-24) |

## Cross-domain dependencies

| Topic | Owner |
|---|---|
| User fields, status transitions, who may deactivate | `domain-users` |
| The `levels` list of `me`: every catalog screen, in catalog order, including `negado` | `domain-permissions` (PERM-05) |
| Per-request permission checks | `domain-permissions` (PERM-12, PERM-13) |
| Problem Details shape and the shared error table | `domain-record-contract` |

## Implementation status

Which parts exist in code and which are still to come is listed at the end of the Authentication section of `docs/agents/guides/architecture.md` ("Not yet covered"). Check it before assuming a rule is already enforced.

## Open questions (not rules)

Production e-mail provider (OQ-04), demo access (OQ-05) and where the front-end is served, which defines CORS and the refresh cookie (OQ-07): `docs/product/open-questions.md`. Ask before building anything that depends on them.

Also ask the owner: AUTH-10 sends the reset link when the login "exists and is active"; the document does not say what a **pending** user gets (found on 2026-10-04).

## How to verify

- Each row of the sign-in table has a test, with a fixed `TimeProvider` for the lockout window.
- A test proves that the answer of the reset request is the same for an existing and an unknown login.
- Integration tests assert status, `code` and the verbatim message, and that no response body contains a credential or a token other than `accessToken`.

## References

- `docs/product/features/authentication.md`
- `docs/api/conventions.md` (API-10, error codes)
- `docs/agents/guides/architecture.md` (Authentication: ports, Identity, sessions, tokens, rate limiting)
- ADR-0019, ADR-0022, ADR-0023, ADR-0030, ADR-0031, ADR-0032, in `docs/adr/`
