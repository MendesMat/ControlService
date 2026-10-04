---
name: plan-issue
description: Plan session of a Control Service issue - survey the rules, report the gaps to the owner, agree on the test list and post the plan comment on the issue. Use when the owner asks for the plan session ("sessão de plano") of an issue, or to plan the next issue of the milestone before any code is written.
argument-hint: "[issue number]"
---

# Plan session

First of the three sessions of an issue. The procedure is sections **1 and 2** of `docs/agents/workflows/implement-a-feature.md` (paths are from the repository root); this skill is its entry point, not a copy. If they disagree, the workflow wins.

Issue: `$ARGUMENTS`. When empty, take the lowest open issue of the milestone (`AGENTS.md`, "Work items").

## Steps

1. Read the issue: `gh issue view <number>`.
2. Load the domain skills the issue touches (`domain-users`, `domain-authentication`, `domain-permissions`) and `domain-record-contract`, then read the feature documents and ADRs they point to. The skills tell you where the traps are; the documents hold the rules and the verbatim messages.
3. Follow section 1 of the workflow: list the rule IDs, the error cases without a message, duplicated IDs and open questions the issue depends on.
4. **Report the survey in Portuguese and wait**: what you read, each gap and inconsistency with your recommendation. Do not fill a gap with an assumption.
5. After the answers, show the test list (section 2 and the test list of `docs/agents/workflows/test-driven-development.md`): one-line names, simplest first, each with its rule ID and its **expected result** (status, code, message or value), plus the files you expect to create. Wait for approval.
6. Check every decision against the rule IDs it touches, then post the plan comment in English with `gh issue comment <number> --body-file <file>`.
7. Stop. The build session is another conversation.

## Hard stops

- No production code and no tests in this session.
- Never post the plan comment before the owner approves the test list.
- A vague decision in the plan is filled in by guesswork later: write the exact expected result.
