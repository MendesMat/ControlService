---
status: accepted
date: 2026-09-23
accepted: 2026-09-24
scope: back-end
tags: [quality]
---

# ADR-0026: Enforce build quality settings across the solution

## Context

With several projects, package versions and compiler settings tend to drift apart. Nullable reference types and analyzers catch whole classes of bugs, but only if they are on everywhere.

## Decision

- **Central Package Management**: every package version is declared once in `Directory.Packages.props`.
- **`Directory.Build.props`** applies to every project:
  - `Nullable` enabled
  - `ImplicitUsings` enabled
  - `TreatWarningsAsErrors` enabled
  - `AnalysisLevel` set to `latest-recommended`
  - `EnforceCodeStyleInBuild` enabled
- An **`.editorconfig`** defines code style and analyzer severities.
- **`global.json`** pins the .NET 10 SDK (ADR-0002).
- NuGet **package auditing** is enabled, so known vulnerable dependencies fail the build.

## Alternatives considered

- **Per-project settings.** Flexible, but inconsistent.
- **Warnings without failing the build.** Less friction, but warnings accumulate and get ignored.

## Consequences

- The codebase stays consistent, and new projects inherit the rules automatically.
- Treating warnings as errors can slow down experiments; suppressions must be local and justified.
