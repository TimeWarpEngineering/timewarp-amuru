# Round 3 — general
**Date:** 2026-11-09
**Scope reviewed:** fix delta `fa56fb5..003885a`. That is `f1fa4db`, which changes `command-result.cs` (the Design line and `ProcessExitCapture` wiring), `command-result.timeout.cs` (`ProcessExitCapture(Action?)`, `TimeoutSession.StopForcefulDeadline`), one new test in `shell-builder.with-timeout.cs`, `command-execution.md`, and the beta.2 release note. It also includes the kanban record `003885a`.

## Summary

The round-2 M3 fix works for the case it targets. `ProcessExitCapture.Publish` calls `TimeoutSession.StopForcefulDeadline`, which runs `ForcefulDelaySource.CancelAfter(Infinite)` when the grace period is positive. A child that exits during grace while a slow reader lags now yields all 40 lines, exit 0, and `TimedOut` false. The new test passes in about 7s. The capture test file passes 16/16 and the modes file passes 14/14. M3 is fixed.

The stop is unconditional, though. It runs on any exit of the direct child, including an exit before the timeout fires, and in every timed mode, not only streaming. It removes the forceful deadline that bounds the run. CliWrap waits for the piping tasks after the process exits, and only the forceful token cancels them. A descendant that keeps stdout or stderr open now holds a timed command past `Timeout + TimeoutGracePeriod`, indefinitely if the descendant never exits. That is a regression in the timeout's core contract, so it is filed as a bug (M4).

Goal check: the restatement's M3 paragraph says the deadline stops "once the process has exited during a positive grace period". The original task says the "grace period elapsed" outcome and "the pipeline's shared window" are unchanged. The code goes further than both, as described above. The restatement still matches the original task, so there is no misread to record under `## Learnings`. The gap is between the code and the restatement.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/core/command-result.timeout.cs:307 (wired at source/timewarp-amuru/core/command-result.cs:393, called from `ProcessExitCapture.Publish` at command-result.timeout.cs:261)
- Description: `StopForcefulDeadline` runs on every exit of the direct child under a positive grace period, whether or not the graceful token fired and whatever the execution mode. After that, nothing bounds the run's wall-clock time except the caller token. CliWrap 3.10.5 awaits the stdout/stderr piping tasks after the process exits, and only the forceful token cancels them. A descendant that inherited the pipe therefore keeps the command running past `Timeout + TimeoutGracePeriod`.
  - Probe (scratch runfile under `/tmp`, `#:project` on the library): `Shell.Builder("sh").WithArguments("-c", "sleep 15 & echo started").WithNoValidation().WithTimeout(1s).WithTimeoutGracePeriod(1s).Build().CaptureAsync()`.
  - HEAD `003885a`: `elapsed=15.0s exit=0 timedOut=False`. The run ends only when the grandchild does. With `sleep infinity &` it never ends.
  - Pre-fix `fa56fb5`, the same probe against a `git archive` copy: `elapsed=2.1s exit=124 timedOut=True`.
  - `command-execution.md:26` still says the forceful token fires at `Timeout` plus `TimeoutGracePeriod` and kills the tree.
  - The new doc paragraph (line 50) and the Design comments say the stop happens "when the child exits during a positive grace period" and describe it for streams. The code also stops on an exit before the timeout, and in capture, run, select, and pipe.
  - The task's Results present "a descendant ... is no longer cut off by the grace timer" as intended. The original task never asked for that: it requires the grace-elapsed outcome and the shared window to be unchanged. Scripts that start a background helper (`cmd &`, daemons, `dotnet build` server nodes) are common.
- Suggestion: Keep a wall-clock bound after the child exits. Pick one:
  - (a) Do not stop the forceful deadline. Record that the child exited before it fired, for example a `ChildExited` flag or a capture that is already complete. Then classify a later `OperationCanceledException` from that deadline as the captured child code with `TimedOut` false. A reader that is still behind past the deadline loses the remaining lines, and the docs say so. The M3 test then asserts the child's code and `TimedOut` false, with a line count of at most 40.
  - (b) Stop it only when the graceful token has already fired, and replace it with a bounded drain deadline instead of `Infinite`, for example the original deadline plus another grace period. A descendant that holds the pipe still ends at a known bound. That cut-off reports the child's code and `TimedOut` false, as in (a).
  - Whichever is chosen, add a test where a timed child exits and a background descendant holds stdout. It should end near `Timeout + TimeoutGracePeriod` (or the chosen bound), not when the descendant exits. Align `command-execution.md`, the release note, both Design regions, and the task Results with the rule.
- Status: open

<!-- No other issues found in the delta. Re-verified: an already-canceled ForcefulDelaySource stays canceled under CancelAfter(Infinite), so a grace-elapsed kill is still 124. A zero grace period skips the stop. ObjectDisposedException after session dispose is swallowed. Caller and abandon links on LinkedForcefulSource are untouched. -->
