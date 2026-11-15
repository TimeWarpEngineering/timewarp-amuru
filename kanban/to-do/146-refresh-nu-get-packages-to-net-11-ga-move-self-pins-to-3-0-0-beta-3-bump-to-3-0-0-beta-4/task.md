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

## Checklist

- [ ] Every row in the table moved; `ganda nuget outdated` reports zero outdated
- [ ] NuGet.Versioning 8.0 release notes read; any breaking change handled and noted in the release note
- [ ] TimeWarp.Build.Tasks 2.0.0 and TimeWarp.Nuru beta.14 breaking changes handled (dev-cli compiles; `./bin/dev --capabilities` still lists the same commands)
- [ ] Self pins at 3.0.0-beta.3; the five git hooks compile and run (`dotnet run --file .githooks/pre-commit.cs` smoke)
- [ ] `./bin/dev build` and `./bin/dev test` green; NativeAOT publish of Amuru.Tools still clean
- [ ] `<Version>` 3.0.0-beta.4; PublicAPI.Unshipped moved to Shipped if anything is pending; release note added
- [ ] `ganda repo audit` zero Errors; CI green on the PR

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-15)

## Notes

Release of 3.0.0-beta.4 is a separate step (`dev release` from master after the merge is green), not part
of this task. The self pins can only follow a release, so this pattern repeats once per release.
