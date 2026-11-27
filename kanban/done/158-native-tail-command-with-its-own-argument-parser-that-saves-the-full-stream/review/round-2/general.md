# Round 2 — general
**Date:** 2026-11-28
**Scope reviewed:** fix delta `7d50f96` (fix(native): keep the tail slice when saving the stream fails) against round-1 M1–M6, plus the updated `task.md` `## Goal (implementer restatement)`, `## Results`, and `### How to validate`. Effort 2, roster general.

## Summary

The fix pass covers five of the six round-1 findings with code, docs, and tests. The spill file is private and deleted on close (M2). Follow prints a header when the output changes files (M3). The comparison now also runs a non-seekable pipe (M4). A seekable stream is sliced from its current position (M5). An overflowing count saturates (M6). M1 is fixed for store creation, mode, stamp, and the shared parent. It is not fixed for a write failure on the saved copy: the buffered `FileStream` is disposed in `finally` with no guard, and `Dispose` flushes the same bytes again and throws. A full disk still gives exit 1 and a leftover partial file.

## Issues

### Issue 1 — Severity: bug (residual of round-1 M1)
- File: source/timewarp-amuru/native/text/tail-engine.cs:190-193, 215-229
- Description: A write failure on the saved copy still escapes as an `IOException`.
  - `TailStore.TryCreate` opens the copy with the default 4096-byte buffer. Small pipe reads (a short output, or a producer that writes a line at a time) sit in that buffer.
  - When the disk is full, `TryFlush(saved)` catches the flush failure and returns. A failed `FlushWrite` inside `TailCaptureStream.Capture` also sets `Failed` with the bytes still buffered.
  - The `finally` block then calls `saved?.Dispose()`. `FileStream.Dispose` flushes the buffered bytes again and rethrows when `disposing` is true. That exception comes from `finally`, so the `catch (IOException)` on the same `try` does not see it.
  - The `File.Delete(path)` that follows in `finally` never runs, so the partial file stays.
  - `Commands.Tail` turns the exception into exit 1 and replaces stderr with `tail: No space left on device ...`. `Direct.Tail` lets it escape to the caller. Later operands are not processed.
  - This breaks the restatement's "or a write fails, the slice is still printed, the exit code stays 0, and a partial file is removed".
  - Targeted check (the one allowed command): a throwaway runfile in a `mktemp -d` folder opened `/dev/full` as a `FileStream`, wrote 10 bytes, then called `Flush` and `Dispose`. Reason: to confirm that `Dispose` rethrows after a failed `Flush`. Output: `flush threw: No space left on device : '/dev/full'` then `dispose threw: No space left on device : '/dev/full'`.
- Suggestion: Guard the dispose of `saved` in `finally` (catch `IOException` and `UnauthorizedAccessException`), and make sure the delete still runs after it. Opening the copy unbuffered (`BufferSize = 0` in `FileStreamOptions`) also stops the bytes from lingering. Add a test that drives a write failure, for example a unit test of the capture and engine path with a stream that throws on write or flush, or a seam on `TailStore` that returns such a stream.
- Status: open

## Re-verified from round 1

- M2 — fixed. `tail-slice.cs:423-445` creates the spill with `UnixCreateMode` 0600 and `FileOptions.DeleteOnClose`. `LargeByteCountOnAPipe_Should_KeepTheTail` covers the spill path.
- M3 — fixed. `WriteFollowHeader` (`tail-engine.cs:353-361`) and `RunState.LastName`. `FollowSeveralFiles_Should_PrintAHeaderWhenTheSourceChanges` checks both directions. Docs updated.
- M4 — fixed. `commands.tail.compare.cs:268-280` reruns each stdin case on a non-seekable `PipeStream` with `SaveStream = false` and compares stdout and exit code.
- M5 — fixed. All seekable slice paths use `[Position, Length)`. `SeekableOffset_Should_SliceFromTheCurrentPosition` covers last lines, one line, last bytes, from byte, and from line.
- M6 — fixed. `ParseMagnitude` saturates on overflow. The `b` suffix saturates. An empty digit run is still rejected earlier (`tail-numbers.cs:37-40`). `OverflowingDecimal_Should_Saturate` covers it.
- M1 — partly fixed. See Issue 1 above. `TryCreate`, `EnsureSharedParent`, and `Cleanup` now catch, and `UnwritableStore` and `DeniedStore` tests cover the create path.

## Other notes (not findings)

- The test runfiles changed mode from 100644 to 100755. That matches the sibling runfiles and the recorded `ganda repo audit --fix` (`runfile-executable`).
- `EnsureSharedParent` leaves an existing parent alone. If another user's 0755 parent already exists, the per-user folder cannot be created and the save is skipped. The slice still prints, so this is best effort as intended.
