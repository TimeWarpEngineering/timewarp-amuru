# Round 2 — merged findings
**Date:** 2026-11-18
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.build.cs:208
- Description: (round 1) Build builder seemed to lack `WithNoLogo()`, which blocked the runfile-coverage build shape.
- Suggestion: (round 1) Add the member, the API entry, a test, and docs.
- Source: general
- Disposition notes: The member was already shipped (`public-api/PublicAPI.Shipped.txt:222`). Fix pass `fff135d` documented it in `dot-net.md` and added a command-string test locking `dotnet build entry.cs --artifacts-path artifacts/owned --verbosity minimal --nologo`. Verified against the builder's argument order.

## Duplicates / conflicts

- None (single reviewer). No new findings on the fix delta.
