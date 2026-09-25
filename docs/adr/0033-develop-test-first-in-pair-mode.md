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
- **Pair mode is the default.** The owner is the navigator and chooses the next test; the agent is the driver and **pauses after each phase**, showing the real test output. The owner approves a test list before the first cycle.
- **Autonomous mode** is used only when the owner asks for it on a task. The phases are the same; each one is proven by the command output in a per-cycle log.
- **Test doubles:** hand-written in-memory fakes are preferred, and tests assert on outcomes. NSubstitute stays available for cases where a fake would be clearly heavier, but not to verify internal calls.
- Pieces with no meaningful Red (dependency injection wiring, configuration, mapping details, migrations) are covered by integration tests, and the agent says so explicitly.
- The procedure is described in [`docs/agents/workflows/test-driven-development.md`](../agents/workflows/test-driven-development.md).

## Alternatives considered

- **Test-after.** Faster at first, but tests shaped by the code tend to confirm the implementation instead of the rules, and the owner would not learn TDD.
- **TDD only in the domain.** Covers the rules, but lets handlers and endpoints grow untested paths; rejected by the owner.
- **Autonomous mode by default.** Much faster for issues with dozens of tests, but the owner would review results instead of steering each step; the owner chose to learn with the pauses.
- **Mocks first (NSubstitute everywhere).** Less test code, but brittle under refactoring.

## Consequences

- Each issue takes more interaction time; the owner can switch a task to autonomous mode when it is routine.
- The test suite documents the rules through rule IDs and becomes the safety net for refactoring.
- The test projects gain a small set of fakes that later features reuse.
- Pull requests state that the change was developed test-first.
