---
status: accepted
date: 2026-09-23
accepted: 2026-09-23
scope: back-end
tags: [platform]
---

# ADR-0002: Use .NET 10 LTS and C# 14

## Context

The back-end will be written in C#. Microsoft ships a new .NET version every November; even-numbered versions are Long Term Support (LTS) releases with three years of support.

.NET 10 was released in November 2025 as an LTS release, supported until November 2028. .NET 8 (LTS) and .NET 9 (STS) both reach end of support on November 10, 2026. A project started today on .NET 8 or 9 would be out of support within weeks.

## Decision

We will target **.NET 10** (`net10.0`), **C# 14** and **ASP.NET Core 10** for every project in the solution.

The SDK version will be pinned in `global.json`, with `rollForward` set to `latestFeature`, so every machine and the CI pipeline build with the same major SDK.

## Alternatives considered

- **.NET 8 LTS.** Widely used in existing systems, but its support ends on November 10, 2026.
- **.NET 9 STS.** Same end-of-support date as .NET 8, with no advantage over .NET 10.

## Consequences

- The project runs on a supported runtime until November 2028, with no forced migration during the portfolio's useful life.
- Some third-party packages may lag behind .NET 10; each dependency must be checked for `net10.0` compatibility before it is added.
- A migration to the next LTS (.NET 12, expected November 2027) should be planned before November 2028.

## References

- [.NET and .NET Core Support Policy](https://dotnet.microsoft.com/platform/support-policy/dotnet-core)
