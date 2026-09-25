# Documentation guide

Documentation is part of the change. A pull request that changes behavior, the API contract or a decision without updating the documents is not done.

## Where each thing is documented

| Change | Update |
|---|---|
| A field, record or system record | `docs/02-modelo-de-dados.md` |
| A business rule, validation or user-facing message | `docs/03-regras-de-negocio.md` |
| Access levels, screen keys, effective access | `docs/04-permissoes.md` and `docs/catalogo-de-telas.json` |
| A route, request, response or error code | `docs/05-integracao-com-o-front.md` |
| A decision is taken or an open question is answered | `docs/07-pendencias.md` (move it to "Resolvidas") |
| A technical decision | A new or superseding ADR in `docs/adr/`, plus the index in `docs/adr/README.md` |
| How to run, build or test | `backend/ControlService/README.md` and, if user-visible, the root `README.md` |
| Roadmap progress | The checklist in the root `README.md` |
| A new convention or procedure for agents | `docs/agents/` (see its README) |

## Rules

- `docs/01-07` are written in Portuguese, in the same plain tone. Keep field names, keys and identifiers in English exactly as they appear in code.
- `docs/` describes **decided** behavior. Open questions go to `docs/07-pendencias.md`, never presented as rules.
- If the code and `docs/` disagree, stop and ask the owner which one is right. Do not "fix" the documentation to match the code silently.
- Keep internal links working. When you rename a heading, search for links to its anchor.
- ADRs follow [record-a-decision.md](../workflows/record-a-decision.md). Accepted ADRs are never rewritten, only superseded.
- The OpenAPI document is generated from the endpoints. Endpoint summaries and descriptions are in English; examples of error messages stay in Portuguese.
