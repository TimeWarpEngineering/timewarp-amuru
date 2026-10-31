# Round 3 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: the five char-search sites (`nuget-package-service.cs`, `file-system-walk.cs`, `glob-matcher.cs`, `text-diff.cs` ×2)
- Description: CA1307 suppression on a false overload claim.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Round 2 verified it. The round-3 delta does not touch these files.

### M2 — Severity: nit — Status: fixed
- File: tools/dev-cli/Directory.Build.props
- Description: CA1812 was in the project-wide `NoWarn`.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Round 2 verified it. The round-3 delta does not touch it.

### M3 — Severity: nit — Status: fixed
- File: documentation/user/installation.md:81, :139
- Description: The `dnx` / `#:package` history was wrong.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Round 3 confirms the sample pass left those lines as written, which is what the task asks.

### M4 — Severity: bug — Status: open
- File: readme.md:14
- Description: The shields badge still says `dotnet-10.0`. The task asks for every current reference to show .NET 11, and checklist 5 says `readme.md` was checked. This readme is the `PackageReadmeFile` for both `3.0.0` nupkgs, so the package page would advertise .NET 10 for a `net11.0`-only release.
- Suggestion: Change the badge to `https://img.shields.io/badge/dotnet-11.0-blue` and the label to `dotnet-11.0`. Add `readme.md` to the `rg` smoke in How to validate.
- Source: general (round 3)
- Disposition notes:

## Duplicates / conflicts

- None.
