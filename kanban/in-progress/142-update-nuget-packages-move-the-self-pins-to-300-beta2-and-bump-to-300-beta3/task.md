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

## Goal (implementer restatement)

Refresh the dependencies this branch is actually behind, move the githook self-pins onto the net11.0 packages that 3.0.0-beta.2 published, and then cut the repo version to 3.0.0-beta.3 in its own commit. No product behavior change and no publish.

Acceptance: `ganda nuget outdated` on this branch decides the package table, and only those reported pins move, plus the deliberate self-pin move the tool will not propose because it stays on the stable channel. `TimeWarp.Ramani` becomes 1.0.0-beta.24. That release adds the demo, the interactions package, and the class-diagram page. The core machine package and its generator are unchanged from beta.23, so the task 138 command-completion oracle and the task 139 JSON-RPC session machine stay as they are. If a diagnostic or generator change still breaks those tests, stop and write that in Notes. Do not edit the machines to match. `TimeWarp.SourceGenerators` becomes 1.0.0-beta.12. Beta.12 drops the retired delegation generator and TW0002 through TW0006. This repo uses TW0001 and TW0007, which stay. `Microsoft.Extensions.Hosting` prefers a stable 11.0.x. If nuget.org has no stable 11.0 yet, take 10.0.14. The reference is test-only, for the MCP stdio validation server, and it must not enter the shipped graph. Those tests pass. `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` become 3.0.0-beta.2, and every `.githooks/*.cs` runfile restores, builds, and runs on the SDK in `global.json`. After that set is green, `<Version>` becomes 3.0.0-beta.3 in a separate commit. Both Unshipped files stay header-only because this task adds no public members. The beta.3 release note records the dependency table that was actually taken, the self-pin move and what it means for the hooks, and that behavior is unchanged. `dev check-version` reports 3.0.0-beta.3 unpublished. `dev build`, `dev test`, and `ganda repo audit` pass. No tag, no `dev release`, and no publish.

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

- [x] `ganda nuget outdated` run on the branch; table in Results matches what was taken
- [x] TimeWarp.Ramani 1.0.0-beta.24; 138 and 139 machine tests pass unchanged
- [x] TimeWarp.SourceGenerators 1.0.0-beta.12
- [x] Microsoft.Extensions.Hosting updated; MCP validation server tests pass
- [x] Self pins at 3.0.0-beta.2; every `.githooks/*.cs` runfile runs
- [x] `<Version>` is 3.0.0-beta.3, own commit
- [x] `documentation/release-notes/3.0.0-beta.3.md` written
- [x] `dev check-version` reports 3.0.0-beta.3 unpublished
- [x] `./tools/dev-cli/dev.cs build` and `./tools/dev-cli/dev.cs test` pass
- [x] `ganda repo audit` is clean

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)
- Implementation: 01a1bf5f-566c-7a93-b87a-ed5507a865a5 (2026-11-09)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bf5f-566c-7a93-b87a-ed5507a865a5, max-turns 500, Done — 2026-11-09T08:32:00Z
- Review oracle: claude-opus-5-5 (Claude Code, headless `ganda task work`), body tw-implementation-review, effort 2, roster general, turn cap 120 — disposition clean, 2026-11-09
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session e6335acb-95d8-4e07-8568-c9d93590ad59, max-turns 120, artifacts general.md, merged.md, disposition.md — 2026-11-09T08:35:11Z

## Results

`8da7577` updates `Directory.Packages.props` only. `0bdfcad` sets `<Version>` to `3.0.0-beta.3`, adds `documentation/release-notes/3.0.0-beta.3.md`, and points the readme at that note. No public members were added. Both Unshipped files stay header-only. Nothing was tagged or published.

`ganda nuget outdated` before the edit named three packages. The versions taken:

| Package | Was | Taken | Why |
|---|---|---|---|
| TimeWarp.Ramani | 1.0.0-beta.23 | 1.0.0-beta.24 | outdated |
| TimeWarp.SourceGenerators | 1.0.0-beta.11 | 1.0.0-beta.12 | outdated |
| Microsoft.Extensions.Hosting | 10.0.1 | 10.0.14 | outdated. No stable 11.0 is listed. Newest prerelease is `11.0.0-rc.3.26522.148`. Newest stable is `10.0.14`. Test-only. |
| TimeWarp.Amuru | 2.0.0 | 3.0.0-beta.2 | deliberate self pin. The first outdated run called `2.0.0` up to date. |
| TimeWarp.Amuru.Tools | 2.0.0 | 3.0.0-beta.2 | same deliberate self pin |

After those pins, `ganda nuget outdated` reports all 21 packages up to date, including the `3.0.0-beta.2` self pins.

Ramani `v1.0.0-beta.23..v1.0.0-beta.24` leaves `source/timewarp-ramani` and `source/timewarp-ramani-generators` unchanged. Beta.24 adds the demo, the interactions package, and the class-diagram page. The machines were not edited. The task 138 command-completion `SendAsync` class passed 19 tests. The task 139 JSON-RPC `SendAsync` classes passed 20, 31, and 14. `Real Mcp Server_ Should_ Initialize And List Tools` passed in 7.26s against `Microsoft.Extensions.Hosting` `10.0.14`.

SourceGenerators beta.12 removes the retired delegation generator and TW0002 through TW0006. This repo uses TW0001 and TW0007. The Release build stayed at 0 warnings and 0 errors.

Each `.githooks/*.cs` runfile restored `TimeWarp.Amuru` `3.0.0-beta.2` and `TimeWarp.Amuru.Tools` `3.0.0-beta.2` for `net11.0` and exited 0. `GANDA_ATTEST_HOOK=0` kept post-checkout, post-commit, and post-merge from attesting. pre-push was given one task-branch destination line. The SDK pin is still `11.0.100-rc.3.26522.148`.

`./tools/dev-cli/dev.cs check-version` printed source `3.0.0-beta.3`, latest NuGet `3.0.0-beta.2`, one prerelease increment ahead, and that the source version is new. `./tools/dev-cli/dev.cs build` packed `TimeWarp.Amuru.3.0.0-beta.3.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.3.nupkg` with 0 warnings. `./tools/dev-cli/dev.cs test` reported 839 total, 838 passed, 1 skipped (`GetCommitsAheadOfDefaultBranch_Given_`, pre-existing). That suite ran on the pin commit and again after the version bump. `ganda repo audit` passed 39, failed 0, skipped 2. `ganda repo audit --fix` wrote the gitignored `.vscode/settings.json` for `vscode-window-icon` because this worktree was missing it. That file stays untracked.

### Review disposition

One round, effort 2, one `general` reviewer. Final counts: bug 0, suggestion 0, nit 0. Nothing is open, fixed, or wontfix. Disposition is **clean**. The reviewer re-ran these checks: `ganda nuget outdated` (all 21 up to date), `dotnet build` of all five `.githooks/*.cs` runfiles (they resolve `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` `3.0.0-beta.2`), `dev check-version` (3.0.0-beta.3 new), and `dev build` (0 warnings). The reviewer did not re-run `dev test`. The goal restatement matches the task, so there is no misread. Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, and `review/disposition.md`.

### How to validate

Smoke: from this worktree, run `./tools/dev-cli/dev.cs check-version` and `git diff 8da7577^..0bdfcad --stat`.

Expect: check-version prints source `3.0.0-beta.3`, latest NuGet `3.0.0-beta.2`, and that the source version is new. `8da7577` touches only `Directory.Packages.props`. `0bdfcad` touches `source/Directory.Build.props`, `readme.md`, and `documentation/release-notes/3.0.0-beta.3.md`.

## Review

- Round 1: Done

## Notes

Filed from the cockpit after the 3.0.0-beta.2 release on 2026-11-09. Steve: "create a task to update NuGets and bump version again `ganda nuget outdated`", with the githook pin move folded in from his earlier request so there is one bump PR, not two.

`ganda nuget outdated` lists `TimeWarp.Amuru 2.0.0 (up to date)` because it compares within the stable channel. The move to 3.0.0-beta.2 is a deliberate prerelease pin, not something the tool will propose. After the pin, the same command lists `3.0.0-beta.2` as up to date because that prerelease is the newest published version.
