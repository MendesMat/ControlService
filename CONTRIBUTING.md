# Contributing

> **AI coding agents:** start with [AGENTS.md](AGENTS.md). It contains the rules and links to the detailed [guides and workflows](docs/agents/README.md).

## Workflow

`main` is protected: nothing is pushed to it directly. Every change goes through a pull request.

1. Update `main` and create a branch named after the kind of change:

   ```bash
   git switch main
   git pull
   git switch -c feat/cpf-value-object
   ```

   Prefixes: `feat/`, `fix/`, `docs/`, `test/`, `refactor/`, `chore/`, `ci/`.

2. Commit in small steps using [Conventional Commits](https://www.conventionalcommits.org/), in English:

   ```
   feat(users): validate CPF check digits
   fix(auth): reset lockout after password reset
   docs(adr): accept ADR-0018
   ```

3. Run the tests before pushing:

   ```bash
   cd backend/ControlService
   dotnet test --solution ControlService.slnx
   ```

4. Push the branch and open a pull request. The template asks what changed, why and how to test it.

   ```bash
   git push -u origin feat/cpf-value-object
   gh pr create --fill
   ```

5. The pull request can be merged when the **CI** checks pass (*Build and test* and *Build API image*) and every review comment is resolved. It is merged with **squash**, so `main` gets one commit per pull request, titled after it, and the branch is deleted automatically.

## Conventions

- Code, identifiers, commits and documentation in English; user-facing messages in Portuguese, verbatim from `docs/product/`.
- Business rules have stable IDs (`USR-06`, `PERM-05`): cite them in tests and pull requests ([docs index](docs/README.md#rule-ids)).
- Business rules live in the Domain project and are covered by unit tests (ADR-0005, ADR-0024).
- Package versions are declared only in `Directory.Packages.props`.
- A significant technical decision gets an ADR in `docs/adr/`.
