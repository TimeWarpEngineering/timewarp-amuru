# Round 1 — general
**Date:** 2026-11-09
**Scope reviewed:** same as framework (branch vs merge-base `61ea2ee`)

## Summary

The change does what the task asks. `ExecuteCliAsync` attaches a `ProcessExitCapture` through CliWrap's `configureProcess` hook. When the graceful token has fired and the forceful deadline has not, the run reports the child's own exit code. Strict validation throws `CommandExecutionException` for a non-zero code. A grace-elapsed kill, including any zero-grace run, is still 124 and `TimedOut` true. `TtyPassthroughAsync` gets the same rule because `TimeoutSession.TimedOut` now means the grace period elapsed. The version bump, the public API roll-over, the release note, the docs, and the 138 Notes edit all match the task.

I checked the main risk, which is the capture's ordering against CliWrap disposing the process. I decompiled CliWrap 3.10.5 `Command.ExecuteAsync(ProcessStartInfo, Action<Process>?, …)`. It calls `configureProcess` after `Start()` and before its own `WaitForExitAsync` registers an `Exited` handler, so the capture's handler runs first and reads `ExitCode` before `using (process)` disposes it. The `HasExited` check in `Attach` covers a child that is already gone.

Two findings, both in the new timed-stream path. One is a backpressure regression and the other is dead code left behind in `ListenWithTimeoutAsync`.

Verification run by the reviewer:

- `shell-builder.with-timeout.cs`: 14/14 pass
- `shell-builder.with-timeout.modes.cs`: 14/14 pass
- A probe in `/tmp` breaks out of a timed `StreamStdoutAsync` after three lines from an endless chatty child. The child was killed in 361 ms, no exception escaped, and `LastOutput` stayed null.
- `ganda repo audit`: all checks pass, including the 500-line source cap
- `./tools/dev-cli/dev.cs check-version`: source `3.0.0-beta.2`, NuGet `3.0.0-beta.1`, safe to release
- Both `PublicAPI.Unshipped.txt` files are only `#nullable enable`

Restatement vs task: the restatement matches the task. The task's Requirements bullet "`<Version>` stays `3.0.0-beta.1`" is superseded by the later "Version bump to 3.0.0-beta.2 (rolled into this task)" section, and the restatement follows the later section. That is not a misread. The zero-grace = 124 rule is an implementer refinement. It is consistent with the oracle (signal and kill are one event) and documented in the Design region and `command-execution.md`.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-amuru/core/command-result.timeout.cs:32
- Description: The timed stream path, `ListenDuringTimeoutAsync`, writes every line into `Channel.CreateUnbounded` with `TryWrite`. The path it replaces, CliWrap `ListenAsync`, uses `CliWrap.Utils.Channel<T>`, a single-slot rendezvous channel. In that channel `TransmitAsync` waits until the reader takes the item, so a slow consumer slows the pipe reader and the OS pipe pushes back on the child. With the new path, a consumer that is slower than the child buffers the whole output in memory for the life of the run. This only applies when `WithTimeout` is set. `StreamStdoutAsync`, `StreamStderrAsync`, and `StreamCombinedAsync` document "streams … without buffering", and that holds only on the no-timeout path now.
- Suggestion: Use a bounded channel, for example `Channel.CreateBounded<CommandEvent>(new BoundedChannelOptions(1) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait })`. Write with the async delegate overload `PipeTarget.ToDelegate(Func<string, CancellationToken, Task>, Encoding)`: `(line, ct) => channel.Writer.WriteAsync(new StandardOutputCommandEvent(line), ct).AsTask()`. CliWrap passes its forceful/panic token to the pipe, so an abandoned or timed-out run still cancels a blocked write, and `CompleteChannelWhenDoneAsync` still closes the reader. A test where a consumer reads slowly from a chatty timed child would pin it, but it is optional.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-amuru/core/command-result.cs:593
- Description: `ListenWithTimeoutAsync` now returns early through `ListenDuringTimeoutAsync` whenever `Timeout is not null`. Below that branch `Timeout` is always null, so these lines can never run: the `TimeoutSession` (its `HasTimeout` is always false), the `ListenAsync(…, session.ForcefulToken, session.GracefulToken)` arm, the `catch (OperationCanceledException) when (session.TimedOut)` clause, and the `timedOut` / `RememberTimeout` branch after the loop. The comment on line 599 ("A caller token passed here is linked into the forceful token") also describes the timeout path that no longer exists. A later reader will think the no-timeout loop still classifies timeouts. `command-result.cs` sits under a line-cap ratchet that this task had to shrink, so the dead lines also cost headroom.
- Suggestion: Reduce the post-branch body to the no-timeout case. Call `InternalCommand!.ListenAsync(cancellationToken)` with no session. Keep the caller-cancellation catch, the `ExitedCommandEvent` exit-code capture, `Remember`, and the dispose swallow. Drop the session, the `timedOut` flag, and the timeout catch and branch. Fix the comment. Lower the baseline in `.timewarp/line-cap-baseline.json` to match.
- Status: open
