# Round 1 — merged findings
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
- File: tools/dev-cli/dev.cs:31
- Description: `workflow-command.cs` now runs `Gh.Run().List()`, `Gh.Api()`, and `Gh.Run().Download()` inside the dev CLI, but `UseTelemetry` only sets `ServiceName`. `GhTelemetry.Start` calls `ActivitySource.StartActivity` on `TimeWarp.Amuru.Gh`. That call returns null unless a listener samples the source, so those gh calls still do not export a span. Task 140 in this repo says a repo ActivitySource is registered with `options.ActivitySources.Add`. `dev.cs` (lines 16–17) and `tools/dev-cli/telemetry.md` (line 5) still say this repo has no ActivitySource.
- Suggestion: Register `Gh.ActivitySourceName` on the dev CLI `UseTelemetry` options (the const is a compile-time constant) and correct those two sentences so they name `TimeWarp.Amuru.Gh`.
- Source: general
- Disposition notes: Fixed. `dev.cs` `UseTelemetry` adds `options.ActivitySources.Add(Gh.ActivitySourceName)`; the generated tracer calls `AddSource("TimeWarp.Amuru.Gh")`. `dev.cs` Design region and `tools/dev-cli/telemetry.md` now describe the Gh source and its tags.

## Duplicates / conflicts

- Single reviewer. Nothing to collapse.
