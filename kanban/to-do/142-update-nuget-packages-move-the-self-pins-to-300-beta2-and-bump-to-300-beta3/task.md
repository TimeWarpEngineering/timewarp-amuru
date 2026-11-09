# Update NuGet packages, move the self pins to 3.0.0-beta.2, and bump to 3.0.0-beta.3

## Description

Dependency refresh after the 3.0.0-beta.2 release (2026-11-09), rolled into one bump PR so the release note and the version are typed once.

`ganda nuget outdated` on master at 22ef777 reported three outdated packages:

| Package | Current | Available | Where used |
|---|---|---|---|
| TimeWarp.Ramani | 1.0.0-beta.23 | 1.0.0-beta.24 | core package (internal JSON-RPC session machine, task 139) |
| TimeWarp.SourceGenerators | 1.0.0-beta.11 | 1.0.0-beta.12 | build |
| Microsoft.Extensions.Hosting | 10.0.1 | 10.0.14 stable, 11.0.0 prerelease | MCP validation server under tests only |

Separately, the self pins that the `.githooks` runfiles consume are still at the published 2.0.0. `3.0.0-beta.2` is on nuget.org now, so they move. The 3.0.0-beta.1 release note deferred exactly this.

## Requirements

### Package updates

- Run `ganda nuget outdated` on the task branch first and act on what it reports then, not only on the table above.
- `TimeWarp.Ramani` to 1.0.0-beta.24. Read the Ramani release or kanban notes for what changed between beta.23 and beta.24. The test `Directory.Build.props` resolves the generator path from the `PackageVersion` item (task 138, review M1), so only `Directory.Packages.props` changes. The 139 and 138 machine tests must pass unchanged. If beta.24 changes a diagnostic or generator behavior that breaks them, stop and record it in Notes rather than editing the machines to fit.
- `TimeWarp.SourceGenerators` to 1.0.0-beta.12.
- `Microsoft.Extensions.Hosting`: the repo targets net11.0. Prefer the 11.0.x stable if it is on nuget.org by the time this runs (.NET 11 GA is 2026-11-10). Otherwise 10.0.14. It is test-only, for the MCP stdio validation server, so it does not enter the shipped graph either way.
- Self pins in `Directory.Packages.props`: `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` from 2.0.0 to 3.0.0-beta.2. Run every `.githooks/*.cs` runfile once (`dotnet run --file .githooks/pre-commit.cs` and the others) to prove they restore and build against the net11-only package. 2.0.0 targeted net10; the hooks must still run on the SDK the repo pins in `global.json`.
- Nothing else in `Directory.Packages.props` changes unless `ganda nuget outdated` reports it. Analyzer packages pinned to match the SDK (NetAnalyzers rc.3, CodeStyle 5.9.0) stay unless a stable build matching the SDK has appeared.

### Version bump to 3.0.0-beta.3

- `<Version>` in `source/Directory.Build.props` from `3.0.0-beta.2` to `3.0.0-beta.3`, its own commit, after the package updates are green.
- Public API: both Unshipped files are header-only after beta.2. If this task adds no public members, nothing moves. If it does, roll Unshipped into Shipped per AGENTS.md "Release".
- Release note `documentation/release-notes/3.0.0-beta.3.md`, same shape as beta.2. Sections: dependency updates (the table above with the versions actually taken), the self-pin move and what it means for the githooks, and unchanged. No behavior change is expected; say so if true.
- `./tools/dev-cli/dev.cs check-version` reports 3.0.0-beta.3 unpublished.
- No tag, no `dev release`, no publish in this task. Release is cut from master after merge with `/tw-release`.

## Checklist

- [ ] `ganda nuget outdated` run on the branch; table in Results matches what was taken
- [ ] TimeWarp.Ramani 1.0.0-beta.24; 138 and 139 machine tests pass unchanged
- [ ] TimeWarp.SourceGenerators 1.0.0-beta.12
- [ ] Microsoft.Extensions.Hosting updated; MCP validation server tests pass
- [ ] Self pins at 3.0.0-beta.2; every `.githooks/*.cs` runfile runs
- [ ] `<Version>` is 3.0.0-beta.3, own commit
- [ ] `documentation/release-notes/3.0.0-beta.3.md` written
- [ ] `dev check-version` reports 3.0.0-beta.3 unpublished
- [ ] `./tools/dev-cli/dev.cs build` and `./tools/dev-cli/dev.cs test` pass
- [ ] `ganda repo audit` is clean

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)

## Notes

Filed from the cockpit after the 3.0.0-beta.2 release on 2026-11-09. Steve: "create a task to update NuGets and bump version again `ganda nuget outdated`", with the githook pin move folded in from his earlier request so there is one bump PR, not two.

`ganda nuget outdated` lists `TimeWarp.Amuru 2.0.0 (up to date)` because it compares within the stable channel. The move to 3.0.0-beta.2 is a deliberate prerelease pin, not something the tool will propose.
