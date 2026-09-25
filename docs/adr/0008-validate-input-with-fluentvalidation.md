---
status: accepted
date: 2026-09-23
accepted: 2026-09-25
scope: back-end
tags: [architecture]
---

# ADR-0008: Validate input with FluentValidation

## Context

The front-end validates every form before saving (`docs/product/features/`), but the back-end cannot trust the client. Validation errors must reach the front-end in a shape it can show next to each field, with the same Portuguese messages.

## Decision

Commands are validated with **FluentValidation** validators in the Application project.

- A validation decorator (ADR-0007) runs the validator before the handler; failures never reach the domain.
- Validators check shape and format: required fields, lengths, formats. Domain invariants stay in value objects and aggregates (ADR-0006).
- Rules that need the database, such as the uniqueness of the user's login and display name and of the profile name, run in the handler. CPF is validated for format only and is not unique.
- A user's profile list may be empty. Profile ids that do not exist are discarded rather than rejected. Asking for confirmation before saving a user without profiles is a front-end concern, not a validation rule. Unique indexes in the database remain the final guarantee (ADR-0011).
- Error messages are the Portuguese messages already defined in `docs/product/features/`.
- Property names in errors use the same camelCase field names the front-end uses (`fullName`, `cpf`, `emergencyContact.phone`), so each message lands next to the right field.

## Alternatives considered

- **Data annotations.** Built in, but limited for conditional rules such as "if filled, must be valid".
- **Validation only in the domain.** Would mix user-input concerns with business invariants.

## Consequences

- The front-end can display server errors exactly like its own client-side errors.
- Messages are defined in two places (front-end and validators) until a shared source is introduced.
