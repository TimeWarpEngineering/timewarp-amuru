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

- [ ] Ramani machine under the test tree, exhaustive over the signal set
- [ ] Tests for exit, interrupt-then-exit, grace timeout, caller cancel winning a race, strict timeout, and the TTY no-throw exemption
- [ ] Tests for the two TTY start refusals, decided before a mock match
- [ ] Oracle source under the test tree; `TimeWarp.Amuru.Tools` has no Ramani reference
- [ ] `./tools/dev-cli/dev.cs test` passes
- [ ] `ganda repo audit` is clean

## Session

- Created: 01a1b4c4-827b-7d70-9fd0-358726b63b0b (2026-11-07)
- Spec review after 139 merged: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-08). Dependency paragraph updated; MachineStep and Signals notes added. Production contract unchanged.
- Implement oracle: implemented by implementer-grok (not reported, model grok-4.7), session 01a1bbe6-b892-72d0-8b8f-ce2a79c01596, max-turns 4, Blocked — 2026-11-08T16:04:19Z

## Notes

Filed from the Ramani review on 2026-11-07. The production contract this oracle copies is the Design region of `source/timewarp-amuru/core/command-result.cs`: graceful token at the timeout, forceful token after the grace period, caller cancellation not stored as `TimedOut`, exit 124 under default validation, `TimeoutException` only under zero-exit validation, and the TTY path reporting a timeout without throwing.

Null commands (`NeverRanExitCode`) and the choice of `RunAsync` versus `CaptureAsync` are outside this machine. They are not phases of one run.

## Blocked

2026-11-08: Finish the command-completion machine so it compiles (SM010 terminal refusals without a dummy transition, and one consistent using-declaration style that satisfies both IDE0007 and IDE0008), get the send-async oracle tests green, run the repo test gate and `ganda repo audit`, write kitchen Results, then commit and push the task branch.

### Resume note (2026-11-08, cockpit a875b3a1)

The first implement pass stopped on two compile conflicts. Both have precedent in this repo:

- **SM010** (terminal states that only refuse are not final). Keep `Exited`, `TimedOut`, and `Canceled` as ordinary non-final states, exactly as 139 did for `Exited` and `Faulted`. Suppress SM010 for the oracle file in the test-tree `.editorconfig` with a reason comment, following the `[timewarp-amuru/json-rpc/json-rpc-session-machine.cs]` block in `source/.editorconfig`. Do not add a dummy transition. A UML final state would discard the signals the spec says to refuse with a reason.
- **IDE0007 vs IDE0008.** Root `.editorconfig` sets `csharp_style_var_when_type_is_apparent = true` and `var_elsewhere = false`. So `using var run = new Run(...)` when the type is apparent from `new`, and an explicit type otherwise. Do not suppress either rule.

The disagreement found so far (TTY and CliWrap runners report exit 124 when the child exits during the grace period, while the oracle says `Exited` with the child's code) is exactly what the spec asks to record under Notes. Leave the runners alone and write it down with the test name that shows it.

Uncommitted work from the first pass is in the worktree: `tests/timewarp-amuru/command-completion/`, `tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs`, and `tests/timewarp-amuru/Directory.Build.props`. Continue from it.
