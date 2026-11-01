# Round 1 — general
**Date:** 2026-11-01
**Scope reviewed:** `4ba051b..HEAD`: `source/Directory.Build.props`, `documentation/release-notes/3.0.0.md` → `3.0.0-beta.1.md`, `AGENTS.md`, task.md

## Summary

The change sets the shared `<Version>` to `3.0.0-beta.1`, renames the release note and updates its three package-version mentions, and updates the one `AGENTS.md` sentence. It does not retarget and does not publish. Risk is very low. Goal restatement matches the task description (no misread).

Verified against the repo:

- `source/Directory.Build.props:9` is the only `<Version>` in the product tree. No csproj has a `<Version>`.
- `dotnet msbuild -getProperty` reports `Version`/`PackageVersion` = `3.0.0-beta.1` for `timewarp-amuru` and `PackageVersion` = `3.0.0-beta.1` for `timewarp-amuru-tools`. `TargetFramework` is still `net11.0`.
- Release note: the `net11.0` breaking text, SDK pin, and analyzer pins are unchanged. The githook-pin line says pins stay at `2.0.0` until `3.0.0-beta.1` is on the feed.
- Both `PublicAPI.Unshipped.txt` files are only `#nullable enable`. No `public-api/` file changed.
- `Directory.Packages.props` self pins stay at `2.0.0`. `kanban/done/**` is untouched.
- No dangling links: `readme.md` and `skills/tw-amuru/SKILL.md` point at `2.0.0.md` only. Nothing in `tools/`, `.github/`, or `source/` derives the release-note path from the version.
- No remaining `3.0.0` (non-beta) reference to the Amuru version outside `kanban/` (the `3.0.0-beta.47` hits under `.agent/workspace/` are TimeWarp.Nuru, unrelated).

`dev check-version` and the Release pack were not re-run here (offline). The implementer's Results record that output. The MSBuild evaluation above confirms the version both packs would use.

## Issues

None.
