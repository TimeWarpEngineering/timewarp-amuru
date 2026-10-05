# Round 1 — merged findings
**Date:** 2026-10-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 1 |
| nit | 0 | 2 | 1 |

## Issues

### M1 — Severity: suggestion — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs (ToolSearch_Should_AcceptDetailSkipAndTake)
- Description: Search smoke depends on the public NuGet feed and English output; fails offline.
- Suggestion: Make it hermetic or drop the output assertion.
- Source: general
- Disposition notes: wontfix (orchestrator). `dotnet tool search` only queries nuget.org and has no source or config option, so a real search smoke has to use the network. This file is an SDK integration smoke, and CI (ubuntu-latest) has network access. The locale half is covered by the M2 fix.

### M2 — Severity: nit — Status: fixed
- File: dot-net.cli-smoke.cs (tool smokes)
- Description: Assertions on English SDK text break under non-English UI language.
- Suggestion: Set DOTNET_CLI_UI_LANGUAGE=en.
- Source: general
- Disposition notes: fixed. All 7 tool smokes set DOTNET_CLI_UI_LANGUAGE=en, and the Design region says why.

### M3 — Severity: nit — Status: fixed
- File: dot-net.cli-smoke.cs (WriteClearedNuGetConfig)
- Description: Source path put into XML without escaping.
- Suggestion: Escape it.
- Source: general
- Disposition notes: fixed with System.Security.SecurityElement.Escape.

### M4 — Severity: nit — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/native/file-system/direct.set-location.cs
- Description: Raw temp path compare could differ on macOS (/tmp symlink); cwd mutation relies on sequential runner.
- Suggestion: Optional real-path compare.
- Source: general
- Disposition notes: wontfix (orchestrator). CI runs only on ubuntu-latest, and the Jaribu runner runs tests sequentially. commands.set-location.cs and repo-clean-service.cs already change cwd and restore it the same way.

## Duplicates / conflicts

- None.

## Verification after fixes

- `dotnet run .../dot-net.cli-smoke.cs`: 18/18 passed. The reviewer's one NuGet Why failure (`No assets file`) did not reproduce, so it was transient and comes from code outside this diff.
