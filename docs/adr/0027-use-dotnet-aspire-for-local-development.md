# ADR-0027: Use .NET Aspire for local development

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Running the API locally requires PostgreSQL, Redis (for HybridCache, ADR-0020), MinIO (ADR-0018) and Mailpit (ADR-0030). Setting these up by hand, with matching connection strings, is a common source of friction. Aspire 13 was released together with .NET 10.

## Decision

- The solution includes an **Aspire AppHost** project that starts PostgreSQL, Mailpit and the API with one command. Connection strings are injected automatically. Redis (ADR-0020) and MinIO (ADR-0018) are added to the AppHost when those decisions are implemented.
- A **ServiceDefaults** project configures OpenTelemetry, health checks, service discovery and HTTP resilience for the API (ADR-0028).
- The **Aspire dashboard** is used locally to inspect logs, traces and metrics.
- Aspire is used for development only. Deployment uses container images (ADR-0029).

## Alternatives considered

- **Docker Compose.** Universal and well known, but connection strings and telemetry must be wired by hand. A `compose.yaml` can still be generated for people who prefer it.
- **Manual installation of each service.** Maximum friction for anyone cloning the repository.

## Consequences

- Anyone cloning the repository can run the whole system with Docker and the .NET SDK installed.
- The solution gains two infrastructure projects (AppHost and ServiceDefaults) in addition to the four Clean Architecture projects.
