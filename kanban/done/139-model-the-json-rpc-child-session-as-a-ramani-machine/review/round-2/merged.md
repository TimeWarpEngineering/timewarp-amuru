# Round 2 — merged findings
**Date:** 2026-11-08
**Sources:** general
**Scope:** fix delta `3704ce6..a45b10f`

## Counts

Carried-over IDs M1–M5 and new IDs M6–M8.

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 2 | 0 |
| suggestion | 1 | 3 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior (round 1)

| ID | Severity | Status | Verification |
|----|----------|--------|--------------|
| M1 | bug | fixed | The Ready `Send` guard plus `.Refuse<Send>("duplicate request id")` is at json-rpc-session-machine.cs:53-54. Covered by two guard tests. |
| M2 | bug | fixed | `JsonRpcFrames.Classify` posts Response only for result or error frames. Notification, null-id error, server request, and malformed-frame tests pass. |
| M3 | suggestion | fixed | Stopping `Do<bool>` completion posts `ChildExited`. Covered by `StoppingCompletion_Should_FailThePendingId`. M6 is the test regression it causes. |
| M4 | suggestion | fixed | Both session files `dotnet run` alone (block-scoped namespace, `#:include` fake, three `InternalsVisibleTo` names). |
| M5 | suggestion | fixed | Finished entries are pruned. M7 and M8 are side effects of how. |

## Issues

### M6 — Severity: bug — Status: open
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.cs:305-317
- Description: `Stopping_ChildExitedFromReadyRequest_Should_FailTheId` does not hold the fake stop, so after the M3 fix Stopping's own `ChildExited` can win the race. The test's `ChildExited` is then `Refused` in Exited. Seen once in about 12 standalone runs.
- Suggestion: Use `StoppingAsync()` (which holds the stop) or call `seam.HoldStop()` before `Dispose`. Check the other Stopping tests too.
- Source: general
- Disposition notes:

### M7 — Severity: bug — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:62-63,94-117
- Description: `Accept` prunes every finished entry, including results the host has not read. The map is the only place a result lives, so a second caller's `Send` can drop the first caller's completed response. This contradicts the Design note "after the host has read it".
- Suggestion: Remove an entry only after it is read (Release or a read-and-release call is the only removal), or give each in-flight entry an awaitable completion so dropping the map entry cannot lose the result. Update the Design note and `NextSend_Should_DropFinishedEntries`.
- Source: general
- Disposition notes:

### M8 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:6,51-60
- Description: `Release` writes the plain `Dictionary` from the caller's thread while pump effects (Accept, FailPending) write or enumerate it. Nothing synchronizes these accesses.
- Suggestion: Run release on the pump, or make the map safe for concurrent use. Record the threading rule in the Design region.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- M7 and M8 both touch the M5 retention fix. M7 is losing a result that has not been read. M8 is thread safety. One redesign (removal only through a pump-side release, or awaitable entries) can close both.
