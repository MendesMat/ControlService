---
status: accepted
date: 2026-09-24
accepted: 2026-09-24
scope: back-end
tags: [security, authentication]
amends: ADR-0019 (session lifetime and revocation)
---

# ADR-0032: Define the session lifetime and check the account on every request

## Context

ADR-0019 issues a 15-minute JWT access token and a rotating refresh token, but it does not say **how long the refresh token lives**, which is what decides when the person sees *"Sua sessão terminou. Entre de novo para continuar."* The simulated server ends a session after **8 hours without any action** (`docs/frontend/simulated-server.md`).

Two business rules also do not fit a self-contained token that stays valid for 15 minutes:

- a deactivated person *"sai do sistema na próxima ação"* (`docs/product/features/users.md`);
- until the Admin changes the initial password, *"nenhuma outra tela fica disponível"*.

## Decision

**Session lifetime.**
- Access token: 15 minutes (unchanged).
- Refresh token: **8 hours, sliding**. Every successful refresh rotates the token and the new one is valid for 8 hours from that moment. After 8 hours without a refresh, the session ends.
- The front-end refreshes **only when it needs to make a request** and the access token has expired, never on a timer. This way "8 hours without any action" really means without any action by the person.
- On page load, the front-end calls the refresh endpoint to find out whether a session exists (the `hasSession` operation).
- Configuration: `Auth:AccessTokenMinutes` (15) and `Auth:RefreshTokenIdleHours` (8).

**Account check on every request.**
- The authorization handler of ADR-0020 loads, for the user in the token, the **account status together with the effective levels**, from the same HybridCache entry.
- If the status is not `active`, the request is rejected with **401 `account_inactive`**, whatever the endpoint.
- Deactivating a user invalidates that cache entry and revokes the user's refresh tokens (ADR-0019). The next request, even with a still-valid access token, is rejected.

**Mandatory password change.**
- While the Admin has not replaced the initial password, the access token carries the claim `must_change_password`.
- A filter on the `/api/v1` group rejects every endpoint with **401 `password_change_required`**, except change password, `me` and sign out.
- Changing the password issues new tokens without the claim.

## Alternatives considered

- **Fixed 8 hours from sign-in.** Safer, but can interrupt someone in the middle of a form.
- **7 days sliding.** More comfortable, but risky on shared computers, which are common among the users.
- **Trust the token until it expires.** No lookup per request, but a deactivated person keeps access for up to 15 minutes.
- **Short-lived tokens only (for example 1 minute).** Reduces the window, but multiplies refresh traffic and still leaves a window.

## Consequences

- Behavior matches the simulated server, so the front-end rules do not change.
- Every authenticated request reads the cache; the database is hit only on a cache miss.
- Integration tests cover: a refresh after 8 idle hours fails (fake `TimeProvider`); a deactivated user's next request gets 401; the Admin with the initial password gets 401 `password_change_required` on any other endpoint.
