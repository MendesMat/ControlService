# Workflow: record a decision

Architecture Decision Records live in `docs/adr/`, in English, one decision per file, in one flat folder ordered by number. The process is defined in [ADR-0001](../../adr/0001-record-architecture-decisions.md), and the metadata format in the [ADR index](../../adr/README.md#metadata).

## When an ADR is needed

- A new technology, library or external service with architectural impact.
- A new pattern or convention that all features must follow.
- A change to an existing decision, including a small one.
- An answer from the owner that settles a technical trade-off.

Implementation details that do not constrain other code (a private helper, a method name) do not need an ADR. Business rules are not ADRs either: they go to `docs/product/`.

## Propose a new decision

1. Take the next free number from `docs/adr/README.md`.
2. Copy `docs/adr/template.md` to `docs/adr/NNNN-short-title-in-the-imperative.md`.
3. Fill in the front matter: `status: proposed`, today's `date`, `scope` and `tags` from the vocabulary in the index.
4. Fill in context, decision, alternatives considered (with the reason each was rejected) and consequences. Link the product documents it relates to and cite rule IDs.
5. Add it to the index in `docs/adr/README.md`, in the matching category.
6. Present it to the owner in Portuguese: the problem, the options with trade-offs and your recommendation.

## Accept a decision

Only the owner accepts a decision. When they confirm:

1. In the front matter, set `status: accepted` and add `accepted: YYYY-MM-DD`.
2. Update the status column and the count of accepted records in `docs/adr/README.md`.
3. If it answers an item in `docs/product/open-questions.md`, move the item to "Resolved" with a link to the ADR.

A *proposed* ADR may still be edited. An **accepted** ADR is never rewritten.

## Change an accepted decision

1. Write a new ADR that explains what changes and why. In its front matter, add `supersedes: ADR-NNNN`, or `supersedes: ADR-NNNN (<part> only)` when only part of it changes. Use `amends` when the new record completes the old one without replacing anything.
2. In the old ADR, change **only the front matter**: `status: superseded` with `superseded-by: ADR-MMMM` for a full replacement; for a partial one, keep `status: accepted` and add `superseded-by: ADR-MMMM (<part> only)` or `amended-by: ADR-MMMM (...)`.
3. Update the index, and every document in `docs/product/` and `docs/api/` that described the old behavior.

Maintenance that does not change the decision (fixing a broken link, a moved path or the metadata) is allowed in accepted records.

## Deliver

ADR changes go through a pull request like any other change ([git and pull requests](git-and-pull-requests.md)), with the `docs(adr)` scope: `docs(adr): propose ADR-0033 for signature storage`.
