# Round 1 — merged findings
**Date:** 2026-11-08
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:49 (machine row json-rpc-session-machine.cs:51)
- Description: A duplicate `Send` id, including the reuse of a finished id, throws `ArgumentException` out of `SendAsync`. It should return `Refused` with a reason. A probe confirmed it.
- Suggestion: Guard Ready `Send` on "id not in map" plus `.Refuse<Send>("duplicate request id")`, and add a test.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-session-activities.cs:53-55; json-rpc-frames.cs:50-64
- Description: An inbound frame with no id, such as a JSON-RPC notification or an `"id": null` error, faults the session and fails every pending request (a probe confirmed it). A server request with an id is posted as a `Response` and can complete a pending client id that happens to match.
- Suggestion: Post `Response` only for response frames (`result` or `error`, no `method`). Do not fault on notifications or server requests. Add tests.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-session-machine.cs:63-67; json-rpc-session-activities.cs:28-31
- Description: Stopping never leaves on its own. Nothing posts `ChildExited` after `StopAsync`, so pending ids stay Pending (a probe confirmed it).
- Suggestion: Post `ChildExited` from Stopping's activity completion, as Starting does with `Started`, or record who owns it for task 083. Add a test.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.cs:14-18; source/timewarp-amuru/timewarp-amuru.csproj:23-25
- Description: The test file does not compile alone (CS8956 file-scoped namespace, CS0246 missing fake). The `json-rpc-session-machine.send-async` `InternalsVisibleTo` entry and its comment are therefore dead.
- Suggestion: Make standalone mode work with block-scoped namespaces and an included fake, or remove the standalone entry point and the `InternalsVisibleTo` entry and fix the comment.
- Source: general
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:18,57-76
- Description: The in-flight map is never pruned, so a long-lived session leaks one entry, with its payload, per request.
- Suggestion: Add a removal path for a finished entry, or record retention as task 083's known gap.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. A single reviewer. M1 and M5 touch the same map but are separate defects: M1 is the error path, M5 is retention.

## Checked, not raised

- SM010 suppression in `source/.editorconfig`: has a design reason and is required by the spec (Exited and Faulted must refuse with a reason). Not a silenced audit.
- CA1849 pragmas in the Ready activity: the spec requires the activity to post with `Send`.
- Goal restatement vs task: consistent. No misread.
