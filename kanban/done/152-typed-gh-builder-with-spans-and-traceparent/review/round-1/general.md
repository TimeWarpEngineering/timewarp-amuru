# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/152-typed-gh-builder-with-spans-and-traceparent` vs merge-base `ab35e18` (product commit `461c9d5`)

## Summary

The `Gh` builders match the task: one file per verb, the shared option plumbing, `WithArgument`, the listed commands, safe span tags, typed results for run list, auth status, and pr view, Jaribu coverage, the readme, the `3.0.0-beta.9` bump, and AMURU0001 rows that require the sub-verb in the second argument. Spans are started before `Build()`/`Shell.Run`, so task 149 reads `Activity.Current` at process start and the child `TRACEPARENT` names the gh span. Tags stay on the fixed set (`gh.command`, `gh.repo`, path-only `gh.api.endpoint`, `gh.exit_code`, `gh.elapsed_ms`); the token, `-f`/`-F` values, bodies, and stdout are not tagged. `gh auth status --json hosts` omits the token unless `--show-token` is passed, and `GetStatusAsync` does not pass it. Recorded gates cover the build, the three gh runfiles, both analyzer runfiles, the dev CLI self-install, and the audit. The restatement matches the task.

## Issues

### Issue 1 — Severity: suggestion
- File: tools/dev-cli/dev.cs:31
- Description: `workflow-command.cs` now runs `Gh.Run().List()`, `Gh.Api()`, and `Gh.Run().Download()` inside the dev CLI, but `UseTelemetry` only sets `ServiceName`. `GhTelemetry.Start` calls `ActivitySource.StartActivity` on `TimeWarp.Amuru.Gh`. That call returns null unless a listener samples the source, so those gh calls still do not export a span. Task 140 in this repo says a repo ActivitySource is registered with `options.ActivitySources.Add`. `dev.cs` (lines 16–17) and `tools/dev-cli/telemetry.md` (line 5) still say this repo has no ActivitySource.
- Suggestion: Register `Gh.ActivitySourceName` on the dev CLI `UseTelemetry` options (the const is a compile-time constant) and correct those two sentences so they name `TimeWarp.Amuru.Gh`.
- Status: open
