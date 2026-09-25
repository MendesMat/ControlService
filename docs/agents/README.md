# Agent guides and workflows

Detailed instructions for AI coding agents. The entry point, with the non-negotiable rules, is [AGENTS.md](../../AGENTS.md) at the repository root. The business rules themselves are in [docs/product](../product/); the map of all documentation is the [docs index](../README.md).

- **Guides** describe *how things are done here*: conventions, patterns and constraints. Read the relevant ones before working.
- **Workflows** are *step-by-step procedures* for recurring tasks. Follow them in order and do not skip steps.

## Guides

| Guide | Covers |
|---|---|
| [architecture.md](guides/architecture.md) | Layers, dependency rule, feature folders, DDD building blocks, Result pattern, endpoints |
| [coding-conventions.md](guides/coding-conventions.md) | Naming, style, language of each artifact, packages, warnings |
| [testing.md](guides/testing.md) | Test projects, naming, commands, coverage, what to test where |
| [documentation.md](guides/documentation.md) | What to update in `docs/` and in the ADRs, and when |
| [local-environment.md](guides/local-environment.md) | Aspire, Docker naming, ports, HTTPS, Windows and PowerShell pitfalls |
| [communication.md](guides/communication.md) | How to talk to the owner, ask questions and report results |

## Workflows

| Workflow | Use it to |
|---|---|
| [git-and-pull-requests.md](workflows/git-and-pull-requests.md) | Deliver any change through a branch and a pull request |
| [implement-a-feature.md](workflows/implement-a-feature.md) | Build a slice of the roadmap from the business rules to a merged pull request |
| [record-a-decision.md](workflows/record-a-decision.md) | Propose, accept or supersede an ADR |
| [review-dependency-updates.md](workflows/review-dependency-updates.md) | Evaluate and merge Dependabot pull requests |

## For the owner

[trabalhando-com-agentes.md](trabalhando-com-agentes.md) (Portuguese): how to ask for tasks, review pull requests and keep conversations productive when pairing with an agent.

## Enforced rules

`.claude/settings.json` blocks, in Claude Code, pushing to `main`, force pushing and trusting certificates. The rules in `AGENTS.md` still apply to every agent; the settings file only makes the most critical ones impossible to break by mistake. Changing it requires the owner's approval.

## Maintaining these files

- Keep `AGENTS.md` short: rules, commands and links. Details belong here.
- One topic per file. A new recurring task gets a workflow; a new convention goes into the matching guide.
- Write in English, in the imperative ("Run", "Never"), with concrete paths and commands.
- When a rule changes, update it in one place and link to it instead of copying it.
- These files change through pull requests like any other file.
