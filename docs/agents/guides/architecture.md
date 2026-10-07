# Architecture guide

The back-end follows Clean Architecture with tactical DDD. The decisions and their reasons are in the [decisions document](../../decisoes-de-arquitetura.md) (in Portuguese); this guide tells you where code goes.

## Projects and the dependency rule

```
Domain  ◄──  Application  ◄──  Infrastructure
                  ▲                  ▲
                  └──────  API  ─────┘   (Infrastructure only for service registration)
```

| Project | Contains | May reference |
|---|---|---|
| `ControlService.Domain` | Aggregates, entities, value objects, domain services, domain errors, `ScreenKeys` | Nothing. No NuGet packages, no EF Core, no ASP.NET Core. |
| `ControlService.Application` | Commands and queries with their handlers, validators, request and response models, interfaces for infrastructure (repositories, `ICurrentUser`) | Domain |
| `ControlService.Infrastructure` | EF Core `DbContext`, entity configurations, migrations, repository implementations, Identity | Application, Domain |
| `ControlService.API` | Minimal API endpoints, authentication and authorization setup, Problem Details mapping, `Program.cs` | Application; Infrastructure only in `Program.cs` |
| `ControlService.AppHost`, `ControlService.ServiceDefaults` | Local orchestration and telemetry | API |

`tests/ControlService.ArchitectureTests` enforces these rules. If a test fails, move the code; never relax the test without the owner's approval (decision 28).

## Feature folders

Inside every project, group code by feature, never by technical type:

```
Domain/Users/                    User.cs, Login.cs, Cpf.cs, UserStatus.cs
Domain/PermissionProfiles/       PermissionProfile.cs, ScreenLevel.cs
Domain/Access/                   AccessLevel.cs, ScreenKeys.cs, EffectiveAccess.cs
Application/Users/CreateUser/    CreateUserCommand.cs, CreateUserHandler.cs, CreateUserValidator.cs
API/Users/                       UsersEndpoints.cs  (MapUserEndpoints)
Infrastructure/Persistence/      AppDbContext.cs, Configurations/UserConfiguration.cs
```

Current features: `Access`, `Auth`, `Users`, `PermissionProfiles`, `Screens`. Shared building blocks (`Result`, `Error`, base types) go in a `Common` folder.

## Domain model (decision 3)

- `User` and `PermissionProfile` are aggregate roots. They reference each other **by id only**.
- Expose behavior, not setters: `user.Deactivate(by, now)`, `profile.SetLevel(screen, level)`. The aggregate protects its invariants (for example, a system record cannot change).
- Value objects are immutable and validated at creation (`Cpf`, `Login`, `EmailAddress`, `PhoneNumber`, `Cep`, `BloodType`, `ScreenKey`, `AccessLevel`). An invalid value object must not be constructible.
- Never inject services or repositories into an aggregate. The handler loads what the aggregate needs and passes it as a parameter.
- Effective access is a domain service: the highest level among the user's profiles, screen by screen (PERM-05, PERM-06 in `docs/product/features/permission-profiles.md`).
- Rules that need data from other aggregates (uniqueness of login, profile in use) are checked in the handler and guaranteed by database constraints.

## Application layer (decisions 5 to 7)

- One folder per use case with its command or query, handler and validator.
- **Validation (decision 6):** each command has a FluentValidation `{UseCase}Validator` that checks shape and format (required fields, lengths, formats), with the Portuguese messages of the feature document. `ValidatingCommandHandler` (`Application/Common`) runs it before the handler and turns the failures into a `validation_failed` error whose `Fields` use camelCase paths (`emergencyContact.phone`). Handlers are registered by hand, each wrapped in the decorator (no Scrutor). Rules that need the database (uniqueness) run in the handler; business invariants stay in the domain.
- **Field errors from the handler:** a uniqueness failure, or a value object that refuses a value, must reach the front-end under its field (API-13, CNV-18). Value objects return `validation_failed` without a field, so the handler returns a new `Error` with `Fields` set (`["cpf"] = [message]`); never pass a field-less `validation_failed` on to the API.
- **Mapping (decision 8):** hand-written extension methods next to each feature's models, such as `user.ToResponse()` and `request.ToCommand()`. No AutoMapper; Mapperly only if mapping becomes repetitive, after asking the owner.
- Handlers implement the in-house `ICommandHandler<TCommand, TResponse>` / `IQueryHandler<TQuery, TResponse>` (`Application/Common`), which return `Task<Result<TResponse>>`. MediatR is not used.
- Expected failures return `Result` / `Result<T>` with an `Error` (stable `code`, Portuguese `message`, optional `Fields` and `Details`). Exceptions are only for unexpected failures.
- Handlers orchestrate; business decisions live in the domain.
- Queries may project straight to response models with `AsNoTracking`.

## API layer (decisions 4 and 24)

- One `{Feature}Endpoints.cs` per feature, with an extension method `Map{Feature}Endpoints` on the `/api/v1` group.
- Endpoints are thin: bind the request, call the handler, translate the `Result` to typed results and Problem Details. A failure becomes `error.ToProblem()` (`API/Common/ErrorResults.cs`), a `ProblemHttpResult`, so endpoints declare `Results<Ok<T>, ProblemHttpResult>`.
- `ErrorStatusCodes` maps every error code of the table in `docs/api/conventions.md#error-codes` to its status. A new code needs a new row and a case in `ErrorResultsTests`; an unmapped code throws, and the exception handler answers with the generic 500 (API-12, API-14).
- `account_inactive` maps to 401, its meaning during a session. The sign-in endpoint returns the 403 of that code explicitly.
- Every endpoint declares its permission: `.RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor)`, with the minimum level from the feature document's *Operations* table (PERM-03).
- Routes, status codes and error codes must match the feature document and `docs/api/conventions.md` exactly: the front-end depends on them.

## Persistence (decisions 10 to 14)

- PostgreSQL through EF Core; `snake_case` names (`EFCore.NamingConventions`); ids are `Guid.CreateVersion7()` generated by the server.
- `Domain/Common/AuditedAggregate` is the abstract base of every aggregate: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` and `Version` (all private-set, filled by infrastructure). `User` and `PermissionProfile` inherit it.
- Concurrency token: `Version` (`uint`), mapped with `.IsRowVersion()` to PostgreSQL's `xmin` system column — no extra column. Conflicts return 409 `concurrency_conflict`, translated by `Infrastructure/Persistence/UnitOfWork` (`Application/Common/IUnitOfWork`), which names the record's latest editor (decision 13).
- Audit fields are filled by `AuditFieldsInterceptor` (an EF Core `SaveChangesInterceptor`) using `ICurrentUser` and `TimeProvider`, never by handlers. On insert, `updatedAt`/`updatedBy` get the same values as `createdAt`/`createdBy` (CNV-19). A change to an owned collection only (a profile's levels, a user's profiles) also counts as a change to the aggregate: the interceptor marks its `updatedAt`/`updatedBy` as modified, which forces an `UPDATE` of the owner row, so `xmin` changes and is checked (CNV-12, CNV-13). A save during a request with no signed-in user throws; outside a request (startup, seeding) `ICurrentUser.UserId` answers the Admin's id instead of `null`, so the seeder's own inserts are authored by the Admin (CNV-20).
- Uniqueness is guaranteed by unique indexes (login, normalized display name, normalized profile name).
- `SystemRecordsSeeder` inserts the Admin, its credential and the Gerenciador profile only when missing (the credential is checked apart from the Admin row, so databases created before authentication get it), through EF Core's `UseSeeding`/`UseAsyncSeeding` (decision 16), which run as part of `Database.Migrate`/`MigrateAsync`.

## Authentication (decisions 18 to 22)

- **Application/Auth** has one folder per use case (`SignIn`, `RefreshSession`, `SignOut`, `ChangePassword`, `GetMe`) and the ports they use: `ICredentialStore` (password check with lockout, mandatory-change flag, replace password), `ISessionStore` (start, rotate, end one, end all) and `IAccessTokenIssuer`. `AuthErrors` holds every authentication error with its Portuguese message; `AuthSettings` carries the two configured values messages state (minimum password length, lockout minutes), because Application cannot read options.
- **Identity** (`Infrastructure/Auth`): `AppDbContext` inherits `IdentityUserContext<UserCredential, Guid>` (no roles). Its own `Users` set is the credentials; the domain users hide it under the same name, and credentials are reached through `Set<UserCredential>()`. `UserCredential.Id` is the user's id, `UserName` is that id too (so a login change needs no syncing), and `MustChangePassword` marks the initial password. There is no `SignInManager` (it pulls in cookie authentication): `CredentialStore` uses `UserManager` for hashing and lockout only.
- **Identity reads the real clock** for lockout, so `CredentialUserManager` overrides only the two virtual methods that compare or set the lockout end to use `TimeProvider`. IdentityModel has no clock seam either: `ConfigureJwtBearer` judges the token lifetime with a `LifetimeValidator` that reads `TimeProvider`, with no skew and an exclusive `exp`.
- **Sessions** are rows of `user_sessions` (`SessionStore`): the SHA-256 of a 32-byte random refresh token, valid while `now < expires_at`. Rotation is a single `UPDATE ... WHERE token_hash = old AND expires_at > now`, so two refreshes with the same token cannot both succeed. Sessions are infrastructure, not aggregates: no audit fields.
- **Access token** (HS256, `API/Auth/JwtAccessTokenIssuer`): `sub` (user id, read by `HttpCurrentUser`), `sid` (session id, used by sign-out because the refresh cookie never reaches it), `iat`, `exp`, and `must_change_password` only while the change is mandatory. `MapInboundClaims` is off so the claim names stay as issued. Configuration is split by layer over the same `Auth` section: `AuthOptions` (Infrastructure) and `JwtOptions` (API).
- **`MapApiV1()`** (`API/Common/ApiV1Group.cs`) builds the `/api/v1` group for `Program.cs` and the test factories: it requires authorization and adds `PasswordChangeRequiredFilter`, which answers 401 `password_change_required` while the token carries `must_change_password`. Endpoints opt out with `.AllowAnonymous()` (sign-in, refresh) or `.AllowPendingPasswordChange()` (`me`, sign-out, change-password).
- **Rate limiting** (`API/Auth/AuthRateLimiting`): a sliding window of 60 seconds in 6 segments per client address on sign-in and refresh; a refused request answers 429 `locked_out` with `Retry-After`.
- **Not yet covered:** the per-request account check and the per-endpoint permission filters (#9; the account check must also cover `me`, sign-out and change-password) and revoking sessions on deactivation (AUTH-21, #11). Not planned for now: a permission cache, activation and reset links by e-mail, and a global rate limit (decisions 21, 22 and 24).

