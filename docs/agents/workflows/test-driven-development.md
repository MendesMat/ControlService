# Workflow: test-driven development

All development in this repository is test-driven (ADR-0033). The goal is *clean code that works*: every behavior starts as a failing test, and the code grows only as far as the tests demand.

## Roles and modes

**Pair mode is the default.** The owner is the **navigator**: chooses the next behavior to test and takes the design decisions. The agent is the **driver**: writes the test, the code and the refactoring, and **pauses after each phase** until the owner says to continue. The owner is learning TDD through this project, so the pauses are the point, not an overhead.

**Autonomous mode** only when the owner explicitly asks for it, for a specific task ("faça sozinho", "modo autônomo", "sem pausas"). The phases and rules stay the same; the pauses are replaced by evidence (see below). The owner can say "passo a passo" at any moment to return to pair mode.

## The two golden rules

1. **Write production code only to make a failing test pass.** No logic without a test that demands it.
2. **Eliminate duplication.** After the test passes, remove duplication in the production code *and* in the tests.

## Before the first cycle: the test list

Read the issue and the feature document, then show the owner a short list of behaviors to test, as one-line test names, ordered from the simplest to the most complex, each with the rule ID it covers:

```
1. Phone_with_11_digits_is_valid                     (USR-10)
2. Phone_with_10_digits_is_valid                     (USR-10)
3. Phone_with_mask_is_stored_as_digits_only          (CNV-07)
4. Phone_with_9_digits_is_rejected_with_message      (USR-10)
```

The owner may reorder, add or remove items. New behaviors discovered along the way go **to the end of the list**; do not chase them immediately.

## The cycle

### Red
- Write **one** small test for the next behavior. Start from the assertion and work backwards to the objects it needs: the test tells the story of how the code should be used.
- Run it with the filtered command and show the real output:

  ```bash
  dotnet test --project tests/ControlService.Domain.Tests --filter-method '*Phone*'
  ```

- Confirm it fails **for the expected reason**: a wrong value, or a type or member that does not exist yet (a compile error is a valid Red). A typo or a broken test is not a Red; fix the test and run it again.
- **Pause:** show the test, the failure and why it is the expected one. Ask "sigo para o Green?".

### Green
- Write the **smallest** code that makes the test pass. Speed beats elegance in this phase. Choose a strategy and say which:
  - **Obvious implementation:** the real code, when it is simple and you are sure.
  - **Fake it:** return a constant, then replace it step by step.
  - **Triangulate:** generalize only when two or more examples demand it.
- Run the filtered tests and show them green.
- **Pause:** show the code, the strategy and the output. Ask "sigo para o Refactor?".

### Refactor
- With the tests green, remove the shortcuts and the duplication from the Green phase, in production code and in tests. Improve names, extract methods, apply the [coding conventions](../guides/coding-conventions.md). Never change behavior here.
- Run the tests again and show them green. "Nothing to refactor" is a valid result: say so instead of inventing a change.
- **Pause:** show what changed (or that nothing did) and propose the next two or three candidate tests from the list, simplest first. The owner chooses.

Commit when a cycle ends green, or after a few small cycles on the same behavior. Never commit a red build to a branch that you push.

### Evidence in autonomous mode

Plan the test list and get it approved as above. Then, for every cycle, log compactly: the test added, why it failed, the Green strategy, what was refactored (or "nothing"), and the suite status with the command output. Hand control back when a requirement is ambiguous, a decision belongs to the owner, a test fails for an unexpected reason twice, or the list is finished.

## Step size

TDD is being *able* to take tiny steps, not always taking them. Use obvious implementation when you know what to type. When a test surprises you (it passes when it should fail, or fails in an unexpected way), shift down: fake it, triangulate, smaller steps.

## Writing the tests

- **Test behavior, not implementation.** Assert on what the public API does, never on private helpers or the order of internal calls. The Refactor phase must be able to change the internals without touching a test.
- **Prefer in-memory fakes to mocks** (ADR-0033). A fake is a small hand-written class in the test project (`InMemoryUserRepository`, `FakeEmailSender` that stores the messages it "sends", a fixed `TimeProvider`); the test checks the outcome. Use NSubstitute only when a fake would be clearly heavier, and never to verify internal calls (`Received()`) unless that call *is* the observable outcome.
- **Evident data:** make the relation between input and expected output obvious (`"(21) 98765-4321"` → `"21987654321"`). Use fictitious personal data only.
- **Edge cases are their own cycles:** empty input, limits (3 and 30 characters for a login), invalid input and each error path get their own failing test first.
- **One behavior per test,** named after it with underscores (`Login_longer_than_30_characters_is_rejected`), citing the rule ID in a comment when the name does not make it obvious. `[Theory]` tables are fine for many examples of the *same* behavior.
- Tests are isolated: no shared mutable state, no order dependency, no real clock.

## TDD in each layer

| Layer | First failing test | Where |
|---|---|---|
| Domain | A unit test of the value object, aggregate method or domain service | `ControlService.Domain.Tests` |
| Application | A unit test of the handler or validator with in-memory fakes of its ports | `ControlService.Application.Tests` |
| Persistence | An integration test against real PostgreSQL (Testcontainers) that saves and reads, or hits a constraint | `ControlService.Api.IntegrationTests` |
| API | An integration test with `WebApplicationFactory` that calls the route and asserts the status code and body from the feature document | `ControlService.Api.IntegrationTests` |

Work **inside out** inside an issue (domain → application → persistence → API), or start with a failing API test that describes the whole operation and let it drive the inner tests. Say which approach you take in the test list.

## When TDD is the wrong tool

Some pieces give no useful Red: registering services in DI, `Program.cs` wiring, Aspire and configuration, EF Core mapping details, migrations, Docker files. Cover them with an integration test that exercises the behavior through them, write that test first when possible, and **say in one sentence** that this piece follows the lighter approach and why. Exploratory spikes are allowed when the approach is unknown: keep what you learned, throw the spike away, and rebuild it test-first. Never drop the discipline silently.

## Bug fixes

1. Write the smallest test that reproduces the bug, with the input from the report.
2. Run it and confirm it fails **for the reason the report describes**. A different failure means a different bug or a wrong test.
3. Green and Refactor as usual. The test stays as a regression test.
4. If you cannot reproduce it with a test, you do not understand the bug yet: investigate or ask instead of changing code.

## Example: two cycles in C#

Illustrative only; the real API of the value objects comes from issue #4.

**Cycle 1, Red.** The type does not exist, so the build fails: a valid Red.

```csharp
[Fact]
public void Phone_with_11_digits_is_valid() // USR-10
{
    var result = PhoneNumber.Create("21987654321");

    result.IsSuccess.ShouldBeTrue();
}
```

**Green (fake it).** `Create` returns a success for any input. The test passes. Nothing to refactor yet.

**Cycle 2, Red.** A second example forces the real rule: the fake returns success, so the assertion fails with the expected message.

```csharp
[Fact]
public void Phone_with_9_digits_is_rejected() // USR-10
{
    var result = PhoneNumber.Create("219876543");

    result.IsFailure.ShouldBeTrue();
    result.Error.Message.ShouldBe("Digite o telefone com DDD. Ex.: (21) 98765-4321.");
}
```

**Green (triangulate).** Two examples now justify the real check: count the digits and accept 10 or 11. **Refactor:** extract the digit extraction (it will serve CNV-07 too) and a test helper for repeated arrangements; run everything green again.
