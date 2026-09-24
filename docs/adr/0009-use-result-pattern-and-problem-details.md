# ADR-0009: Use the Result pattern for business errors and Problem Details for HTTP errors

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Some failures are expected outcomes, not exceptional situations: a login already taken, a profile still in use, an attempt to change a system record. Using exceptions for them makes control flow hard to follow and hard to test. The HTTP layer also needs a consistent error format.

## Decision

- Handlers return a small in-house **`Result` / `Result<T>`** type carrying either a value or an `Error` (a stable `code` and a Portuguese `message`). No external library is used.
- Exceptions are reserved for unexpected failures (database down, bugs).
- The API translates results into **Problem Details** (RFC 9457) with `AddProblemDetails`:

| Situation | Status | `code` |
|---|---|---|
| Validation failure, **including a login, display name or profile name already in use** | 400 | `validation_failed`, with `errors` by field |
| Not signed in or session expired | 401 | `session_expired` |
| Wrong login or password | 401 | `invalid_credentials` |
| Account deactivated during a session | 401 | `account_inactive` |
| Initial password not changed yet | 401 | `password_change_required` |
| Correct password, but the account is deactivated (sign-in only) | 403 | `account_inactive` |
| Not allowed by permissions | 403 | `forbidden` |
| Record not found | 404 | `not_found` |
| Concurrency conflict | 409 | `concurrency_conflict` |
| Business rule conflict | 409 | `profile_in_use`, `system_record`, `self_deactivation`, `not_pending`, `not_inactive`, `email_missing` |
| Activation or password-reset link expired or already used | 410 | `link_invalid` |
| Lockout after repeated failed sign-ins, or rate limit (ADR-0023) | 429 | `locked_out`, with `Retry-After` |
| Unexpected error | 500 | Generic Portuguese message, details only in logs |

Duplicates are validation failures, not 409 conflicts, because the front-end shows them under the field, like any other validation message (`docs/03-regras-de-negocio.md`). The database unique index is still the last line of defense: if two requests race, the index violation is translated into the same 400 response.

**Response body.** Every error has this shape. `code`, `message` and, when present, `errors` and `details` are Problem Details extensions:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "code": "validation_failed",
  "message": "Alguns campos precisam ser corrigidos.",
  "errors": {
    "login": ["Já existe um usuário com o login “ana.souza”. Escolha outro."],
    "address.cep": ["O CEP precisa ter 8 números."]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

- `errors` uses the model path of each field in camelCase (`login`, `address.cep`, `emergencyContact.phone`). On the password screens, the fields are `password` and `passwordConfirmation`.
- `details` carries rule-specific data: `updatedByName` for `concurrency_conflict`, `userNames` for `profile_in_use`.
- The front-end's HTTP client converts this body into its `ApiError` (`status`, `code`, `message`, `details.fields`), so the screens do not change (`docs/05-integracao-com-o-front.md`).

- Every Problem Details response includes the request `traceId`, so a user report can be matched to logs (ADR-0028).

## Alternatives considered

- **Exceptions for everything, handled by middleware.** Less code at first, but hides expected outcomes in `catch` blocks.
- **A Result library (for example ErrorOr or FluentResults).** Works well; an in-house type was preferred to keep dependencies minimal.
- **409 Conflict for duplicates.** Semantically common, but the front-end would need a second path to show a field message.

## Consequences

- Handler signatures show which failures are possible.
- The front-end must map `code` values to its dialogs, for example "profile in use".
- The error table above becomes part of the API contract and should appear in the OpenAPI document.
