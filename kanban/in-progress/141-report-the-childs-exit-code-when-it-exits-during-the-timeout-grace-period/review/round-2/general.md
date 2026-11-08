# Round 2 — general
**Date:** 2026-11-09
**Scope reviewed:** the fix delta `f7aed63..fa56fb5` (commit `6ce685d`, which fixes the stream backpressure, and the kanban record `fa56fb5`). Files: `source/timewarp-amuru/core/command-result.timeout.cs`, `source/timewarp-amuru/core/command-result.cs`, `tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs`, `.timewarp/line-cap-baseline.json`. Surrounding call sites read: `ExecuteCliAsync` and `TimeoutSession` classification.

## Summary

The fix pass closes both round-1 suggestions as written. The timed stream now uses a one-slot `Wait` channel and calls `WriteAsync` with CliWrap's pipe token. `ListenWithTimeoutAsync` after the timeout branch is only the no-timeout listen loop, and the `command-result.cs` baseline is 1339, which matches `wc -l`. A dropped enumerator still unblocks a waiting write. I checked this with a scratch probe: a 100k-line child, a reader that breaks after one line, and the run returned in 1.2s with no leftover python process.

The backpressure brings a new defect. A child can exit by itself during the grace period while its output is still waiting for a slow reader. When the forceful deadline comes, it cancels that blocked write, and the run is classified as a timeout. The capture runner does not have this problem. Before this delta, the timed stream did not have it either.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/core/command-result.timeout.cs:53 (the classification is at source/timewarp-amuru/core/command-result.cs:418)
- Description: The timed stream now holds pipe writes until the reader takes a line. CliWrap's `ExecuteAsync` waits for the pipes to finish before it throws, and those pipes use the forceful token. The child can handle SIGINT and exit during the grace period with its output still in the OS pipe. If the reader is more than the grace period behind, the `timeout + grace` timer fires while a write is still waiting. That timer cancels the write, the rest of the output is dropped, and `ExecuteCliAsync` hits `catch … when (session.GraceElapsed)` first. The run then reports exit 124 with `TimedOut` true, even though the process exited with its own code inside the grace window. This breaks the Goal: "If the child exits during that wait, the run is that child's exit code and `TimedOut` is false." Under zero-exit validation the stream would throw `TimeoutException` for what was a cooperative stop. The old unbounded channel never blocked, so this case was correct before `6ce685d`.
  Reproduced with a scratch runfile under `/tmp`, outside the product tree. The child is python3. Its SIGINT handler prints 40 lines of about 1 KB each, which fits in the OS pipe, then `SystemExit(0)`. The run uses `WithTimeout(1s)`, `WithTimeoutGracePeriod(2s)`, and `WithNoValidation`, and calls `StreamStdoutAsync`. The reader waits 150 ms per line, so it needs about 6 seconds. Result: `lines=16 exit=124 timedOut=True`. With a reader that does not wait, the result is `lines=41 exit=0 timedOut=False`.
- Suggestion: Once the process has exited, the grace deadline should no longer apply. One way: `ProcessExitCapture.Attach` gets a callback from the session. On `Exited` it calls `ForcefulDelaySource.CancelAfter(Timeout.InfiniteTimeSpan)`, which stops the timer. The caller token and the abandon token stay linked into the forceful token. The pipes then drain at the reader's pace, `ExecuteAsync` throws the graceful OCE, and the `GracefulSignaled` branch reads the child's code. A second way: `GraceElapsed` could check whether `capture` published an exit before the deadline. That fixes the classification, but the tail of the output is still lost. Add a test like the probe: a child that exits during grace with a slow reader expects every line, the child's exit code, and `TimedOut` false.
- Status: open

## Checks run

- `dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs` — 15/15 passed, including `StreamStdout_WithTimeout_Should_PushBackOnASlowConsumer`.
- `wc -l source/timewarp-amuru/core/command-result.cs` — 1339, which matches the baseline.
- Scratch probes under `/tmp/amuru-r2-probe*` (not product files): the slow reader during grace reproduces the bug, and the dropped-enumerator case is fine.
