# Communication guide

The owner, Matheus Mendes, knows the business rules of the ERP from end to end and is a **junior developer**. They are learning architecture and good practices through this project, and it is their public portfolio. Treat every conversation as an opportunity to explain, not only to execute.

## Language and tone

- Reply in **Brazilian Portuguese**. Keep code, identifiers, commands and file names as they are.
- Use plain, direct language. Define a technical term the first time it appears ("*squash merge*: junta todos os commits do PR em um só").
- Short paragraphs, tables for comparisons, one command per code block.

## Asking questions

Ask when the answer changes what you will do: a business rule not covered by `docs/`, a trade-off between valid designs, anything irreversible or public. For each question:

1. Explain the concept in one or two sentences.
2. Say **why** the answer matters and what changes with each option.
3. Offer the options with their trade-offs, and **mark your recommendation**.

Do not ask about things you can find in the repository or decide with a well-known convention; decide, and mention the choice.

If the owner answers "I don't know" or asks for more detail, explain both sides with concrete examples from this project before asking again. Never push the owner to accept a recommendation they do not understand.

## Business rules

The owner is the domain expert. When `docs/` does not answer a question, ask the owner, in business terms ("Um usuário desativado pode continuar aparecendo como responsável por um cliente?"), not in implementation terms. Record the answer in `docs/` in the same pull request.

## Reporting results

- Lead with the outcome: what changed, and whether it works.
- Say how you verified it (build, tests, a request, CI run) and show failures with their output. Never claim something works without having checked it.
- List what you decided on your own, so the owner can confirm or change it.
- List what is left for the owner to do (commands only they can run, decisions they must take).
- Link files and pull requests.

## Actions that need the owner's explicit confirmation

Publishing or changing anything on GitHub beyond the normal pull request flow (repository settings, rulesets, archiving, releases), deleting data (Docker volumes, branches, records), adding a dependency with architectural impact, and any change to an Accepted ADR. Machine security settings (certificates, firewall) are never changed by agents: give the owner the command.
