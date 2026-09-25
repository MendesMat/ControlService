# Coding conventions

The compiler enforces most of these: `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended` and `EnforceCodeStyleInBuild` are on for every project (`Directory.Build.props`), and `.editorconfig` defines style and naming. A build with a warning is a failed build.

## Language of each artifact

| Artifact | Language |
|---|---|
| Identifiers, code comments, commit messages, pull requests, ADRs, agent guides | English |
| Functional documentation (`docs/01-07`), user-facing messages, e-mail templates | Portuguese (pt-BR) |
| Wire values that already exist in the front-end contract (`negado`, `leitor`, `editor`, `gerenciador`, screen keys) | Keep exactly as documented |

User-facing messages are copied **verbatim** from `docs/03-regras-de-negocio.md`. Do not paraphrase them. A new message follows the same plain tone and is added to that document.

## Naming

| Symbol | Convention | Example |
|---|---|---|
| Types, methods, properties, public fields | PascalCase | `PermissionProfile`, `Deactivate` |
| `const` fields and locals | PascalCase | `private const string ApiNamespace` |
| `static readonly` fields | PascalCase | `private static readonly Assembly DomainAssembly` |
| Other private fields | `_camelCase` | `_userRepository` |
| Locals and parameters | camelCase | `displayName` |
| Interfaces | `I` + PascalCase | `IEmailSender` |
| Use case types | `{Verb}{Noun}Command` / `Query` / `Handler` / `Validator` | `CreateUserHandler` |
| Endpoints | `{Feature}Endpoints`, `Map{Feature}Endpoints` | `UsersEndpoints` |

Translate business terms exactly as in the [domain glossary](domain-glossary.md); add new terms there. Avoid technical filler names such as `Helper`, `Processor`, `Data`, and never use `Manager` as a class-name suffix: in this domain, Manager is an access level (`AccessLevel.Manager`).

## Style

- File-scoped namespaces; `using` directives outside the namespace; one top-level type per file, named after the file.
- `var` when the type is apparent; expression bodies for one-line members.
- Prefer small methods with early returns over nested conditionals.
- Records for value objects and request/response models; `sealed` classes by default.
- Comments explain *why*, not *what*. No commented-out code.
- Nullable reference types are on: do not silence warnings with `!` unless the reason is obvious or commented.

## Packages

- Versions exist only in `backend/ControlService/Directory.Packages.props`. In a `.csproj`, write `<PackageReference Include="Name" />`.
- Before adding a package, check whether an ADR already chose one for that purpose. A new dependency with architectural impact needs an ADR.
- Never add packages to `ControlService.Domain`.
- Avoid libraries with commercial licenses for this project, such as MediatR 13+ (ADR-0007), FluentAssertions 8+ (ADR-0024) and AutoMapper, which has had a commercial edition since July 2025 (ADR-0010).

## Suppressing a diagnostic

Only when the rule is wrong for that specific line, with the narrowest scope (`#pragma warning disable` around the line, or `[SuppressMessage]` with `Justification`). Never lower a severity for the whole solution to make a build pass.
