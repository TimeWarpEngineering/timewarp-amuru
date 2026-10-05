# Round 1 — general
**Date:** 2026-10-05
**Scope reviewed:** same as framework

## Summary

Single `<Version>` bump to `2.0.0-beta.1`. PublicAPI promotion is correct: core Unshipped held 3 additions, now in Shipped. Tools Unshipped held 131 additions and 2 `*REMOVED*` lines (`DotNetListPackagesBuilder.WithConfig`, `DotNetWatchBuilder.WithTargetFramework`). The matching Shipped lines were deleted, both Unshipped files are header-only, and no `*REMOVED*` remains. Release-note claims spot-checked against source: `WithCollect(string)`, the DevCerts `WithExportPath` throw, the NuGet delete `--configfile` removal with `WithNonInteractive`, and the `GetCommitsAheadAsync` default. All match. All five breaking PRs merged after tag `v1.1.1`, so listing them as 2.0 changes is correct. Risk is low; both findings are doc-only.

## Issues

### Issue 1 — Severity: suggestion
- File: readme.md:49
- Description: The diff drops `--prerelease` from the Tools install line, so both install commands now resolve to stable 1.1.1. The readme documents the 2.0 surface (`GetDefaultWorktreePathAsync`, `WithConfigFile`, …), and the release notes say to install with `--prerelease`. A reader following the readme gets an API that does not match the docs.
- Suggestion: Keep the plain install commands, which are correct after 2.0.0 GA. Add a line saying the readme documents 2.0 and pointing to `--prerelease` and the release notes until GA.
- Status: open

### Issue 2 — Severity: nit
- File: documentation/release-notes/2.0.0.md:27
- Description: "`WithExport()` without `WithExportPath` throws" (and the matching upgrade-guide line) does not name the owning type. Every other entry is type-qualified.
- Suggestion: Qualify as `DotNetDevCertsHttpsBuilder.WithExport()`.
- Status: open
