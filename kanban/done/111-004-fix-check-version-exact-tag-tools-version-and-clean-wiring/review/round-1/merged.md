# Round 1 — merged findings
**Date:** 2026-09-30
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-amuru-tools/repo/RepoCleanService.cs:71
- Description: `CleanLocalFeedAsync` uses `SearchOption.AllDirectories`, which follows directory symlinks. The per-path reparse guard does not stop the walk. `git ls-files` on a path through the link exits 0 with no output, so the tracked-file guard does not save the target. Deleting the enumerated path removes an outside `TimeWarp.Amuru.*.nupkg` or package-id directory. Breaks the cleaner invariant that enumeration never follows reparse points.
- Suggestion: Top-directory walk only. Skip reparse points instead of descending. Skip the feed root when it is a reparse point. Keep the existing delete guards.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. No overlap.
