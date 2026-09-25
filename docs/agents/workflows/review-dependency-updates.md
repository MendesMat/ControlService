# Workflow: review dependency updates

Dependabot opens pull requests every Monday for NuGet packages (grouped as `aspire`, `opentelemetry`, `microsoft` and `testing`), GitHub Actions and the Docker base image. It also opens security updates at any time when a vulnerability is published. Configuration: `.github/dependabot.yml`.

## Steps

1. List them:

   ```bash
   gh pr list --author app/dependabot
   ```

2. For each pull request, read the version change and check whether it is a patch, minor or **major** update:

   ```bash
   gh pr view <number>
   ```

3. Check CI:

   ```bash
   gh pr checks <number>
   ```

4. Classify it for the owner:

   | Situation | Recommendation |
   |---|---|
   | Security update, CI green | Merge soon; say which vulnerability it fixes |
   | Patch or minor, CI green | Safe to merge |
   | Major version, or a package that changed its license | Read the release notes and the license; summarize breaking changes and whether an ADR is affected before recommending |
   | CI red | Investigate the failure on the Dependabot branch; propose a fix in a separate pull request if code must change |

5. Report to the owner in Portuguese, one line per pull request, with your recommendation. **Merge only the pull requests the owner explicitly approves:**

   ```bash
   gh pr merge <number> --squash
   ```

## Rules

- Never pin a package to an older version or ignore an update without telling the owner why.
- A license change to a commercial model (as happened with MediatR, AutoMapper and FluentAssertions) is a blocker: do not merge; raise it with the owner.
- If `global.json` or the .NET SDK major version changes, treat it as a decision (ADR-0002) and ask the owner.
