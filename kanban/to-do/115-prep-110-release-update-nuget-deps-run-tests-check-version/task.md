# Prep 1.1.0 release: update NuGet deps, run tests, check-version

## Description

Prepare timewarp-amuru for its next NuGet release. `source/Directory.Build.props`
already carries `1.1.0` while the latest tag is `v1.0.0` (2026-07-05); master is
82 commits / 13 merged PRs ahead of that tag. Before cutting the release, bring
every NuGet dependency to latest, prove the full test suite is green, and confirm
`dev check-version` accepts the version. Open the PR; the release itself
(`dev release` from clean synced master, per `tw-release`) is a separate step.

## Checklist

- [x] Run `ganda nuget outdated` in the claim worktree; record the report in Notes
- [x] Run `ganda nuget outdated --update --force` so every package is on latest; review the diff for any major bumps that need code changes
- [x] `dotnet run tools/dev-cli/dev.cs -- build` succeeds
- [x] `dotnet run tools/dev-cli/dev.cs -- verify-samples` succeeds
- [x] `dotnet run tools/dev-cli/dev.cs -- test` passes (failed == 0, total > 0)
- [x] `dotnet run tools/dev-cli/dev.cs -- check-version` passes; if it refuses, bump `<Version>` in `source/Directory.Build.props` as it directs and re-run
- [x] `ganda repo audit` clean
- [x] Commit with conventional message (`chore(deps): …` / `chore(release): …`)
- [ ] PR open against origin home with the outdated report and test totals in the body

## Session

- Created: 2010361 (2026-09-23)
- Implement: Claude Fable 5.1 under `ganda task work` (2026-09-23) — deps bump, audit clean, gates green

## Notes

- Do not tag or run `dev release` on this task. The PR is the deliverable; release follows merge.
- The `pr/merge` workflow in `tools/dev-cli/endpoints/workflow-command.cs` is `clean -> build -> verify-samples -> test -> check-version`; `dev workflow pr/merge` may cover the middle checklist items in one shot.
- If a dependency bump breaks a build or test, fix forward in this task and note it here; do not pin back without saying why.

### `ganda nuget outdated` report (2026-09-23, before update)

| Package | Current | Latest | Type |
|---------|---------|--------|------|
| CliWrap | 3.10.2 | 3.10.5 | patch |
| NuGet.Versioning | 7.6.0 | 7.9.0 | minor |
| Shouldly | 4.3.0 | up to date | - |
| TimeWarp.Build.Tasks | 1.0.0 | up to date | - |
| TimeWarp.Jaribu | 1.0.0-beta.13 | 1.0.0-beta.15 | patch |
| TimeWarp.Nuru | 3.0.0-beta.71 | 3.0.0-beta.76 | patch |
| TimeWarp.Terminal | 1.0.0 | 1.0.2 | patch |
| Roslynator.Analyzers | 4.15.0 | 5.0.0 | major |
| Roslynator.CodeAnalysis.Analyzers | 4.15.0 | 5.0.0 | major |
| Roslynator.Formatting.Analyzers | 4.15.0 | 5.0.0 | major |
| Microsoft.CodeAnalysis.NetAnalyzers | 10.0.301 | 10.0.401 | patch |
| Microsoft.CodeAnalysis.CSharp.CodeStyle | 5.6.0 | 5.9.0 | minor |
| Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | up to date | - |

10 outdated (3 major, 2 minor, 5 patch). The three majors are Roslynator 5.0.0
(analyzer-only); the build stayed at 0 warnings / 0 errors, so no code changes were
needed for any bump. Only `Directory.Packages.props` changed for the deps commit.

### `ganda repo audit` was not clean on master

The bump itself left the audit at 19 passed / 10 failed (8 errors), all pre-existing
repo-hygiene checks unrelated to the deps: `assembly-metadata`, `bin-dev`,
`dev-cli-capabilities`, `global-usings-analyzer`, `kebab-path-names`,
`nuget-package-urls`, `runfile-executable`, `runfile-shebang` (+ 2 warnings:
`memsearch-scaffold`, `vscode-window-icon`). Resolved with `ganda repo audit --fix`
plus manual cleanup (see `chore(audit)` commit):

- `--fix` added `TimeWarp.Build.Tasks` to the root `Directory.Build.props` while
  `source/Directory.Build.props` still referenced it, which broke restore with
  NU1504 (duplicate PackageReference). Removed the `source/` copy; root now owns it.
- `--fix` appended its XML on single lines; reformatted the root and `source/`
  props plus `Directory.Packages.props` to the file style.
- `TimeWarp.SourceGenerators` 1.0.0-beta.11 is now referenced repo-wide (TW000x
  analyzers, `TW0007.filename = global-usings.cs`). Build/tests green with it.
- `TimeWarp.Amuru` 1.0.0 and `TimeWarp.Amuru.Tools` 1.0.0-beta.2 are pinned in
  `Directory.Packages.props` because the new `.githooks/*.cs` runfiles use
  `#:package TimeWarp.Amuru` / `.Tools` without a version under CPM.
- `kebab-path-names` flagged the eight `.agent/workspace/YYYY-MM-DDTHH-MM-SS_slug.md`
  reports. That format is mandated by the `tw-analysis-report` skill, so instead of
  renaming (10 kanban/done links point at them) an `[ganda.audit]`
  `kebab-path-names.prune = .agent` exception was added to `.editorconfig`.
  Follow-up (not this task): reconcile the audit check and the skill in
  timewarp-flow / ganda so no per-repo exception is needed.
- `cliwrap-exit-code-tests/README.md` renamed to `readme.md` (no inbound links).
- `bin-dev` / `dev-cli-capabilities` need the gitignored AOT binary; satisfied
  locally by `dotnet run tools/dev-cli/dev.cs -- self-install`. Not part of the diff.
- `core.hooksPath=.githooks` was already set in the shared `.git/config`; the
  scaffolded hooks compiled and ran on the audit commit (pre-commit passed,
  post-commit attested the tree).

## Results

Branch `task/115-prep-110-release-update-nuget-deps-run-tests-check`, two product commits:

- `fa6af4b chore(deps): update NuGet dependencies to latest` — 10 packages to latest (table above)
- `c69c328 chore(audit): bring repo to ganda repo audit clean` — 124 files (106 shebangs,
  20 exec bits, props/editorconfig/scaffold, README rename, audit prune)

Gate results after both commits:

| Gate | Result |
|------|--------|
| `dev build` | Build succeeded, 0 warnings, 0 errors |
| `dev verify-samples` | 2/2 samples compile |
| `dev test` | Total 495, Passed 494, Failed 0, Skipped 1 (git-history test skipped in CI mode) |
| `dev check-version` | source 1.1.0 vs latest tag v1.0.0: "Version in source is new — safe to release" |
| `ganda repo audit` | Passed 29, Failed 0, Skipped 0 |

`TimeWarp.Amuru.Tools` stays at its own `1.0.0-beta.2`; only the core `1.1.0` is the
release version per `AGENTS.md`. No tag, no `dev release` (release follows merge).

### How to validate

Smoke (in the task worktree):

```bash
dotnet run tools/dev-cli/dev.cs -- build
dotnet run tools/dev-cli/dev.cs -- verify-samples
dotnet run tools/dev-cli/dev.cs -- test
dotnet run tools/dev-cli/dev.cs -- check-version
dotnet run tools/dev-cli/dev.cs -- self-install   # produces gitignored bin/dev for the audit
ganda repo audit
ganda nuget outdated
```

Expect:

- build: `Build succeeded.` with `0 Warning(s)` / `0 Error(s)`, both nupkgs created
  (`TimeWarp.Amuru.1.1.0.nupkg`, `TimeWarp.Amuru.Tools.1.0.0-beta.2.nupkg`)
- verify-samples: `All 2 sample(s) verified successfully!`
- test: `Failed: 0`, `Total: 495` (≥ 494 passed, 1 skipped), `All tests passed!`
- check-version: `Version in source is new — safe to release.`
- audit: `Passed: 29 | Failed: 0`, no "blocking audit failures" line
- nuget outdated: every package reports `(up to date)`; `Directory.Packages.props`
  shows Roslynator 5.0.0, CliWrap 3.10.5, NuGet.Versioning 7.9.0, Nuru 3.0.0-beta.76,
  Jaribu 1.0.0-beta.15, Terminal 1.0.2, NetAnalyzers 10.0.401, CodeStyle 5.9.0
- `unzip -p artifacts/packages/TimeWarp.Amuru.1.1.0.nupkg TimeWarp.Amuru.nuspec | grep projectUrl`
  prints `https://timewarp.software/projects/timewarp-amuru/`
