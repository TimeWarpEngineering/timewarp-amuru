# Round 4 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 2 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

Counts include the carried IDs:

- M1 and M2 are suggestions fixed in round 2.
- M3 is a bug fixed in round 3.
- M4 is a bug fixed this round.
- M5 is open.

## Resolved prior

- M1 (suggestion) was fixed in round 2. This delta does not touch it.
- M2 (suggestion) was fixed in round 2. The `command-result.cs` file is still 1339 lines, which matches the baseline.
- M3 (bug) was fixed in round 3. It is re-verified: `StreamStdout_ChildExitsDuringGrace_Should_KeepEveryLineAndTheChildCode` passes in 7.08s with a 1s timeout and a 4s grace. That run outlasts the 5s original deadline and stays inside the 9s drain bound.
- M4 (bug) is fixed and re-verified for a positive grace period. `NoteChildExit` keeps the forceful deadline armed. The round-3 probe `sh -c "sleep 15 & echo started"` (timeout 1s, grace 1s) ends in 2.1s with exit 0 and `TimedOut` false. A descendant forked from the grace handler ends at the 3s drain bound. Strict capture, strict non-zero, and `StreamStdoutAsync` with a descendant all end at about 2.1s with the child's result. `shell-builder.with-timeout.cs` passes 18/18 and `shell-builder.with-timeout.modes.cs` passes 14/14.

## Issues

### M5 — Severity: bug — Status: open
- File: source/timewarp-amuru/core/command-result.timeout.cs:318 (classified at source/timewarp-amuru/core/command-result.cs:418)
- Description: A zero grace period skips `ChildExitedBeforeDeadline`. In this case a direct child exits with 0 before the timeout and a descendant holds stdout. The run reports exit 124 with `TimedOut` true at `Timeout`. Probe with grace 0: `elapsed=1.1s exit=124 timedOut=True`. The new `command-execution.md:39` row, the `command-result.timeout.cs:10-11` Design text, and the Goal restatement all say that a deadline cut-off after an exit before the signal keeps the child's code with `TimedOut` false. They exempt zero grace only from moving the deadline. This is not a regression from the pre-task behavior, but this delta wrote the contract that it breaks.
- Suggestion: Choose one fix.
  - (a) Preferred. Set the flag for zero grace when the child exits before the graceful signal, skip only the deadline move, and add a zero-grace descendant test.
  - (b) Document zero grace plus a descendant as the 124 kill in the table, both Design regions, the release note, and the Goal restatement.
  - See `general.md`.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
