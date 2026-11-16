# Round 1 — merged findings
**Date:** 2026-11-17
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: documentation/developer/guides/releasing.md:56
- Description: The release guide still says "Pull requests keep the paths filter." That sentence is the operator contract from the promote-artifact pipeline. This change removes the `pull_request` paths list so every pull request reports `CI`, and `dev workflow` skips build and test only after `PullRequestBuildClassifier` says the diff is non-build. The guide now states the policy this task retires. The same paragraph's statement that every master push runs CI, because `push` has no paths filter, remains true.
- Suggestion: Replace the pull-request sentence. State that `pull_request` has no paths filter, that every pull request reports `CI`, and that `dev workflow` calls `PullRequestBuildClassifier` and skips build and test only when every changed path is a declared non-build path. Keep the master-push sentence.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: tools/dev-cli/endpoints/workflow-command.cs:146
- Description: The recorded gates do not compile the program this change edits. `## Results` records `./bin/dev build` (0 warnings, 0 errors), `./bin/dev test` (879 total, 878 passed, 1 skipped), and `ganda repo audit` (pass, including `workflow-file` and `dev-workflow-classifier`). `timewarp-amuru.slnx` has no `tools/dev-cli` project. `./bin/dev` is the gitignored AOT binary (`.gitignore` line 462). CI runs `dotnet run --file tools/dev-cli/dev.cs -- workflow` (`.github/workflows/workflow.yml` line 112). The audit check is a text scan for `PullRequestBuildClassifier.DecideAsync(`. `### How to validate` tells a reader to smoke `./bin/dev workflow`, and it does not record that smoke's output. A failed compile of `workflow-command.outcomes.cs` or of the new call would not show up in the recorded gates. Gates were not re-run. This finding is the missing record.
- Suggestion: Compile the dev CLI the way CI does and record the result in `### Gates`. Use `dotnet run --file tools/dev-cli/dev.cs -- workflow` for the two smokes already written under `### How to validate`, and paste the classifier line and the exit code. A full `./bin/dev build`, `./bin/dev test`, or `ganda repo audit` does not supply this record.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- One general reviewer. No overlaps.
