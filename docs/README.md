# Documentation

Everything about Control Service that is not code: what the system does, how the front-end and the back-end talk, why the back-end is built the way it is, and how AI agents work on it. Written in English for both people and AI agents; user-facing messages stay in Portuguese, verbatim.

## Start here

| If you want to…                         | Read                                                                                                                                                   |
| --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Understand the product                  | [product/overview.md](product/overview.md), then [product/glossary.md](product/glossary.md)                                                            |
| Implement or change a feature           | Its file in [product/features/](product/features/), plus [product/conventions.md](product/conventions.md) and [api/conventions.md](api/conventions.md) |
| Add a new screen                        | [product/features/template.md](product/features/template.md) and the [implement a feature](agents/workflows/implement-a-feature.md) workflow           |
| Know why the back-end is built this way | [adr/](adr/README.md)                                                                                                                                  |
| Check what is still undecided           | [product/open-questions.md](product/open-questions.md)                                                                                                 |
| Work on the front-end prototype         | [frontend/](frontend/README.md)                                                                                                                        |
| Work as an AI agent                     | [AGENTS.md](../AGENTS.md), then [agents/](agents/README.md)                                                                                            |
| Run the system                          | [backend/ControlService/README.md](../backend/ControlService/README.md)                                                                                |

## Layout

```
docs/
├── README.md                 This index
├── product/                  WHAT the system does: the business rules (source of truth)
│   ├── overview.md           Product, users, design principles, navigation, screen keys
│   ├── glossary.md           Business terms (pt-BR) → names in code → values on the wire
│   ├── conventions.md        Rules for every record and screen (CNV)
│   ├── open-questions.md     Undecided topics (OQ)
│   ├── screen-catalog.json   Areas, screens, keys and access levels, as data
│   └── features/             One document per feature, mirroring the code's feature folders
│       ├── template.md
│       ├── authentication.md (AUTH)
│       ├── users.md          (USR)
│       └── permission-profiles.md (PERM)
├── api/
│   └── conventions.md        What every endpoint shares: routes, paging, versions, errors (API)
├── adr/                      Architecture Decision Records, numbered, with metadata
├── frontend/                 The front-end prototype and its simulated server
└── agents/                   Rules, guides and workflows for AI coding agents
```

**Product documents describe decided behavior only.** Anything undecided lives in [open-questions.md](product/open-questions.md) until the owner decides it.

## Rule IDs

Every rule has a stable ID, so that tests, pull requests and conversations can cite the exact rule ("the test covers USR-06").

| Prefix | Document |
|---|---|
| `CNV` | [product/conventions.md](product/conventions.md) |
| `AUTH` | [product/features/authentication.md](product/features/authentication.md) |
| `USR` | [product/features/users.md](product/features/users.md) |
| `PERM` | [product/features/permission-profiles.md](product/features/permission-profiles.md) |
| `API` | [api/conventions.md](api/conventions.md) |
| `OQ` | [product/open-questions.md](product/open-questions.md) (questions, not rules) |

- **Format:** `PREFIX-nn`, numbered in order of appearance in the document.
- **Never reused or renumbered.** A new rule takes the next free number, even if it is placed in the middle of the document. A removed rule keeps its ID with the text *Retired: reason*.
- **A new feature gets a new prefix** of 3 to 5 capital letters, added to this table.
- **Changing a rule's meaning** is a product decision: the owner confirms it, and the pull request says which IDs changed.
- **Tests cite the rule** they cover in a comment when the test name alone does not make it obvious.

## Writing conventions

- **English** for everything, except user-facing messages (Portuguese, verbatim, CNV-16), wire values (`negado`, `gerenciamento/usuarios`) and the owner's personal guide ([agents/trabalhando-com-agentes.md](agents/trabalhando-com-agentes.md)).
- **One topic per file**, named in kebab-case. Link instead of copying: each fact lives in one place.
- **Relative links** between documents, so they work on GitHub, in the IDE and in Obsidian.
- **Documentation changes with the code**, in the same pull request ([documentation guide](agents/guides/documentation.md)).
