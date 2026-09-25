---
status: accepted
date: 2026-09-24
accepted: 2026-09-24
scope: back-end
tags: [authentication, email]
supersedes: ADR-0030 (delivery of activation e-mails only)
---

# ADR-0031: Send activation e-mails within the request

## Context

ADR-0030 sends every e-mail through an in-process background queue after the database transaction commits. But the front-end contract needs to know the outcome **in the response**:

- `createUser` returns `{ user, emailSent }`, `resendAccess` returns `{ emailSent, email }` and `reactivateUser` returns `{ user, emailSent }` (`docs/product/features/users.md`);
- the screens show different messages depending on it: *"Enviamos o link de ativação para {e-mail}"* or *"…mas não conseguimos enviar o e-mail de ativação. Use 'Reenviar acesso' para tentar de novo."* (`docs/product/features/authentication.md`).

With a background queue, the server does not know yet whether the e-mail left when it answers.

The project owner also asked whether the user should only be saved after the link is confirmed. It should not: the user must exist first, as `pending`, because the link must point to a stored account, the login and display name must be reserved immediately, and the Users screen must list pending accounts so someone can resend access. A lost e-mail never requires deleting the user: correcting the address and using "Reenviar acesso" solves it.

## Decision

- **Activation e-mails** (create user, resend access, reactivate a user who never activated) are sent **within the request**, right **after** the database transaction commits, through `IEmailSender`.
- The send has a short, configurable timeout (`Email:SendTimeoutSeconds`, 10 seconds by default).
- The outcome is returned as `emailSent`. A failure or timeout never rolls back the saved user: the account stays `pending`, the new token stays valid, and "Reenviar acesso" issues another one.
- Because the response already tells the outcome, the account does **not** store a delivery-failure field.
- **Password-reset e-mails keep the background delivery** described in ADR-0030. Their response must be identical, in content **and in duration**, whether or not the login exists. Sending in the request would make responses for existing logins measurably slower and reveal which logins exist.
- The security rules of ADR-0030 still apply: links carry only the random token, and tokens are never logged.

## Alternatives considered

- **Background queue for every e-mail (ADR-0030).** Faster responses, but the front-end would have to change its messages to "estamos enviando" and show failures later on the user's record.
- **Save the user only after the link is confirmed.** Rejected by the reasons in the context: nothing to attach the token to, and logins could be taken twice while both links are pending.
- **Transactional outbox.** Guarantees delivery across restarts; still a possible future improvement.

## Consequences

- Creating a user takes as long as the SMTP handshake; with Mailpit locally this is milliseconds, and the timeout bounds the worst case.
- The messages in `docs/product/features/authentication.md` stay exactly as they are.
- Integration tests cover `emailSent = false` by making the SMTP server unreachable, and check that the user was still saved as `pending`.
- The application keeps two delivery paths: direct for activation, queued for password reset.
