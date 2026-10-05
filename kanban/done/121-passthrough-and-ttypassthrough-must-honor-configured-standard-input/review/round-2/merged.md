# Round 2 — merged findings
**Date:** 2026-10-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru/core/command-result.cs
- Description: Passthrough modes ignored pipe-sourced stdin (from round 1).
- Source: general
- Disposition notes: Verified fixed. IsPipeline guard in PassthroughAsync and TtyPassthroughAsync, with tests.

## Duplicates / conflicts

- None.
