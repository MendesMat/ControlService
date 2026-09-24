# ADR-0019: Authenticate with ASP.NET Core Identity, JWT and activation links

- **Status:** Accepted; session lifetime and revocation are completed by [ADR-0032](0032-define-session-lifetime-and-per-request-account-checks.md)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

There is no sign-in yet. The top bar of the front-end shows a fixed name, and permissions cannot be enforced without knowing who is signed in.

The project owner defined how accounts work (see `docs/03-regras-de-negocio.md`):

- Nobody signs up on their own. A new user is always created by someone with access to the Users screen.
- Users sign in with a new **`login`** field, which is unique.
- The **e-mail** is required but **not unique**: several people may share a department mailbox.
- The first access happens through an **activation link** sent by e-mail, where the person creates their own password. A temporary password sent by e-mail was considered and rejected.

## Decision

**Credentials.** ASP.NET Core Identity manages the password hash, lockout and security stamps. The domain `User` aggregate references the Identity user by id and holds business data, including `login`, `email` and `status`.

**Login.** 3 to 30 characters, limited to unaccented letters, digits, `.`, `-` and `_`. Stored in lowercase and unique among all users, including inactive ones. The server can suggest a free login from the full name (`ana.souza`, then `ana.souza2`).

**Account status.** Accounts move through three states: `pending` → `active` ↔ `inactive`. Only `active` accounts can sign in.

**Activation.**
1. Creating a user sets the status to `pending` and sends an activation link to the registered e-mail (ADR-0030).
2. The link carries a random, single-use token, valid for 72 hours. Only its hash is stored.
3. Opening the link lets the person set a password. The account then becomes `active`.
4. "Resend access" issues a new token to the current e-mail and invalidates the previous one.

**Password reset.** The person provides their **login**, not their e-mail, because e-mails are not unique. If the login exists and is active, a single-use reset link valid for 2 hours is sent to the registered e-mail. The response is identical whether or not the login exists, to avoid account enumeration.

**Password rules.** Minimum of 8 characters, with no composition rules, following current NIST guidance that favors length over complexity.

**Lockout.** 5 consecutive failures lock the login for 15 minutes. Setting a new password through a reset link also clears an active lockout. Error messages never reveal whether the login or the password was wrong.

**Tokens.**
- A successful sign-in returns a short-lived **JWT access token** (15 minutes) and a **refresh token**.
- Refresh tokens are random, stored hashed, rotated on every use and revoked on sign-out, on password change and when the user is deactivated.
- In the browser, the access token lives in memory only.
- The refresh token travels in an `HttpOnly`, `Secure`, `SameSite=Strict` cookie scoped to the refresh endpoint.

**Current user.** `GET /api/v1/me` returns the signed-in user's display name and effective access levels (ADR-0020).

All durations and limits above are configuration values, not constants.

## Alternatives considered

- **Temporary password sent by e-mail.** Rejected by the project owner in favor of activation links. With a link, users never have to copy a random password, which suits the audience, and the mailbox never keeps a credential. This matters more because mailboxes may be shared.
- **Sign in with e-mail.** Not possible, because e-mails are not unique.
- **Cookie-only authentication.** Simple for a same-site front-end, but JWT is more common in job requirements and works for future mobile clients.
- **External identity provider (Keycloak, Auth0, Entra ID).** Production-grade, but it moves the most interesting part of the project out of the codebase.
- **`MapIdentityApi` endpoints.** Quick to set up, but they do not support the activation flow and do not issue standard JWTs.

## Consequences

- The user form gains **Login** and **E-mail** fields, and the Users screen gains **Resend access**.
- The front-end needs new screens: sign in, activate account, forgot password and set a new password.
- The system depends on e-mail delivery. A failed delivery must not undo the user creation (ADR-0030).
- Secrets such as the JWT signing key come from configuration and user secrets, never from the repository.
