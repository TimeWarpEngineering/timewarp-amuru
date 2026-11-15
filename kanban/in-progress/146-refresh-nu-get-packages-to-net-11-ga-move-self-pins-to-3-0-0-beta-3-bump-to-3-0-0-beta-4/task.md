# Refresh NuGet packages to .NET 11 GA, move self pins to 3.0.0-beta.3, bump to 3.0.0-beta.4

## Description

`ganda nuget outdated` on master (2026-11-15, after task 143 and the v3.0.0-beta.3 release) lists ten
outdated packages. The `nuru` audit check reports the Nuru lag as an Error, so the pre-PR gate is red
until it moves. Same shape as task 142: refresh, move the self pins, bump, release note.

Targets (all in `Directory.Packages.props`):

| Package | Current | Target | Note |
|---|---|---|---|
| Microsoft.Extensions.Hosting | 10.0.14 | 11.0.0 | .NET 11 GA; the follow-up task 142 deferred |
| Microsoft.CodeAnalysis.NetAnalyzers | 11.0.100-rc.3.26522.148 | 11.0.100 | GA off the rc |
| Microsoft.CodeAnalysis.CSharp.CodeStyle | 5.9.0 | 5.12.0 | |
| NuGet.Versioning | 7.9.0 | 8.0.0 | major; library dependency of the public package. Read the 8.0 release notes, check for breaking API, mention in release note |
| TimeWarp.Build.Tasks | 1.0.0 | 2.0.0 | major, dev-only. Check the build still produces identical package metadata |
| TimeWarp.Nuru | 4.0.0-beta.12 | 4.0.0-beta.14 | audit Error until moved |
| TimeWarp.Nuru.DevCli | 4.0.0-beta.12 | 4.0.0-beta.14 | |
| TimeWarp.SourceGenerators | 1.0.0-beta.12 | 1.0.0-beta.16 | |
| TimeWarp.Jaribu | 1.0.0-beta.15 | 1.0.0-beta.16 | |
| TimeWarp.Terminal | 2.0.0-beta.1 | 2.0.0-beta.2 | library dependency |
| TimeWarp.Amuru / TimeWarp.Amuru.Tools (self pins) | 3.0.0-beta.2 | 3.0.0-beta.3 | consumed by `.githooks/*.cs` via CPM; beta.3 is on nuget.org |

If a package in the table has moved again by the time this runs, take the newest version
`ganda nuget outdated` reports. Prefer stable over prerelease where both exist.

Version bump: `<Version>` in `source/Directory.Build.props` 3.0.0-beta.3 to 3.0.0-beta.4. Add
`documentation/release-notes/3.0.0-beta.4.md` listing the dependency moves, with the NuGet.Versioning
major and Microsoft.Extensions.Hosting 11.0.0 called out. Follow the beta.3 note's format.

## Goal (implementer restatement)

Move every outdated pin in `Directory.Packages.props` to the newest version `ganda nuget outdated` offers
(stable over prerelease): .NET 11 GA for Microsoft.Extensions.Hosting and NetAnalyzers, CodeStyle 5.12.0,
NuGet.Versioning 8.0.0, TimeWarp.Build.Tasks 2.0.0, Nuru/Nuru.DevCli beta.14, SourceGenerators beta.16,
Jaribu beta.16, Terminal beta.2. Move the githook self pins (Amuru, Amuru.Tools) to 3.0.0-beta.3. Since
NetAnalyzers moves to the 11.0.100 GA build, the `global.json` SDK pin follows it to 11.0.100.
Bump `<Version>` to 3.0.0-beta.4 and add a beta.4 release note in the beta.3 format.

Acceptance:
- `ganda nuget outdated` reports zero outdated.
- NuGet.Versioning 8.0 and Build.Tasks 2.0 / Nuru beta.14 breaking changes handled; dev-cli compiles and lists the same commands.
- Githooks compile against the beta.3 self pins.
- `./bin/dev build`, `./bin/dev test` green; `ganda repo audit` zero errors.
- PublicAPI Unshipped moved to Shipped if pending; release note added.

## Checklist

- [x] Every row in the table moved; `ganda nuget outdated` reports zero outdated
- [x] NuGet.Versioning 8.0 release notes read; any breaking change handled and noted in the release note
- [x] TimeWarp.Build.Tasks 2.0.0 and TimeWarp.Nuru beta.14 breaking changes handled (dev-cli compiles; `./bin/dev --capabilities` still lists the same commands)
- [x] Self pins at 3.0.0-beta.3; the five git hooks compile and run (`dotnet run --file .githooks/pre-commit.cs` smoke)
- [x] `./bin/dev build` and `./bin/dev test` green; NativeAOT publish of Amuru.Tools still clean
- [x] `<Version>` 3.0.0-beta.4; PublicAPI.Unshipped moved to Shipped if anything is pending; release note added
- [ ] `ganda repo audit` zero Errors; CI green on the PR

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-15)
- Implement (2026-11-15): merged origin/master (kanban land commit). Moved all ten pins plus self pins; SDK pin to 11.0.100 GA.
  - TimeWarp.Build.Tasks pinned 2.0.1-beta.1, not 2.0.0: Nuru 4.0.0-beta.14 requires `>= 2.0.1-beta.1` and 2.0.0 fails restore with NU1605.
  - SourceGenerators beta.16 adds TWA0004/TWA0038: Purpose region added to `run-tests.cs`; the 14 files in `.timewarp/line-cap-baseline.json` get per-file `TWA0038.max_lines` (current length) in `.editorconfig`, flexbox ratchet pattern. Splitting them is out of scope for a dependency refresh.
  - Jaribu beta.16: `TestSkipped(string)` became `TestSkipped(ITerminal, string)`; six call sites pass `TimeWarpTerminal.Default`.
  - NuGet.Versioning 8.0: no published release-note page found; package diff shows lib net8.0 -> net10.0 (net472 kept). Only `NuGetVersion.TryParse` + comparison used; compiles unchanged, nuget-package-service tests green.
  - Boy-scout: `ganda repo audit --fix` cleared two Warnings (memsearch-scaffold: Purpose regions in five githooks + new `prepare-commit-msg` hook; vscode-window-icon: ignored `.vscode/settings.json`).

## Results

Dependency refresh landed on `task/146-…`: every row in the table moved (Build.Tasks to 2.0.1-beta.1, see Session), self pins at 3.0.0-beta.3,
`global.json` at SDK 11.0.100, `<Version>` 3.0.0-beta.4, `documentation/release-notes/3.0.0-beta.4.md` added, readme version line updated.
PublicAPI.Unshipped files were header-only (nothing to move).

Gates (run once each, in order, on the final tree):

| Gate | Command | Result |
|---|---|---|
| build | `./bin/dev build` | green, 0 warnings, 0 errors |
| test | `./bin/dev test` | green, 839 total: 838 passed, 1 skipped, 0 failed |
| audit | `ganda repo audit` | exit 0, zero Errors; 2 Warnings fixed with `ganda repo audit --fix` ("Repository passes all audit checks.") |

Extra checks:
- `ganda nuget outdated`: "All packages are up to date!"
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` (NativeAOT publish referencing Amuru.Tools): clean, no IL2xxx/IL3xxx warnings; `./bin/dev --capabilities` lists the same 8 commands as master's binary.
- `dotnet build .githooks/<hook>.cs` for pre-commit, post-commit, pre-push, post-merge, post-checkout, prepare-commit-msg: all exit 0 on the 3.0.0-beta.3 self pins.
- beta.4 nuspecs vs published beta.3 nuspecs: differ only in version, repository commit/branch, and dependency versions; file lists identical (nuget.org adds `.signature.p7s`).

### How to validate

Smoke:
```bash
./bin/dev build && ./bin/dev test
ganda nuget outdated
./bin/dev --capabilities | grep '"pattern"'
dotnet build .githooks/pre-commit.cs
```

Expect: build and test green (838 passed, 1 skipped); `ganda nuget outdated` prints "All packages are up to date!";
eight dev commands (build, clean, test, verify-samples, workflow, check-version, release, self-install); hook compiles.
`artifacts/packages/TimeWarp.Amuru.Tools.3.0.0-beta.4.nupkg` nuspec depends on NuGet.Versioning 8.0.0 and TimeWarp.Amuru 3.0.0-beta.4.

## Notes

Release of 3.0.0-beta.4 is a separate step (`dev release` from master after the merge is green), not part
of this task. The self pins can only follow a release, so this pattern repeats once per release.
