# Permission profiles

Access levels, profiles, effective access and what each person sees. This is the most distinctive business rule of the project.

- **Screen:** Permissões, key `gerenciamento/permissoes`.
- **Decisions:** ADR-0006 (effective access as a domain service), ADR-0020 (authorization per screen), ADR-0021 (screen keys), ADR-0022 (Gerenciador profile).
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
| PERM-03 | Operations require a minimum level on their screen. This table is the reference for every screen; a screen with exceptions documents them (ADR-0020, confirmed by the owner on 2026-09-24). |
| PERM-04 | Exception: listing profiles is also allowed with Reader on the **Usuários** screen, even without access to Permissões, because the user form needs the profiles to choose from. |

| Operation | Minimum level |
|---|---|
| Open the screen, list and view records | Reader |
| Create, change, duplicate a profile, resend access | Editor |
| Delete a profile, deactivate or reactivate a user | Manager |

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
| PERM-11 | Buttons follow PERM-03. A **Reader** sees records read-only, with the notice *"Você pode consultar este cadastro, mas não alterar. Para mudar alguma informação, fale com quem tem acesso de Editor nesta tela."*; the buttons "Novo", "Salvar", "Duplicar perfil" and "Reenviar acesso" are hidden, and "Cancelar" becomes "Voltar para a lista". An **Editor** does not see "Excluir perfil", "Desativar usuário" or "Reativar usuário". Nobody sees "Desativar usuário" on their own record. |
| PERM-12 | **The server decides.** Hiding screens and buttons helps people; the real barrier is the server, which refuses any request above the person's level (CNV-01). |

## When permissions change

| ID | Rule |
|---|---|
| PERM-13 | A change to someone's profiles takes effect on the server from that person's next request, without signing out (ADR-0020, ADR-0032). |
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
| `levels` | list of objects | yes | Items `{ "screen": key, "level": level }`. A missing screen counts as `negado` |

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

### Creating, duplicating and deleting

| ID | Rule |
|---|---|
| PERM-18 | A new profile starts with every screen at `negado`. |
| PERM-19 | Any profile, including the Gerenciador, can be duplicated. The copy is named "Cópia de *{nome original}*", with the same description and levels, and is stored only when the person saves. Duplicating is done by the front-end, which opens a new record prefilled with the original's data. |
| PERM-20 | A profile can be truly deleted, but only when **no user** has it, whether active, pending or deactivated. A database constraint guarantees it even if someone assigns the profile at the same moment. The Gerenciador profile can never be deleted. |
| PERM-21 | Each area on the Permissões screen has the selector **Mudar todas desta área para**, which sets the same level on every screen of the area and warns *"Telas de {área} marcadas como {nível}. Salve o perfil para confirmar."* Nothing is stored until the person saves. |

The delete confirmation is **Excluir este perfil?** *O perfil {nome} será apagado. Não dá para desfazer.* If someone has the profile, deletion is refused with **Este perfil está em uso** and *"{nomes} usa(m) este perfil. Tire o perfil dessa(s) pessoa(s) na tela Usuários e depois volte para excluir."*

### The Gerenciador system profile

| ID | Rule |
|---|---|
| PERM-22 | The **Gerenciador** profile always exists and cannot be changed or deleted. It is stored in the database with a fixed id and marked `isSystem: true` (ADR-0022). |
| PERM-23 | Its levels are **not stored**. The server computes `gerenciador` for every screen of the catalog and returns the full `levels` list like any other profile, so a new screen is covered without data changes. |

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

## Operations

| Front-end method | Route | Minimum level | Notes |
|---|---|---|---|
| `listProfiles()` | `GET /api/v1/permission-profiles` | Reader on Permissões **or** Usuários (PERM-04) | Whole list, not paged. Each item has `userCount`, `grantedCount` and `screenCount` |
| `getProfile(id)` | `GET /api/v1/permission-profiles/{id}` | Reader | |
| `createProfile(data)` | `POST /api/v1/permission-profiles` | Editor | |
| `updateProfile(id, data, version)` | `PUT /api/v1/permission-profiles/{id}` | Editor | Version in `If-Match` |
| `deleteProfile(id, version)` | `DELETE /api/v1/permission-profiles/{id}` | Manager | Version in `If-Match` |
| `listScreens()` | `GET /api/v1/screens` | Signed in | Areas, screens and keys (ADR-0021) |

The effective level of the signed-in person on every screen comes from `me()` ([authentication](authentication.md#operations)).

## Errors

| Status | `code` | When |
|---|---|---|
| 400 | `validation_failed` | PERM-16, PERM-17 |
| 403 | `forbidden` | Operation above the person's level; the front-end refreshes the menu (PERM-14) |
| 404 | `not_found` | Unknown profile id |
| 409 | `concurrency_conflict` | CNV-13 |
| 409 | `profile_in_use` | PERM-20; `details.userNames` lists who has it |
| 409 | `system_record` | Changing or deleting the Gerenciador profile (PERM-22) |
