# Round 4 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: the five char-search sites (`nuget-package-service.cs`, `file-system-walk.cs`, `glob-matcher.cs`, `text-diff.cs` ×2)
- Description: CA1307 suppression on a false overload claim.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Round 2 verified it. The round-4 delta does not touch these files.

### M2 — Severity: nit — Status: fixed
- File: tools/dev-cli/Directory.Build.props
- Description: CA1812 was in the project-wide `NoWarn`.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Round 2 verified it. The round-4 delta does not touch it.

### M3 — Severity: nit — Status: fixed
- File: documentation/user/installation.md:81, :139
- Description: The `dnx` / `#:package` history was wrong.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Round 4 confirms those lines are unchanged and are the only `.NET 10` hits left outside the kitchen.

### M4 — Severity: bug — Status: fixed
- File: readme.md:14
- Description: The shields badge said `dotnet-10.0`, and both `3.0.0` nupkgs pack this readme.
- Source: general (round 3)
- Disposition notes: fixed in `ea2155d`. The badge is now `https://img.shields.io/badge/dotnet-11.0-blue`. The How to validate `rg` smoke includes `readme.md` and `dotnet-10\.0`. Round 4 ran it: exit 1, no output.

## Duplicates / conflicts

- None. Round 4 raised no new findings.
