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

- [ ] Options record and parser, with a test per accepted form above
- [ ] Line slice and byte slice from a stream and from a file
- [ ] Multiple files, headers, `-q`, `-v`, `-z`
- [ ] Stream save, the stderr note, cap, and cleanup
- [ ] Polling `-f`
- [ ] Comparison tests: for each accepted form, stdout and exit code equal the system `tail`
      on the same input (skip a form the system `tail` on that OS does not have)
- [ ] `x=$(seq 100 | tail -1)` style capture gets only the slice
- [ ] Bash alias in `native/aliases/bash.cs` if the other native commands have one
- [ ] User documentation for the command and the options record

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
