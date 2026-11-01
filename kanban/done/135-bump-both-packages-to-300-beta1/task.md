# Bump both packages to 3.0.0-beta.1

## Description

Steve 2026-11-01: the unpublished package version is `3.0.0`. NuGet and the GitHub release are still `2.0.0`. Move the source version to `3.0.0-beta.1` before anyone publishes `3.0.0`. Do not publish.

Both packages share the one `<Version>` in `source/Directory.Build.props`. Do not add a `<Version>` to a csproj.

## Goal (implementer restatement)

Source still says `3.0.0`, and that version was never published. NuGet and the GitHub release are still `2.0.0`. This task replaces that unpublished version with `3.0.0-beta.1` so the next package anyone can publish is the beta, and it does not publish and does not retarget.

Acceptance:

- The only `<Version>` stays in `source/Directory.Build.props`, and its value is `3.0.0-beta.1`. No csproj gains a `<Version>`.
- `documentation/release-notes/3.0.0.md` becomes `documentation/release-notes/3.0.0-beta.1.md`. Every place that file names the package version says `3.0.0-beta.1`. The `net11.0` breaking-change text stays. The note still says the githook pins remain at the published `2.0.0` until `3.0.0-beta.1` is on the feed.
- `AGENTS.md` names `3.0.0-beta.1` as the version that changes the target framework only. Both `PublicAPI.Unshipped.txt` files stay header-only. The Shipped files stay the `2.0.0` surface.
- `kanban/done/**` stays the record of task 119. `Directory.Packages.props` self pins stay at `2.0.0`.
- `dev check-version` reports `3.0.0-beta.1` as new against NuGet `2.0.0`. A Release build packs `TimeWarp.Amuru.3.0.0-beta.1.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg`.

## Checklist

- [x] `source/Directory.Build.props` `<Version>` is `3.0.0-beta.1`.
- [x] Rename `documentation/release-notes/3.0.0.md` to `documentation/release-notes/3.0.0-beta.1.md` and say `3.0.0-beta.1` everywhere that file names the package version. Keep the `net11.0` breaking-change text. Githook pins stay at the published `2.0.0` until `3.0.0-beta.1` is on the feed.
- [x] `AGENTS.md` names `3.0.0-beta.1` as the version that changes the target framework only. `PublicAPI.Unshipped.txt` stays header-only. The Shipped baseline stays the `2.0.0` surface.
- [x] Leave `kanban/done/**` as the record of task 119. Leave the `Directory.Packages.props` self pins at `2.0.0`.
- [x] `dev check-version` reports `3.0.0-beta.1` as new against NuGet `2.0.0`. Release build packs `TimeWarp.Amuru.3.0.0-beta.1.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg`.

## Session

- Created: 01a19090-91e1-7aa1-bc02-6725799fd66b (2026-11-01)
- Steve 2026-11-01: source is `3.0.0` and was never published. The version to ship next is `3.0.0-beta.1`.
- Implementation: 01a195dc-c1b2-7cb3-86dc-e636c5a4bebc (2026-11-01)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a195dc-c1b2-7cb3-86dc-e636c5a4bebc, max-turns 500, Done — 2026-11-01T06:51:39Z
- Review oracle: general reviewer (claude, model claude-opus-5-5), effort 1, turn cap 80, round 1, disposition clean — 2026-11-01
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 8b9b531f-446f-4be0-a41e-99f58bc1d099, max-turns 80 — 2026-11-01T06:53:43Z

## Results

`<Version>` in `source/Directory.Build.props` is `3.0.0-beta.1`. No csproj has a `<Version>`. The release note is `documentation/release-notes/3.0.0-beta.1.md`. Its package-version lines say `3.0.0-beta.1`. The `net11.0` break, the SDK pin, and the analyzer pins are unchanged. The note still says the githook pins stay at published `2.0.0` until `3.0.0-beta.1` is on the feed. `AGENTS.md` names `3.0.0-beta.1` as the target-framework-only version. Both `PublicAPI.Unshipped.txt` files are still only `#nullable enable`. Shipped was not edited. `kanban/done/**` was not edited. `Directory.Packages.props` still pins `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` at `2.0.0`. This walk did not publish and did not run `dev release`.

`./bin/dev check-version` printed `Version in source: 3.0.0-beta.1`, `Latest NuGet version: 2.0.0`, and `Version in source is new — safe to release.` `dotnet build timewarp-amuru.slnx -c Release -warnaserror` finished with 0 warnings and 0 errors and wrote both `3.0.0-beta.1` nupkgs. Each nuspec version is `3.0.0-beta.1` and each package contains `lib/net11.0`. `TimeWarp.Amuru.Tools` depends on `TimeWarp.Amuru` `3.0.0-beta.1`. Older `TimeWarp.Amuru.3.0.0.nupkg` and `TimeWarp.Amuru.Tools.3.0.0.nupkg` files are still in local `artifacts/packages` from the previous pack. They are not the source version.

### How to validate

Smoke:

```bash
./bin/dev check-version
dotnet build timewarp-amuru.slnx -c Release -warnaserror
unzip -p artifacts/packages/TimeWarp.Amuru.3.0.0-beta.1.nupkg TimeWarp.Amuru.nuspec
unzip -p artifacts/packages/TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg TimeWarp.Amuru.Tools.nuspec
```

Expect: `check-version` reports source `3.0.0-beta.1` as new against NuGet `2.0.0`. The Release build reports 0 warnings and 0 errors and creates `artifacts/packages/TimeWarp.Amuru.3.0.0-beta.1.nupkg` and `artifacts/packages/TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg`. Both nuspec `<version>` values are `3.0.0-beta.1`, and both packages list `lib/net11.0`. This walk ran those commands and saw that output.

### Review disposition

- Rounds: 1. Roster: general only, effort 1.
- Final counts: bug 0, suggestion 0, nit 0 (0 open, 0 fixed, 0 wontfix).
- Disposition: `clean`. No wontfix and no escalation. The goal restatement matches the task, so there was no misread.
- The reviewer confirmed with `dotnet msbuild -getProperty` that both packable projects evaluate `PackageVersion` to `3.0.0-beta.1` on `net11.0`. `dev check-version` and the pack were not re-run during review.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Review

- Round 1: Done

## Notes

`3.0.0` landed on master in pull request 129 and has no NuGet package and no git tag. This task replaces that unpublished version. It does not retarget, and it does not run `dev release`.
