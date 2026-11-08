# Prove command completion with a Ramani test oracle

## Description

`CommandResult` finishes a run in one of a few phases, and two runners must agree on which phase a signal produces. The CliWrap wait (`TimeoutSession` plus `ListenWithTimeoutAsync`) and `TtyPassthroughAsync` each implement that protocol. Encode the protocol as a Ramani state machine and step it from tests. The runners stay the production implementation.

The machine is a test oracle, so its source lives under the test tree, not in a shipped package. Since task 139 merged (2026-11-08), `source/timewarp-amuru` already references `TimeWarp.Ramani` 1.0.0-beta.23 and the test files reach it with `using TimeWarp.Ramani;` through the core project reference. The test tree needs no package reference of its own. `source/timewarp-amuru-tools` must not gain a Ramani reference.

## Goal (implementer restatement)

A test-only Ramani machine is the oracle for how one command run finishes. It is stepped with four signals and five states. A normal exit records the child's exit code and is not a timeout. A timeout signal moves the run into an interrupting grace phase and is not yet a result. If the child then exits, that exit stands. If the grace period elapses first, the result is a timeout with exit 124. Caller cancellation from either active phase becomes cancellation, leaves no last output, and is not a timeout. When cancellation and the end of the grace period are both true, cancellation wins. Strict validation records the timeout and then throws; a TTY run records the same timeout and does not throw. Starting a TTY run refuses configured standard input or a pipeline, with the messages the product already uses, and that decision is recorded before any mock match. Every other state and signal pair is an explicit refusal. The production runners stay as they are. A runner that classifies a case differently is written up in Notes.

## Requirements

- States are `Running`, `Interrupting`, `Exited`, `TimedOut`, and `Canceled`.
- Signals are `ChildExited` (with the child's exit code), `TimeoutElapsed`, `GraceElapsed`, and `CallerCanceled`.
- `Running` + `ChildExited` goes to `Exited` with that exit code. `TimedOut` is false.
- `Running` + `TimeoutElapsed` goes to `Interrupting`. This is the SIGINT the grace period represents. It is not yet a result.
- `Interrupting` + `ChildExited` goes to `Exited` with the child's exit code. A child that dies on the signal and is reported as an exit event is an exit, not a timeout.
- `Interrupting` + `GraceElapsed` goes to `TimedOut`. The result exit code is `CommandResult.TimeoutExitCode` (124).
- `CallerCanceled` from `Running` or `Interrupting` goes to `Canceled`. `TimedOut` stays false. This matches `TimeoutSession.TimedOut`, which is false when the caller token has fired.
- `CallerCanceled` wins when it races `GraceElapsed`. The outcome is `Canceled`, not `TimedOut`.
- A context flag selects validation. From `TimedOut`, default validation records the result and does not throw. Zero-exit validation records the result, then throws `TimeoutException`. `Canceled` throws `OperationCanceledException` and does not write `LastOutput`.
- A second context flag selects TTY. TTY timeout still ends in `TimedOut` with exit 124, and it does not throw, including under zero-exit validation. That is the documented `TtyPassthroughAsync` exemption.
- Starting a TTY run with configured standard input is refused, with the existing message `TtyPassthroughAsync requires console stdin; configured standard input cannot be used with a TTY`.
- Starting a TTY run of a pipeline is refused, with the existing message `TtyPassthroughAsync cannot run a pipeline; the upstream stage would be dropped`.
- Both refusals are decided before any mock match. The machine records that order.
- `.Exhaustive()` is on. Declare the signal universe with `.Signals<ChildExited, TimeoutElapsed, GraceElapsed, CallerCanceled>()` so the proof covers all four. Every other pair in this signal set is a refusal with a reason, not an implicit ignore. Use per-state `.Refuse<T>(reason)` or a state-level `RefuseOthers`, not a machine-wide one.
- Tests read the refusal from the `MachineStep` that `SendAsync` returns (Ramani 1.0.0-beta.22 and later): `Outcome == Refused` and `Reason`. No step observer and no completion source on the signal.
- Behaviors are copied into generated static methods: lambdas capture nothing, and the validation and TTY flags live on the context. The machine is a `partial` class whose `Machine.Named(...)...Build()` chain is one expression. See `source/timewarp-amuru/json-rpc/json-rpc-session-machine.cs` for the shape this repo already uses.
- The machine source lives under the test tree. Production `CommandResult`, `TimeoutSession`, and `TtyPassthroughAsync` are not rewritten in this task.
- A test steps each transition above. Where today's runner disagrees with the machine, record the disagreement in Notes and leave the runner alone.

## Checklist

- [x] Ramani machine under the test tree, exhaustive over the signal set
- [x] Tests for exit, interrupt-then-exit, grace timeout, caller cancel winning a race, strict timeout, and the TTY no-throw exemption
- [x] Tests for the two TTY start refusals, decided before a mock match
- [x] Oracle source under the test tree; `TimeWarp.Amuru.Tools` has no Ramani reference
- [x] `./tools/dev-cli/dev.cs test` passes
- [x] `ganda repo audit` is clean

## Session

- Created: 01a1b4c4-827b-7d70-9fd0-358726b63b0b (2026-11-07)
- Spec review after 139 merged: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-08). Dependency paragraph updated; MachineStep and Signals notes added. Production contract unchanged.
- Implement oracle: implemented by implementer-grok (not reported, model grok-4.7), session 01a1bbe6-b892-72d0-8b8f-ce2a79c01596, max-turns 4, Blocked — 2026-11-08T16:04:19Z
- Implement resume: session 01a1bbe9-8923-72c1-846c-362c633470a7 (2026-11-08). Removed the dummy terminal transitions, suppressed SM010 in `tests/.editorconfig`, and constructed the harness with `using var run = new Run(...)` so IDE0007 and IDE0008 both hold. Oracle tests are green. Runners were left alone.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bbe9-8923-72c1-846c-362c633470a7, max-turns 500, Done — 2026-11-08T16:24:02Z

## Notes

Filed from the Ramani review on 2026-11-07. The production contract this oracle copies is the Design region of `source/timewarp-amuru/core/command-result.cs`: graceful token at the timeout, forceful token after the grace period, caller cancellation not stored as `TimedOut`, exit 124 under default validation, `TimeoutException` only under zero-exit validation, and the TTY path reporting a timeout without throwing.

Null commands (`NeverRanExitCode`) and the choice of `RunAsync` versus `CaptureAsync` are outside this machine. They are not phases of one run.

Runner disagreement, left in place. `SendAsync_Given_.Interrupting_ChildExited_Should_ExitWithTheChildCode` steps `TimeoutElapsed` then `ChildExited(130)` and records `Exited` with that code and `TimedOut` false. The runners report exit 124 when the child exits during the grace period. `TimeoutSession.TimedOut` is true once the graceful token has fired, and both paths treat that as the result:

- CliWrap (`ExecuteCliAsync`, used by capture, run, select, and passthrough) takes the `OperationCanceledException` when `session.TimedOut` and stores exit 124. `WithTimeout_Given_.DefaultValidation_Should_ReportExit124WithoutThrowing` runs a child that exits 0 on SIGINT and expects 124. The same shape is in `Passthrough_Should_ReportTimeout`, `Select_Should_ReturnCapturedTextAndRecordTimeout`, `Run_Should_Return124`, and `RunAndCapture_Should_Report124AndKeepStdout`.
- `TtyPassthroughAsync` checks `session.TimedOut` after `WaitForExitAsync` returns, so a child that has already exited is still exit 124. `WithTimeout_Given_.TtyPassthrough_Should_ReportTimeoutWithoutThrowing` is that case, and it also shows the no-throw exemption under zero-exit validation. The oracle matches the exemption in `TimedOut_TtyZeroExit_Should_RecordWithoutThrowing`.

## Results

The test tree holds `CommandCompletionMachine`, an exhaustive Ramani oracle over `ChildExited`, `TimeoutElapsed`, `GraceElapsed`, and `CallerCanceled`. `Exited`, `TimedOut`, and `Canceled` stay non-final and refuse every signal with a reason. SM010 is suppressed for that file in `tests/.editorconfig`, because a final state would discard those signals. The generator is attached from the Ramani package the core project already restores. `source/timewarp-amuru-tools` has no Ramani reference. Production `CommandResult`, `TimeoutSession`, and `TtyPassthroughAsync` are unchanged.

`SendAsync_Given_` steps each accepted transition, the caller-cancel race, strict timeout, the TTY no-throw exemption, both TTY start refusals (including empty standard input, and standard input winning over a pipeline), and every refusal. `./tools/dev-cli/dev.cs test`: 826 total, 825 passed, 1 skipped (`GetCommitsAheadOfDefaultBranch_Given_`, pre-existing).

### How to validate

**Smoke**

```bash
dotnet run tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs
```

**Expect**

- 19 passed, 0 failed.
- `Running` + `ChildExited(3)` ends in `Exited`, exit code 3, `TimedOut` false, and the order is tty-stdin, tty-pipeline, mock-match.
- `TimeoutElapsed` then `ChildExited(130)` ends in `Exited` with 130, not a timeout.
- `TimeoutElapsed` then `GraceElapsed` ends in `TimedOut` with exit 124.
- `CallerCanceled`, and `GraceElapsed` after `CallerTokenFired`, throw `OperationCanceledException`, stay `Canceled`, and leave `LastOutput` unset.
- Zero-exit validation records exit 124 and then throws `TimeoutException`. The same path with `IsTty` records 124 and does not throw.
- A TTY start with configured standard input (empty included) throws `TtyPassthroughAsync requires console stdin; configured standard input cannot be used with a TTY` and the order stops at `refusal`, before `mock-match`.
- A TTY pipeline throws `TtyPassthroughAsync cannot run a pipeline; the upstream stage would be dropped` after the stdin check and before `mock-match`.
- Every other signal in `Exited`, `TimedOut`, and `Canceled` is `MachineStepOutcome.Refused` with that state's reason.

**Automated**

```bash
./tools/dev-cli/dev.cs test
# expect: Failed: 0 (this session: Passed 825, Skipped 1, Total 826)
ganda repo audit
```
