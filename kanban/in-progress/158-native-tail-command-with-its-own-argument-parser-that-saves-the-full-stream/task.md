# Native Tail command with its own argument parser that saves the full stream

## Description

Add `Tail` to Amuru's native commands, beside `GetContent`, `SelectString` and
`ReplaceInFiles` under `source/timewarp-amuru/native/`. It is a full rewrite of `tail`, not a
wrapper around the system one, so it behaves the same on Linux, macOS and native Windows and
can be called in-process from C#.

**Why.** Agents pipe long output through `tail -40` by reflex, the part that mattered is cut,
and they run the whole command again. Task 157 measured it: 139 re-runs after a `tail` cut in
79 days of Claude transcripts. Our `tail` saves the whole stream and says where it is.

**This corrects task 157's split.** Its Results say "Nothing goes in Amuru" and recommend a
POSIX `sh` shim. Steve rejected that on 2026-11-27: Nuru and Amuru exist so shell commands can
be overridden fully or partially, `sh` does not cover native Windows, and in-process use is part
of the point. The measurements in 157 still stand. The split does not.

**Scope here:** the library only. The `tail` executable and the agent PATH setup are
timewarp-ganda task 731.

## Requirements

### Argument parser

Unix tools do not share one option syntax, so `Tail` owns its parser. It turns `string[]` into
a typed options record. In-process callers build that record directly.

Forms to accept (counts from task 157, Q3):

- `-N` (bare number, for example `-40`), `-n N`, `-nN`, `--lines N`, `--lines=N`
- no arguments: last 10 lines of stdin
- `-n +N` and bare `+N`: from line N on
- `-c N`, `-cN`, `--bytes N`, with size suffixes (`1K`, `1M`, `b`), and `-c +N`
- `-q`, `-v`, `-z`
- `-f` (see Follow)
- bundled short flags, for example `-fn 20`, `-qn 5`
- zero or more file operands, before or after options; `-` is stdin
- `--` ends options
- a repeated option takes the last value, as the real `tail` does
- `--help` and `--version` are reported to the caller as parsed requests, not handled here

An option the parser does not know is an error that names the supported forms. It does not
fall back to another binary.

### Slice

- Work on bytes, not strings. Binary and non-UTF-8 input pass through unchanged.
- A last line with no trailing newline counts as a line.
- For a file, seek from the end. Do not read a large file whole.
- Several files print the `==> name <==` headers the real tool prints. `-q` and `-v` control
  them.
- Error text and exit codes for a missing or unreadable file match GNU `tail` closely enough
  that an agent reading them is not misled.

### Save the full stream

Only when reading stdin (a pipe) with a last-N-lines request:

- Save the whole stream to a file while reading.
- If lines were cut, write one line to stderr:
  `tail: showed last N of M lines; full output: <path>`.
- If nothing was cut, delete the file and write nothing.
- stdout is byte-identical to the real `tail` in both cases.
- Folder: `<temp>/tw-agent-streams/<user>/`, private to the user. Name carries a UTC stamp,
  the pid and a random suffix, so concurrent walks do not collide.
- Stop saving at 64 MiB and say so in the note.
- Remove saved files older than 24 hours, checked at most once an hour.
- The save is an option on the record, so an in-process caller can turn it off.

### Follow

`-f` in the first version is simple polling on one or more files: print appended bytes, handle
truncation. No `-F` rotation handling, no `--pid`, no `-s`. Those are errors that say so.

## Checklist

- [x] Options record and parser, with a test per accepted form above
- [x] Line slice and byte slice from a stream and from a file
- [x] Multiple files, headers, `-q`, `-v`, `-z`
- [x] Stream save, the stderr note, cap, and cleanup
- [x] Polling `-f`
- [x] Comparison tests: for each accepted form, stdout and exit code equal the system `tail`
      on the same input (skip a form the system `tail` on that OS does not have)
- [x] `x=$(seq 100 | tail -1)` style capture gets only the slice
- [x] Bash alias in `native/aliases/bash.cs` if the other native commands have one
- [x] User documentation for the command and the options record

## Goal (implementer restatement)

Amuru ships its own `Tail`, next to `SelectString` and `ReplaceInFiles`. Callers get the same stdout bytes and exit code as the system `tail` on Linux, macOS, and Windows, either from argv or from a `TailOptions` value. The parser accepts the forms agents type (`-40`, `-n +5`, `--lines=10`, `-c 1K`, `-q`, `-v`, `-z`, `-f`, bundled shorts, files on either side of the options, `--`). The last copy of a repeated option wins. `--help` and `--version` are flags, not text printed here. An unknown option, and `-F`, `--pid`, and `-s`, fail with an error that names the problem. Nothing is handed to another `tail`.

Slicing is on bytes. A final line with no newline still counts. A seekable stream is sliced from its current position through the end, reading a last-N request from the end of that range so a large file is not loaded whole. Several files get `==> name <==` headers. `-q` and `-v` turn those off or on. A missing or unreadable file uses an exit code and wording an agent would recognize from GNU tail.

A piped last-N-lines read copies the stream under `<temp>/tw-agent-streams/<user>/` while it prints the slice. Cut lines produce one stderr line with that path. An uncut stream deletes the file and stays quiet on stderr. stdout stays the system-tail bytes. The copy stops at 64 MiB and says so. Files older than 24 hours are removed, at most once an hour. `SaveStream` can turn the copy off.

That copy is best effort. If the folder cannot be created, the mode cannot be set, the cleanup stamp cannot be written, or a write fails, the slice is still printed, the exit code stays 0, stderr stays empty of that failure, and a partial file is removed. Closing the copy is part of the same rule: after a failed write or flush, `Dispose` can flush again and throw. That throw is swallowed, so the delete still runs and a later operand is not skipped. When this process creates the shared parent, it is mode `01777` so another user can add a private folder. The per-user folder stays `0700` and the file stays `0600`.

A pipe whose byte or line window is too large to keep in memory spills to a temp file that is mode `0600` and removed when the read ends.

`-f` polls one or more files, prints bytes that appear, and notices truncation. When headers are on and the next bytes come from a different file, that file's header is printed first. Renames, a pid, and a sleep interval from argv are errors.

A count that does not fit, including a decimal past `ulong.MaxValue` and a `b` suffix whose product does not fit, saturates. It is not rejected as an invalid number.

Done means the checklist is implemented, the round-1 findings and the round-2 write-failure residual are covered by tests, the user docs match, and `./bin/dev build`, each touched test runfile, and `ganda repo audit` pass.

## Session

- Created: 549bb8ea-d8c7-4ef7-9b7e-95b0ae556b64 (2026-11-27)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a21d9e-6047-7342-b282-13ee711456f7, max-turns 500, Done — 2026-11-27T16:30:43Z
- Review oracle round 1: claude (claude-opus-5-5), body tw-implementation-review, effort 3, roster general, turn cap 200. The outcome was 6 open findings and a fix pass requested (2026-11-27).
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 51eb38e5-ceaa-46bc-b869-4e401ae9fa11, max-turns 200, artifacts general.md, merged.md — 2026-11-27T16:33:40Z
- Review oracle round 2: claude (claude-opus-5-5), body tw-implementation-review, effort 2, roster general, turn cap 120. Scope: fix commit 7d50f96. Outcome: M2–M6 fixed, M1 still open for the write-failure path, fix pass requested (2026-11-28).

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6
- Fix pass after round 1: M1–M6 are implemented on this branch.
- Round 2: Blocked — open M1
- Fix pass after round 2: the saved copy's close swallows a failed flush, then the partial file is deleted. A later operand still runs.
## Notes

- `head` is out of scope. Draining the stream instead of exiting early changes `pipefail`
  status and keeps upstream running (task 157, Q2).
- Related: timewarp-nuru 602 (raw catch-all route), timewarp-ganda 731 (the executable and
  PATH), timewarp-flow 232 (hand-launched sessions).
- timewarp-ganda 731 needs this in a released Amuru package. Cutting that release waits for
  Steve's ask.
- Research and numbers: `kanban/done/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md`.

## Results

Native `Tail` lives under `source/timewarp-amuru/native/text/`. `TailParser` owns the argv grammar. `Commands.Tail` returns the stdout bytes. `Direct.Tail` writes caller streams. A non-seekable last-N-lines read saves the pipe under `<temp>/tw-agent-streams/<user>/` and, when lines were cut, writes one stderr line with that path. The save is best effort: a failure to create the folder, set its mode, write the cleanup stamp, or write the copy still prints the slice and exits 0, and a partial file is removed. The copy stream swallows `IOException` and `UnauthorizedAccessException` from close, so a failed flush during `Dispose` does not replace that result and does not skip a later operand. When this process creates the shared parent, that directory is mode `01777`. A large pipe window spills to a mode `0600` file that is removed when the read ends. `-f` polls, and when headers are on it prints a header before bytes from a different file. A seekable stream is sliced from its current position through the end. A count that overflows, including a decimal past `ulong.MaxValue` and a `b` suffix, saturates. `-F`, `--pid`, and `-s` are errors. `Commands.Tail` does not open the process standard input; the caller passes the stream. A bare `-N` or `+N` is digits only. A `+` token that is not a count is a file name. No package version bump.

Comparison on this machine is uutils tail 0.10.0. The equality tests skip live `-f` on a file, `--pid`, `-s`, `-F`, `--help`, `--version`, and bare `+0`. uutils prints nothing for `+0` and the whole file for `-n +0`. Tail follows `-n +0`. Stdin comparisons also run on a non-seekable stream with the save off.

### How to validate

Smoke: `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.full-store.cs`

Expect: `FullStore_Should_StillPrintTheSliceAndRemoveTheFile` passes. On a full tmpfs the slice is `20` and a newline, then `keep` and a newline, the exit code is 0, stderr is empty, and no `tail-*` file remains. The same holds for `Direct.Tail`, and the file operand after stdin is still read.

- `./bin/dev build` — passed, 0 warnings, 0 errors
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.full-store.cs` — passed, 1 test
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.cs` — passed, 12 tests
- `ganda repo audit` — passed, 43 passed, 0 failed, 0 warnings
- The product commit's post-commit `ganda repo attest` failed with `Text file busy` on `bin/dev`. This note commits so the hook runs again.

Each test file above is a standalone Jaribu runfile (`#if !JARIBU_MULTI`). This fix pass did not re-run `tail-parser.parse.cs`, `commands.tail.cs`, `commands.tail.follow.cs`, or `commands.tail.compare.cs`.

### Review

- Round 1 used effort 3 and the general roster. Findings are in `review/round-1/general.md` and `review/round-1/merged.md`. The framework is `review/review-framework.md`.
- Counts after round 1: 1 bug, 3 suggestions, and 2 nits. All 6 were open (M1–M6).
- The fix pass implements M1–M6. Disposition is still pending. Round 2 re-reviews this branch. `review/disposition.md` is not written yet.
- Round 2 (effort 2, general) reviewed fix commit 7d50f96. Files: `review/round-2/general.md` and `review/round-2/merged.md`. Counts: 1 bug open (M1), 3 suggestions fixed (M2–M4), 2 nits fixed (M5, M6).
- Fix pass after round 2 closes M1's write-failure path. `CopyStream.Dispose` swallows `IOException` and `UnauthorizedAccessException`, so a second flush during close does not escape `finally`. The partial file is deleted and a later operand still runs. `commands.tail.save.full-store.cs` drives that path on a full tmpfs for `Commands.Tail` and `Direct.Tail`. Disposition waits for round 3.
