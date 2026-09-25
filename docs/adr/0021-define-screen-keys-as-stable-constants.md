---
status: accepted
date: 2026-09-23
scope: back-end
tags: [permissions, domain]
---

# ADR-0021: Define screen keys as stable constants owned by the back-end

## Context

Screen keys such as `gerenciamento/usuarios` are currently derived from the screen names in the front-end menu. Renaming a screen changes its key and silently invalidates every permission stored for it (`docs/product/open-questions.md`).

### Example

Suppose "Relatório de Vendas" is renamed to "Vendas por Período".

- **Key derived from the name:** the key silently changes from `relatorios/relatorio-de-vendas` to `relatorios/vendas-por-periodo`. No profile has a level for the new key, and a missing level means `negado`. The screen therefore **disappears for everyone** except holders of the Gerenciador profile, with no warning, and every profile must be reconfigured by hand.
- **Stable key:** the key stays `relatorios/relatorio-de-vendas` and only the display name changes. All permissions keep working.

Fixing an accent ("Relatorio" → "Relatório") or changing "x" to "vs." in "Custo x Faturamento" would cause the same problem.

Separating an identifier from its presentation is standard practice: products keep an internal code when their description changes, and translated applications use keys such as `menu.reports.sales` instead of the displayed text.

## Decision

- The back-end defines every screen key as a constant in a `ScreenKeys` class in the Domain project. This class is the single source of truth.
- The existing keys are kept exactly as they are (see `docs/product/screen-catalog.json`), so stored permissions remain valid. The `area/screen` format is only a readability convention; once created, a key never follows changes to the display name.
- Keys are never reused. A retired key is not assigned to a different screen.
- The API exposes the catalog (keys, display names, areas) at `GET /api/v1/screens`, used by the permission screen of the front-end.
- The front-end menu stores each screen's key explicitly instead of deriving it from the name.
- Removing a screen is a deliberate change: its key is marked as retired, and stored levels for it are cleaned up by a migration.

## Alternatives considered

- **Keep deriving keys from names.** No change needed, but renaming a screen breaks permissions.
- **Numeric screen ids.** Stable, but unreadable in data and logs.

## Consequences

- Display names can change freely without touching permissions.
- Adding a screen requires changes in both the back-end constants and the front-end menu.
