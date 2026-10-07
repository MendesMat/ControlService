# Domain glossary

The business speaks Portuguese; the code speaks English. This table is the single translation, so that people and agents name the same concept the same way (the *ubiquitous language* of DDD). Wire values are the ones the front-end already uses and must not change ([API conventions](../api/conventions.md)).

The value objects (`Login`, `EmailAddress`, `Cpf`, `PhoneNumber`, `Cep`, `BloodType`, `AccessLevel`, `ScreenKey`), the aggregates (`User`, `PermissionProfile`), `UserStatus`, `SystemIds` and the domain services (`EffectiveAccess`, `LoginSuggestion`) already exist in the Domain project; the other code names are the ones the rest of the first slice must use. If implementation shows a better name, change it here in the same pull request, so the glossary and the code never disagree. When a new business term appears, add it too. Sources: the [feature documents](features/), [conventions](conventions.md) and decision 3 of the [decisions document](../decisoes-de-arquitetura.md), where every decision cited by number is.

## Access and permissions

| Business term (pt-BR) | Code | Wire value / field | Notes |
|---|---|---|---|
| Tela | `Screen` | — | Each submenu item, such as Usuários or Contas a Pagar |
| Chave da tela | `ScreenKey`, constants in `ScreenKeys` | `screen`, e.g. `gerenciamento/usuarios` | Fixed forever, never derived from the name (decision 25) |
| Área | `Area` | `id`, e.g. `gerenciamento` | Menu group; a presentation concept, not a code boundary |
| Nível de acesso | `AccessLevel` | `level` | Ordered: `Denied < Reader < Editor < Manager` |
| Negado | `AccessLevel.Denied` | `negado` | Absence of permission, not a prohibition |
| Leitor | `AccessLevel.Reader` | `leitor` | |
| Editor | `AccessLevel.Editor` | `editor` | |
| Gerenciador (nível) | `AccessLevel.Manager` | `gerenciador` | See the ambiguity note below |
| Perfil de permissão | `PermissionProfile` | `profiles`, `profileIds` | Aggregate root |
| Níveis do perfil | `ScreenLevel` items | `levels: [{ screen, level }]` | A missing screen counts as `Denied` |
| Gerenciador (perfil) | System profile, `SystemIds.ManagerProfile` | id `00000000-0000-7000-8000-000000000002` | Manager on every screen; levels computed, not stored |
| Acesso efetivo | `EffectiveAccess` (domain service) | `me().levels` | Highest level among the user's profiles, screen by screen |
| Registro do sistema | `IsSystem` | `isSystem: true` | Admin user and Gerenciador profile; cannot change |

**Ambiguity: "Gerenciador".** It names both the **system profile** and the **highest access level**. In code, never write `Gerenciador`: use `AccessLevel.Manager` for the level and the system profile id for the profile. In conversation and documents, say "o nível Gerenciador" or "o perfil Gerenciador".

## Users and accounts

| Business term (pt-BR) | Code | Wire value / field | Notes |
|---|---|---|---|
| Usuário | `User` | `users` | Aggregate root |
| Admin | System user, `SystemIds.AdminUser` | id `00000000-0000-7000-8000-000000000001`, login `admin` | Demo account: fixed password, no mandatory password change (AUTH-13, AUTH-15) |
| Login | `Login` (value object) | `login` | Unique, lowercase, 3–30 characters |
| E-mail | `EmailAddress` (value object) | `email` | Optional and **not** unique |
| Nome completo | `FullName` | `fullName` | |
| Nome de exibição | `DisplayName` | `displayName` | Unique ignoring case, accents and outer spaces |
| CPF | `Cpf` (value object) | `cpf` | Digits only, informational, not unique |
| Telefone | `PhoneNumber` (value object) | `phone` | 10 or 11 digits |
| CEP | `Cep` (value object) | `address.cep` | 8 digits |
| Tipo sanguíneo | `BloodType` (value object) | `bloodType` | Closed list; sensitive personal data |
| Endereço | `Address` | `address` | Always present, fields optional |
| Contato de emergência | `EmergencyContact` | `emergencyContact` | Always present, fields optional |
| Assinatura | `Signature` | `signature` | Stored in the user's record (USR-36); out of the first slice |
| Situação da conta | `UserStatus` | `status` | |
| Ativo | `UserStatus.Active` | `active` | |
| Desativado | `UserStatus.Inactive` | `inactive` | Users are deactivated, never deleted |
| Desativar / Reativar | `Deactivate` / `Reactivate` | `deactivateUser` / `reactivateUser` | Manager level |
| Senha temporária | Temporary password | `temporaryPassword` | Set by who registers the user or resets the password; replaced at sign-in (AUTH-14) |
| Troca obrigatória de senha | `MustChangePassword` | `mustChangePassword`, `password_change_required` | True while the account has a temporary password |
| Redefinir senha | `ResetUserPassword` | `resetUserPassword` | Manager level; sets a new temporary password (AUTH-29) |
| Sugestão de login | `LoginSuggestion` (domain service), `SuggestLogin` (use case) | `suggestLogin` | |
| Bloqueio por tentativas | Lockout | `locked_out` | 5 failures, 15 minutes |

## Records in general

| Business term (pt-BR) | Code | Wire value / field | Notes |
|---|---|---|---|
| Autoria | Audit fields | `createdAt`, `createdBy`, `updatedAt`, `updatedBy` | Filled by the server (decision 14) |
| Versão | Concurrency token (`xmin`) | `version`, `ETag`, `If-Match` | Opaque; conflicts return 409 |
| Cadastro | Record / aggregate | — | "Cadastro de Ana Souza" = Ana's user record |
