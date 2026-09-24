# ADR-0020: Authorize per screen with custom policies

- **Status:** Accepted (2026-09-24)
- **Date:** 2026-09-23
- **Scope:** Back-end

## Context

Permissions are defined per screen and per level (`docs/04-permissoes.md`), but they are not enforced anywhere yet. The effective level of a user is the highest level among their profiles. What each level allows on each screen was also still undefined. This is the most distinctive business rule of the project and the main showcase of the portfolio.

## Decision

**Requirement and handler.** A custom authorization requirement, `ScreenAccessRequirement(ScreenKey, AccessLevel minimum)`, is evaluated by a handler that:

1. reads the user id from the token;
2. obtains the user's effective level for the screen, which is the highest level among all of the user's profiles (ADR-0006);
3. succeeds if the effective level is at least the minimum.

**Endpoint usage.** Endpoints declare it through an extension method, for example `.RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor)`.

**Default mapping of levels to operations:**

| Operation | Minimum level |
|---|---|
| List and view | Reader (`leitor`) |
| Create, edit, duplicate a profile, resend access | Editor (`editor`) |
| Delete a profile, deactivate or reactivate a user | Manager (`gerenciador`) |

Exceptions are documented on the endpoint and in the OpenAPI description. The first one already exists: listing permission profiles is allowed with Reader on **either** the Permissions screen or the Users screen, because the user form needs the list of profiles to choose from.

This mapping is already applied by the front-end's simulated server and user interface (`docs/04-permissoes.md`). The project owner confirmed it on 2026-09-24, including that a Reader on the Users screen can see people's signatures.

**Caching.** Effective permissions, together with the account status (ADR-0032), are cached per user with **HybridCache**, in memory only while the API runs as a single instance. Redis is added as the distributed layer when a second instance exists. Entries are invalidated when a user's profiles or status change or a profile's levels change.

**Front-end support.** `GET /api/v1/me` returns the effective level for every screen. The front-end uses it to apply the visibility rules confirmed by the project owner (`docs/04-permissoes.md`):

- a screen whose effective level is `negado` is not shown in the menu or in the screen search;
- a menu area whose screens are all `negado` is not shown at all;
- opening a denied screen through a saved link or a restored tab shows a "no access" message, and the API returns 403 for its data;
- a user with no accessible screen sees an empty menu and a message on the home screen.

The front-end check is only for user experience; the API is the authority.

## Alternatives considered

- **Role-based authorization (`[Authorize(Roles = ...)]`).** Cannot express "a level per screen" without an explosion of roles.
- **Checks inside each handler.** Works, but spreads authorization across the codebase and is easy to forget.
- **Putting all permissions in the JWT.** Avoids lookups, but changes would only apply after the token expires, and the token would grow with every new screen.

## Consequences

- Permission changes take effect on the next request after cache invalidation, without signing out.
- Integration tests must cover allowed and denied paths for each level.
- The level-to-operation table becomes the reference for all future screens.
