# Round 1 — merged findings
**Date:** 2026-10-07
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 1 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs:209
- Description: `CaptureTwoMillionAsync` does not keep `output` / `lines` alive past the second `GC.GetTotalMemory(true)`; an optimizing JIT may collect them early, shrinking the capture delta and weakening the 25% ratio assertion.
- Suggestion: `GC.KeepAlive(lines); GC.KeepAlive(output);` after the second sample.
- Source: general
- Disposition notes: Fixed — KeepAlive added; Design region notes why.

### M2 — Severity: nit — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs:38
- Description: The "interleaved" child wrote all stdout and then all stderr, so it did not interleave and the ordering assertion was trivially satisfied by the child itself.
- Suggestion: Alternate writes per line.
- Source: general
- Disposition notes: Fixed — script alternates stdout/stderr per index; `RunAndCaptureAsync` still yields all stdout then all stderr (test passes).

### M3 — Severity: nit — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs:163
- Description: `RequireLinux` guard does not check for `python3` / `seq`.
- Suggestion: Probe for the binaries.
- Source: general
- Disposition notes: wontfix (orchestrator) — task brief explicitly allows a python child on Linux CI (ubuntu ships python3 and coreutils); a missing binary fails loudly, which is preferable to a silent skip.

## Duplicates / conflicts

- None.
