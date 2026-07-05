# Unify ExecutionResult and CommandOutput result types

## Description

Two competing result types model the same concept with clashing member names: `core/execution-result.cs` (`ExecutionResult.StandardOutput`/`IsSuccess`, returned by `PassthroughAsync`/`TtyPassthroughAsync`) vs `core/command-output.cs` (`CommandOutput.Stdout`/`Success`, returned by `CaptureAsync`/`RunAndCaptureAsync`). Consolidating after 1.0 is breaking; do it now.

**BLOCKER for 1.0.**

## Checklist

- [ ] Decide: merge into one result type, or keep two but align member naming (`Stdout` vs `StandardOutput`, `Success` vs `IsSuccess`)
- [ ] `ExecutionResult.WriteToConsole` — keep or fold into the unified type. Facts (2026-07-05): zero callers in ANY repo (checked all local TimeWarp worktrees incl. ganda), but the `TimeWarp.Terminal` dependency stays regardless (core uses `TimeWarpTerminal.Default` for TTY streams/mock echo/RunAsync pipes; `System.Console` is banned stack-wide), so keeping it costs nothing — decide on API-shape grounds only
- [ ] Also align timestamp types while touching these: `OutputLine.Timestamp` is `DateTime` while `ExecutionResult` uses `DateTimeOffset` (`core/output-line.cs:35`)
- [ ] Update all call sites, tests, samples, and docs
- [ ] Public-API diff review after the change

## Notes

Found by multi-agent release review (2026-07-04). Paths relative to `source/timewarp-amuru/`.
