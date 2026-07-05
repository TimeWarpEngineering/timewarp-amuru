# Unify ExecutionResult and CommandOutput result types

## Description

Two competing result types model the same concept with clashing member names: `core/execution-result.cs` (`ExecutionResult.StandardOutput`/`IsSuccess`, returned by `PassthroughAsync`/`TtyPassthroughAsync`) vs `core/command-output.cs` (`CommandOutput.Stdout`/`Success`, returned by `CaptureAsync`/`RunAndCaptureAsync`). Consolidating after 1.0 is breaking; do it now.

**BLOCKER for 1.0.**

## Checklist

- [x] DECIDED AND DONE (2026-07-05): merged into ONE result type. `ExecutionResult` deleted; `PassthroughAsync`/`TtyPassthroughAsync` (and all builder wrappers) return `CommandOutput`. `CommandOutput` gained `RunTime` plus the formatting members (`ToString`, `ToSummary`, `ToDetailedString`, `WriteToConsole` — now printing real captured output)
- [x] `WriteToConsole` kept — folded into `CommandOutput`, where it prints real captured output (on `ExecutionResult` it only ever saw empty strings)
- [x] `OutputLine` converted to a sealed record with `DateTimeOffset Timestamp` (value equality; no callers used the old 3-arg ctor)
- [x] All call sites updated (shell-builder, every dot-net builder wrapper, passthrough/tty tests); readme's `ExecutionResult` mention rides with task 102
- [x] Verified: `ExecutionResult` gone from the surface; suite 364/365 green

## Notes

Found by multi-agent release review (2026-07-04). Paths relative to `source/timewarp-amuru/`.
