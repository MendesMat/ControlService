---
status: accepted
date: 2026-09-23
scope: back-end
tags: [security, email]
superseded-by: ADR-0031 (delivery of activation e-mails only)
---

# ADR-0030: Send transactional e-mails through SMTP with Mailpit in development

## Context

Account activation and password reset depend on e-mails (ADR-0019). These messages must be reliable, easy to inspect during development and testing, and must never reach real people from a local machine or a CI run.

## Decision

**Abstraction.** The Application project defines an `IEmailSender` interface, implemented in Infrastructure.

**Transport.** Messages are sent over **SMTP** using **MailKit**. Microsoft recommends MailKit over the older `System.Net.Mail.SmtpClient` for new development.

**Development and CI.** **Mailpit** runs as a container, started by the Aspire AppHost (ADR-0027) and by Testcontainers in integration tests. It accepts every message and shows it in a local web inbox, so activation links can be opened during development. Nothing is actually delivered.

**Production.** Any SMTP-capable transactional e-mail provider, configured by environment variables. The provider for the public demo is still open (`docs/product/open-questions.md`).

**Delivery.**
- E-mails are sent **after** the database transaction commits, through an in-process background queue (`Channel<T>` consumed by a `BackgroundService`), with a few retries.
- A failed delivery never rolls back the user creation. The failure is recorded on the pending account and shown on the Users screen, so someone can use **Resend access**.

**Content.**
- Messages are in Portuguese, written in the same plain tone as the rest of the system.
- They are rendered from templates, in both HTML and plain text.
- They state who created the account and include only the link. They never contain a password.

**Security.**
- Links point to the front-end and carry only the random token.
- Tokens and links are never written to logs (ADR-0028).

## Alternatives considered

- **A provider's HTTP API and SDK.** Often richer (delivery status, webhooks), but ties the code to one vendor. SMTP keeps the provider replaceable by configuration.
- **`System.Net.Mail.SmtpClient`.** Built in, but not recommended for new development.
- **Sending inside the request.** Simpler, but a slow or failing mail server would slow down or fail the user creation.
- **A transactional outbox table.** Guarantees delivery across restarts. It is a good future improvement, but more than this stage needs.

## Consequences

- Developers can test the complete activation flow locally without any real e-mail account.
- Messages queued in memory are lost if the process stops before sending them. "Resend access" covers that case.
- Integration tests can read the Mailpit inbox to assert that the right link was sent to the right address.
