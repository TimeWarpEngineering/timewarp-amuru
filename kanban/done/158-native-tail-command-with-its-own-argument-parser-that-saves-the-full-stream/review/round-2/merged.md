# Round 2 — merged findings
**Date:** 2026-11-28
**Sources:** general
**Scope:** fix delta `7d50f96` vs round-1 `merged.md` (M1–M6)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-amuru/native/text/tail-engine.cs:190-193, 215-229
- Description: Partly fixed. Store creation, modes, the cleanup stamp, and the shared parent no longer fail the tail. A write failure on the saved copy still does.
  - The copy is a buffered `FileStream`. On a full disk, `TryFlush` returns false (or `Capture` sets `Failed` after a failed buffer flush) with bytes still in the buffer.
  - `finally` calls `saved?.Dispose()`. `Dispose` flushes again and throws. The exception comes from `finally`, so the `catch (IOException)` does not handle it, and the `File.Delete(path)` after it never runs.
  - Result: exit 1, stderr replaced by `tail: No space left on device ...` in `Commands.Tail`, an exception out of `Direct.Tail`, later operands skipped, and a partial file left behind. This breaks the restatement's "a write fails, the slice is still printed, the exit code stays 0, and a partial file is removed".
  - Targeted check: a throwaway runfile wrote 10 bytes to a `FileStream` on `/dev/full`, then called `Flush` and `Dispose`. Both threw `No space left on device`. Reason: to confirm `Dispose` rethrows after a failed flush.
- Suggestion: Catch `IOException` and `UnauthorizedAccessException` around `saved?.Dispose()` (and `capture?.Dispose()`) in `finally` so the delete still runs. Optionally open the copy with `BufferSize = 0`. Add a test that drives a write or flush failure on the copy and expects the slice, exit 0, empty stderr, and no file left.
- Source: general (round 1 and round 2)
- Disposition notes: Round 2 — still open for the write-failure path. The create-path half is fixed and tested (`UnwritableStore_Should_StillPrintTheSlice`, `DeniedStore_Should_StillPrintTheSlice`).

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru/native/text/tail-slice.cs:423-445
- Disposition notes: Spill is created 0600 on Unix with `FileOptions.DeleteOnClose`. Covered by `LargeByteCountOnAPipe_Should_KeepTheTail`.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru/native/text/tail-engine.cs:296-312, 353-361
- Disposition notes: `WriteFollowHeader` writes the header when the output file changes. Covered by `FollowSeveralFiles_Should_PrintAHeaderWhenTheSourceChanges`. Docs updated.

### M4 — Severity: suggestion — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/native/text/commands.tail.compare.cs:268-280
- Disposition notes: Each stdin case also runs on a non-seekable stream with the save off and is compared with the system tail.

### M5 — Severity: nit — Status: fixed
- File: source/timewarp-amuru/native/text/tail-slice.cs:58-70, 149-160, 244-288
- Disposition notes: Seekable slices use `[Position, Length)`. Covered by `SeekableOffset_Should_SliceFromTheCurrentPosition`. Docs updated.

### M6 — Severity: nit — Status: fixed
- File: source/timewarp-amuru/native/text/tail-numbers.cs:48, 67-75, 120-135
- Disposition notes: Decimal and `b`-suffix overflow saturate. Covered by `OverflowingDecimal_Should_Saturate`.

## Duplicates / conflicts

- Single reviewer. The round-2 residual is carried on M1, not given a new id.

## Evidence

- No screenshots were listed under `evidence/` for this launch, and none were viewed.
- One targeted command was run (the `/dev/full` `FileStream` probe above) to confirm M1's residual. No build, full test run, audit, or hook was run. The recorded gates in `task.md` `## Results` are listed as passing; no finding disputes that record. The record does not include a test that drives a write failure on the saved copy, which is why the residual went unnoticed.
