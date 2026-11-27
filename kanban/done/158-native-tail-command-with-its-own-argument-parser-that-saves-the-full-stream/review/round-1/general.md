# Round 1 — general
**Date:** 2026-11-27
**Scope reviewed:** branch vs master: `source/timewarp-amuru/native/text/*tail*`, `native/aliases/bash.cs`, `PublicAPI.Unshipped.txt`, the five new test runfiles plus `bash-aliases.cs`, docs (`native-text-commands.md`, `overview.md`, `readme.md`), and the task.md `## Results` record.

## Summary

The change adds a native `Tail` (parser, byte slice, headers, saved stdin, polling follow) with strong parser and slice tests and good docs. The parser grammar, the slice algorithms, the note text and the cleanup throttle match the task. The main risk is the save path: it is an add-on, but any failure in it (folder not creatable, chmod refused, disk full mid-stream) makes the whole `tail` fail and print nothing, and from `Direct.Tail` it can throw. Secondary themes: a spill temp file that is not private, follow output without per-file headers, and comparison tests that only run our side on a seekable stream.

The implementer restatement matches the task. No misread.

No commands were run. All findings come from reading the diff.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/native/text/tail-engine.cs:163-199, source/timewarp-amuru/native/text/tail-store.cs:19-45
- Description: A failure in the stream save makes the slice itself fail. `TailStore.Create` runs inside the `try` in `ReadStandardInput`. These calls can throw: `Directory.CreateDirectory`, `TightenDirectory` (`File.SetUnixFileMode`), the cleanup stamp write, and `new FileStream`. During the read, `TailCaptureStream.Capture` writes to the saved file, so ENOSPC also throws there.
  - An `IOException` is caught. stderr gets `tail: error reading 'standard input': …` and the exit code is 1. stdout is empty, because the forward slice writes only at end of stream.
  - An `UnauthorizedAccessException` is not caught in the engine. `Direct.Tail` and `Bash.TailDirect` throw it to the caller. `Commands.Tail` turns it into exit 1 with empty stdout.
  - A realistic trigger is a shared machine. `<temp>/tw-agent-streams` is created with the default mode (0777 & umask, usually 0755), and only the per-user leaf is tightened. The first user who runs Tail owns the parent, so the next user gets EACCES creating their own `<user>` folder, and every piped `tail -N` for that user fails. A leaf that already exists and belongs to someone else fails the same way, at the chmod. A full disk or a read-only temp has the same effect.
  - This breaks "stdout is byte-identical to the real `tail` in both cases". The save exists to help agents, and here it makes the slice disappear.
- Suggestion: Treat the save as best effort. If creating the store fails, slice without the capture. If a capture write fails mid-stream, stop capturing (as the cap does), finish the slice, and delete the partial file or say in the note that the save failed. Create the shared parent so other users can add their own folder (for example sticky 01777, set only when this process created it), or catch the failure and skip the save. Add a test where `StreamDirectory` points at an unwritable path and the slice is still printed with exit 0.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-amuru/native/text/tail-slice.cs:410-432
- Description: `Spill` copies the whole pipe to `Path.GetTempPath()/amuru-tail-spill-<guid>`. That happens for `-c N` with N over 8 MiB, or `-n N` with N over 1,000,000, on a pipe. The file is created with the default mode (0644 under a typical umask) in the shared temp folder, so other local users can read the piped content while tail runs. The task asks for saved stream data to be private to the user, and the store does that (0700/0600). This second copy is not private. Its size also has no cap: the 64 MiB limit applies only to the saved stream.
- Suggestion: Create the spill with `FileStreamOptions { UnixCreateMode = UserRead | UserWrite, Options = FileOptions.DeleteOnClose }`, or put it in the private store folder.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-amuru/native/text/tail-engine.cs:268-306
- Description: With `-f` on several files, `Poll` writes appended bytes with no `==> name <==` header when the output switches to a different file. GNU and uutils `tail -f a b` print `\n==> b <==\n` before bytes from a file that was not the last one written. Without it, an agent cannot tell which file a new line came from, even though the initial slice had headers. The docs (`native-text-commands.md`, Follow) do not mention the difference.
- Suggestion: Track the last file written. When `state.Headers` is on and the source changes, write the header (with its leading blank line) before the appended bytes. Add a two-file follow test.
- Status: open

### Issue 4 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/native/text/commands.tail.compare.cs:243-264
- Description: `Compare` gives the system `tail` a pipe (`PipeSource.FromBytes`) but gives ours a `MemoryStream`, which is seekable. Every stdin comparison therefore runs our seek-from-end paths. The pipe paths, which agents actually hit, are never checked against system tail: the forward line window (`CopyLastLinesForward`), the byte ring buffer (`CopyLastBytes` on a non-seekable stream), the non-seekable `-c +N` skip, and the save wrapper. The save tests cover `-n N` on a pipe for a few inputs only. `-c N`, `-z`, a partial last line, and binary input on a pipe have no equality check.
- Suggestion: In `Compare`, also run ours on a non-seekable wrapper (with `SaveStream = false`, or `StreamDirectory` set to a temp folder) and assert the same stdout and exit code.
- Status: open

### Issue 5 — Severity: nit
- File: source/timewarp-amuru/native/text/tail-slice.cs:56-66, 145-151, 229-277
- Description: The seekable paths use absolute offsets from 0 (`input.Position = skip`, `length - count`, and `Position = 0` when there are fewer than N lines). They ignore where the stream already was. A caller's stream that was partly read, or `{ read first; tail; } < file`, prints bytes from before the current position. GNU tail slices from the current offset.
- Suggestion: Record `start = input.Position` and treat `[start, Length)` as the input. Or document that a seekable stream is sliced from offset 0.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-amuru/native/text/tail-numbers.cs:47-50, 74-77
- Description: The Design region says overflow saturates, and the docs say "A count that overflows saturates". Two cases do not:
  - A plain decimal count too big for `ulong`, such as `-n 99999999999999999999999`, fails `ulong.TryParse` and is reported as `invalid number of lines`.
  - The `b` suffix returns false on overflow, so it is also reported as invalid.
  GNU tail treats both as "everything". Only the K/M/G… path saturates.
- Suggestion: Saturate in both places, to match the docs, and add a parser test for each.
- Status: open
