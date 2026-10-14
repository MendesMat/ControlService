# Permission profiles

Access levels, profiles, effective access and what each person sees. This is the most distinctive business rule of the project.

- **Screen:** Permissões, key `gerenciamento/permissoes`.
- **Decisions:** in the [decisions document](../../decisoes-de-arquitetura.md): 3 (effective access as a domain service), 16 (Gerenciador profile), 23 (levels per screen), 24 (authorization per screen), 25 (screen keys).
- **Read with:** [conventions](../conventions.md), [API conventions](../../api/conventions.md), [overview: screen keys](../overview.md#screen-keys).

In this document, a **screen** is each submenu item (Usuários, Contas a Pagar) and an **area** is each menu group (Gerenciamento, Financeiro).

## Access levels

| ID | Rule |
|---|---|
| PERM-01 | Permissions are organized in **profiles**. A profile defines one **access level** for each screen. Each user has zero, one or more profiles. |
| PERM-02 | Levels are ordered, and each one includes everything the previous one allows. |

| Order | Wire value | Code | Name on screen | Allows |
|---|---|---|---|---|
| 0 | `negado` | `AccessLevel.Denied` | Negado | Does not see the screen |
| 1 | `leitor` | `AccessLevel.Reader` | Leitor | Sees the information, changes nothing |
| 2 | `editor` | `AccessLevel.Editor` | Editor | Sees, creates and changes information |
| 3 | `gerenciador` | `AccessLevel.Manager` | Gerenciador | Everything, including deleting and deactivating records |

"Gerenciador" is also the name of a system profile ([glossary](../glossary.md)); say which one you mean.

### What each level allows

| ID | Rule |
|---|---|
| PERM-03 | Operations require a minimum level on their screen. This table is the reference for every screen; a screen with exceptions documents them (decision 24, confirmed by the owner on 2026-09-24). |
| PERM-04 | Exception: listing profiles is also allowed with Reader on the **Usuários** screen, even without access to Permissões, because the user form needs the profiles to choose from. |
| PERM-26 | A request above the person's level on the screen (including a screen at `negado`) is refused with 403 `forbidden` and the message *"Você não tem permissão para fazer isso. Se precisar, fale com o responsável pelo sistema."* The same text serves a screen without access and an operation above the level. When several refusals apply, the account and the password change come first (AUTH-30). |

| Operation | Minimum level |
|---|---|
| Open the screen, list and view records | Reader |
| Create, change, duplicate a profile | Editor |
| Delete a profile, deactivate or reactivate a user, reset a user's password (AUTH-29) | Manager |

A Reader on Usuários therefore sees people's signatures, as confirmed by the owner.

## Effective access with several profiles

| ID | Rule |
|---|---|
| PERM-05 | When a user has several profiles, **the highest level on each screen wins**, screen by screen, independently. A screen missing from a profile counts as `negado`. With no profiles, every screen is `negado`. An id in `profileIds` that matches no profile is ignored. |
| PERM-06 | The **Gerenciador** profile gives `gerenciador` on every screen, including screens created later, whatever the other profiles say. |
| PERM-07 | `negado` is the **absence** of permission, not a prohibition: a profile can never take away access another profile grants. To restrict someone, remove the profile that grants the access or lower a level inside it. Adding a profile never reduces what someone could do. |

Example: Carla has the profiles Vendedor and Financeiro.

| Screen | Vendedor | Financeiro | Carla's access |
|---|---|---|---|
| `comercial/clientes` | editor | leitor | **editor** |
| `financeiro/contas-a-receber` | leitor | gerenciador | **gerenciador** |
| `gerenciamento/usuarios` | negado | negado | **negado** |

```
effectiveAccess(user, screen) =
  max { profileLevel(profile, screen) for each profile in user.profileIds }   // empty set → negado

profileLevel(profile, screen) =
  "gerenciador"                         if profile is the Gerenciador profile
  the level of the item for that screen, if it exists
  "negado"                              otherwise
```

Changing a profile affects everyone who has it. Lowering a level in a profile only reduces the access of people who do not get a higher level from another profile.

## What the person sees

| ID | Rule |
|---|---|
| PERM-08 | A screen whose effective access is `negado` does not appear in the menu or in the screen search ("Buscar tela…"). An area where **every** screen is `negado` does not appear either. |
| PERM-09 | Opening a denied screen through a saved link or a restored tab shows *"Você não tem acesso a esta tela. Se precisar dela, fale com o responsável pelo sistema."* No data is loaded, because the server refuses the request. |
| PERM-10 | A person without access to any screen (no profiles, or profiles that deny everything) sees an empty menu, and the home screen shows *"Você ainda não tem acesso a nenhuma tela. Fale com o responsável pelo sistema."* |
| PERM-11 | Buttons follow PERM-03. A **Reader** sees records read-only, with the notice *"Você pode consultar este cadastro, mas não alterar. Para mudar alguma informação, fale com quem tem acesso de Editor nesta tela."*; the buttons "Novo", "Salvar" and "Duplicar perfil" are hidden, and "Cancelar" becomes "Voltar para a lista". An **Editor** does not see "Excluir perfil", "Desativar usuário", "Reativar usuário" or "Redefinir senha". Nobody sees "Desativar usuário" on their own record. |
| PERM-12 | **The server decides.** Hiding screens and buttons helps people; the real barrier is the server, which refuses any request above the person's level (CNV-01). |

## When permissions change

| ID | Rule |
|---|---|
| PERM-13 | A change to someone's profiles takes effect on the server from that person's next request, without signing out (decision 24). |
| PERM-14 | The person's menu is refreshed when they sign in, when they reload the page and whenever they themselves save, deactivate, reactivate or delete a record. Opening a new tab does **not** refresh it. If they try something they lost in the meantime, the server refuses, and the menu is refreshed with the no-access message. |
| PERM-15 | A new screen appears on the Permissões screen with `negado` in every existing profile until someone changes and saves the profile. Only the Gerenciador profile sees it right away. This follows from "missing means `negado`" and needs no data migration. |

## Profiles

### Data model

Collection `profiles`. Besides these fields, every profile has the audit fields and `version`.

| Field | Type | Required | Format and notes |
|---|---|---|---|
| `id` | text | yes | GUID generated by the server (CNV-04) |
| `name` | text | yes | Profile name. Unique (PERM-17) |
| `description` | text | no | Free description |
| `levels` | list of objects | yes | Items `{ "screen": key, "level": level }`. A missing screen counts as `negado`; an unknown screen refuses the save (PERM-24) |

```json
{
  "id": "0192f1a2-1b4d-7e8f-a0c3-5d6e7f8a9b0c",
  "name": "Vendedor",
  "description": "Equipe comercial",
  "levels": [
    { "screen": "gerenciamento/usuarios", "level": "negado" },
    { "screen": "comercial/clientes", "level": "editor" },
    { "screen": "comercial/roteiro-diario", "level": "editor" },
    { "screen": "financeiro/contas-a-receber", "level": "leitor" }
  ],
  "createdAt": "2026-09-10T11:20:00Z",
  "createdBy": "00000000-0000-7000-8000-000000000001",
  "updatedAt": "2026-09-23T14:10:44Z",
  "updatedBy": "0192f1a4-7c3e-7b21-9f5a-2d8e4c1b6a70",
  "version": "318"
}
```

### Validation on save

| ID | Field | Rule | Message |
|---|---|---|---|
| PERM-16 | `name` | Required. | Dê um nome ao perfil. |
| PERM-17 | `name` | Unique among all profiles, **including the Gerenciador**, with the comparison of CNV-09: "Financeiro", "financeiro" and "FINANCEIRO" are the same name. | Já existe um perfil chamado *{nome existente}*. Escolha outro nome. |
| PERM-24 | `levels` | Every `screen` must be a key of the catalog (decision 25). An unknown key refuses the whole save instead of being ignored: ignoring it would drop the level without warning, so a key mistyped in the front-end menu would look saved and grant nothing. It happens only with a page opened before a screen was retired, or with a front-end bug. | Esta tela não existe mais no sistema. Atualize a página e tente de novo. |
| PERM-27 | `levels` | Every `level` must be one of the four wire values (`negado`, `leitor`, `editor`, `gerenciador`). An unknown value refuses the whole save, for the same reason as PERM-24: it happens only with a page opened before a level was retired, or with a front-end bug. | Este nível de acesso não existe. Atualize a página e tente de novo. |

The errors of `levels` (PERM-24 and PERM-27) come under the field `levels`, with a single message, however many items are wrong.

### Creating, duplicating and deleting

| ID | Rule |
|---|---|
| PERM-18 | A new profile starts with every screen at `negado`. |
| PERM-19 | Any profile, including the Gerenciador, can be duplicated. The copy is named "Cópia de *{nome original}*", with the same description and levels, and is stored only when the person saves. Duplicating is done by the front-end, which opens a new record prefilled with the original's data. |
| PERM-20 | A profile can be truly deleted, but only when **no user** has it, whether active or deactivated. A database constraint guarantees it even if someone assigns the profile at the same moment. The Gerenciador profile can never be deleted. |
| PERM-21 | Each area on the Permissões screen has the selector **Mudar todas desta área para**, which sets the same level on every screen of the area and warns *"Telas de {área} marcadas como {nível}. Salve o perfil para confirmar."* Nothing is stored until the person saves. |

The delete confirmation is **Excluir este perfil?** *O perfil {nome} será apagado. Não dá para desfazer.* If someone has the profile, deletion is refused with **Este perfil está em uso** and *"{nomes} usa(m) este perfil. Tire o perfil dessa(s) pessoa(s) na tela Usuários e depois volte para excluir."* `{nomes}` is the display names of everyone who has the profile, deactivated users included, in alphabetical order (CNV-09) and separated by a comma and a space; `details.userNames` lists the same names. "(m)" and "(s)" are written literally, so the sentence reads well for one person or several.

### The Gerenciador system profile

| ID | Rule |
|---|---|
| PERM-22 | The **Gerenciador** profile always exists and cannot be changed or deleted. It is stored in the database with a fixed id and marked `isSystem: true` (decision 16). |
| PERM-23 | Its levels are **not stored**. The server computes `gerenciador` for every screen of the catalog and returns the full `levels` list like any other profile, so a new screen is covered without data changes. |
| PERM-25 | Changing a level or deleting the Gerenciador profile is refused with the message *"O perfil Gerenciador é do sistema e não pode ser alterado nem excluído."* |

| Field | Value |
|---|---|
| `id` | `00000000-0000-7000-8000-000000000002` |
| `name` | Gerenciador |
| `description` | Acesso total a todas as telas do sistema. |

## Messages

Verbatim, in Portuguese. The other messages of this feature are inside the rules above.

| Situation | Message |
|---|---|
| Profile saved | Perfil *{nome}* salvo. |
| Profile deleted | Perfil *{nome}* excluído. |
| Operation above the person's level (PERM-26) | Você não tem permissão para fazer isso. Se precisar, fale com o responsável pelo sistema. |

## Operations

| Front-end method | Route | Minimum level | Notes |
|---|---|---|---|
| `listProfiles()` | `GET /api/v1/permission-profiles` | Reader on Permissões **or** Usuários (PERM-04) | Whole list, not paged. Each item has `userCount`, `grantedCount` and `screenCount` |
| `getProfile(id)` | `GET /api/v1/permission-profiles/{id}` | Reader | |
| `createProfile(data)` | `POST /api/v1/permission-profiles` | Editor | |
| `updateProfile(id, data, version)` | `PUT /api/v1/permission-profiles/{id}` | Editor | Version in `If-Match` |
| `deleteProfile(id, version)` | `DELETE /api/v1/permission-profiles/{id}` | Manager | Version in `If-Match` |
| `listScreens()` | `GET /api/v1/screens` | Signed in | Areas, screens and keys (decision 25) |

The effective level of the signed-in person on every screen comes from `me()` ([authentication](authentication.md#operations)).

### Requests and responses

| Operation | Response |
|---|---|
| `GET /api/v1/permission-profiles` | `200`, a plain list (not paged, API-08) of `{ id, name, description, isSystem, userCount, grantedCount, screenCount }`, in alphabetical order of the name with the comparison of CNV-09; the Gerenciador profile sits wherever its name falls |
| `GET /api/v1/permission-profiles/{id}` | `200`, `{ id, name, description, isSystem, levels, createdAt, createdBy, createdByName, updatedAt, updatedBy, updatedByName, version }` and the `ETag` |
| `POST /api/v1/permission-profiles` with `{ name, description, levels }` | `201`, the record as above, `Location` and `ETag` (API-05) |
| `PUT /api/v1/permission-profiles/{id}` with the same body and `If-Match` | `200`, the record as above and the new `ETag` |
| `DELETE /api/v1/permission-profiles/{id}` with `If-Match` | `204` |
| `GET /api/v1/screens` | `200`, `{ areas: [ { id, label, screens: [ { key, label } ] } ] }`, in menu order |

- **`userCount`** counts everyone who has the profile, deactivated users included: it is the number that blocks the deletion (PERM-20). **`grantedCount`** is the number of screens above `negado`, and **`screenCount`** is the number of screens of the catalog. The Gerenciador profile has both at the total (PERM-23).
- **`isSystem`** is `true` only for the Gerenciador profile.
- **`levels` in a response** always has every screen of the catalog, in catalog order, with `negado` for the ones not stored (the same format as `me()`).
- **`levels` in a request:** a screen left out becomes `negado` (PERM-18); if the same screen comes twice, the last one counts; `levels` may be omitted. The whole list replaces the stored one on `PUT`. Unknown screens and levels refuse the save (PERM-24, PERM-27).
- **`version` and `ETag`.** `version` is text in the body, and the `ETag` carries the same value between quotes (`"318"`). `If-Match` is accepted with or without quotes. A missing or unreadable `If-Match` is answered like an old version: 409 `concurrency_conflict`, with the message of CNV-14 (API-06).
- **The catalog** of `GET /api/v1/screens` is written in code next to the screen keys, and any signed-in person may read it, whatever their levels.

## Errors

| Status | `code` | When |
|---|---|---|
| 400 | `validation_failed` | PERM-16, PERM-17, PERM-24, PERM-27 (the last two under `levels`) |
| 403 | `forbidden` | Operation above the person's level, with the message of PERM-26; the front-end refreshes the menu (PERM-14) |
| 404 | `not_found` | Unknown profile id, or a profile deleted by someone else while the save was running (API-16) |
| 409 | `concurrency_conflict` | CNV-13, also when `If-Match` is missing |
| 409 | `profile_in_use` | PERM-20; `details.userNames` lists who has it, in alphabetical order |
| 409 | `system_record` | Changing or deleting the Gerenciador profile (PERM-22, PERM-25), checked before `profile_in_use` |
