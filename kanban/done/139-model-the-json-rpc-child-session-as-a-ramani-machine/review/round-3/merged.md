# Round 3 — merged findings
**Date:** 2026-11-08
**Sources:** general
**Scope:** fix delta `a45b10f..c20e40a` (product commit 24c8595)

## Counts

Final ledger for all rounds. M1–M8 are carried over, and M9 is new.

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 4 | 0 |
| suggestion | 0 | 4 | 1 |
| nit | 0 | 0 | 0 |

## Resolved prior

| ID | Severity | Status | Verification |
|----|----------|--------|--------------|
| M1 | bug | fixed | Round 2 verified this. The guard is unchanged in this delta. |
| M2 | bug | fixed | Round 2 verified this. `JsonRpcFrames.Classify` is unchanged in this delta. |
| M3 | suggestion | fixed | Round 2 verified this. `StoppingCompletion_Should_FailThePendingId` passes. |
| M4 | suggestion | fixed | Both files ran alone and passed in round 3 (31/31 ×14, 12/12 ×5). |
| M5 | suggestion | fixed | The map no longer keeps a result after the host is done with it, because the host can now `Release` it. The fix moved from pruning on `Accept` to an explicit `Release` (see M7). |
| M6 | bug | fixed | The test holds the fake stop and asserts Stopping before it sends `ChildExited`. No failures in 14 standalone runs. |
| M7 | bug | fixed | `Accept` does not prune. `NextSend_Should_KeepAnUnreadResult` covers this. |
| M8 | suggestion | fixed | `Release` is a guarded internal signal, and its effect runs on the pump. The host-thread `context.Release(string)` call is gone. |

## Issues

### M9 — Severity: suggestion — Status: wontfix
- File: source/timewarp-amuru/json-rpc/json-rpc-session-context.cs:11,21,45
- Description: The host is meant to read `Status` and `Payload` by looking up `InFlight`, a view over a plain `Dictionary`. The pump adds keys (`Accept`) and removes them (`Release`) for other callers at the same time. A concurrent lookup is not supported. Each entry is safe to read from another thread. The map lookup that finds it is not.
- Suggestion: Use a `ConcurrentDictionary`, or state in the Design note that the host must not look up the map while a `Send` or `Release` can run.
- Source: general
- Disposition notes: wontfix in task 139, decided by the review oracle (round 3). This task has no concurrent host reader. The tests are the only host, and each reads after its own `SendAsync` returns. Task 083 owns the public client and the path that reads results. That task decides how a caller waits: a concurrent map, a read on the pump, or an awaitable completion. This handoff is recorded in task.md Notes for 083. If 083 keeps host-side lookups, it must change `Requests` before concurrent callers can reach it.

## Duplicates / conflicts

- M9 is what remains of M8 on the read side. M8 covered the write from the host thread, and that write is fixed.
