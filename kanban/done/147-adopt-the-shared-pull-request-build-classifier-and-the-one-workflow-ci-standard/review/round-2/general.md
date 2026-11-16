# Round 2 — general
**Date:** 2026-11-17
**Scope reviewed:** Delta since round-1 base `07d422c` (commit `692634b`). Product file: `documentation/developer/guides/releasing.md`. Re-checked M1 and M2 against that diff, `PullRequestBuildClassifier` in TimeWarp.Nuru.DevCli 4.0.0-beta.15, the print and exit-0 return in `tools/dev-cli/endpoints/workflow-command.cs`, and the kitchen `## Results` / `### How to validate` record. Read-only `git diff --name-only` on the two three-dot ranges the gates name (`e9200fe...9ff24c1` and `8b95c33^1...8b95c33`). Not a build, a test run, or an audit.

## Summary

The release guide now says `pull_request` has no paths filter, every pull request reports `CI`, and `dev workflow` calls `PullRequestBuildClassifier` and skips build and test with exit 0 only when every changed path is a declared non-build path. The master-push sentence is unchanged. The gates table records a `dotnet run --file tools/dev-cli/dev.cs -- workflow` compile plus both smokes, with the classifier line and `exit=0`. Those lines are what the classifier prints for the named ranges: the skip range is one kanban path, and the build range's first path is `.github/workflows/workflow.yml`. No new issues.

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: documentation/developer/guides/releasing.md:56
- The pull-request sentence matches the classifier contract. Extra build paths are pointed at `.timewarp/dev.jsonc`. The master-push sentence remains.

### M2 — Severity: bug — Status: fixed
- File: kanban/in-progress/147-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/task.md:89
- The record compiles the dev CLI the way CI does and pastes both classifier lines and exit codes. `### How to validate` uses the same `dotnet run --file` command. The named ranges produce those lines. Build, test, and audit were not re-run.

## Issues

None.
