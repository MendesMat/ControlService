# ADR-0010: Map objects manually, with Mapperly as an option

- **Status:** Proposed
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Entities must be converted to response DTOs and requests to commands. AutoMapper was the traditional choice, but it launched a commercial edition in July 2025 alongside MediatR. Reflection-based mapping also hides errors until runtime.

## Decision

We will map objects with **hand-written extension methods** (`user.ToResponse()`, `request.ToCommand()`), kept next to the DTOs of each feature.

If mapping code grows repetitive, **Mapperly** may be introduced. It generates plain mapping code at compile time, so errors surface in the build.

## Alternatives considered

- **AutoMapper.** Familiar, but now commercially licensed, and its runtime configuration can fail silently.
- **Mapster.** Capable, with both runtime and code-generation modes; Mapperly's compile-time-only approach was preferred.

## Consequences

- Mappings are explicit, easy to debug and friendly to "find all references".
- Adding a field requires updating the mapping by hand, which also forces a conscious decision about exposing it.
