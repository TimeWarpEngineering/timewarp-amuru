# Round 5 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 3 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

Counts include the carried IDs:

- M1 and M2 are suggestions. Both were fixed in round 2.
- M3 is a bug fixed in round 3.
- M4 is a bug fixed in round 4.
- M5 is a bug fixed in this round.

## Resolved prior

- M1 (suggestion) was fixed in round 2. This delta does not touch it.
- M2 (suggestion) was fixed in round 2. `command-result.cs` is still 1339 lines.
- M3 (bug) was fixed in round 3. Re-verified: `StreamStdout_ChildExitsDuringGrace_Should_KeepEveryLineAndTheChildCode` passed in 7.08s.
- M4 (bug) was fixed in round 4. Re-verified: the original-deadline descendant test took 2.21s and the drain-bound test took 3.18s.
- M5 (bug) is fixed and re-verified. With grace 0, `NoteChildExit` records an exit that happens before the signal and leaves the deadline at `Timeout`. `Capture_DescendantHoldsStdoutWithZeroGrace_Should_KeepTheChildCodeAtTimeout` passed in 1.18s. Probes with grace 0 gave these results:
  - Stream: 1.1s, exit 0, `TimedOut` false.
  - Strict capture: 1.1s, exit 0, no throw.
  - Child still running at the deadline: exit 124 with `TimedOut` true.
  - The docs, the Design regions, and the Goal restatement all match this behavior.

## Issues

None. The fix delta raised no new findings.

## Duplicates / conflicts

- None (single reviewer).
