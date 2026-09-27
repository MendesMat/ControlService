---
status: accepted
date: 2026-09-25
accepted: 2026-09-25
scope: back-end
tags: [process, testing, quality]
amends: ADR-0024 (test doubles and when tests are written)
---

# ADR-0033: Develop test-first, in pair mode, with in-memory fakes

## Context

ADR-0024 defines what is tested and with which tools, but not **when** tests are written. The agent guidelines only required domain rules to be test-first, so handlers, persistence and endpoints could be written before their tests.

The project owner wants all development to follow test-driven development (TDD). Code is written by AI agents working as pair programmers with the owner, a junior developer who is learning the practice through this project. Two needs follow: the owner must keep control of the direction of each change, and the agents must prove each step instead of claiming it.

ADR-0024 also chose NSubstitute for the application layer. Mocks that verify internal calls tie tests to the current implementation, which makes the Refactor step of TDD expensive.

## Decision

- **Every behavior change starts with a failing test**, in every layer: unit tests for the domain and the application, integration tests for persistence and the API. The cycle is Red → Green → Refactor, with the two rules "write code only to make a failing test pass" and "eliminate duplication".
- **Pair mode is the default.** The owner is the navigator and chooses the next test; the agent is the driver. The owner approves a test list before the first cycle.
- **One pause per test cycle.** The agent runs Red, Green and Refactor for one test, then pauses once. At the pause it shows each phase with its real output:
  - **Red:** the test, the failure and why it is the expected one.
  - **Green:** the code and the strategy.
  - **Refactor:** what changed, or "nothing to refactor".
  - The suite status, and the next candidate tests.
- The agent **stops in the middle of a cycle**, before going on, when:
  - a test fails for an unexpected reason, or passes when it should fail;
  - a business rule or a design decision belongs to the owner.
- On any task, the owner can ask for a pause after each phase ("pause a cada fase").
- **Autonomous mode** is used only when the owner asks for it on a task. The phases are the same; each one is proven by the command output in a per-cycle log.
- **Test doubles:** hand-written in-memory fakes are preferred, and tests assert on outcomes. NSubstitute stays available for cases where a fake would be clearly heavier, but not to verify internal calls.
- Pieces with no meaningful Red (dependency injection wiring, configuration, mapping details, migrations) are covered by integration tests, and the agent says so explicitly.
- The procedure is described in [`docs/agents/workflows/test-driven-development.md`](../agents/workflows/test-driven-development.md).

## Alternatives considered

- **Test-after.** Faster at first, but tests shaped by the code tend to confirm the implementation instead of the rules, and the owner would not learn TDD.
- **TDD only in the domain.** Covers the rules, but lets handlers and endpoints grow untested paths; rejected by the owner.
- **Autonomous mode by default.** Much faster for issues with dozens of tests, but the owner would review results instead of steering each step; the owner chose to learn with the pauses.
- **A pause after each phase.** The first version of this record. Three pauses per test slowed the work without adding decisions: the next choice only comes after the Refactor, and the report at the end of the cycle still shows every phase.
- **Mocks first (NSubstitute everywhere).** Less test code, but brittle under refactoring.

## Consequences

- Each issue takes more interaction time; the owner can switch a task to autonomous mode when it is routine.
- A Red that fails for the wrong reason must be caught by the agent, which is why it stops in that case instead of going on.
- The test suite documents the rules through rule IDs and becomes the safety net for refactoring.
- The test projects gain a small set of fakes that later features reuse.
- Pull requests state that the change was developed test-first.

## Revisions

- **2026-09-27:** the pause moved from after each phase to once per test cycle. The owner asked for it during issue #5, where 43 tests meant about 130 pauses. As an exception decided by the owner, this record was updated in place instead of being superseded.
