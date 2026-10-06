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

- [ ] `CommandOptions`: `Timeout`, `TimeoutGracePeriod`, `WithTimeout(TimeSpan)`, `WithTimeoutGracePeriod(TimeSpan)`; copied by the snapshot path builders use
- [ ] `ShellBuilder.WithTimeout` and `DotNetBuilder.WithTimeout`
- [ ] Execution core: linked token source, graceful-then-forceful kill via CliWrap's two-token `ExecuteAsync`, `TimedOut` + exit 124 result, strict-validation throw with a timeout message, caller cancellation still propagates
- [ ] `TtyPassthroughAsync` `Process` path honors the timeout
- [ ] `Pipe` stages share the window
- [ ] `CommandOutput.TimedOut`
- [ ] `CommandMock` timeout simulation; mock/real parity test
- [ ] Tests (`tests/timewarp-amuru/single-file-tests/core/`): default validation → `TimedOut` true, exit 124, no throw; strict validation → throws with timeout message; caller token cancel → `OperationCanceledException`, `TimedOut` false; graceful signal observed before force kill (child traps SIGINT and exits 0 within grace → not force-killed, `TimedOut` true); child ignores SIGINT → force-killed after grace; streaming mode ends cleanly; passthrough and tty paths; pipe stage; no timeout set → behavior unchanged (existing suite green)
- [ ] Cross-platform note: Windows has no SIGINT for non-console children; document that graceful falls through to the forceful kill there and test what is testable on CI (ubuntu)
- [ ] PublicAPI Unshipped updated for every new member; XML docs; `./bin/dev build` 0 warnings / 0 errors
- [ ] Docs: `documentation/` core execution page gets a Timeout section with the result contract; `readme.md` one-liner; `skills/amuru/SKILL.md` mention
- [ ] Full runner green; `ganda repo audit` clean
- [ ] Results: API summary, the CliWrap overload used, exit-code/exception contract, test list

## Notes

- GNU `timeout` exits 124 when the command times out; reuse that code so shell-minded callers recognize it.
- The existing per-call `CancellationToken` parameters stay; `Timeout` composes with them.
- Reference for result-vs-throw philosophy: task 090 Results; for mock parity: tasks 089 and 121.

## Session

- Created: 2025-12-12
- Rewritten: 522eb63d (2026-10-06)
