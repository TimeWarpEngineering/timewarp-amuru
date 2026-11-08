# Round 2 — general
**Date:** 2026-11-08
**Scope reviewed:** the fix delta `3704ce6..a45b10f` (commit a45b10f, plus kanban commit 9455592). Product files: `source/timewarp-amuru/json-rpc/{ijson-rpc-process-seam,json-rpc-frames,json-rpc-session-activities,json-rpc-session-context,json-rpc-session-machine}.cs`, `source/timewarp-amuru/timewarp-amuru.csproj`, `tests/timewarp-amuru/multi-file-runners/Directory.Build.props`, `tests/timewarp-amuru/single-file-tests/json-rpc/*.cs`. Round 1 findings M1–M5 were re-checked against this delta.

## Summary

The fix pass covers each round 1 finding with code and tests. A duplicate id is refused. Notifications, null-id errors, and server requests are ignored. Stopping's completion posts `ChildExited`. Finished entries are pruned. Both session test files now run alone. Build, both standalone files, and `dev test` pass. Three new problems come from the fixes themselves. The M3 change makes one older Stopping test race the new automatic `ChildExited`: it failed once in about 12 standalone runs. The M5 prune in `Accept` can drop a completed result the host has not read yet. `Release` adds a write to the map from off the pump.

## Gates run

- `./tools/dev-cli/dev.cs build`: Build succeeded, 0 warnings, 0 errors.
- `dotnet run …/json-rpc-session-machine.send-async.guard.cs`: Total 9, Passed 9.
- `dotnet run …/json-rpc-session-machine.send-async.cs`: 12 runs, 11 green. One run had Total 31, Passed 30, Failed 1, with `Stopping_ChildExitedFromReadyRequest_Should_FailTheId` failing on `MachineStepOutcome.Refused should be Taken`.
- `./tools/dev-cli/dev.cs test`: Total 782, Passed 781, Skipped 1, `All tests passed!` (one run).

## Round 1 re-check

- M1: fixed. The Ready `Send` guard `!context.Contains(id)` plus `.Refuse<Send>("duplicate request id")` is at json-rpc-session-machine.cs:53-54. Tests: `DuplicatePendingId_Should_Refuse` and `FinishedId_Should_RefuseUntilReleased`.
- M2: fixed. `JsonRpcFrames.Classify` returns Response only for an object with no method, with `result` or `error`, and with a string or number id. Ready ignores everything else and throws only on Malformed. Tests cover a notification, a null-id error, a server request that reuses a client id, and a malformed frame.
- M3: fixed in product code. Stopping is `.Do<bool>(StopChildAsync, … Send(new ChildExited()))`, and `StoppingCompletion_Should_FailThePendingId` covers it. The test regression it causes is filed as Issue 1.
- M4: fixed. Both files use a block-scoped namespace and `#:include` the fake, and both `dotnet run` alone. The aggregate removes the fake from its glob and adds `CS2002` to NoWarn.
- M5: fixed for retention. The new data-loss and thread-safety problems are filed as Issues 2 and 3.

## Issues

### Issue 1 — Severity: bug
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.cs:305-317
- Description: `Stopping_ChildExitedFromReadyRequest_Should_FailTheId` sends `Dispose` without `seam.HoldStop()` and then sends `ChildExited` itself. After the M3 fix, the Stopping activity finishes right away (the fake `StopAsync` is synchronous unless held) and posts its own `ChildExited`. When that posted signal wins, the machine is already Exited and the test's `ChildExited` is `Refused`. Seen: 1 failure in about 12 standalone runs (`MachineStepOutcome.Refused should be MachineStepOutcome.Taken`). This makes CI flaky.
- Suggestion: Use `StoppingAsync()`, which already calls `HoldStop()`, or call `seam.HoldStop()` before `Dispose`. Then send `ChildExited` and release the stop. Check the other Stopping tests that do not hold the stop.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:62-63,94-117
- Description: `Accept` calls `RemoveFinished()`, which drops every Completed, TimedOut, or Failed entry, whether or not the host has read it. `InFlight[id]` is the only way to read a result: `JsonRpcInFlight` has no completion source and the payload lives only in the map. With two callers this loses results. Caller A sends `1`, response `1` arrives, caller B sends `2` before A reads `InFlight["1"]`, and A's result is gone (`KeyNotFoundException`). This contradicts the file's own Design note, "Release drops one finished entry after the host has read it". `NextSend_Should_DropFinishedEntries` asserts the lossy behavior.
- Suggestion: Make removal follow a read. One way: `Release` (or a read-and-release call) is the only removal, and Accept does not prune. Another: give `JsonRpcInFlight` a completion the host awaits, so dropping the map entry cannot lose a result. Update the Design note and the test to match.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:6,51-60
- Description: `Release` removes from a plain `Dictionary` and runs on the caller's thread. The tests call it from the test thread, outside any effect. Effects on the pump (`Accept` add and prune, `FailPending` enumeration) write or enumerate the same dictionary. A host that releases one id while another caller's `Send` or a `ChildExited` is processed has unsynchronized concurrent writes to a `Dictionary`, which can corrupt it or throw "collection was modified". The Design note records the exception ("except Release") but does not make it safe.
- Suggestion: Run release on the pump (for example as an effect, or with a documented host rule that it is called only from pump code), or make the map safe for concurrent use. Record the rule in the context's Design region.
- Status: open

## Checked, not raised

- `CS2002` in `multi-file-runners/Directory.Build.props` NoWarn: both test files `#:include` the one fake seam and the aggregate sees it twice. The compiler de-duplicates the file, and the warning reports no real defect. The fake is also removed from the glob. This does not silence a measured check.
- `InternalsVisibleTo` now has three test assembly names. Each one matches a real assembly name (`run-tests` and the two standalone file names).
- Stopping `Do<bool>` result posts `ChildExited` with `Send`, the same pattern Starting uses for `Started`. A host-sent `ChildExited` in Stopping still works, and the late one is refused in Exited with a reason.
- Classifier edge cases: `{}` and non-JSON are Malformed. An object with a method is Ignored even if it has a result. A response with a missing or empty id is Ignored. These are reasonable for the scope of task 083.
- Goal restatement: the fix pass rewrote it to describe the fixes. It still matches the task description and requirements. No misread.
