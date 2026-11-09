# Round 3 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 1 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 0 | 0 |

Counts include carried IDs: M1 and M2 are suggestions fixed in round 2, M3 is the bug fixed this round, and M4 is open.

## Resolved prior

- M1 (suggestion), fixed in round 2. The delta does not touch it.
- M2 (suggestion), fixed in round 2. The delta does not touch it.
- M3 (bug), fixed and re-verified. When a child exits during a positive grace period, `TimeoutSession.StopForcefulDeadline` stops the forceful deadline. A slow reader then receives every line, the exit code is the child's, and `TimedOut` is false. `StreamStdout_ChildExitsDuringGrace_Should_KeepEveryLineAndTheChildCode` passes in about 7s. `shell-builder.with-timeout.cs` passes 16/16 and `shell-builder.with-timeout.modes.cs` passes 14/14.

## Issues

### M4 — Severity: bug — Status: open
- File: source/timewarp-amuru/core/command-result.timeout.cs:307 (wired at source/timewarp-amuru/core/command-result.cs:393)
- Description: The M3 fix stops the forceful deadline on any exit of the direct child: before the timeout as well as during grace, and in every timed mode. After that, only the caller token bounds the run. CliWrap awaits the pipe tasks after the exit and only the forceful token cancels them, so a descendant that holds stdout or stderr keeps a timed command running past `Timeout + TimeoutGracePeriod`. If the descendant never exits, the command never returns.
  - Probe: `sh -c "sleep 15 & echo started"` with timeout 1s and grace 1s.
  - HEAD: `elapsed=15.0s exit=0 timedOut=False`.
  - Pre-fix `fa56fb5`: `elapsed=2.1s exit=124 timedOut=True`.
  - The docs (`command-execution.md:26`) still promise the deadline. The new doc text and the Design comments describe a stream-only, grace-only stop that the code does not match.
- Suggestion: Keep a wall-clock bound after the child exits. Either (a) keep the deadline and classify a later deadline cancellation as the captured child code with `TimedOut` false, or (b) replace `Infinite` with a bounded drain that applies only after the graceful signal. Add a background-descendant test, and align the docs, release note, Design regions, and Results. See `general.md` for details.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
