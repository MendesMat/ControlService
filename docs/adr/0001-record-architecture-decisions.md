---
status: proposed
date: 2026-09-23
scope: back-end
tags: [process]
---

# ADR-0001: Record architecture decisions

## Context

Control Service is a portfolio ERP. The front-end already exists as a single HTML file, and the back-end will be built in C# with .NET. Many technical choices will be made before and during that work, and the reasons behind them are easy to lose. Recruiters and interviewers who read the repository also benefit from seeing why each choice was made, not only what was chosen.

## Decision

We will record every significant architectural decision as an Architecture Decision Record (ADR) in `docs/adr/`, written in English.

Each ADR is a Markdown file named `NNNN-short-title.md` and follows the structure in [template.md](template.md): YAML metadata (status, dates, tags and relations to other records, described in the [index](README.md#metadata)), context, decision, alternatives considered and consequences. An ADR covers one decision. All records stay in one folder, ordered by number.

An ADR starts as **Proposed**. It becomes **Accepted** when the decision is implemented or explicitly agreed. Accepted ADRs are not rewritten. If a decision changes, a new ADR is written and the old one is marked **Superseded by ADR-NNNN**.

The product documentation in `docs/product/` describes *what* the system does. ADRs describe *how* the back-end is built and *why*.

## Alternatives considered

- **A single architecture document.** Easier to start, but it tends to be rewritten in place, which erases the history of why things changed.
- **No written record.** Decisions would live only in commit messages and memory.

## Consequences

- Each decision has a stable place to be discussed, reviewed and referenced from code and pull requests.
- Writing an ADR adds a small cost to every significant decision.
- The index in [README.md](README.md) must be kept up to date when ADRs are added or superseded.
