# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-amuru

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-amuru.
Filter: (all)
Outdated:
  TimeWarp.Jaribu 1.0.0-beta.17 -> 1.0.0-beta.18 (dev-only: tests/timewarp-amuru/Directory.Build.props)
  TimeWarp.Amuru 3.0.0-beta.9 -> 3.0.0-beta.10 (dev-only: this repo produces this package)
  TimeWarp.Amuru.Tools 3.0.0-beta.9 -> 3.0.0-beta.10 (dev-only: this repo produces this package)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump three dev-only package pins in the root `Directory.Packages.props` to their latest versions:
TimeWarp.Jaribu to 1.0.0-beta.18, and TimeWarp.Amuru and TimeWarp.Amuru.Tools (the self pins that
the `.githooks` runfiles use) to 3.0.0-beta.10.

Acceptance criteria:
- `Directory.Packages.props` pins exactly those three versions. No other file changes.
- `<Version>` in `source/Directory.Build.props` stays the same. This wave does not release.
- `./bin/dev build` passes, a Jaribu runfile restores and runs on beta.18, and `ganda repo audit` passes.

## Session

- Minted by ganda task wave run 20261119T023451Z-133310
- 2026-11-19 implementer: bumped the three pins in `Directory.Packages.props`.

## Results

Changed: `Directory.Packages.props` — Jaribu 1.0.0-beta.17 → 1.0.0-beta.18; Amuru and Amuru.Tools 3.0.0-beta.9 → 3.0.0-beta.10.
No test file was added or edited, and no production code changed. One standalone Jaribu runfile was run to check the Jaribu bump.

Gates:
- `./bin/dev build` — passed (0 warnings, 0 errors).
- `dotnet run --file tests/timewarp-amuru/single-file-tests/script-support/script-context.from-relative-path.cs` — passed (2/2). Run only to check that Jaribu beta.18 restores and runs; it is not a changed file.
- `ganda repo audit` — passed with 1 advisory warning (`vscode-window-icon`: `.vscode/settings.json` missing). Ran `ganda repo audit --fix`, which wrote the local, gitignored `.vscode/settings.json`, so no tracked file changed.
- `ganda repo audit` (rerun after the fix) — passed all checks.

### How to validate

Smoke: `grep -E 'TimeWarp\.(Jaribu|Amuru|Amuru\.Tools)"' Directory.Packages.props && ./bin/dev build && dotnet run --file tests/timewarp-amuru/single-file-tests/script-support/script-context.from-relative-path.cs`
Expect: the pins show Jaribu `1.0.0-beta.18` and Amuru/Amuru.Tools `3.0.0-beta.10`; the build succeeds; the runfile reports 2 passed.
