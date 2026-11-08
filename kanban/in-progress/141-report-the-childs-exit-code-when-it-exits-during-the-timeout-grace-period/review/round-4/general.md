# Round 4 — general
**Date:** 2026-11-09
**Scope reviewed:** fix delta `003885a..93204a3`. That is commit `1ad4bfc`, which bounds the timeout drain after the child exits, plus the kanban record `93204a3`. The review re-verifies M4 and looks for new defects in that delta.

## Summary

`StopForcefulDeadline` is now `TimeoutSession.NoteChildExit`. When the direct child exits, it does not turn the forceful deadline off. An exit during a positive grace period moves the deadline to `Timeout + 2 × grace`. An exit before the graceful signal leaves `Timeout + grace`. In either case it sets `ChildExitedBeforeDeadline`, and the `GraceElapsed` catch skips the 124 classification when that flag is set. So a later deadline cancel goes to the captured-code branch.

M4 is fixed for a positive grace period. The round-3 probe (`sh -c "sleep 15 & echo started"`, timeout 1s, grace 1s) now ends in 2.1s with exit 0 and `TimedOut` false. The race checks hold:

- A deadline that fires before the exit event leaves the flag unset, so a killed child is still 124.
- `CancelAfter` on a source that is already canceled is a no-op.
- A second `Publish` from `Attach` recomputes the same target.

One gap remains. Zero grace returns before the flag is set, which contradicts the new documentation row and the Goal restatement.

Verification:

- `shell-builder.with-timeout.cs` passes 18/18. The two new descendant tests take 2.09s and 3.09s, and the slow-reader grace test takes 7.08s.
- `shell-builder.with-timeout.modes.cs` passes 14/14.
- `command-result.cs` is 1339 lines, which matches `.timewarp/line-cap-baseline.json`.

Probes against HEAD, using a runfile under `/tmp` that references the project:

| Case | Elapsed | Result |
| --- | --- | --- |
| Strict capture, descendant, exit 0 | 2.1s | exit 0, `TimedOut` false |
| Strict capture, descendant, exit 3 | 2.1s | `CommandExecutionException` (3) |
| `StreamStdoutAsync`, descendant | 2.1s | 1 line, exit 0, `TimedOut` false |
| Zero grace, descendant, exit 0 | 1.1s | exit 124, `TimedOut` true (see Issue 1) |

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/core/command-result.timeout.cs:318 (classified at source/timewarp-amuru/core/command-result.cs:418)
- Description: With a zero grace period, `NoteChildExit` returns before it sets `ChildExitedBeforeDeadline`. Take a direct child that exits with 0 well before the timeout while a descendant still holds stdout. The armed deadline fires at `Timeout` and the run reports exit 124 with `TimedOut` true. Strict validation would throw `TimeoutException`.
  - Probe: `sh -c "sleep 15 & echo started"`, timeout 1s, grace 0, default validation. Result: `elapsed=1.1s exit=124 timedOut=True stdout=started`.
  - This contradicts the new row in `documentation/developer/reference/command-execution.md:39`: "Direct child has already exited, and a descendant still holds a pipe when the armed deadline fires → The child's exit code, `TimedOut` false". That row has no grace qualifier.
  - It also contradicts the Design region in `command-result.timeout.cs:10-11`: "A later deadline cancel after either exit reports the captured code with TimedOut false. A zero grace period does not move the deadline." That says zero grace only skips the move.
  - The Goal restatement says the same thing: "An exit before the graceful signal leaves the original … deadline, and a pipe that stays open is cut off there on the same rule … A zero grace period does not move the deadline."
  - The behavior is not a regression. It matches `fa56fb5` and the pre-task code. But the contract that this delta wrote promises otherwise.
- Suggestion: Pick one of two fixes.
  - (a) Code, which matches the Goal restatement. With zero grace, still set the flag when the child exits before the graceful signal (`!GracefulSignaled` and the forceful source not canceled). Skip only the `CancelAfter` move. An exit at or after the zero-grace signal stays 124. Add a zero-grace descendant capture test that expects exit 0 and `TimedOut` false at about `Timeout`.
  - (b) Docs. State that with a zero grace period, a descendant that holds a pipe at `Timeout` is the kill: exit 124 and `TimedOut` true. Then align the table row, both Design regions, the release note, and the Goal restatement.
  - Either fix closes the finding. Option (a) is preferred because the task treats an exit before the signal as the child's own result.
- Status: open
