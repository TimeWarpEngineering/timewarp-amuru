# Round 2 — merged findings
**Date:** 2026-09-30
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-tools/repo/RepoCleanService.cs:72
- Description: `CleanLocalFeedAsync` walked with `SearchOption.AllDirectories` and followed directory symlinks. The tracked-file guard did not see the outside target as tracked, so the delete removed it.
- Suggestion: Top-directory walk; do not descend into reparse points.
- Source: general
- Disposition notes: Fixed on this task. `DeleteLocalFeedEntriesAsync` enumerates only the current directory and skips reparse points, including when `artifacts/packages` itself is a reparse point. Regression coverage is `CleanLocalFeedAsync_ShouldDeleteNupkgsAndSkipTrackedAndReparse` (outside nupkg and `timewarp.amuru` behind `linked-tree` survive). `repo-clean-service.cs`: 5 passed.

## Resolved prior

- M1 carried from round 1. No new IDs.

## Duplicates / conflicts

- Single reviewer. No overlap.
