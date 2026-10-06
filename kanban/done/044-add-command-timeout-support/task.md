# Add command timeout support

## Description

Add a configurable per-command timeout to TimeWarp.Amuru so a hung child process cannot stall a runfile, hook, or dev-cli forever. Today the only control is a caller-supplied `CancellationToken` on each execution method; there is no way to say "this command may take at most N seconds" on the builder or in `CommandOptions`, and a cancelled command surfaces as `OperationCanceledException` rather than as a result.

**Rewritten 2026-10-06.** The original card predates the Amuru rename (`TimeWarp.Cli`, `GetStringAsync`, `CLAUDE.md`, ADRs). Current facts: `CommandOptions` (`source/timewarp-amuru/core/command-options.cs`) holds working directory, environment, and validation and is applied to CliWrap in `ApplyTo`. Every execution method on `CommandResult` (`core/command-result.cs`) takes a `CancellationToken` and calls CliWrap `ExecuteAsync(cancellationToken)`. Default validation is `None` (task 090): failures are reported through `CommandOutput.ExitCode` / `Success`, not thrown. 2.0 is in beta, so additive API is fine and goes in `public-api/PublicAPI.Unshipped.txt`.

## Design decisions

- **Where it lives:** `CommandOptions.Timeout` (`TimeSpan?`, null = none) plus `CommandOptions.WithTimeout(TimeSpan)`. Fluent `WithTimeout(TimeSpan)` on `ShellBuilder` and on `DotNetBuilder` (base), so every dotnet builder that inherits the base gets it. Tools leaf builders that take a `CommandOptions` snapshot already receive it through the options; do **not** roll a `WithTimeout` method across every Tools builder in this task (that is the task 100 pattern and can follow if wanted). `FzfBuilder`: through its `CommandOptions` as well.
- **Semantics on expiry:** link the timeout to the caller's token into one `CancellationTokenSource`. Use CliWrap's two-token overload `ExecuteAsync(forcefulCancellationToken, gracefulCancellationToken)`: on timeout send the graceful signal first (SIGINT / Ctrl+C), then force-kill after a grace period (`CommandOptions.TimeoutGracePeriod`, default 5 s). Record how CliWrap 3.10.5 exposes this and verify both branches with a sleeping child.
- **Result contract (follows 090):** a timeout is a *failure result*, not an exception, under default validation. Add `CommandOutput.TimedOut` (bool) and surface `ExitCode = 124` (GNU `timeout`'s code) when the process was killed by the timeout. `Success` stays `ExitCode == 0`, so it is false. Under `WithZeroExitCodeValidation()` a timeout throws, and the exception must say it was a timeout (a `TimeoutException`, or `CommandExecutionException` with a timeout message; pick one and document it). A cancellation that comes from the **caller's** token still propagates as `OperationCanceledException`: the caller asked for it.
- **Applies to every execution mode:** `RunAsync`, `CaptureAsync`, `RunAndCaptureAsync`, `StreamStdoutAsync` / `StreamStderrAsync` (the enumerator ends and the final state is observable), `SelectAsync`, `PassthroughAsync`, `TtyPassthroughAsync` (the `Process`-based path must honor it too: kill the process on expiry), and `Pipe` stages (each stage shares the one timeout window).
- **Mocks:** `CommandMock` honors `Timeout`: a mock setup gains `.TimesOut()` (or the existing delay hook at `testing/mock-setup.cs:87` is used) so tests can assert `TimedOut` without a real child. Real and mock produce the same `CommandOutput` shape.
- **No global/static default.** Keep it per-command via options; a global knob was in the old card and is dropped (hidden coupling across runfiles).

## Checklist

- [x] `CommandOptions`: `Timeout`, `TimeoutGracePeriod`, `WithTimeout(TimeSpan)`, `WithTimeoutGracePeriod(TimeSpan)`; copied by the snapshot path builders use
- [x] `ShellBuilder.WithTimeout` and `DotNetBuilder.WithTimeout`
- [x] Execution core: linked token source, graceful-then-forceful kill via CliWrap's two-token `ExecuteAsync`, `TimedOut` + exit 124 result, strict-validation throw with a timeout message, caller cancellation still propagates
- [x] `TtyPassthroughAsync` `Process` path honors the timeout
- [x] `Pipe` stages share the window
- [x] `CommandOutput.TimedOut`
- [x] `CommandMock` timeout simulation; mock/real parity test
- [x] Tests (`tests/timewarp-amuru/single-file-tests/core/`): default validation → `TimedOut` true, exit 124, no throw; strict validation → throws with timeout message; caller token cancel → `OperationCanceledException`, `TimedOut` false; graceful signal observed before force kill (child traps SIGINT and exits 0 within grace → not force-killed, `TimedOut` true); child ignores SIGINT → force-killed after grace; streaming mode ends cleanly; passthrough and tty paths; pipe stage; no timeout set → behavior unchanged (existing suite green)
- [x] Cross-platform note: Windows has no SIGINT for non-console children; document that graceful falls through to the forceful kill there and test what is testable on CI (ubuntu)
- [x] PublicAPI Unshipped updated for every new member; XML docs; `./bin/dev build` 0 warnings / 0 errors
- [x] Docs: `documentation/` core execution page gets a Timeout section with the result contract; `readme.md` one-liner; `skills/amuru/SKILL.md` mention
- [x] Full runner green; `ganda repo audit` clean
- [x] Results: API summary, the CliWrap overload used, exit-code/exception contract, test list

## Notes

- GNU `timeout` exits 124 when the command times out; reuse that code so shell-minded callers recognize it.
- The existing per-call `CancellationToken` parameters stay; `Timeout` composes with them.
- Reference for result-vs-throw philosophy: task 090 Results; for mock parity: tasks 089 and 121.

## Results

Per-command timeout lives on `CommandOptions` and is copied onto `CommandResult`. There is no static default. `Timeout` null means no limit. `TimeoutGracePeriod` defaults to `CommandOptions.DefaultTimeoutGracePeriod` (5 seconds).

**API**

- `CommandOptions.Timeout`, `TimeoutGracePeriod`, `WithTimeout(TimeSpan)`, `WithTimeoutGracePeriod(TimeSpan)`, `DefaultTimeoutGracePeriod`
- `ShellBuilder.WithTimeout`, `ShellBuilder.WithTimeoutGracePeriod`
- `DotNetBuilder.WithTimeout`, `DotNetBuilder.WithTimeoutGracePeriod` (leaf tool builders are unchanged; a child that already receives a `CommandOptions` snapshot keeps a timeout stored on that snapshot)
- `CommandOutput.TimedOut`
- `CommandResult.TimeoutExitCode` = 124, `CommandResult.LastOutput` (set when an execution finishes, including a timeout; left unchanged when the caller cancels)
- `MockSetup.TimesOut()`

**CliWrap 3.10.5**

`Command.ExecuteAsync(CancellationToken forcefulCancellationToken, CancellationToken gracefulCancellationToken)`. The graceful token fires at `Timeout` and CliWrap calls `TryInterrupt` (SIGINT on Linux and macOS, Ctrl+C via `WindowsSignaler` on Windows). The forceful token fires at `Timeout` plus `TimeoutGracePeriod`, or immediately when the caller's token cancels, and CliWrap kills the process tree. Streaming uses `ListenAsync(Encoding, Encoding, forceful, graceful)`, which is the same pair of tokens. After the child exits, CliWrap still throws `OperationCanceledException` when either token is cancelled, including when the child trapped SIGINT and exited 0. Amuru catches that and records the timeout result. Disposing `ListenAsync` awaits the command task again and can throw that same cancellation; that dispose exception is swallowed so the result already classified by the loop stands.

`TtyPassthroughAsync` does not use CliWrap. On Linux and macOS it sends SIGINT with `kill(pid, 2)`, then `Process.Kill(entireProcessTree: true)` after the grace period. Windows non-console children get no SIGINT, so the grace period ends in the kill.

`Pipe` keeps one window: the shorter timeout, and the grace period from that shorter side (equal timeouts keep the smaller grace). CliWrap applies the two tokens to the last stage. Earlier stages are that stage's stdin command and are cancelled with the forceful token when the pipe stops.

**Result contract**

| Outcome | Caller sees |
| --- | --- |
| Timeout, default validation | `TimedOut` true, exit 124, `Success` false, no throw. The child's own exit status is not preserved |
| Timeout, `WithZeroExitCodeValidation()` | `TimeoutException` (`Command timed out after {seconds} seconds and was terminated.`). `LastOutput` is set first |
| `TimesOut()` with no timeout configured, strict | `TimeoutException`: `Command timed out and was terminated.` |
| Caller cancels | `OperationCanceledException`. `LastOutput` unchanged |
| `TtyPassthroughAsync` timeout | Exit 124 result, including under strict validation. This method does not throw |

**Tests**

- `tests/timewarp-amuru/single-file-tests/core/command-options.timeout.cs`
- `tests/timewarp-amuru/single-file-tests/core/command-mock.timeout.cs`
- `tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs` (capture, strict, caller cancel, SIGINT-then-exit-0, ignored SIGINT, stdout and stderr streams, passthrough, tty, pipe, select, run, run-and-capture, stream-to-file)
- `tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.timeout.cs`

`./bin/dev build`: 0 warnings, 0 errors. `./bin/dev test`: 720 passed, 0 failed, 1 skipped (pre-existing `GetCommitsAheadOfDefaultBranch`). `ganda repo audit`: exit 0. One pre-existing advisory remains (`memsearch-scaffold`: `.memsearch.toml` and the post-339 githooks). It is not from this change.

### How to validate

**Smoke**

```bash
./bin/dev build
dotnet run tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs
dotnet run tests/timewarp-amuru/single-file-tests/core/command-mock.timeout.cs
./bin/dev test
```

**Expect**

- Build reports 0 warnings and 0 errors.
- `shell-builder.with-timeout.cs`: 18 passed. A python child that traps SIGINT and exits 0 is `TimedOut` with exit 124 and stdout containing `GRACEFUL`, in about 1 second rather than after the grace period. A child that ignores SIGINT is `TimedOut` with exit 124 only after timeout plus grace. Caller cancellation throws `OperationCanceledException` and leaves `LastOutput` null.
- `command-mock.timeout.cs`: 11 passed. `TimesOut()` and a delay at least as long as `WithTimeout` both report exit 124 and `TimedOut`, with mock `RunTime` of zero.
- `./bin/dev test`: 720 passed, 0 failed, 1 skipped.

### Review disposition

- **Outcome:** accepted-exceptions (0 open). Effort 3, roster `general`, 2 rounds.
- **Final counts:** bug 1 fixed; suggestion 3 fixed; nit 2 fixed, 1 wontfix.
- **Fixed:** M1 `TryKill`/`TryInterrupt` swallow every exception (token callbacks on timer threads must not throw; regression vs master); M2 `LastOutput` docs narrowed; M3 tests for TTY ignored-SIGINT force kill and stream caller cancel; M4 DotNet mock-delay timeout test; M5 dead code removed, shared `RememberTimeout`; M6 "1 second" singular.
- **Wontfix:** M7 (python3 / SIGINT-handler timing assumption in graceful tests): low risk on ubuntu CI; skipping would mask regressions.
- **Paths:** `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Session

- Created: 2025-12-12
- Rewritten: 522eb63d (2026-10-06)
- Implementation: 2026-10-06
- Review: 2026-10-06, review oracle (Claude Opus 5.5) + general reviewer subagent; fixes by implementer subagent on this id
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-06T11:42:10Z
