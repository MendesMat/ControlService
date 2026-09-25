---
status: proposed
date: 2026-09-23
scope: back-end
tags: [operations]
---

# ADR-0029: Containerize the API and automate CI/CD

## Context

A portfolio project is much stronger when reviewers can see a green pipeline and try a live demo. Builds must also be reproducible on any machine.

## Decision

**Containers.** The API is published as an OCI image with the .NET SDK container support (`dotnet publish` with the `PublishContainer` target), without a hand-written Dockerfile. The image runs as a non-root user.

**Continuous integration.** GitHub Actions runs on every pull request and on every push to `main`:

1. restore and build, with warnings as errors (ADR-0026);
2. run unit, architecture and integration tests (Testcontainers, ADR-0024);
3. collect and publish code coverage;
4. check that the committed `openapi.json` matches the generated one (ADR-0004).

**Continuous delivery.** On `main`, the image is pushed to GitHub Container Registry and deployed to a container hosting platform for the public demo. **Azure Container Apps** is the preferred target because it fits the .NET ecosystem; the final provider is still open.

**Database migrations** run as a separate pipeline step before the new version starts (ADR-0013).

## Alternatives considered

- **Hand-written Dockerfile.** Full control and well known; SDK container publishing needs less maintenance. A Dockerfile can be added if a custom image becomes necessary.
- **Azure DevOps Pipelines.** Common in companies, but GitHub Actions keeps everything visible in the public repository.

## Consequences

- The README can show build status and coverage badges and link to the live demo.
- Hosting may have a cost; the demo environment should use the smallest available tier and scale to zero when idle.
