# Round 1 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru/core/command-result.timeout.cs:32
- Description: The timed stream path (`ListenDuringTimeoutAsync`) feeds an unbounded channel with `TryWrite`. The CliWrap `ListenAsync` path it replaces uses a single-slot rendezvous channel that pushes back on the child. A consumer slower than the child now buffers all output in memory, but only when `WithTimeout` is set. That breaks the "streams … without buffering" contract on `StreamStdoutAsync`, `StreamStderrAsync`, and `StreamCombinedAsync`.
- Suggestion: Use a bounded channel (capacity 1, `FullMode = Wait`, `SingleReader = true`). Write through `PipeTarget.ToDelegate(Func<string, CancellationToken, Task>, Encoding)` with `channel.Writer.WriteAsync(…, ct)`, so CliWrap's pipe token still cancels a blocked write when the run is abandoned or killed. A slow-consumer test is optional.
- Source: general
- Disposition notes: fixed — one-slot channel, `FullMode = Wait`, `SingleReader = true`, and `WriteAsync` with CliWrap's pipe token. `StreamStdout_WithTimeout_Should_PushBackOnASlowConsumer` stalls a 4MB burst.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru/core/command-result.cs:593
- Description: `ListenWithTimeoutAsync` returns through `ListenDuringTimeoutAsync` whenever `Timeout is not null`. The code after that branch always runs with no timeout. That makes the `TimeoutSession`, the graceful/forceful `ListenAsync` arm, the `when (session.TimedOut)` catch, and the `timedOut` / `RememberTimeout` branch unreachable. The line-599 comment is also stale.
- Suggestion: Reduce the remainder to the no-timeout `ListenAsync(cancellationToken)` loop. Keep caller-cancellation propagation, the `ExitedCommandEvent` exit code, `Remember`, and the dispose swallow. Fix the comment and lower the line-cap baseline.
- Source: general
- Disposition notes: fixed — the remainder is `ListenAsync(cancellationToken)` only. Caller cancellation, `ExitedCommandEvent`, `Remember`, and the dispose swallow stay. The `command-result.cs` baseline is 1339.

## Duplicates / conflicts

- None (single reviewer).
