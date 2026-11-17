# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/149-propagate-traceparent-to-child-processes-from` vs `master` (merge-base `5c514e59`). `TraceContextPropagation`, every `CommandResult` launch path, `TtyPassthroughAsync`, the JSON-RPC child, the Jaribu runfile, docs, and the `3.0.0-beta.6` bump.

## Summary

Amuru writes `TRACEPARENT` (`00-{traceId}-{spanId}-{01|00}`) and, when `TraceStateString` is non-empty, `TRACESTATE` from `Activity.Current` at the moment the child starts. Caller keys already on the command, including an explicit null, are left alone, so an opt-out wins and a missing activity leaves an inherited header in place. The header is not written to logs: `Command.ToString()` is only the file and arguments. Recorded `./bin/dev build`, `./bin/dev test` (890 total, 889 passed, 1 skipped, `TraceContext_Given_` 11/11), and `ganda repo audit` after the pin and exec-bit fix cover this diff.

The restatement matches the task. The task names `CommandOptions.ApplyTo` and also requires the value at start time, and it says to record the choice. The restatement takes start time. That is the choice the task asks for, not a misread. `ApplyTo` still only stores the caller's dictionary, which is how a null removal stays visible. CliWrap 3.10.5 (`Command.ExecuteAsync`) removes a null environment entry and runs a piped upstream command with that command's own environment, so rebuilding each `Pipe` stage reaches the hidden upstream process as well as the outer stage. JSON-RPC captures inside `StartChild` at `StartAsync`, which is the same start-time rule. No issues.

## Issues
