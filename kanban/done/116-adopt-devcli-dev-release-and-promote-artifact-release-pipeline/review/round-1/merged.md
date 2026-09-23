# Round 1 — merged findings
**Date:** 2026-09-23
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 2 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: suggestion — Status: wontfix
- File: .github/workflows/workflow.yml:116-120
- Description: A bare `workflow_dispatch` (mode=merge) on master uploads its own `Packages-{run_number}` artifact and enters the keep-last-two rotation; two such dispatches could prune the push run's artifact for the same commit. Reviewer predicted the release pipeline would then abort at Step 4/6.
- Suggestion: Exclude `workflow_dispatch` from the upload condition, or scope the prune to push-event artifacts.
- Source: general
- Disposition notes: **wontfix** (decided by review oracle). Re-verified against DevCli `ci-run-promotion.cs` `OrderCandidateRuns`: only `pull_request` runs are excluded from candidacy; `workflow_dispatch` runs at the same `headSha` are candidates (push preferred, then newest). A dispatch merge run on master executes the identical `clean → build → verify-samples → test` pipeline at master HEAD, so its artifact is equally tested and promotable; locate-run walks to it after the pruned push run yields `NoneMatching`. `dev release` guard 5 also forces HEAD == origin/master, so the release commit is always the commit dispatch runs build. The predicted abort does not occur; the extra upload path is a valid recovery route (regenerate an expired artifact without `gh run rerun`). No change.

### M2 — Severity: nit — Status: fixed
- File: kanban/to-do/116-adopt-devcli-dev-release-and-promote-artifact-release-pipeline/task.md:35
- Description: Ticked checklist item still described the old local check-version behaviour (props `<Version>` vs newest tag) although that comparison now lives in `dev release` guard 6 (tag exists) and DevCli `check-version` (already on NuGet.org).
- Suggestion: Reword the checklist item to match what shipped.
- Source: general
- Disposition notes: fixed — checklist item reworded to name the two guards that replaced the tag comparison.

### M3 — Severity: nit — Status: fixed
- File: kanban/to-do/116-adopt-devcli-dev-release-and-promote-artifact-release-pipeline/task.md:49
- Description: Notes asked for a checklist item recording the Tools `<Version>` override decision; the implementation chose a third option (independent cadence via `.timewarp/dev.jsonc` + per-project MSBuild version evaluation) but added no checklist item.
- Suggestion: Add a checklist item stating the chosen approach.
- Source: general
- Disposition notes: fixed — checklist item added recording the independent-cadence decision and pointing at `.timewarp/dev.jsonc` and the guide's appendix.

### M4 — Severity: suggestion — Status: wontfix
- File: tools/dev-cli/Directory.Build.props:19-20
- Description: The extended `NoWarn` (IDE0055 etc.) is project-wide, so it also silences those style rules for first-party endpoint files, not only the DevCli content compiled from the NuGet cache.
- Suggestion: Folder-scoped `.editorconfig` or per-file pragmas so only vendored content is suppressed; no change if intentional parity with timewarp-terminal.
- Source: general
- Disposition notes: **wontfix** (decided by review oracle). The DevCli content files compile from `~/.nuget/packages/...` where neither a repo `.editorconfig` nor a per-file pragma can be placed, and the C# compiler has no per-file `NoWarn`; project-wide suppression is the only mechanism available to package consumers. Results state this matches the timewarp-terminal list (that repo was not checked out locally during review, so parity is taken from the implementer's note). Revisit if DevCli ships a `.globalconfig` for its content.

## Duplicates / conflicts

- None; single reviewer.
