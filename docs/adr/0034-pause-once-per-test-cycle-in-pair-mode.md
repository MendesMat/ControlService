---
status: accepted
date: 2026-09-27
accepted: 2026-09-27
scope: back-end
tags: [process, testing]
supersedes: ADR-0033 (pause after each phase only)
---

# ADR-0034: Pause once per test cycle in pair mode

## Context

ADR-0033 made all development test-first in pair mode, with the agent pausing **after each phase** of the Red → Green → Refactor cycle. Issue #5 had a list of 43 tests, so that rule meant about 130 pauses. After the first three cycles, the project owner asked for one pause per test instead, while still seeing the real output of every phase.

The pauses exist so the owner, who is learning TDD, keeps control of the direction of each change: choosing the next test and checking that each Red failed for the expected reason. A report of the whole cycle still shows every phase; what the owner chooses between cycles is the next test.

## Decision

- In pair mode, the agent runs the **whole cycle** for one test (Red, Green and Refactor) and then **pauses once**.
- The report at the pause shows each phase with its real output:
  - **Red:** the test, the failure and why it is the expected one.
  - **Green:** the code and the strategy (obvious implementation, fake it or triangulate).
  - **Refactor:** what changed, or "nothing to refactor".
  - The suite status, and the next two or three candidate tests. The owner chooses the next one.
- The agent **stops in the middle of a cycle**, before going on, when:
  - a test fails for an unexpected reason, or passes when it should fail;
  - a business rule or a design decision belongs to the owner.
- The owner can ask for a pause after each phase ("pause a cada fase") on any task.
- Autonomous mode and everything else in ADR-0033 stay unchanged.

## Alternatives considered

- **Pause after each phase (ADR-0033).** Three pauses per test slowed the work without adding decisions: the owner approves the Red and the Green, but the next choice only comes after the Refactor.
- **Pause after a group of tests.** Fewer interruptions, but the owner wants to choose each next test.
- **Autonomous mode by default.** Rejected for the same reason as in ADR-0033: the owner would review results instead of steering each step.

## Consequences

- About a third of the pauses, with the same evidence per phase.
- A Red that fails for the wrong reason is caught by the agent, not by the owner at a pause, which is why the agent must stop in that case.
- `AGENTS.md`, the [test-driven development workflow](../agents/workflows/test-driven-development.md) and the owner's guide describe the new cadence.
