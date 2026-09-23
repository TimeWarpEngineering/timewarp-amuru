# Prep 1.1.0 release: update NuGet deps, run tests, check-version

## Description

Prepare timewarp-amuru for its next NuGet release. `source/Directory.Build.props`
already carries `1.1.0` while the latest tag is `v1.0.0` (2026-07-05); master is
82 commits / 13 merged PRs ahead of that tag. Before cutting the release, bring
every NuGet dependency to latest, prove the full test suite is green, and confirm
`dev check-version` accepts the version. Open the PR; the release itself
(`dev release` from clean synced master, per `tw-release`) is a separate step.

## Checklist

- [ ] Run `ganda nuget outdated` in the claim worktree; record the report in Notes
- [ ] Run `ganda nuget outdated --update --force` so every package is on latest; review the diff for any major bumps that need code changes
- [ ] `dotnet run tools/dev-cli/dev.cs -- build` succeeds
- [ ] `dotnet run tools/dev-cli/dev.cs -- verify-samples` succeeds
- [ ] `dotnet run tools/dev-cli/dev.cs -- test` passes (failed == 0, total > 0)
- [ ] `dotnet run tools/dev-cli/dev.cs -- check-version` passes; if it refuses, bump `<Version>` in `source/Directory.Build.props` as it directs and re-run
- [ ] `ganda repo audit` clean
- [ ] Commit with conventional message (`chore(deps): …` / `chore(release): …`)
- [ ] PR open against origin home with the outdated report and test totals in the body

## Session

- Created: 2010361 (2026-09-23)

## Notes

- Do not tag or run `dev release` on this task. The PR is the deliverable; release follows merge.
- The `pr/merge` workflow in `tools/dev-cli/endpoints/workflow-command.cs` is `clean -> build -> verify-samples -> test -> check-version`; `dev workflow pr/merge` may cover the middle checklist items in one shot.
- If a dependency bump breaks a build or test, fix forward in this task and note it here; do not pin back without saying why.
