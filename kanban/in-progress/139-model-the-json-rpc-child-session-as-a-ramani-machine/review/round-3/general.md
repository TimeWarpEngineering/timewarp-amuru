# Round 3 — general
**Date:** 2026-11-08
**Scope reviewed:** fix delta `a45b10f..c20e40a` (product commit 24c8595): `source/.editorconfig`, `source/timewarp-amuru/json-rpc/json-rpc-in-flight.cs`, `json-rpc-session-context.cs`, `json-rpc-session-machine.cs`, `json-rpc-session-signals.cs`, and the two session test files. Round 2 findings M6–M8 were checked again against this delta.

## Summary

The fix pass adds a `Release(Id)` signal. Ready, Stopping, Exited, and Faulted handle it as an internal transition guarded on a finished id and refuse any other id. Idle and Starting refuse it. The effect is the only way an entry leaves the map, and `Accept` no longer prunes. The Stopping child-exited test now holds the fake stop. The machine is still exhaustive: the generator accepts the second `.Signals<Release>()` call, and the build is clean. Each state has a refusal test for `Release`. Four tests cover a taken `Release` (Ready, Stopping, Exited, Faulted). M6, M7, and M8 are fixed. One leftover from M8 remains: the host reads the plain map while pump effects add keys. Nothing in this task reads the map that way, because the tests serialize their reads. The finding is recorded as M9 below.

Gates run by the reviewer at c20e40a: `dev build` passed (0 warnings, 0 errors). `json-rpc-session-machine.send-async.cs` ran alone 14 times and passed 31/31 each time. `json-rpc-session-machine.send-async.guard.cs` ran alone 5 times and passed 12/12 each time. `dev test`: Total 785, Passed 784, Skipped 1, Failed 0. `ganda repo audit`: "Repository passes all audit checks."

## Resolved prior (round 2)

| ID | Status | Verification |
|----|--------|--------------|
| M6 | fixed | `Stopping_ChildExitedFromReadyRequest_Should_FailTheId` calls `seam.HoldStop()` before `Dispose` and asserts Stopping (send-async.cs:308-321). The other Stopping tests that send a signal use `StoppingAsync()` or `HoldStop()`. `StoppingCompletion_Should_FailThePendingId` deliberately lets the stop finish. No failures in 14 standalone runs. |
| M7 | fixed | `Accept` no longer calls `RemoveFinished` (context.cs:55-63). `NextSend_Should_KeepAnUnreadResult` checks that a completed payload survives a later `Send`, and that `Release` then removes it. |
| M8 | fixed | `Release` is a signal (signals.cs:26) with a guarded internal transition in Ready, Stopping, Exited, and Faulted (machine.cs:64-65, 78-79, 88-89, 100-101). The removal now runs on the pump. The public `context.Release(string)` call that ran from the host thread is gone. |

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:11,21,45
- Description: The Design region now says "The host reads Status and Payload. It does not add or remove keys." The host reaches an entry through `InFlight`, a view over a plain `Dictionary`. The pump still adds keys (`Accept`) and removes them (`Release`) while signals from other callers run. A `Dictionary` lookup during a concurrent `Add` or `Remove` is not supported and can throw or return the wrong entry. `JsonRpcInFlight` publishes Status after Payload, which makes each entry safe to read from another thread. The map lookup that finds the entry is not safe. In this task the tests are the only host, and they serialize their reads, so nothing fails today. Task 083's public client, with concurrent callers, would hit this.
- Suggestion: Either make `Requests` a `ConcurrentDictionary`, or change the Design note to say the host must not look up the map while another `Send` or `Release` can run. In the second case, 083 must read results on the pump or through an awaitable completion.
- Status: open
