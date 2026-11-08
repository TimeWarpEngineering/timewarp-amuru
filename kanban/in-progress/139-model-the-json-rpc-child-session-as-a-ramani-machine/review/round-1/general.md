# Round 1 — general
**Date:** 2026-11-08
**Scope reviewed:** branch vs `master` at HEAD 3704ce6, as in `review-framework.md`.

## Summary

The change adds an internal, exhaustive Ramani machine (`JsonRpcSessionMachine`) with six states and eight declared signals. It also adds a four-method process seam, an in-flight map plus an outbound channel on the context, one Ready do-activity that both writes and reads, and 31 SendAsync tests through a fake seam. The shape matches the spec. Every (state, signal) pair is decided, there is no machine-wide `RefuseOthers`, Starting's completion posts `Started`, `OnFault` leads to Faulted, a timeout stays in Ready, `ChildExited` fails every pending id, and unknown ids are refused. Public API, `<Version>`, and `nuget.config` are unchanged, and Tools does not reference Ramani. `dev test` passes (773 total, 772 passed, 1 skipped; `SendAsync_Given_` 31/31).

A scratch probe outside the repo (`/tmp/rv139`, built under the friend assembly name) confirmed three runtime defects. A duplicate `Send` id throws out of `SendAsync` instead of being refused. A server frame with no id, such as a JSON-RPC notification, faults the whole session. After `Dispose`, Stopping never leaves on its own. The standalone mode of the test file does not compile, so one `InternalsVisibleTo` entry is dead.

Checked and not raised:
- **SM010 suppression** (`source/.editorconfig`). SM010 warns that a dead-end non-final state discards every signal. Exited and Faulted refuse every declared signal with a reason, so nothing is discarded. The spec requires those states to refuse `Send` and `Start`, and a Final state cannot do that. The suppression has a design reason and covers one file, so it is not a silenced audit. Ramani follow-up: SM010 should not fire when every declared signal has an explicit `Refuse`.
- **CA1849 pragmas** (`json-rpc-session-activities.cs:46,59`). The spec requires the activity to post with `Send` and leaves `SendAsync` to the host. The pragmas guard exactly that call.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:49 (with json-rpc-session-machine.cs:51)
- Description: Ready's `Send` row has no guard. `Accept` calls `Requests.Add(send.Id, …)`, which throws `ArgumentException` when the id is already in the map. Entries are never removed, so this also happens when the earlier request with that id already completed or timed out. The probe ran `Send("1")` twice. The second `SendAsync` threw `ArgumentException: An item with the same key has already been added. Key: 1` instead of returning `Outcome == Refused` with a reason. That breaks the task's contract that a refused `SendAsync` is the caller's refusal ("a refusal reason on every illegal pair"; no exception route). It also means a reused id can never be sent again on the session.
- Suggestion: Guard the Ready `Send` row on `!context.InFlight.ContainsKey(signal.Id)` (or a `Contains` helper) and add `.Refuse<Send>("duplicate request id")` as the fallback, using the same guard-plus-Refuse pattern as `Response`. Add a test: two `Send` signals with the same id produce `Refused` with that reason and the state stays Ready.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-amuru/json-rpc/json-rpc-session-activities.cs:53-55 (with json-rpc-frames.cs:50-64)
- Description: Any inbound frame without a usable id throws `JsonException` in the Ready activity. `OnFault` then moves the session to Faulted and fails every pending request. JSON-RPC 2.0 notifications have no id, and MCP stdio servers send them routinely (`notifications/message`, `notifications/progress`). An error response may also carry `"id": null`. The probe sent one notification with request "1" pending. The result was `state=Faulted` and `id1=Failed "a JSON-RPC frame has no id"`. A related problem: a server-initiated request has both `id` and `method`. `TryReadId` returns that id and the activity posts it as a `Response`. If the id collides with a pending client id, the client request completes with the server's request frame. Task 083's MCP sample runs on this read loop, so a normal server would kill the session on its first log notification.
- Suggestion: Tell response frames apart from other inbound frames. A response has `result` or `error` and no `method`. Post `Response` only for responses. Notifications and server requests should not fault the session: drop or ignore them now, and leave routing to task 083. Fault only on malformed JSON or a protocol violation. Add fake-seam tests for a notification (the session stays Ready and the pending id stays Pending) and for a server request whose id matches a pending one (the pending id is not completed).
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-amuru/json-rpc/json-rpc-session-machine.cs:63-67 (with json-rpc-session-activities.cs:28-31)
- Description: After `Dispose`, Stopping runs `StopChildAsync`, but nothing ever posts `ChildExited`. The Ready read loop was cancelled when Ready exited, and the seam has no exit notification. The probe sent `Send("1")`, then `Dispose`, then waited one second. The result was `state=Stopping` and `id1=Pending`. The session never reaches Exited, and pending requests are never failed, unless the host sends `ChildExited` itself, which the tests do. Starting is symmetric and posts `Started` from its activity's completion; Stopping does not post `ChildExited`.
- Suggestion: Make Stopping's activity `.Do<bool>(StopChildAsync, static (context, _) => context.Machine.Send(new ChildExited()))`, which mirrors Starting. Alternatively, record in the Design region and in task 083's Notes who posts `ChildExited` once the real seam exists. Add a test for Dispose to Exited that has no host `ChildExited`.
- Status: open

### Issue 4 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.cs:14-18 (with source/timewarp-amuru/timewarp-amuru.csproj:23-25)
- Description: Run alone, the test file does not compile. `dotnet build` on it fails with `CS8956: File-scoped namespace must precede all other members` because of the `#if !JARIBU_MULTI return …` top-level statement. It also fails with `CS0246 FakeJsonRpcProcessSeam` because the sibling fake is not included. Other single-file tests (for example `core/command-mock.cs`) use block-scoped namespaces so they run alone, and the multi-file runner's `NoWarn` comment refers to that convention. As a result, the `InternalsVisibleTo Include="json-rpc-session-machine.send-async"` entry and its csproj comment describe a run that cannot happen.
- Suggestion: Either make standalone mode work (block-scoped namespaces in both files, and bring the fake in through a file-based-app include directive or another standalone-compatible route), or drop the standalone `InternalsVisibleTo` entry and the `#if !JARIBU_MULTI` entry point, and correct the csproj comment and task Notes.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:18,57-76
- Description: The in-flight map only grows. Completed, timed-out, and failed entries stay in the dictionary for the life of the session, and each one holds the full response frame. A long-lived session (the case this machine exists for) leaks one entry per request. Together with Issue 1, it also makes every id single-use.
- Suggestion: Give the host a way to remove a finished entry once the caller has read it, for example a `Forget(id)` effect on a signal or a take-and-remove read helper. Or record in Notes that task 083 owns retention, with this as a known gap.
- Status: open
