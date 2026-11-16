# Round 2 — merged findings
**Date:** 2026-11-17
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: documentation/developer/guides/releasing.md:56
- Description: Round 1: the release guide still said pull requests keep the paths filter. The fix replaces that sentence. `pull_request` has no paths filter, every pull request reports `CI`, and `dev workflow` calls `PullRequestBuildClassifier` and skips build and test with exit 0 only when every changed path is a declared non-build path. Extra build paths are in `.timewarp/dev.jsonc`. The master-push sentence is unchanged.
- Suggestion: None remaining.
- Source: general
- Disposition notes: Fixed in `692634b`. Re-checked against the classifier contract in TimeWarp.Nuru.DevCli 4.0.0-beta.15.

### M2 — Severity: bug — Status: fixed
- File: kanban/in-progress/147-adopt-the-shared-pull-request-build-classifier-and-the-one-workflow-ci-standard/task.md:89
- Description: Round 1: the recorded gates did not compile `tools/dev-cli`. The fix records `dotnet run --file tools/dev-cli/dev.cs -- workflow` for both smokes, with the classifier line and `exit=0`, and points `### How to validate` at that same command. The skip range `e9200fe...9ff24c1` is one kanban path, so the skip line and `(1 changed paths)` match `SkipMessage`. The build range `8b95c33^1...8b95c33` lists `.github/workflows/workflow.yml` first, which is a build path, so `Build path changed: .github/workflows/workflow.yml. Full build.` matches `FirstBuildPath`. The handler prints `decision.Message` and, on skip, sets exit code 0 and returns.
- Suggestion: None remaining.
- Source: general
- Disposition notes: Fixed in `692634b`. The smoke text was checked against the classifier and the named ranges. Build, test, and audit were not re-run.

## Duplicates / conflicts

- One general reviewer. No new findings. M1 and M2 carried forward from round 1 as fixed.
