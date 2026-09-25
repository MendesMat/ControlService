# Architecture guide

The back-end follows Clean Architecture with tactical DDD. The decisions and their reasons are in the ADRs; this guide tells you where code goes.

## Projects and the dependency rule

```
Domain  ◄──  Application  ◄──  Infrastructure
                  ▲                  ▲
                  └──────  API  ─────┘   (Infrastructure only for service registration)
```

| Project | Contains | May reference |
|---|---|---|
| `ControlService.Domain` | Aggregates, entities, value objects, domain services, domain errors, `ScreenKeys` | Nothing. No NuGet packages, no EF Core, no ASP.NET Core. |
| `ControlService.Application` | Commands and queries with their handlers, validators, request and response models, interfaces for infrastructure (repositories, `IEmailSender`, `ICurrentUser`) | Domain |
| `ControlService.Infrastructure` | EF Core `DbContext`, entity configurations, migrations, repository implementations, Identity, e-mail sending | Application, Domain |
| `ControlService.API` | Minimal API endpoints, authentication and authorization setup, Problem Details mapping, `Program.cs` | Application; Infrastructure only in `Program.cs` |
| `ControlService.AppHost`, `ControlService.ServiceDefaults` | Local orchestration and telemetry | API |

`tests/ControlService.ArchitectureTests` enforces these rules. If a test fails, move the code; never relax the test without a new ADR.

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

## Domain model (ADR-0006)

- `User` and `PermissionProfile` are aggregate roots. They reference each other **by id only**.
- Expose behavior, not setters: `user.Deactivate(by, now)`, `profile.SetLevel(screen, level)`. The aggregate protects its invariants (for example, a system record cannot change).
- Value objects are immutable and validated at creation (`Cpf`, `Login`, `EmailAddress`, `PhoneNumber`, `Cep`, `BloodType`, `ScreenKey`, `AccessLevel`). An invalid value object must not be constructible.
- Never inject services or repositories into an aggregate. The handler loads what the aggregate needs and passes it as a parameter.
- Effective access is a domain service: the highest level among the user's profiles, screen by screen (PERM-05, PERM-06 in `docs/product/features/permission-profiles.md`).
- Rules that need data from other aggregates (uniqueness of login, profile in use) are checked in the handler and guaranteed by database constraints.

## Application layer (ADR-0007, ADR-0009)

- One folder per use case with its command or query, handler and validator.
- **Validation (ADR-0008):** each command has a FluentValidation `{UseCase}Validator` that checks shape and format (required fields, lengths, formats), with the Portuguese messages of the feature document and camelCase property paths (`emergencyContact.phone`). A validation decorator runs it before the handler. Rules that need the database (uniqueness) run in the handler; business invariants stay in the domain.
- **Mapping (ADR-0010):** hand-written extension methods next to each feature's models, such as `user.ToResponse()` and `request.ToCommand()`. No AutoMapper; Mapperly only if mapping becomes repetitive, after asking the owner.
- Handlers implement the in-house `ICommandHandler<TCommand, TResult>` / `IQueryHandler<TQuery, TResult>`. MediatR is not used.
- Expected failures return `Result` / `Result<T>` with an `Error` (stable `code`, Portuguese `message`). Exceptions are only for unexpected failures.
- Handlers orchestrate; business decisions live in the domain.
- Queries may project straight to response models with `AsNoTracking`.

## API layer (ADR-0003, ADR-0020)

- One `{Feature}Endpoints.cs` per feature, with an extension method `Map{Feature}Endpoints` on the `/api/v1` group.
- Endpoints are thin: bind the request, call the handler, translate the `Result` to typed results and Problem Details.
- Every endpoint declares its permission: `.RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor)`, with the minimum level from the feature document's *Operations* table (PERM-03).
- Routes, status codes and error codes must match the feature document and `docs/api/conventions.md` exactly: the front-end depends on them.

## Persistence (ADR-0011 to ADR-0015)

- PostgreSQL through EF Core; `snake_case` names; ids are `Guid.CreateVersion7()` generated by the server.
- Concurrency token: PostgreSQL `xmin`. Conflicts return 409 `concurrency_conflict`.
- Audit fields are filled by a `SaveChangesInterceptor` using `ICurrentUser` and `TimeProvider`, never by handlers.
- Uniqueness is guaranteed by unique indexes (login, normalized display name, normalized profile name).
