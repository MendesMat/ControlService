# Documentation guide

Documentation is part of the change. A pull request that changes behavior, the API contract or a decision without updating the documents is not done. The layout and writing conventions are in the [docs index](../../README.md).

## Where each thing is documented

| Change | Update |
|---|---|
| A field, rule, message or operation of a feature | Its document in `docs/product/features/` |
| A new screen or feature | A new document from `docs/product/features/template.md`, with a new rule ID prefix registered in `docs/README.md` |
| A rule that applies to every record or screen | `docs/product/conventions.md` |
| A business term | `docs/product/glossary.md` |
| A screen, area or screen key | `backend` `ScreenKeys`, `docs/product/screen-catalog.json` and the navigation tables in `docs/product/overview.md` |
| Routes, paging, versions or error codes shared by all endpoints | `docs/api/conventions.md` |
| An open question is raised or answered | `docs/product/open-questions.md` (answered ones move to "Resolved") |
| A technical decision | A new or superseding ADR in `docs/adr/`, plus the index ([record a decision](../workflows/record-a-decision.md)) |
| The front-end prototype | `docs/frontend/` |
| How to run, build or test | `backend/ControlService/README.md` and, if user-visible, the root `README.md` |
| Roadmap progress | The checklist in the root `README.md` |
| A new convention or procedure for agents | `docs/agents/` and, if it is a rule every agent must know, `AGENTS.md` |

## Rules

- Everything is in **English**, except user-facing messages (Portuguese, verbatim), wire values and the owner's guide `docs/agents/trabalhando-com-agentes.md`.
- `docs/product/` describes **decided** behavior. Open questions go to `docs/product/open-questions.md`, never presented as rules.
- **Rule IDs are stable**: a new rule takes the next free number; a removed rule is marked *Retired*, never deleted or renumbered. Changing a rule's meaning needs the owner's confirmation, and the pull request lists the IDs that changed.
- **One fact in one place.** Link to the document that owns a fact instead of repeating it.
- If the code and `docs/` disagree, stop and ask the owner which one is right. Do not "fix" the documentation to match the code silently.
- Keep relative links working. When you rename a file or a heading, search for links to it.
- ADRs follow [record-a-decision.md](../workflows/record-a-decision.md). Accepted ADRs are never rewritten, only superseded.
- The OpenAPI document is generated from the endpoints. Endpoint summaries and descriptions are in English; examples of error messages stay in Portuguese.
