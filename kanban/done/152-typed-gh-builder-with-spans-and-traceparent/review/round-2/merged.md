# Round 2 — merged findings
**Date:** 2026-11-18
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

### M1 — Severity: suggestion — Status: fixed
- File: tools/dev-cli/dev.cs:36
- Description: `UseTelemetry` adds `Gh.ActivitySourceName`. That const is `"TimeWarp.Amuru.Gh"`, and Nuru 4.0.0-beta.15 folds it with `GetConstantValue` into `AddSource("TimeWarp.Amuru.Gh")`. `dev.cs` and `tools/dev-cli/telemetry.md` name the source and its tags.
- Suggestion: none remaining.
- Source: general
- Disposition notes: Fixed on the round-1 fix pass (`2ff6e19`). Re-verified on this delta. No new defect.

## Issues

<!-- No new issues. -->

## Duplicates / conflicts

- Single reviewer. Nothing to collapse.
