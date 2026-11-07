# Update NuGet packages from ganda nuget outdated

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

- [ ] Re-run `ganda nuget outdated` in the claim worktree and record the report in Notes
- [ ] Apply the reported updates (`ganda nuget outdated --update --force`) and review the diff
- [ ] `./tools/dev-cli/dev.cs build` succeeds
- [ ] `./tools/dev-cli/dev.cs test` passes (failed == 0, total > 0)
- [ ] `ganda repo audit` is clean
- [ ] `ganda nuget outdated` reports every package up to date
- [ ] Commit as `chore(deps): …`

## Session

- Created: 01a1b4c4-827b-7d70-9fd0-358726b63b0b (2026-11-07)

## Notes

Do not tag or run `dev release` on this task. The pull request is the deliverable.

### `ganda nuget outdated` report (2026-11-07, master, before update)

18 packages checked. 3 outdated (3 patch). The other 15 were up to date, including CliWrap 3.10.5, NuGet.Versioning 7.9.0, Shouldly 4.3.0, TimeWarp.Build.Tasks 1.0.0, TimeWarp.Jaribu 1.0.0-beta.15, TimeWarp.Nuru 4.0.0-beta.11, TimeWarp.Nuru.DevCli 4.0.0-beta.11, TimeWarp.Terminal 1.0.2, Microsoft.CodeAnalysis.NetAnalyzers 11.0.100-rc.3.26522.148, Microsoft.CodeAnalysis.CSharp.CodeStyle 5.9.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, Microsoft.CodeAnalysis.PublicApiAnalyzers 5.6.0, TimeWarp.SourceGenerators 1.0.0-beta.11, TimeWarp.Amuru 2.0.0, and TimeWarp.Amuru.Tools 2.0.0.

| Package | Current | Latest | Band |
|---------|---------|--------|------|
| Roslynator.Analyzers | 5.0.0 | 5.0.1 | patch |
| Roslynator.CodeAnalysis.Analyzers | 5.0.0 | 5.0.1 | patch |
| Roslynator.Formatting.Analyzers | 5.0.0 | 5.0.1 | patch |
