## What

<!-- What does this pull request change? One or two sentences. -->

## Why

<!-- Which business rule (cite its ID, e.g. USR-06), open question or ADR motivates it? Link docs/product/ or docs/adr/ when relevant. -->

## How to test

<!-- Steps or requests (Scalar / .http file) that show the change working. -->

## Checklist

- [ ] Tests cover the new behavior (domain rules have unit tests)
- [ ] `dotnet test --solution ControlService.slnx` passes locally
- [ ] Documentation updated (`docs/product/`, ADRs, README) when behavior or decisions changed
- [ ] Rule IDs covered or changed are listed above (for example USR-06, PERM-05)
- [ ] User-facing messages are verbatim from the feature document (CNV-16)
