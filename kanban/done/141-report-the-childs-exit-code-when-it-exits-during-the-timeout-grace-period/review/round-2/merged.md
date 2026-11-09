# Round 2 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

- M1 (suggestion), fixed and re-verified. The timed stream uses `Channel.CreateBounded` with capacity 1, `FullMode = Wait`, and `SingleReader = true`. Both pipe delegates call `WriteAsync` with CliWrap's pipe token. `StreamStdout_WithTimeout_Should_PushBackOnASlowConsumer` passes. A dropped enumerator unblocks a waiting write: in the probe it returned in 1.2s and no child was left behind.
- M2 (suggestion), fixed and re-verified. `ListenWithTimeoutAsync` after the timeout branch is `ListenAsync(cancellationToken)` only. Caller cancellation still propagates, the exit code still comes from `ExitedCommandEvent`, `Remember` still runs, and the dispose swallow is still there. The comment is updated. The baseline is 1339, which matches the file.

## Issues

### M3 — Severity: bug — Status: open
- File: source/timewarp-amuru/core/command-result.timeout.cs:53 (classification at source/timewarp-amuru/core/command-result.cs:418)
- Description: The M1 backpressure fix causes this. Suppose the child exits by itself during the grace period, and the stream reader is more than the grace period behind. The forceful deadline cancels the waiting pipe write, the remaining lines are dropped, and `ExecuteCliAsync` classifies the run as `GraceElapsed`, which gives exit 124 and `TimedOut` true. Strict validation would throw `TimeoutException`. That breaks the Goal's rule that a child exiting during the grace period keeps its own code with `TimedOut` false. Probe: a SIGINT handler prints 40 lines of about 1 KB each and exits 0. Settings are timeout 1s, grace 2s, and a reader waiting 150 ms per line. Result: `lines=16 exit=124 timedOut=True`. With a fast reader the result is `lines=41 exit=0 timedOut=False`.
- Suggestion: When the process exits, stop the forceful deadline timer. For example, a session hook from `ProcessExitCapture`'s `Exited` handler can call `ForcefulDelaySource.CancelAfter(Timeout.InfiniteTimeSpan)`, which keeps the caller and abandon links. The pipes then drain, and the `GracefulSignaled` branch reports the child's code. Add a test: a child that exits during grace with a slow reader expects every line, the child's code, and `TimedOut` false.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
