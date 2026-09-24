# ADR-0023: Rate limit authentication endpoints

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Sign-in and token refresh endpoints are the main target of brute-force and credential-stuffing attempts. A public portfolio demo will be exposed to the internet.

## Decision

- Use the built-in ASP.NET Core **rate limiting middleware** on authentication endpoints, with a sliding window per client IP.
- Endpoints that send e-mails ("forgot password" and "resend access") get stricter limits, both per IP and per target login, so they cannot be used to flood a mailbox (ADR-0030).
- Combine it with **Identity lockout**, which blocks an account after repeated failed attempts, regardless of IP.
- Rejected requests return **429 Too Many Requests** with a `Retry-After` header and a Portuguese message.
- A looser global limit protects the rest of the API.

## Alternatives considered

- **Rely on lockout only.** Protects accounts, but not the service itself.
- **Rate limiting only at a reverse proxy or gateway.** Valid in production, but keeps the behavior out of the codebase and out of tests.

## Consequences

- Limits must be tuned so that normal use, such as several people behind the same office IP, is not blocked.
- Integration tests should confirm the 429 behavior.
