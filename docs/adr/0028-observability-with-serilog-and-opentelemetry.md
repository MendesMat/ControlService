---
status: proposed
date: 2026-09-23
scope: back-end
tags: [operations]
---

# ADR-0028: Observability with Serilog and OpenTelemetry

## Context

When something fails in the demo or in a reviewer's hands, the cause must be found quickly. Error responses already carry a `traceId` (ADR-0009), which is only useful if logs and traces can be searched by it.

## Decision

- **Serilog** for structured logging, with request logging that records method, path, status, duration, user id and **display name**. The id is the stable reference; the display name is unique (see `docs/product/features/users.md`), so people reading the logs can recognize who acted without looking the id up. Personal data such as CPF, phone and signatures is never logged, and neither are passwords, tokens or activation and reset links.
- **OpenTelemetry** for traces and metrics (ASP.NET Core, HttpClient, EF Core, Npgsql), configured in ServiceDefaults (ADR-0027) and exported over OTLP.
  - Locally, the destination is the Aspire dashboard.
  - In the demo environment, it is the hosting platform's monitoring or any OTLP-compatible backend.
- **Health checks** at `/health/live` (process is up) and `/health/ready` (database and storage reachable), used by the container platform.

## Alternatives considered

- **Built-in `ILogger` only.** Sufficient for development, but less convenient for structured sinks and enrichment.
- **A vendor-specific agent.** Easy to set up, but ties the code to one vendor; OpenTelemetry keeps it portable.

## Consequences

- Any error reported by a user can be traced end to end using the `traceId` shown in the error response.
- Logging rules must be reviewed whenever new personal data is added.
