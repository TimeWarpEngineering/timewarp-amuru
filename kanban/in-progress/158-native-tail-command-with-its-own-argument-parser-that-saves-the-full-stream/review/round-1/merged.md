# Round 1 — merged findings
**Date:** 2026-11-27
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 2 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-amuru/native/text/tail-engine.cs:163-199; source/timewarp-amuru/native/text/tail-store.cs:19-45
- Description: Any failure in the best-effort stream save makes the whole tail fail.
  - The triggers: the store folder cannot be created, the chmod is refused, the cleanup stamp cannot be written, or the capture write hits ENOSPC mid-stream.
  - An `IOException` gives exit 1, `error reading 'standard input'`, and empty stdout.
  - An `UnauthorizedAccessException` escapes `Direct.Tail` and `Bash.TailDirect`. `Commands.Tail` turns it into exit 1 with empty stdout.
  - A shared parent `<temp>/tw-agent-streams` created 0755 by the first user denies every other user's leaf folder. On a shared host that breaks every piped `tail -N` for those users.
  - This violates "stdout is byte-identical to the real tail in both cases".
- Suggestion:
  - Make the save best effort. If the store cannot be created, slice without the capture. If a capture write fails, stop capturing and finish the slice.
  - Make the shared parent usable by other users, for example sticky 01777 when this process created it, or skip the save.
  - Add a test with an unwritable `StreamDirectory` that expects the slice and exit 0.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/native/text/tail-slice.cs:410-432
- Description: `Spill` writes the whole pipe to `<temp>/amuru-tail-spill-<guid>` with the default mode (0644 under a typical umask). That happens for `-c` over 8 MiB or `-n` over 1M on a pipe. Other local users can read the piped data while it runs, and there is no size cap. The saved stream is private (0700/0600). This copy is not.
- Suggestion: Create the spill with `UnixCreateMode` 0600 and `FileOptions.DeleteOnClose`, or put it in the private store folder.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/native/text/tail-engine.cs:268-306
- Description: When `-f` follows several files, the poll loop writes appended bytes with no `==> name <==` header when the output switches files. GNU and uutils print one, so an agent can tell which file a new line came from. The docs do not mention the difference.
- Suggestion: When headers are on and the output source changes, write the header before the appended bytes. Add a two-file follow test.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: tests/timewarp-amuru/single-file-tests/native/text/commands.tail.compare.cs:243-264
- Description: The comparison gives the system tail a pipe but gives ours a seekable `MemoryStream`. None of the pipe slice paths is compared to the system tail: the forward line window, the byte ring buffer, the non-seekable `-c +N`, and `-z` on a pipe.
- Suggestion: Also run ours on a non-seekable wrapper, with the save off or in a temp folder, and assert the same stdout and exit code.
- Source: general
- Disposition notes:

### M5 — Severity: nit — Status: open
- File: source/timewarp-amuru/native/text/tail-slice.cs:56-66, 145-151, 229-277
- Description: Seekable slicing uses absolute offsets from 0 and ignores the stream's current position. Content from before that position can be printed. GNU slices from the current offset.
- Suggestion: Slice `[Position, Length)`, or document the offset-0 behavior.
- Source: general
- Disposition notes:

### M6 — Severity: nit — Status: open
- File: source/timewarp-amuru/native/text/tail-numbers.cs:47-50, 74-77
- Description: Some overflowing counts are rejected as `invalid number`. That covers a decimal count above `ulong.MaxValue`, and the `b` suffix on overflow. The docs and the Design region both say overflow saturates.
- Suggestion: Saturate in both places and add parser tests.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. No duplicates.

## Evidence

- No screenshots were listed under `evidence/` for this launch, and none were viewed.
- No commands were run. All findings come from reading the diff, `task.md` `## Results`, and `### How to validate`. The recorded gates (build, the six test runfiles, `ganda repo audit`) are listed as passing. No finding disputes that record.
