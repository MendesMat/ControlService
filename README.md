# Control Service

[![CI](https://github.com/MendesMat/ControlService/actions/workflows/ci.yml/badge.svg)](https://github.com/MendesMat/ControlService/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

An ERP for service companies: users and permissions, service catalog, clients and routes, accounts payable and receivable, and reports. It is built for people with little familiarity with technology, so every message, label and error is written in plain Portuguese.

> **Status:** work in progress. The documentation and the architecture are defined, and the back-end skeleton is ready. The first feature slice being built is **sign-in, user management and per-screen permissions**.

## Highlights

- **Per-screen permissions.** Each permission profile gives one of four ordered levels (*Denied < Reader < Editor < Manager*) to every screen. A user may hold several profiles, and the effective level on each screen is the highest one among them.
- **Accounts created by the team.** Nobody signs up alone: whoever registers a user sets a temporary password, and the system makes the person replace it when they first sign in.
- **Safe concurrent editing.** Optimistic concurrency rejects a save when someone else changed the record in the meantime, and tells who did it.
- **Traceability.** Every record stores who created it, who last changed it and when. Users are deactivated, never deleted.
- **Test-driven development.** Every behavior starts as a failing test, in every layer, and business rules carry stable IDs that the tests cite ([decision 27](docs/decisoes-de-arquitetura.md)).
- **Architecture enforced by tests.** Clean Architecture layers are separate projects, and architecture tests fail the build if a layer depends on the wrong one.
- **Decisions on record.** Every significant technical choice is explained in the [decisions document](docs/decisoes-de-arquitetura.md), with what it costs and the alternatives considered.
- **AI-assisted, human-reviewed.** AI coding agents implement changes following [AGENTS.md](AGENTS.md): they work on branches and open pull requests, and the owner reviews and merges every one of them.

## Tech stack

| Area | Technology |
|---|---|
| Runtime | .NET 10, C# 14 |
| API | ASP.NET Core Minimal APIs, OpenAPI with Scalar |
| Architecture | Clean Architecture, tactical DDD, CQRS without MediatR, Result pattern with Problem Details |
| Data | PostgreSQL, EF Core |
| Security | ASP.NET Core Identity, JWT with rotating refresh tokens, rate limiting |
| Local environment | .NET Aspire (PostgreSQL in Docker, dashboard with logs and traces) |
| Tests | xUnit v3, Shouldly, NetArchTest, WebApplicationFactory, Testcontainers |
| Quality | Warnings as errors, analyzers, central package management, GitHub Actions, Dependabot |

## Architecture

```
backend/ControlService/src
├── ControlService.Domain           Entities, value objects and business rules (no dependencies)
├── ControlService.Application      Use cases and the interfaces they need
├── ControlService.Infrastructure   EF Core, Identity: implementations of those interfaces
├── ControlService.API              Minimal API endpoints and composition root
├── ControlService.AppHost          Aspire orchestration for local development
└── ControlService.ServiceDefaults  Telemetry and health checks
```

Dependencies point inward only: `API → Application → Domain`, with `Infrastructure` implementing the Application interfaces. Inside each project, code is grouped by feature (`Users`, `PermissionProfiles`, `Auth`). See the [decisions document](docs/decisoes-de-arquitetura.md) and the [back-end README](backend/ControlService/README.md).

## Getting started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Docker Desktop](https://www.docker.com/products/docker-desktop/).

Trust the HTTPS development certificate once per machine:

```bash
dotnet dev-certs https --trust
```

Start PostgreSQL and the API with one command:

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
| [Product documentation](docs/README.md) | English | Product overview, glossary, business rules by feature (with stable rule IDs), API conventions and open questions |
| [Architecture decisions](docs/decisoes-de-arquitetura.md) | Portuguese | How the back-end is built, why, what each choice costs and which alternatives were rejected |
| [Back-end guide](backend/ControlService/README.md) | Portuguese | How to run, project structure and what each Docker resource is |
| [AGENTS.md](AGENTS.md) and [agent guides](docs/agents/README.md) | English | Rules, guides and workflows for the AI coding agents that work on this repository |
| [Trabalhando com agentes](docs/agents/trabalhando-com-agentes.md) | Portuguese | How the owner pairs with AI agents: asking for tasks, reviewing pull requests |

## Roadmap

The current slice is tracked in the milestone [M1: Sign-in, users and permissions](https://github.com/MendesMat/ControlService/milestone/1), one issue per step.

- [x] Product documentation and architecture decisions
- [x] Back-end skeleton, local environment with Aspire, CI
- [x] Domain: value objects, users, permission profiles, effective access
- [x] Persistence: EF Core, audit fields, concurrency, seeded system records
- [x] Sign-in, sessions and the current user (`/me`)
- [ ] Per-screen authorization
- [ ] Permission profiles and users endpoints, with temporary passwords
- [ ] Connect the front-end to the real API
- [ ] Remaining screens: catalog, commercial, finance and reports

## License

[MIT](LICENSE) © Matheus Mendes
