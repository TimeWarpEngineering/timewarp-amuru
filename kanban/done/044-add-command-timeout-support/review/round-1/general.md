# Round 1 — general
**Date:** 2026-10-06
**Scope reviewed:** branch task/044-add-command-timeout-support vs master

## Summary
The timeout implementation is well structured and matches the contract in task.md. I checked it against the decompiled CliWrap 3.10.5 source: `ExecuteAsync(Process, forceful, graceful)` checks both tokens after exit and before zero-exit validation, so classifying a timeout from the `OperationCanceledException` is sound. Options snapshots copy `Timeout` and `TimeoutGracePeriod` through the single `Copy()` path, and `TimeoutSession` and the registrations are disposed in the right order. `./bin/dev build` reports 0 warnings and 0 errors. All four new test runfiles pass (18, 11, 4, and 1 tests). The one real defect is a regression in the `TtyPassthroughAsync` kill callback's exception handling. The other findings are test-coverage gaps and small cleanups.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/core/command-result.cs:583
- Description: `TryKill` now catches only `InvalidOperationException` and `Win32Exception`. Master's TTY cancellation callback used a catch-all. `Process.Kill(entireProcessTree: true)` throws `AggregateException` ("termination incomplete") when it cannot signal a descendant. On Unix, `KillTree` collects a `Win32Exception` for any error other than ESRCH, such as EPERM. That happens with a typical TTY case like `sudo …`, `ssh`, or any setuid descendant. `TryKill` runs as a `CancellationToken` registration on `session.ForcefulToken`. If the forceful *timer* fires, the `AggregateException` leaves a timer-thread callback unhandled and crashes the host process. If the *caller's* token fires (with or without a timeout), the exception surfaces from the caller's `cts.Cancel()`. That is a regression against master, which swallowed every exception there. `TryInterrupt` has the same narrow catch around the P/Invoke: a `DllNotFoundException` or `EntryPointNotFoundException` from the timer callback would also crash the process.
- Suggestion: In `TryKill` and `TryInterrupt`, catch every exception, as master did (with a CA1031 suppression and a justification: the callback runs on a timer thread and must never throw). Or add `AggregateException`, `NotSupportedException`, and the interop exceptions to the catch list. Optionally add a test that runs `TtyPassthroughAsync` on a child that ignores SIGINT, so the forceful-callback path is exercised (see Issue 3).
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-amuru/core/command-result.cs:123
- Description: The doc for `LastOutput` says it holds the "most recent execution on this instance that ran to completion". Several completions still leave it stale: a strict non-zero exit (CliWrap throws `CommandExecutionException` from `ExecuteCliAsync`, so `FinishCli` never runs), a mock configured with `Throws`, the graceful-degradation `catch` in `SelectAsync`, and a stream the consumer abandons early. A caller who reuses a `CommandResult` and reads `LastOutput` after catching `CommandExecutionException` gets the previous run's output with nothing to signal that it is stale. Concurrent executions on one instance also race on the property; last writer wins, which is benign but not documented.
- Suggestion: Either narrow the doc to state when it is set: a reported result or a timeout, and not on any exception other than `TimeoutException`. Or clear `LastOutput` (set it to null) when an execution starts, so a stale value cannot be confused with the current run. Mention that the instance is not meant for concurrent executions.
- Status: open

### Issue 3 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs:281
- Description: Coverage gaps for the paths with the most custom logic. TTY is only tested with a child that handles SIGINT and exits. Three branches have no test: a TTY child that ignores SIGINT (forceful callback plus `WaitForExitAfterKillAsync` in the `catch … when (session.TimedOut)` branch, lines 893-897), TTY caller cancellation, and caller cancellation during a stream (`ListenWithTimeoutAsync` `CallerCancelled` branch, line 522). `StreamCombinedAsync` and strict validation on a `Pipe` timeout are also untested.
- Suggestion: Add `TtyPassthrough_IgnoredSigInt_Should_BeForceKilledAfterGrace`, a TTY caller-cancel test, and a `StreamStdoutAsync(token)` caller-cancel test that asserts `OperationCanceledException` and that `LastOutput` stays null.
- Status: open

### Issue 4 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.timeout.cs:26
- Description: `BaseBuilder_Should_RunWithinTheTimeout` only proves that a 30-second timeout does not break `dotnet --version`. It would still pass if `DotNetBuilder.WithTimeout` were a no-op, so it does not verify that the timeout reaches `CommandResult`.
- Suggestion: Add a mock-based case: `CommandMock.Setup("dotnet", "--version").Delays(TimeSpan.FromSeconds(5))` with `DotNet.Builder().WithArguments("--version").WithTimeout(TimeSpan.FromMilliseconds(100))`. Assert `TimedOut` is true and the exit code is 124. That only passes if the timeout reached the result, and it needs no real child process.
- Status: open

### Issue 5 — Severity: nit
- File: source/timewarp-amuru/core/command-result.cs:449
- Description: In `WaitForMockTimeoutAsync`, `try { await Task.Delay(...) } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }` does nothing; `Task.Delay` alone has the same behavior. Likewise, `catch (TimeoutException) { throw; }` in `SelectAsync` (line 966) is dead code, because `ExecuteCliAsync` never throws `TimeoutException`; only `FinishCli`, which runs after the try, does. Also, the timed-out branch of the `FinishCli(…, IReadOnlyList<OutputLine>, …)` overload (lines 379-392) repeats `RememberTimeout`, and the stopwatch in `ExecuteCliAsync` is unused on the success path.
- Suggestion: Remove the no-op try/catch and the dead catch, or add a comment if the catch is meant as a guard for later changes. Consider giving `RememberTimeout` a `CommandOutput` parameter so both overloads share it.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-amuru/core/command-result.cs:300
- Description: `DescribeTimeout` produces "Command timed out after 1 seconds and was terminated." for whole-second values ("1 seconds"). It is a minor wording issue in a user-facing exception message.
- Suggestion: Handle the singular, or format it as a `TimeSpan` (for example `after 00:00:01`). Change the docs and the mock test only if they assert the exact text (`ConfiguredTimeout_Should_NameTheDuration`).
- Status: open

### Issue 7 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs:23
- Description: The graceful-path tests assume `python3` exists and installs its SIGINT handler before the 1-second timeout. If it does not (missing interpreter, or a heavily loaded CI host), the child dies from `KeyboardInterrupt`. The test then fails on `ShouldContain("GRACEFUL")` instead of showing up as an environment problem. The risk is low because Python starts in tens of milliseconds, but the failure would be confusing.
- Suggestion: Optionally have the script print `READY` after `signal.signal(...)`, and skip with a message when `python3` cannot be resolved.
- Status: open
