# Review framework — task 147

**Date:** 2026-11-17
**Host task:** kanban/in-progress/147-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/
**Diff scope:** branch `task/147-adopt-the-shared-pull-request-build-classifier-and` vs merge-base with `master` (`cc748881`). Product commits `cba45b8` and `a37a115`. Paths: `.github/workflows/workflow.yml`, `.timewarp/dev.jsonc`, `Directory.Packages.props`, `tools/dev-cli/endpoints/workflow-command.cs`, `tools/dev-cli/endpoints/workflow-command.outcomes.cs`.
**Plan / brief:** Drop the `pull_request` paths filter so every pull request reports `CI`. `dev workflow` calls `PullRequestBuildClassifier.DecideAsync` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15 and skips build and test only for a pull request whose diff is declared non-build. Extra build paths live in `.timewarp/dev.jsonc`.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle round 1 Grok 4.7 session `01a1e59e-bed5-7411-81e2-eaa624aaa5eb` (2026-11-17). Review oracle round 2 Grok 4.7 session `01a1e5ac-0896-72f0-a8e2-8d6fd5ffa751` (2026-11-17). Implementer session `7ebb37dc-78d0-4e0d-868c-d4eb848e9094`.

## Budget (by-diff)

- Lines changed: 122
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Round 2

**Date:** 2026-11-17
**Diff scope:** delta since round-1 base `07d422c` (commit `692634b`). Product path: `documentation/developer/guides/releasing.md`. Re-check of open M1 and M2 plus the updated `## Results` record. The header effort above is round 1. This round's budget is the block in this file: 122 lines, effort 1, roster general, turn cap 80.
**Session:** `01a1e5ac-0896-72f0-a8e2-8d6fd5ffa751`
