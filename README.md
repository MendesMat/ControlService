# Control Service

[![CI](https://github.com/MendesMat/ControlService/actions/workflows/ci.yml/badge.svg)](https://github.com/MendesMat/ControlService/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

An ERP for service companies: users and permissions, service catalog, clients and routes, accounts payable and receivable, and reports. It is built for people with little familiarity with technology, so every message, label and error is written in plain Portuguese.

> **Status:** work in progress. The documentation and the architecture are defined, and the back-end skeleton is ready. The first feature slice being built is **sign-in, user management and per-screen permissions**.

## Highlights

- **Per-screen permissions.** Each permission profile gives one of four ordered levels (*Denied < Reader < Editor < Manager*) to every screen. A user may hold several profiles, and the effective level on each screen is the highest one among them.
- **Invitation-based accounts.** Nobody signs up alone: a new user receives a single-use activation link by e-mail and creates their own password. Nobody ever sees or types someone else's password.
- **Safe concurrent editing.** Optimistic concurrency rejects a save when someone else changed the record in the meantime, and tells who did it.
- **Traceability.** Every record stores who created it, who last changed it and when. Users are deactivated, never deleted.
- **Architecture enforced by tests.** Clean Architecture layers are separate projects, and architecture tests fail the build if a layer depends on the wrong one.
- **Decisions on record.** Every significant technical choice has an [Architecture Decision Record](docs/adr/README.md) with the alternatives considered.
- **AI-assisted, human-reviewed.** AI coding agents implement changes following [AGENTS.md](AGENTS.md): they work on branches and open pull requests, and the owner reviews and merges every one of them.

## Tech stack

| Area | Technology |
|---|---|
| Runtime | .NET 10, C# 14 |
| API | ASP.NET Core Minimal APIs, OpenAPI with Scalar |
| Architecture | Clean Architecture, tactical DDD, CQRS without MediatR, Result pattern with Problem Details |
| Data | PostgreSQL, EF Core |
| Security | ASP.NET Core Identity, JWT with rotating refresh tokens, rate limiting |
| E-mail | SMTP with MailKit; Mailpit as the local test inbox |
| Local environment | .NET Aspire (PostgreSQL and Mailpit in Docker, dashboard with logs and traces) |
| Tests | xUnit v3, Shouldly, NSubstitute, NetArchTest, WebApplicationFactory, Testcontainers |
| Quality | Warnings as errors, analyzers, central package management, GitHub Actions, Dependabot |

## Architecture

```
backend/ControlService/src
├── ControlService.Domain           Entities, value objects and business rules (no dependencies)
├── ControlService.Application      Use cases and the interfaces they need
├── ControlService.Infrastructure   EF Core, Identity, e-mail: implementations of those interfaces
├── ControlService.API              Minimal API endpoints and composition root
├── ControlService.AppHost          Aspire orchestration for local development
└── ControlService.ServiceDefaults  Telemetry, health checks and resilience
```

Dependencies point inward only: `API → Application → Domain`, with `Infrastructure` implementing the Application interfaces. Inside each project, code is grouped by feature (`Users`, `PermissionProfiles`, `Auth`). See [ADR-0005](docs/adr/0005-adopt-clean-architecture.md) and the [back-end README](backend/ControlService/README.md).

## Getting started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Docker Desktop](https://www.docker.com/products/docker-desktop/).

Trust the HTTPS development certificate once per machine:

```bash
dotnet dev-certs https --trust
```

Start PostgreSQL, Mailpit and the API with one command:

```bash
cd backend/ControlService
dotnet run --project src/ControlService.AppHost
```

The console prints the Aspire dashboard login link. The interactive API reference is at `https://localhost:7243/scalar`.

Run the tests:

```bash
dotnet test --solution ControlService.slnx
```

## Documentation

| Document | Language | Content |
|---|---|---|
| [Functional documentation](docs/README.md) | Portuguese | Product, data model, business rules, permissions and the front-end contract |
| [Architecture Decision Records](docs/adr/README.md) | English | What was decided for the back-end, why, and which alternatives were rejected |
| [Back-end guide](backend/ControlService/README.md) | Portuguese | How to run, project structure and what each Docker resource is |
| [AGENTS.md](AGENTS.md) and [agent guides](docs/agents/README.md) | English | Rules, guides and workflows for the AI coding agents that work on this repository |

## Roadmap

- [x] Functional documentation and architecture decisions
- [x] Back-end skeleton, local environment with Aspire, CI
- [ ] Domain: value objects, users, permission profiles, effective access
- [ ] Persistence: EF Core, audit fields, concurrency, seeded system records
- [ ] Authentication and per-screen authorization
- [ ] Permission profiles and users endpoints, activation and password reset links
- [ ] Connect the front-end to the real API
- [ ] Remaining screens: catalog, commercial, finance and reports

## License

[MIT](LICENSE) © Matheus Mendes
