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

Amuru grows a native `Tail` next to `SelectString` and `ReplaceInFiles`. It is a real tail, not a call to the system binary: the same bytes and exit codes on Linux, macOS, and Windows, and a C# caller can pass a typed options value instead of argv.

The parser accepts the forms agents actually type (`-40`, `-n 20`, `-n+5`, `--lines=10`, `-c 1K`, `-q`, `-v`, `-z`, `-f`, bundled shorts, files before or after options, `--`). A repeated option keeps the last value. `--help` and `--version` come back as flags and are not printed here. An unknown option, and `-F`, `--pid`, and `-s`, fail with text that names what is wrong. Nothing falls through to another `tail`.

Slicing works on bytes. A final line with no newline still counts. A file is read from the end, not loaded whole. Several files get `==> name <==` headers; `-q` and `-v` turn those off or on. A missing or unreadable file uses an exit code and a message an agent would recognize from GNU tail.

When the input is a pipe and the request is "last N lines", the whole stream is saved under `<temp>/tw-agent-streams/<user>/` while the slice is printed. If lines were dropped, stderr gets one line pointing at that file. If nothing was dropped, the file is removed and stderr stays quiet. stdout stays the same bytes as system tail. The save stops at 64 MiB and says so. Files older than 24 hours are removed, and that sweep runs at most once an hour. The save is a flag on the options, so an in-process caller can turn it off.

`-f` polls one or more files, prints bytes that appear, and notices truncation. It does not follow renames, a pid, or a custom sleep from argv.

Done means the checklist below is implemented, user docs describe the command and the options type, the bash alias exists, and the build, the new test files, and `ganda repo audit` pass.

## Session

- Created: 549bb8ea-d8c7-4ef7-9b7e-95b0ae556b64 (2026-11-27)

## Notes

- `head` is out of scope. Draining the stream instead of exiting early changes `pipefail`
  status and keeps upstream running (task 157, Q2).
- Related: timewarp-nuru 602 (raw catch-all route), timewarp-ganda 731 (the executable and
  PATH), timewarp-flow 232 (hand-launched sessions).
- timewarp-ganda 731 needs this in a released Amuru package. Cutting that release waits for
  Steve's ask.
- Research and numbers: `kanban/done/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md`.

## Results

Native `Tail` lives under `source/timewarp-amuru/native/text/`. `TailParser` owns the argv grammar. `Commands.Tail` returns the stdout bytes. `Direct.Tail` writes caller streams. A non-seekable last-N-lines read saves the pipe under `<temp>/tw-agent-streams/<user>/` and, when lines were cut, writes one stderr line with that path. `-f` polls. `-F`, `--pid`, and `-s` are errors. `Commands.Tail` does not open the process standard input; the caller passes the stream. A bare `-N` or `+N` is digits only. A `+` token that is not a count is a file name. No package version bump.

Comparison on this machine is uutils tail 0.10.0. The equality tests skip live `-f` on a file, `--pid`, `-s`, `-F`, `--help`, `--version`, and bare `+0`. uutils prints nothing for `+0` and the whole file for `-n +0`. Tail follows `-n +0`.

### How to validate

Smoke: `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.cs`

Expect: the pipe-capture test passes. Stdout is `100` and a newline, and does not contain the note. Stderr contains `tail: showed last 1 of 100 lines; full output:` and the saved name matches `tail-<utc>-<pid>-<suffix>`.

- `./bin/dev build` — passed, 0 warnings, 0 errors
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/tail-parser.parse.cs` — passed, 48 tests
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.cs` — passed, 19 tests
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.cs` — passed, 10 tests
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.follow.cs` — passed, 5 tests
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/commands.tail.compare.cs` — passed, 8 tests
- `dotnet run --file tests/timewarp-amuru/single-file-tests/native/text/bash-aliases.cs` — passed, 7 tests
- `ganda repo audit` — passed, 42 passed, 0 failed, 1 warning (`vscode-window-icon`, already present)

Each test file above is a standalone Jaribu runfile (`#if !JARIBU_MULTI`).
