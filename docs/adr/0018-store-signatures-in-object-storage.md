---
status: proposed
date: 2026-09-23
scope: back-end
tags: [data]
---

# ADR-0018: Store signatures in object storage

## Context

The user's signature is currently stored inside the user record as a data URL of up to about 180,000 characters (`docs/product/features/users.md`). Large binary content inside rows makes every user query heavier and the database larger.

## Decision

- Signature images are stored in an **S3-compatible object storage**: **MinIO** in local development and CI, and an S3-compatible service in the demo environment.
- The `User` aggregate stores only the object key.
- Uploads go through a dedicated endpoint that accepts PNG, JPEG or WebP up to 5 MB and checks the actual content type.
- The front-end displays signatures through short-lived pre-signed URLs returned by the API. The storage is never public.
- Access to the storage is behind an `IFileStorage` interface in Application, implemented in Infrastructure.

## Alternatives considered

- **Keep the data URL in the row (current behavior).** No extra infrastructure, but bloats queries.
- **A binary column in PostgreSQL.** Keeps everything in one place, but still inflates backups and rows.
- **Azure Blob Storage with the Azurite emulator.** Equally valid; MinIO was chosen because the S3 API is the most portable.

## Consequences

- One more service to run locally, handled by Aspire (ADR-0027).
- The front-end signature component must upload the image separately from the user form.
