# Bump both packages to 3.0.0-beta.1

## Description

Steve 2026-11-01: the unpublished package version is `3.0.0`. NuGet and the GitHub release are still `2.0.0`. Move the source version to `3.0.0-beta.1` before anyone publishes `3.0.0`. Do not publish.

Both packages share the one `<Version>` in `source/Directory.Build.props`. Do not add a `<Version>` to a csproj.

## Checklist

- [ ] `source/Directory.Build.props` `<Version>` is `3.0.0-beta.1`.
- [ ] Rename `documentation/release-notes/3.0.0.md` to `documentation/release-notes/3.0.0-beta.1.md` and say `3.0.0-beta.1` everywhere that file names the package version. Keep the `net11.0` breaking-change text. Githook pins stay at the published `2.0.0` until `3.0.0-beta.1` is on the feed.
- [ ] `AGENTS.md` names `3.0.0-beta.1` as the version that changes the target framework only. `PublicAPI.Unshipped.txt` stays header-only. The Shipped baseline stays the `2.0.0` surface.
- [ ] Leave `kanban/done/**` as the record of task 119. Leave the `Directory.Packages.props` self pins at `2.0.0`.
- [ ] `dev check-version` reports `3.0.0-beta.1` as new against NuGet `2.0.0`. Release build packs `TimeWarp.Amuru.3.0.0-beta.1.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg`.

## Session

- Created: 01a19090-91e1-7aa1-bc02-6725799fd66b (2026-11-01)
- Steve 2026-11-01: source is `3.0.0` and was never published. The version to ship next is `3.0.0-beta.1`.

## Notes

`3.0.0` landed on master in pull request 129 and has no NuGet package and no git tag. This task replaces that unpublished version. It does not retarget, and it does not run `dev release`.
