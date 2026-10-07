# Round 2 — merged findings (re-verification)
**Date:** 2026-10-07
**Sources:** review oracle re-verification of fix delta

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 1 |

## Resolved prior

- M1 fixed — `GC.KeepAlive(lines)` / `GC.KeepAlive(output)` present after the second sample.
- M2 fixed — `InterleavedScript` alternates writes; `InterleavedStreams_Should_OrderStdoutBeforeStderr` passes.
- M3 wontfix — unchanged rationale.

## New findings

- None on the fix delta.

## Verification

- `dotnet run .../command-result.large-output.cs`: 4 passed, 7.9 s wall clock.
- `./bin/dev build`: 0 warnings, 0 errors.
- `./bin/dev test`: 733 total, 732 passed, 1 skipped (pre-existing), 0 failed.
- `ganda repo audit`: passes all checks.
