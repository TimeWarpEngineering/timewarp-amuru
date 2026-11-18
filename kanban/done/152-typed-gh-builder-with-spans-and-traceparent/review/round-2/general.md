# Round 2 — general
**Date:** 2026-11-18
**Scope reviewed:** commit `2ff6e19` since round-1 base `751b122`. Product delta is `tools/dev-cli/dev.cs` and `tools/dev-cli/telemetry.md`.

## Summary

Round 1's M1 is fixed. `UseTelemetry` now adds `Gh.ActivitySourceName` (`public const string` `"TimeWarp.Amuru.Gh"` in `source/timewarp-amuru-tools/gh-commands/gh.cs`), and the Design region plus `tools/dev-cli/telemetry.md` name that source and its tags. Nuru 4.0.0-beta.15 `TelemetryOptionsExtractor.ApplyAdd` takes `SemanticModel.GetConstantValue` on the `Add` argument, so a const field is emitted as `AddSource("TimeWarp.Amuru.Gh")`; a non-const would be `NURU_T001` and dropped. The dev CLI project-references `timewarp-amuru-tools`, and `global using TimeWarp.Amuru` brings `Gh` into scope. The fix-pass record covers `./bin/dev build`, the dev CLI self-install, that generator line, and `ganda repo audit`. The restatement still matches the task. No new issues.

## Resolved prior

### M1 — Severity: suggestion — Status: fixed
- File: tools/dev-cli/dev.cs:36
- Description: Re-checked on this delta. `options.ActivitySources.Add(Gh.ActivitySourceName)` is the registration M1 asked for, and the two sentences that said this repo has no ActivitySource now describe `TimeWarp.Amuru.Gh`.
- Suggestion: none remaining.
- Status: fixed

## Issues

<!-- No new issues. -->
