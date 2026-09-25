# Workflow: record a decision

Architecture Decision Records live in `docs/adr/`, in English, one decision per file. The process is defined in [ADR-0001](../../adr/0001-record-architecture-decisions.md).

## When an ADR is needed

- A new technology, library or external service with architectural impact.
- A new pattern or convention that all features must follow.
- A change to an existing decision, including a small one.
- An answer from the owner that settles a technical trade-off.

Implementation details that do not constrain other code (a private helper, a method name) do not need an ADR.

## Propose a new decision

1. Take the next free number from `docs/adr/README.md`.
2. Copy `docs/adr/template.md` to `docs/adr/NNNN-short-title-in-the-imperative.md`.
3. Fill in context, decision, alternatives considered (with the reason each was rejected) and consequences. Link the functional documents in `docs/` it relates to.
4. Status **Proposed**, today's date.
5. Add it to the index in `docs/adr/README.md` and update the count of accepted records if needed.
6. Present it to the owner in Portuguese: the problem, the options with trade-offs and your recommendation.

## Accept a decision

Only the owner accepts a decision. When they confirm:

1. Change the status to `Accepted (YYYY-MM-DD)`.
2. Update the index and the count in `docs/adr/README.md`.
3. If it answers an item in `docs/07-pendencias.md`, move the item to "Resolvidas" with a link to the ADR.

A *Proposed* ADR may still be edited. An **Accepted** ADR is never rewritten.

## Change an accepted decision

1. Write a new ADR that explains what changes and why, with a line `- **Supersedes:** [ADR-NNNN](...)` below the status (or "the X section of ADR-NNNN" when only part changes).
2. In the old ADR, change **only the status line**: `Superseded by [ADR-MMMM](...)`, or `Accepted; the X section is superseded by [ADR-MMMM](...)`.
3. Update the index, and every document in `docs/` that described the old behavior.

## Deliver

ADR changes go through a pull request like any other change ([git and pull requests](git-and-pull-requests.md)), with the `docs(adr)` scope: `docs(adr): propose ADR-0033 for signature storage`.
