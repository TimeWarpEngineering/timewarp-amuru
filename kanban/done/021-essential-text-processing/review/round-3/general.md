# Round 3 — general
**Date:** 2026-10-06
**Scope reviewed:** commit b87a9c5 (fix delta for M12–M15), plus re-verification of M12–M15

## Summary

b87a9c5 fixes the four round-2 items. Unencodable replacement output is now an `InvalidDataException` that names the path, so the walk handles it per file; this also applies to a dry run. `RequirePaths` validates a path sequence before any read or write, for all four overloads. The docs and Design regions now cover the backup location for a followed symlink and the skipping of BOM-less UTF-16/32 files. The text test files pass 66 of 66 (bash-aliases 4, commands.replace-in-files 10, commands.select-string 12, direct.replace-in-files 23, direct.select-string 17), and each process exits 0. The fix delta adds no new issues.

## Prior findings

| ID | Status | Evidence |
|----|--------|----------|
| M12 | fixed | `TextReplace.Encode` wraps `EncoderFallbackException` as `InvalidDataException` with `ReasonKey`; new Commands test covers the surrogate split (emoji file reported with its path, other file still processed, exit 1) |
| M13 | fixed | `TextFiles.RequirePaths` materializes and validates the paths before the walk; Commands/Direct overloads call it; test `[file, " "]` leaves the file untouched |
| M14 | fixed | documented in the docs and the `text-replace.cs` Design region |
| M15 | fixed | documented in the docs and the `text-encoding.cs` Design region |

## New issues

None.
