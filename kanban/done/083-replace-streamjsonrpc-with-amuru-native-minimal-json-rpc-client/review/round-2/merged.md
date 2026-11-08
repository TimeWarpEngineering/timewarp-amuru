# Round 2 — merged findings
**Date:** 2026-11-08
**Sources:** general
**Scope:** fix delta `a087dba..b47b9bb`, plus a re-check of the round 1 IDs

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 4 | 0 |
| nit | 0 | 2 | 2 |

## Resolved prior (round 1)

| ID | Severity | Status | Evidence |
|----|----------|--------|----------|
| M1 | bug | fixed | `TypeInfo(Type)` uses `GetTypeInfo` and the `JsonTypeInfo` overloads. No IL2026 or IL3050 suppression is left. The Release build has 0 warnings. |
| M2 | bug | fixed | ModelContextProtocol 2.4.0 stdio server. `RealMcpServer_Should_InitializeAndListTools` passes. |
| M3 | suggestion | fixed | `NotifyAsync` and the `Notify` signal. Guard tests cover the frame and the refusals. |
| M4 | suggestion | fixed | `WaitForExitStatusAsync` with a timeout. The EOF policy is documented. |
| M5 | suggestion | fixed | Metadata and serializer failures throw `JsonRpcException`. The docs list that case. |
| M6 | suggestion | fixed | Serializer options, malformed stdout, and send-after-dispose tests pass. |
| M7 | nit | fixed | `StopAsync` disposes `StandardErrorCancellation`. |
| M8 | nit | fixed | Dispose awaits `Context.Settled`, bounded by grace plus 2 s. |

## Issues

### M9 — Severity: nit — Status: wontfix
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:453
- Description: `WaitUntilStatePublishedAsync` loops on `Task.Yield()` with no delay, bounded at 2 s, to cover the gap between a gate completing in the effect and the machine publishing the state.
- Suggestion: Decide start success from `Ready` and `Settled` alone, or add a short delay.
- Source: general
- Disposition notes: wontfix, decided by the review oracle (round 2). The gap is the pump's synchronous step from the effect to the state assignment, so the loop runs a few iterations, and the 2 s bound caps it. M8's request was to stop the unbounded 10 ms poll in dispose, and that is done. This loop is not worth another fix pass. It can be folded into any later change to the Ramani effect ordering.

### M10 — Severity: nit — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs:863
- Description: The MCP server build cache checks only the `.cs` timestamp. On Windows it misses the `.exe` suffix, and concurrent runs share one temp directory.
- Suggestion: Add the props files to the staleness check, add the Windows suffix, or build into a per-run directory.
- Source: general
- Disposition notes: wontfix, decided by the review oracle (round 2). This is test-only. On Windows the result is a rebuild, not a wrong binary. A stale build is possible only on a developer machine after a props edit, and touching the server source or deleting the temp directory fixes it. Product behavior is not affected.

## Duplicates / conflicts

- None. There was one reviewer, and neither new nit overlaps a round 1 ID.
