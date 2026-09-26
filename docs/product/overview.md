# Product overview

## The product

Control Service is an ERP for a service company. It brings together the operation's master data (users, products, vehicles, warranties), the commercial side (clients, routes, renewals), finance (accounts payable and receivable) and reports.

## Who uses it

The system will be used by people with very different roles, and most of them are not familiar with technology. This constraint drives every interface decision, and it also applies to what the back-end returns: error messages, field names and confirmation texts must be written in plain Portuguese, without technical terms (see [conventions](conventions.md#user-facing-messages)).

## Design principles

- **Minimal and calm.** The interface uses only shades of white and gray, with a light and a dark theme. Color is reserved for what needs attention, such as errors and destructive actions, so alerts stand out instead of competing with the frame.
- **Readability before aesthetics.** Text is at least 16 px, in the Atkinson Hyperlegible font, designed to tell apart similar letters such as I, l and 1. All contrasts meet WCAG level AA. Clickable items are at least 44 px tall.
- **Show only what the person can use.** Screens and menu areas the person has no access to do not appear, instead of appearing disabled.
- **Actions explain themselves.** Buttons say what they do ("Salvar usuário", "Desativar usuário", "Excluir perfil"), fields that may stay empty are marked "(opcional)", and every action that deletes data, removes someone's access or discards changes asks for confirmation first.

## Access

Every use of the system starts at the sign-in screen, with login and password. Nobody creates their own account: each person is registered by someone with access to the Users screen and receives an e-mail link to create their own password. See [authentication](features/authentication.md).

## Navigation

The side menu has four **areas**. An area works like a drawer: clicking it only opens or closes its list of screens, and only one drawer is open at a time. Each **screen** opens in a tab inside the system (see the [front-end prototype](../frontend/README.md)).

Each person sees only the screens they have access to, and an area without any accessible screen does not appear (see [permission profiles](features/permission-profiles.md#what-the-person-sees)).

### Screen keys

Each screen has a fixed **key**, in the format `area/screen`, that identifies it in permissions and in addresses. The key is different from the **name** shown in the menu:

| | Example | For | Can change? |
|---|---|---|---|
| Key | `relatorios/relatorio-de-vendas` | The system | **Never** |
| Name | "Relatório de Vendas" | People | Whenever needed |

- **Defined in code** (`ScreenKeys` in the Domain project, ADR-0021), written once when the screen is created and never recalculated from the name. If keys followed names, renaming a screen would silently make it disappear for everyone except the Gerenciador profile, because stored permissions would point to the old key.
- **Format:** lowercase, without accents, hyphens instead of spaces. This is only a readability convention: once created, a key does not follow changes to the name.
- **Never reused.** A removed screen has its key retired, and its stored levels are cleaned up by a migration.
- **Values preserved.** The current keys were kept exactly as the prototype used them, so no stored permission needs migration.

The same list, with areas and access levels, is available as data in [screen-catalog.json](screen-catalog.json).

### Gerenciamento (Management)

| Screen | Key | Code | Status |
|---|---|---|---|
| Usuários | `gerenciamento/usuarios` | `ScreenKeys.Users` | Built ([users](features/users.md)) |
| Permissões | `gerenciamento/permissoes` | `ScreenKeys.PermissionProfiles` | Built ([permission profiles](features/permission-profiles.md)) |
| Perfis CNPJ | `gerenciamento/perfis-cnpj` | `ScreenKeys.CnpjProfiles` | Not specified yet |
| Naturezas de Serviço | `gerenciamento/naturezas-de-servico` | `ScreenKeys.ServiceNatures` | Not specified yet |
| Objetos de Serviço | `gerenciamento/objetos-de-servico` | `ScreenKeys.ServiceObjects` | Not specified yet |
| Produtos | `gerenciamento/produtos` | `ScreenKeys.Products` | Not specified yet |
| Garantias | `gerenciamento/garantias` | `ScreenKeys.Warranties` | Not specified yet |
| Formas de Pagamento | `gerenciamento/formas-de-pagamento` | `ScreenKeys.PaymentMethods` | Not specified yet |
| Veículos | `gerenciamento/veiculos` | `ScreenKeys.Vehicles` | Not specified yet |

### Comercial (Commercial)

| Screen | Key | Code | Status |
|---|---|---|---|
| Clientes | `comercial/clientes` | `ScreenKeys.Customers` | Not specified yet |
| Roteiro Diário | `comercial/roteiro-diario` | `ScreenKeys.DailyRoute` | Not specified yet |
| Roteiro Mensal | `comercial/roteiro-mensal` | `ScreenKeys.MonthlyRoute` | Not specified yet |
| Acompanhamento | `comercial/acompanhamento` | `ScreenKeys.FollowUp` | Not specified yet |
| Renovações | `comercial/renovacoes` | `ScreenKeys.Renewals` | Not specified yet |

### Financeiro (Finance)

| Screen | Key | Code | Status |
|---|---|---|---|
| Contas a Receber | `financeiro/contas-a-receber` | `ScreenKeys.AccountsReceivable` | Not specified yet |
| Contas a Pagar | `financeiro/contas-a-pagar` | `ScreenKeys.AccountsPayable` | Not specified yet |

### Relatórios (Reports)

| Screen | Key | Code | Status |
|---|---|---|---|
| Relatório de Vendas | `relatorios/relatorio-de-vendas` | `ScreenKeys.SalesReport` | Not specified yet |
| RAAE | `relatorios/raae` | `ScreenKeys.RaaeReport` | Not specified yet |
| Incongruências | `relatorios/incongruencias` | `ScreenKeys.InconsistencyReport` | Not specified yet |
| Custo x Faturamento | `relatorios/custo-x-faturamento` | `ScreenKeys.CostVersusRevenueReport` | Not specified yet |

Screen names stay in Portuguese because they are what people see in the menu. The constants in code use English names, like every identifier; only the key keeps the Portuguese value. "Built" means the prototype already implements the screen; the back-end implements them slice by slice (see the [roadmap](../../README.md#roadmap)).
