# Round 1 — merged findings
**Date:** 2026-11-07
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: source/timewarp-amuru-tools/nu-get/nuget-package-service.cs:430
- Description: An external registration page that returns 404 is skipped, as in the old code. The resulting partial list is now held in the process cache, so a transient page 404 is not retried later in the same process.
- Suggestion: Mark a snapshot as incomplete when a page is skipped, and do not reuse an incomplete snapshot.
- Source: general
- Disposition notes: wontfix, decided by the review oracle. The task's no-cache requirement covers the index 404 ("successful empty list") and thrown failures, and both are implemented and tested. Page-404 tolerance is pre-existing behavior. The first call in a process already returned the same partial list before this change. The main consumer is the short-lived ganda `nuget outdated` process, so the window is one CLI run. Cache freshness and revalidation on the ganda side belong to ganda task 538 (disk cache, ETag, flat-container). Revisit if a long-lived host starts using the parameterless constructor.

## Duplicates / conflicts

- None (single reviewer).
