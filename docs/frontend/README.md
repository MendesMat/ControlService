# Front-end prototype

The front-end is a single-page prototype, `control-service-erp.html`, with no build step. It is **not in this repository yet**. It implements the navigation, the Users and Permissions screens and every business rule of [product](../product/), using a [simulated server](simulated-server.md) instead of the real API.

Back-end work does not need this document: the contract between both sides is in the [API conventions](../api/conventions.md) and in each [feature](../product/features/). Read it when working on the front-end or when connecting it to the real API.

## Code organization

One file holds the frame's HTML (side menu, top bar and tab bar), the CSS and the JavaScript. The only external dependency is the Atkinson Hyperlegible font, loaded from Google Fonts; if it fails, the browser falls back to the system font.

The script is split into sections marked with comments, in this order:

| Section | Responsibility |
|---|---|
| Screen catalog | Menu with the fixed keys, access levels, account statuses, durations, messages and page references |
| Utilities | Icons, HTML escaping, text normalization, id generation |
| Simulated server storage | Adapters for the Claude artifact database and for the browser |
| Full user record | The complete shape of a user, with empty values |
| Masks and validation | CPF, CEP and phone masks; CPF and phone validation |
| Side menu, screen search, show/hide the sidebar | Menu behavior |
| Light and dark theme | Theme switching and memory |
| Navigation, tabs | Opening tabs, navigation inside a tab, Back button and history |
| Common screen structure, lists, forms | Reusable page, table, field, error and button blocks |
| Users screen, signature, Permissions screen | The two built screens |
| Dialog and toasts | Confirmations and temporary notices, with an optional action ("Ver e-mail") |
| Permissions of the signed-in person | Level of each screen, visible screens and areas, name in the top bar |
| Display formatting | Masks, Brasília time, account status, authorship footer |
| Server error handling | What to do with each error code ([API conventions](../api/conventions.md#error-codes)) |
| Access screens | Sign in, first access, activate account, forgot password, new password |
| Signing in and out | Opening the system after signing in, resuming the session, signing out |
| Home | Welcome screen and no-access messages |
| Paged lists | Search, status filter and paging requested from the server |
| Test inbox | Shows the e-mails "sent" by the simulation |
| Simulated server | Every back-end operation and rule |
| Startup | Binds events, checks the session and opens the sign-in screen or the system |

## Menu

The menu is generated from the `MENU` constant, a list of areas with name, icon and screens. Each screen declares its **key** and **name** explicitly ([screen keys](../product/overview.md#screen-keys)).

The menu shows only the screens the person can access and hides areas without any visible screen (PERM-08). The search "Buscar tela…" filters screens while the person types, ignoring accents and case; typing an area name shows all its screens, and Enter opens the first result in a new tab.

## Tabs and navigation

These rules were defined with the product owner and apply to every screen.

- **Only the menu opens tabs.** Each click on a menu screen opens a new tab, even if that screen is already open. That is how someone works with the same screen twice, such as two user records at once.
- **Inside a tab, navigation stays in the tab.** "Novo usuário", clicking a list row, "Duplicar perfil", "Cancelar" and the breadcrumb links replace the tab's content. The tab name follows the screen shown.
- **Exceptions that open a new tab:** the "Permissões" link inside the user form (so the person does not lose what they typed; the text next to it says so, and it appears only to people with access to Permissões), and Ctrl + click, Shift + click or middle click on any link or list row, as in a browser.
- **Back.** Each tab keeps its own history of up to 20 steps. When there is a previous screen, a left-arrow button appears next to the title; on hover it shows the destination ("Voltar para Usuários"). The browser's Back button does the same. After saving, deleting, deactivating or reactivating, the form leaves the history, so going back from the list does not reopen a finished record.
- **Unsaved changes.** Editing a form adds a dot next to the tab name. Closing the tab, going back or leaving the screen asks for confirmation ("Descartar as alterações?"). Closing or reloading the page makes the browser warn too.
- **After saving, deleting, deactivating or reactivating,** the tab returns to the updated list. Lists open in other tabs refresh when the person returns to them. Forms open in other tabs never refresh, so nothing being typed is lost.
- **Home.** The welcome screen appears only when no tab is open, and then the tab bar is hidden. Closing the last tab shows Home again.
- **Limit.** At most 15 tabs. Opening more asks to close one first.
- **Keyboard.** With the focus on a tab, the arrow keys switch tabs, Home and End go to the first and last, and Delete closes the tab.

## Routes

The page address follows the active tab:

```
#/{area}/{screen}                     list                   #/gerenciamento/usuarios
#/{area}/{screen}/novo                new record             #/gerenciamento/usuarios/novo
#/{area}/{screen}/{id}                existing record        #/gerenciamento/usuarios/{id}
#/{area}/{screen}/novo?copiar={id}    copy of a profile      #/gerenciamento/permissoes/novo?copiar={id}
#/                                    Home (no tabs)
```

Opening the system with one of these addresses opens that screen in a tab after the person signs in. The access screens have their own routes, which do not open tabs:

```
#/entrar                         sign in
#/esqueci-senha                  request a password reset link
#/ativar?token={token}           create the password from the activation link
#/redefinir-senha?token={token}  create a new password from the reset link
```

## Preferences stored in the browser

Stored in each person's `localStorage`; not business data.

| Key | Content |
|---|---|
| `control-service:theme` | `light` or `dark`, when the person chose a theme. Without it, the system follows the device theme |
| `control-service:sidebar-hidden` | `true` when the person hid the side menu on a computer |
| `control-service:abas:{user id}` | Each person's open tabs, with route and history, and the active tab. Restored when the same person signs in again |
| `control-service:sessao` | The simulated session: who is signed in and until when. Removed on sign-out |

## Themes and colors

Every color is a CSS variable defined at the top of the style, in three blocks: light theme, automatic dark theme and chosen dark theme. The main tokens are `--bg` (content background), `--surface` (cards and top bar), `--ink` and `--ink-muted` (text), `--line` (borders), `--side-*` (side menu), `--active-*` (active item) and `--danger*` (errors and deletions). To change a color everywhere, change the token in the theme blocks.

## Accessibility

The menu uses buttons with `aria-expanded` for the drawers and marks the current screen with `aria-current`. The tab bar follows the WAI-ARIA tabs pattern (`tablist`, `tab`, `tabpanel`). Form errors are linked to their fields with `aria-describedby` and marked with `aria-invalid`. When the screen changes, and on the access screens, the focus goes to the title for screen reader users. All animations are disabled when the operating system asks for reduced motion.

## Access and session

- **Sign-in screen.** While nobody is signed in, the sign-in screen replaces everything. It also offers "Esqueci minha senha" and, below, the demo notice with the **E-mails de teste** button.
- **Top bar.** With someone signed in, it shows the person's initials and display name, the test inbox button and **Sair**. Signing out with unsaved changes asks for confirmation.
- **E-mail links.** Activation and reset links open `#/ativar?token=…` and `#/redefinir-senha?token=…`. If someone is already signed in on the same browser, the screen offers to go back to the system or sign out and use the other account.
- **Session ended.** If the session ends or the account is deactivated, the sign-in screen appears **over** the system without closing the tabs. If the same person signs in again, everything is where it was, including unsaved forms. If someone else signs in, the tabs are replaced by theirs.

## Record forms

- **Buttons follow the level** (PERM-11).
- **Users.** In a new record, the login is suggested while the full name is typed (USR-14). The section "Acesso ao sistema" shows the account status and, for pending accounts with an e-mail, **Reenviar acesso**. CPF, phone and CEP are shown masked and sent digits only. Saving without a profile asks for confirmation (USR-22).
- **Authorship footer** on every open record (CNV-11).
- **Version.** Each tab keeps the version of the open record and sends it when saving; on a conflict it shows "Recarregar" / "Continuar aqui" (CNV-14).
- **Lists.** The user list is paged on the server, 10 per page, with search and status filter. Each tab remembers the search, filter and page of each list.

## Adding a screen

1. Add the screen to the right area in `MENU`, with its name and **key** written explicitly. The same key must exist in the back-end's `ScreenKeys`. The screen then appears on the Permissões screen, `negado` in every profile except Gerenciador.
2. Until it has its own render function, the screen shows "Tela em construção".
3. To give it content, write an async function `(tab, entry, param, query)` and register it in `SCREENS` under the screen key. It fetches data through the `api` object and uses `canAccess(key, level)` to decide which buttons to show. `param` is `null` for the list, `"novo"` for a new record or the record `id`; `query` holds the parameters after `?`. The Users and Permissions screens are the model: `renderScreen` builds the page, `formSectionHtml` and `textFieldHtml` build the form, and `bindFormBehavior` wires validation, masks, change detection and saving.
4. If the screen stores data, add the operations and the collection to the simulated server, applying the level rules with `requireLevel`, and document the feature in [product/features](../product/features/) from the [template](../product/features/template.md).

## Connecting to the real back-end

Every decision in [product](../product/) is already implemented in the prototype with the simulated server. The only step left is to replace the simulated server with an HTTP client for the real API, handling the differences listed in [simulated server](simulated-server.md#differences-from-the-real-back-end).
