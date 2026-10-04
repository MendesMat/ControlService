---
name: review-pr
description: Review session of a Control Service pull request before the owner merges it - check the diff against the issue, the plan comment, the business rules, the accepted ADRs and the delivery rules, fix what is a correction on the same branch and ask the owner what is a decision. Use when the owner asks for the review session ("sessão de revisão") of a pull request, in a new conversation.
argument-hint: "[pull request number]"
---

# Review session

Third of the three sessions of an issue, in a **new conversation**: a reviewer that did not write the code. The procedure is section **9** of `docs/agents/workflows/implement-a-feature.md` (paths are from the repository root); this skill is its entry point, not a copy. If they disagree, the workflow wins.

Pull request: `$ARGUMENTS`.

## Steps

1. Read the pull request (`gh pr view <number>`, `gh pr diff <number>`), the issue it closes and its plan comment.
2. Load the domain skills the diff touches and `domain-record-contract`, then open the rules they cite. Review against the documents, not against the skills.
3. Check the five points of section 9:
   - every rule ID of the plan has a test, and the code does what the **rule** says, not only what the test asserts;
   - the code matches `docs/` and the accepted ADRs;
   - what the pull request and the tests claim is true;
   - the scope of the issue is complete, and nothing outside it slipped in;
   - the delivery rules: commits per green cycle, author identity, no attribution lines.
4. Compare every user-facing message in the diff with the feature document, character by character (CNV-16).
5. Report the findings in Portuguese, most serious first, each with the rule ID or ADR and the file and line.
6. Fix corrections as new commits on the same branch, test-first. Ask the owner about anything that is a decision, and when the document may be the wrong side of a mismatch.
7. Run `powershell.exe -NoProfile -File check.ps1`, push, wait for CI and hand the pull request over again. The owner merges.

## Hard stops

- Never merge, and never approve on the owner's behalf.
- A problem that belongs to another issue is reported, not fixed.
- Never change `docs/` to match the code silently.
