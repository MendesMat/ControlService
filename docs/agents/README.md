# Agent guides and workflows

Detailed instructions for AI coding agents. The entry point, with the non-negotiable rules, is [AGENTS.md](../../AGENTS.md) at the repository root. The business rules themselves are in [docs/product](../product/); the map of all documentation is the [docs index](../README.md).

## The three commands

Work on an issue goes through three commands, each one a skill in `.claude/skills/`, written in Portuguese because they are the owner's own process and everything they produce is in Portuguese. Claude Code loads them by name; any other agent reads the file.

| Command | Skill | Ends with |
|---|---|---|
| `/levantar-issue <n>` | [levantar-issue](../../.claude/skills/levantar-issue/SKILL.md) | The issue rewritten on GitHub: scope, numbered tests, acceptance criteria |
| `/executar-issue <n>` | [executar-issue](../../.claude/skills/executar-issue/SKILL.md) | The pull request open with CI green, and the final report |
| `/revisar-issue <n>` | [revisar-issue](../../.claude/skills/revisar-issue/SKILL.md) | Findings by severity, explained for the owner to decide |

## Guides

| Guide | Covers |
|---|---|
| [architecture.md](guides/architecture.md) | Layers, dependency rule, feature folders, DDD building blocks, Result pattern, endpoints |
| [coding-conventions.md](guides/coding-conventions.md) | Naming, style, language of each artifact, packages, warnings |
| [testing.md](guides/testing.md) | Test projects, naming, commands, what to test where |
| [documentation.md](guides/documentation.md) | What to update in `docs/`, and when |
| [local-environment.md](guides/local-environment.md) | Aspire, Docker naming, ports, HTTPS, Windows and PowerShell pitfalls |
| [communication.md](guides/communication.md) | How to talk to the owner, ask questions and report results |

## Other procedures

| Procedure | Use it to |
|---|---|
| [git-and-pull-requests.md](workflows/git-and-pull-requests.md) | Deliver any change through a branch and a pull request |
| [record-a-decision.md](workflows/record-a-decision.md) | Change an ADR, while the ADRs exist |
| [review-dependency-updates.md](workflows/review-dependency-updates.md) | Evaluate and merge Dependabot pull requests |

## For the owner

[trabalhando-com-agentes.md](trabalhando-com-agentes.md) (Portuguese): how to use the three commands, read the final report and decide on review findings.

## Enforced rules

`.claude/settings.json` blocks, in Claude Code, pushing to `main`, force pushing and trusting certificates. The rules in `AGENTS.md` still apply to every agent; the settings file only makes the most critical ones impossible to break by mistake. Changing it requires the owner's approval.

## Maintaining these files

- Keep `AGENTS.md` short: rules, commands and links.
- A new convention goes into the matching guide. A new procedure is added only when a task really recurs.
- When a rule changes, update it in one place and link to it instead of copying it.
- These files change through pull requests like any other file.
