# Open questions

What is still undecided and affects the back-end or its integration with the front-end. Nothing here is a rule: agents must ask the owner before implementing anything that depends on an open question. When one is answered, move it to [Resolved](#resolved) with a link to where the decision now lives.

## Open

### Data

| ID | Question |
|---|---|
| OQ-01 | **Where to store the signature.** The prototype keeps the image inside the user record, as text of up to about 180,000 characters. ADR-0018 proposes separate object storage, with the record holding only a reference. The signature is out of the first back-end slice and must be decided before it enters (USR-25). |
| OQ-02 | **Full change history.** Authorship records who created a record and who changed it last, but not what changed. A field-by-field history is left for a later stage (CNV-10). |
| OQ-03 | **Live updates between people.** An open list does not refresh by itself when someone else changes a record; it refreshes when the page is reloaded, the list is reopened or the person saves something. Protection against simultaneous saves is decided (CNV-12); automatic list refresh is not. |

### Access

| ID | Question |
|---|---|
| OQ-04 | **Production e-mail provider.** The system sends e-mails (ADR-0030); the provider for the public environment is not chosen yet. |
| OQ-05 | **Demo access in the public environment.** The prototype shows the login `admin` and the password `admin123`, and e-mails go to its test inbox. For the real back-end published to reviewers, it is undecided how someone signs in without receiving an activation link: for example, a demo user with a password published in the README, limited profiles and data restored periodically. |
| OQ-06 | **Personal data of the Admin.** The Admin cannot be changed (USR-23). Should it still be possible to fill in data such as phone and signature, or does it stay a technical account? Until decided, it is a technical account. |
| OQ-07 | **Where the front-end is served.** The session refresh cookie works only if the page is on `localhost` or on the API's own site (API-10). Serving the HTML from the API or from another address is undecided, and it defines the CORS configuration. |
| OQ-11 | **Unknown screen in a profile's levels.** When a profile is saved with an item in `levels` whose screen key is not in the catalog (for example, from a page opened before a screen was retired), should the server refuse the save (400 `validation_failed`) or ignore that item, as it ignores unknown ids in `profileIds` (PERM-05, USR-13)? The domain already refuses to create such a `ScreenKey`, with the message confirmed by the owner: *"Esta tela não existe mais no sistema. Atualize a página e tente de novo."* Must be decided before the permission profile endpoints. |

### Content

| ID | Question |
|---|---|
| OQ-08 | **RAAE.** The screen name in Relatórios was kept as given. The meaning of the acronym and the report's content still need to be described. |
| OQ-09 | **Screens not specified yet.** 18 of the 20 menu screens have no fields defined. Each one will get a document in [features](features/), from the [template](features/template.md). |
| OQ-10 | **Profiles in an open user form.** A profile created in another tab appears among the options of an open user form only when the form is reopened. The behavior was kept so nothing being typed is lost. |

## Resolved

| Question | Decision | Where |
|---|---|---|
| There is no login | Unique `login` field, required non-unique e-mail, activation link sent by e-mail, password reset by login | [authentication](features/authentication.md), ADR-0019, ADR-0030 |
| CPF, phone and CEP stored with masks | Informational, stored digits only, formatted on display; CPF not unique | [conventions](conventions.md#informational-documents-cpf-phone-and-cep), ADR-0006 |
| Screen key derived from its name | Fixed keys declared in code, current values preserved | [overview](overview.md#screen-keys), ADR-0021 |
| No authorship or creation date | Who created, who changed and when, filled by the server and shown in the record footer | [conventions](conventions.md#authorship-audit), ADR-0015 |
| Users deleted for good | Users are deactivated and can be reactivated; unused profiles can still be deleted | [users](features/users.md#deactivation-and-reactivation), ADR-0016 |
| Two people saving the same record | Version control: the second save is refused, naming who changed it | [conventions](conventions.md#several-people-editing-at-the-same-time-optimistic-concurrency), ADR-0014 |
| Whole lists loaded at once | Server-side paging, search and sorting; small reference lists stay whole | [API conventions](../api/conventions.md#lists), ADR-0017 |
| Permissions not enforced | Denied screens and empty areas are hidden, and the server refuses operations above the level | [permission profiles](features/permission-profiles.md#what-the-person-sees), ADR-0020 |
| Levels of removed screens left orphaned | Removed screens have their key retired, and their levels are cleaned up | [overview](overview.md#screen-keys), ADR-0021 |
| What each level allows | Reader views; Editor creates, changes, duplicates profiles and resends access; Manager also deletes, deactivates and reactivates. A Reader on Usuários sees signatures | [permission profiles](features/permission-profiles.md#what-each-level-allows), ADR-0020 |
| Identifier format | UUID v7 generated by the server; fixed GUIDs for the Admin and the Gerenciador profile | [conventions](conventions.md#record-fields), ADR-0012 |
| Back-end structure | Four projects by layer with feature folders, Minimal APIs everywhere | ADR-0005, ADR-0003 |
| Knowing right away whether the activation e-mail left | The user is saved first as pending, and the e-mail is sent right after, in the same operation | [authentication](features/authentication.md#accounts-and-activation), ADR-0031 |
| Session lifetime in the real back-end | 8 hours from the last action, with the account checked on every request | [authentication](features/authentication.md#sessions), ADR-0032 |
| Duplicated login or name: field error or conflict | Validation error (400), with the message under the field | [API conventions](../api/conventions.md#errors), ADR-0009 |
| Documentation language and structure | English by default, organized by feature, with stable rule IDs | [docs index](../README.md) |
