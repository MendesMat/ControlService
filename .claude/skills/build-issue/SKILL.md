---
name: build-issue
description: Build session of a Control Service issue - implement the approved plan comment test-first, in pair mode, from the domain to the API, until the pull request is open with CI green. Use when the owner asks for the build session ("sessão de construção") of an issue that already has a plan comment.
argument-hint: "[issue number]"
---

# Build session

Second of the three sessions of an issue. The procedure is sections **3 to 8** of `docs/agents/workflows/implement-a-feature.md`, with every step following `docs/agents/workflows/test-driven-development.md` and the delivery following `docs/agents/workflows/git-and-pull-requests.md` (paths are from the repository root). This skill is their entry point, not a copy. If they disagree, the workflows win.

Issue: `$ARGUMENTS`. If the issue has no plan comment, stop and say so: the plan session comes first.

## Steps

1. Read the issue and its plan comment (`gh issue view <number> --comments`). Start from the plan instead of repeating the survey; read the documents it cites.
2. Load the domain skills the plan touches and `domain-record-contract`.
3. Prepare the branch from an updated `main` and check the commit identity (git workflow, steps 1 and 2).
4. Run the test list in order, inside out: domain → application → infrastructure → API. One Red → Green → Refactor cycle per test, with the real output of each phase.
5. Verify with `powershell.exe -NoProfile -File check.ps1` from `backend/ControlService`, then end to end (workflow section 7).
6. Update `docs/` in the same pull request (`docs/agents/guides/documentation.md`). When a rule owned by a domain skill changes, update that skill too.
7. Open the pull request with `Closes #<number>`, wait for CI, hand over in Portuguese and stop.

## Mode

**Pair mode is the default (ADR-0033):** run the whole cycle for one test and pause once at its end, until the owner says to continue. Autonomous mode only when the owner asks for it on this task. This project's TDD workflow takes precedence over any generic TDD skill or habit.

## Hard stops

- A test fails for an unexpected reason, or passes when it should fail: show the output and ask.
- An approach from the plan does not work: stop and ask. Never replace it silently.
- Two failed attempts at the same problem: stop and explain what you tried.
- Anything in the "Ask the owner first" list of `AGENTS.md`: a package, a migration, a contract change.
- Never merge; never add attribution lines to commits or to the pull request body.
