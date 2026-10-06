# Round 1 — merged findings
**Date:** 2026-10-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 2 | 1 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru/core/command-result.cs:583
- Description: `TryKill` / `TryInterrupt` catch only narrow exception types; `Process.Kill(entireProcessTree: true)` can throw `AggregateException` (EPERM descendant, e.g. sudo) and the P/Invoke can throw interop exceptions. These run as token registrations (timer thread), so an escape crashes the host or surfaces from the caller's `Cancel()`. Regression vs master's catch-all.
- Suggestion: Catch all in both helpers (justified CA1031 suppression if needed).
- Source: general
- Disposition notes: fixed — TryKill/TryInterrupt catch Exception with CA1031/RCS1075 suppressions + justification.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru/core/command-result.cs:123
- Description: `LastOutput` doc says "most recent execution"; strict non-zero exit, mock `Throws`, `SelectAsync` degradation, and abandoned streams leave it stale.
- Suggestion: Narrow the doc to state exactly when it is set; note it is not for concurrent executions.
- Source: general
- Disposition notes: fixed — LastOutput XML doc, Design region, and command-execution.md narrowed; concurrency note added.

### M3 — Severity: suggestion — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs:281
- Description: No test for TTY child ignoring SIGINT (forceful path), TTY caller cancel, stream caller cancel.
- Suggestion: Add those tests.
- Source: general
- Disposition notes: fixed — added TtyPassthrough_Should_ForceKillAChildThatIgnoresSigInt and StreamStdout_Should_PropagateCallerCancellationAndLeaveLastOutputUnset. TTY caller-cancel and StreamCombined/strict-pipe not added (covered by shared TimeoutSession paths).

### M4 — Severity: suggestion — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.timeout.cs:26
- Description: DotNet test passes even if `WithTimeout` is a no-op.
- Suggestion: Mock with delay > timeout; assert `TimedOut` and exit 124.
- Source: general
- Disposition notes: fixed — MockDelayLongerThanTheTimeout_Should_ReportTimeout in dot-net.timeout.cs.

### M5 — Severity: nit — Status: fixed
- File: source/timewarp-amuru/core/command-result.cs:449
- Description: No-op try/catch in `WaitForMockTimeoutAsync`; dead `catch (TimeoutException)` in `SelectAsync`; duplicated timeout branch in `FinishCli` overload.
- Suggestion: Remove dead code; share `RememberTimeout`.
- Source: general
- Disposition notes: fixed — no-op try/catch and dead TimeoutException catch removed (verified unreachable); RememberTimeout(CommandOutput, bool) overload shared by both FinishCli paths.

### M6 — Severity: nit — Status: fixed
- File: source/timewarp-amuru/core/command-result.cs:300
- Description: Message reads "after 1 seconds".
- Suggestion: Singular for exactly 1.
- Source: general
- Disposition notes: fixed — singular "1 second"; mock test assertion updated.

### M7 — Severity: nit — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs:23
- Description: Graceful tests assume `python3` and handler install within 1 s.
- Suggestion: READY handshake / skip without python3.
- Source: general
- Disposition notes: wontfix (orchestrator) — CI is ubuntu with python3; Python installs the handler in tens of ms against a 1 s window; the existing tests already passed repeatedly. Adding a skip would hide a real regression on hosts that should run it.

## Duplicates / conflicts

- None (single reviewer).
