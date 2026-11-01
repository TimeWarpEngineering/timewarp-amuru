# Round 4 — general
**Date:** 2026-10-31
**Scope reviewed:** delta `445f7ad..52a3bfb`. The product change is commit `ea2155d` (`readme.md:14`, one line). The kitchen commit `52a3bfb` was read only to check its claims.

## Summary

The fix pass closes round-3 M4. `readme.md:14` now uses `https://img.shields.io/badge/dotnet-11.0-blue` with the label `dotnet-11.0`. Both `timewarp-amuru.csproj:29` and `timewarp-amuru-tools.csproj:32` pack `$(RepositoryRoot)readme.md` as `PackageReadmeFile`, so the `3.0.0` package pages now say .NET 11. The How to validate `rg` smoke now covers `readme.md` and matches `dotnet-10\.0`. I ran it, and it exits 1 with no output.

A sweep for `dotnet-10`, `badge/dotnet`, and `.NET 10` outside `kanban/`, oracle logs, and journals finds only `documentation/user/installation.md:81` and `:139`. Those are the `#:package` and `dnx` history lines that the task says to keep (round-1 M3). No other current reference to .NET 10 is left.

The change is Markdown only, so no build or test re-run was needed for this delta. The round-3 Release `-warnaserror` build and the multi-file runner (732 passed, 1 skipped) still stand.

Goal check: the updated `## Goal (implementer restatement)` now says the enumerated sample list was not the whole of "every current reference", and it names the badge. That matches Steve's open work. The round-3 misread is already in `## Learnings`. There is no new misread.

## Issues

None.
