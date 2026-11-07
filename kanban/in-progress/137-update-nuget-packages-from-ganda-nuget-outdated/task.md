# Update NuGet packages from ganda nuget outdated

## Goal (implementer restatement)

Refresh every Central Package Management pin that a fresh `ganda nuget outdated` run in this claim worktree still marks outdated. The master snapshot named three Roslynator analyzer patch bumps (5.0.0 to 5.0.1); the claim-worktree run is the list that gets applied, and anything that run marks current stays put.

Done means the same command then reports every package current, `Directory.Packages.props` is the only pin file that moved unless a named bump forces a matching reference, the product version stays `3.0.0-beta.1`, the SDK pin and its matching `Microsoft.CodeAnalysis.NetAnalyzers` pin stay together unless that analyzer is named, and the consumed `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` pins stay at published `2.0.0` unless they are named. Build and tests pass after the bump. A break is fixed here; rolling a pin back requires a written reason. This task does not tag or release.

## Description

Bring every Central Package Management pin in `Directory.Packages.props` to the version `ganda nuget outdated` reports, then prove the repo still builds and tests. The 2026-11-07 report on master found three patch bumps. Re-run the command in the claim worktree before editing pins. Take whatever that run lists. Do not bump a package the report marks up to date.

## Requirements

- `ganda nuget outdated` in the claim worktree reports every package up to date after the change.
- `Directory.Packages.props` is the only package-pin file that changes, unless a bump forces a matching reference the report names.
- Leave the product `<Version>` in `source/Directory.Build.props` at `3.0.0-beta.1`. This task does not cut a release.
- Leave the SDK pin in `global.json` and the matching `Microsoft.CodeAnalysis.NetAnalyzers` pin alone unless the outdated report names that analyzer. The pin must stay on the same build as the SDK.
- Leave the consumed `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` pins at the published `2.0.0` unless the outdated report names them. Those pins are the githook packages, separate from the product version.
- A bump that breaks the build or tests is fixed in this task. Do not pin a package back without writing why in Notes.

## Checklist

- [x] Re-run `ganda nuget outdated` in the claim worktree and record the report in Notes
- [x] Apply the reported updates (`ganda nuget outdated --update --force`) and review the diff
- [x] `./tools/dev-cli/dev.cs build` succeeds
- [x] `./tools/dev-cli/dev.cs test` passes (failed == 0, total > 0)
- [x] `ganda repo audit` is clean
- [x] `ganda nuget outdated` reports every package up to date
- [x] Commit as `chore(deps): …`

## Session

- Created: 01a1b4c4-827b-7d70-9fd0-358726b63b0b (2026-11-07)
- Implementation: 01a1b4d6-d61f-72b3-b624-e1ab99056847 (2026-11-07)

## Notes

Do not tag or run `dev release` on this task. The pull request is the deliverable.

### `ganda nuget outdated` report (2026-11-07, master, before update)

18 packages checked. 3 outdated (3 patch). The other 15 were up to date, including CliWrap 3.10.5, NuGet.Versioning 7.9.0, Shouldly 4.3.0, TimeWarp.Build.Tasks 1.0.0, TimeWarp.Jaribu 1.0.0-beta.15, TimeWarp.Nuru 4.0.0-beta.11, TimeWarp.Nuru.DevCli 4.0.0-beta.11, TimeWarp.Terminal 1.0.2, Microsoft.CodeAnalysis.NetAnalyzers 11.0.100-rc.3.26522.148, Microsoft.CodeAnalysis.CSharp.CodeStyle 5.9.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, Microsoft.CodeAnalysis.PublicApiAnalyzers 5.6.0, TimeWarp.SourceGenerators 1.0.0-beta.11, TimeWarp.Amuru 2.0.0, and TimeWarp.Amuru.Tools 2.0.0.

| Package | Current | Latest | Band |
|---------|---------|--------|------|
| Roslynator.Analyzers | 5.0.0 | 5.0.1 | patch |
| Roslynator.CodeAnalysis.Analyzers | 5.0.0 | 5.0.1 | patch |
| Roslynator.Formatting.Analyzers | 5.0.0 | 5.0.1 | patch |

### `ganda nuget outdated` report (2026-11-07, claim worktree, before update)

18 packages checked. 3 outdated (3 patch). Same three Roslynator analyzer pins as the master snapshot. The other 15 were up to date, including CliWrap 3.10.5, NuGet.Versioning 7.9.0, Shouldly 4.3.0, TimeWarp.Build.Tasks 1.0.0, TimeWarp.Jaribu 1.0.0-beta.15, TimeWarp.Nuru 4.0.0-beta.11, TimeWarp.Nuru.DevCli 4.0.0-beta.11, TimeWarp.Terminal 1.0.2, Microsoft.CodeAnalysis.NetAnalyzers 11.0.100-rc.3.26522.148, Microsoft.CodeAnalysis.CSharp.CodeStyle 5.9.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, Microsoft.CodeAnalysis.PublicApiAnalyzers 5.6.0, TimeWarp.SourceGenerators 1.0.0-beta.11, TimeWarp.Amuru 2.0.0, and TimeWarp.Amuru.Tools 2.0.0.

| Package | Current | Latest | Band |
|---------|---------|--------|------|
| Roslynator.Analyzers | 5.0.0 | 5.0.1 | patch |
| Roslynator.CodeAnalysis.Analyzers | 5.0.0 | 5.0.1 | patch |
| Roslynator.Formatting.Analyzers | 5.0.0 | 5.0.1 | patch |

`ganda nuget outdated --update --force` wrote those three pins and also a UTF-8 BOM on `Directory.Packages.props`. The BOM was stripped so the pin diff is only the three versions. No other pin moved. Product `<Version>` stays `3.0.0-beta.1`. `global.json` and `Microsoft.CodeAnalysis.NetAnalyzers` stay on `11.0.100-rc.3.26522.148`. Consumed `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` stay at `2.0.0`.

### Audit (boy scout)

`ganda repo audit` failed on two pre-existing checks before any pin-related code change:

- `vscode-settings-gitignore`: `.vscode/settings.json` was tracked and re-included. `ganda repo audit --fix` dropped the `!.vscode/settings.json` line and ran `git rm --cached`. The working copy stays, because Peacock rewrites that file.
- `source-file-line-cap`: 14 C# files were already over 500 lines, and origin has no `.timewarp/line-cap-baseline.json`. That missing origin file is how the first ratchet lands, so this task records the current counts. The recorded count can only shrink after this merges. Splitting those files by responsibility is later work; it was not done in this pin bump.

After that, `ganda repo audit` passed: 38 passed, 0 failed, 2 skipped.

## Results

Claim-worktree `ganda nuget outdated` listed the same three Roslynator analyzer patch bumps as the master snapshot. Those pins are now `5.0.1`. A second run reports every package up to date.

| Gate | Result |
|------|--------|
| `./tools/dev-cli/dev.cs build` | Build succeeded, 0 warnings, 0 errors. Restore graph uses Roslynator 5.0.1. Packages: `TimeWarp.Amuru.3.0.0-beta.1.nupkg`, `TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg` |
| `./tools/dev-cli/dev.cs test` | Total 733, Passed 732, Failed 0, Skipped 1. `All tests passed!` |
| `ganda repo audit` | Passed 38, Failed 0, Skipped 2, after the vscode untrack and the first line-cap ratchet |
| `ganda nuget outdated` | 18 packages, all up to date |

No package was pinned back. No tag and no `dev release`.

### How to validate

**Smoke**

```bash
./tools/dev-cli/dev.cs build
./tools/dev-cli/dev.cs test
ganda repo audit
ganda nuget outdated
```

**Expect**

- Build prints `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`, and writes `TimeWarp.Amuru.3.0.0-beta.1.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg`.
- Tests print `Failed: 0` with `Total` greater than 0 (this run: Total 733, Passed 732, Skipped 1) and `All tests passed!`.
- Audit prints `Failed: 0` and `Repository passes all audit checks.` `source-file-line-cap` passes because `.timewarp/line-cap-baseline.json` records the 14 files that are still over 500 lines.
- `ganda nuget outdated` prints `All packages are up to date!` The three Roslynator pins in `Directory.Packages.props` are `5.0.1`. `source/Directory.Build.props` `<Version>` is still `3.0.0-beta.1`. `Microsoft.CodeAnalysis.NetAnalyzers`, `TimeWarp.Amuru`, and `TimeWarp.Amuru.Tools` are unchanged.
