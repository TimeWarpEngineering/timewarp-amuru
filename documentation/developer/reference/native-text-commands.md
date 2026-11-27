# Native text commands

`SelectString`, `ReplaceInFiles`, and `Tail` live in `TimeWarp.Amuru.Native.Text`. Select and replace follow the file-system split: `Direct` returns typed results and throws, `Commands` returns `CommandOutput` and does not throw. `Tail` is a byte filter, so both layers keep the raw bytes: `Commands.Tail` returns `TailOutput` and `Direct.Tail` writes the caller's streams. Bash aliases are `Grep` / `GrepDirect`, `Sed` / `SedDirect`, and `Tail` / `TailDirect`.

Input is one path, a sequence of paths, or a directory plus a glob. A sequence is checked before any work starts: a null or blank entry makes `Direct` throw `ArgumentException` and makes `Commands` return an error exit code, and no file is read or written. Globs go through `FindItem` and `FindCriteria`. A name-only glob matches the file name. A glob that contains a slash or `**` matches the relative path. `Include` and `Exclude` on the options are extra filters of the same kind.

`SelectString` also accepts a `TextReader` or a `Stream`. There is no string-content overload, because a string is already a path. Pass a `StringReader` for in-memory text. A reader or stream with no path is labeled `-`. Include and exclude globs are ignored for those inputs.

## SelectString

The pattern is a regular expression unless `SelectStringOptions.SimpleMatch` is set. Matching is line by line, so `^` and `$` mean the line. Options cover case, invert, context before and after, a per-file limit on matching lines (`MaxMatches`, like `grep -m`), and include/exclude globs. Line numbers are always on the result.

`Direct.SelectString` returns `IAsyncEnumerable<TextMatch>` (`Path`, `LineNumber`, `Line`, `Match`, `ContextBefore`, `ContextAfter`). It reads one line at a time. An inverted hit still carries a `Match`, and that match is unsuccessful. A line with two hits yields two results. `MaxMatches` counts lines, so both hits on a counted line are returned.

A file whose first 8000 bytes contain a NUL byte, and that has no UTF-16/32 BOM, is treated as binary and skipped with no results. This includes UTF-16 and UTF-32 files without a BOM. Unlike grep, no "Binary file matches" line is written. `TextReader` and `Stream` inputs are not checked.

`Commands.SelectString` writes `path:line:text` once per matching line, even when the line has several hits, and uses grep's exit codes: 0 when something matched, 1 when nothing matched, 2 on a regex or I/O error. A missing input path, an unreadable directory, or an unreadable file is written to stderr as `SelectString: <path>: <reason>`. The walk continues, output already produced is kept, and the exit code is 2. Context lines stay on `TextMatch`. They are not written to stdout.

## ReplaceInFiles

The replacement honors `$1` and `${name}`. Options cover a literal pattern, dry run, `.bak` backup, a per-file replacement limit, case, and `RegexOptions.Multiline` / `Singleline`.

The newline style is CRLF when the first `\n` follows a `\r`, otherwise LF. A file with no `\n` at all that contains `\r` uses CR. The file is matched after `\r\n` is folded to `\n` (or, for a CR file, `\r` to `\n`), then written back in the detected style. A lone `\r` in an LF or CRLF file stays a literal character, so a pattern can match it. When the text changes, the whole file is written in the detected style, so a file with mixed newlines comes out uniform. A byte-order mark is kept. A file with no BOM is written as UTF-8 without one. A trailing newline is kept when the file had one. Newlines that the replacement itself inserts are written in the detected style.

Decoding is strict. A file that is not valid text in its detected encoding (for example Latin-1 bytes in a file with no BOM) is never rewritten: `Direct.ReplaceInFiles` throws `InvalidDataException` with the path in the message, and `Commands.ReplaceInFiles` writes `ReplaceInFiles: <path>: not valid utf-8 text` to stderr, sets exit code 1, and keeps walking. A replacement that leaves text the encoding cannot write, such as half of a surrogate pair, fails the same way with `replacement produced invalid utf-8 text`. This check also runs on a dry run. A UTF-8 or no-BOM file that contains a NUL byte is binary and is skipped: zero replacements, `Changed` false, no write. A UTF-16 or UTF-32 file without a BOM contains NUL bytes, so it is also treated as binary and skipped silently. Add a BOM to such a file to have it edited or searched.

`Direct.ReplaceInFiles` returns one `ReplaceResult` per file, including files that did not change: `Path`, `ReplacementCount`, `Changed`, `Preview`, `BackupPath`. `Changed` is false when the replacement leaves the text identical, so the file is not rewritten and its mtime stays. Dry run sets `Preview` to unified-diff lines, writes nothing, and does not create a backup. A backup is `path.bak`, and only when the text changes.

The write is atomic: a temp file in the same directory, then a move over the original. The temp file is removed if the move fails. A symlink is followed to its final target, like `sed --follow-symlinks`: the target is rewritten (temp file in the target's directory) and the link stays a link. With `Backup`, the `.bak` goes beside the link path that was passed in (`link.bak`), not beside the target, and it is a regular file holding the target's old contents. A hard link is split, because the move replaces the directory entry. Only the Unix mode is copied to the new file; owner, ACLs, and extended attributes are not.

`Commands.ReplaceInFiles` writes `path: N replacement(s)` for each changed file, including a dry run. Exit code 0 means the walk finished, even when nothing matched. Exit code 1 means a regex or I/O error. A missing input path, an unreadable directory, or a file that cannot be read or decoded is written to stderr as `ReplaceInFiles: <path>: <reason>`. The walk continues, and stdout still lists every file already changed.

## Tail

`Tail` is a byte-for-byte `tail`. It does not call the system binary. The same slice is produced on Linux, macOS, and Windows. `TailParser.Parse` turns an argument vector into a `TailOptions` value and the file operands. An in-process caller builds `TailOptions` directly and skips the parser. `Commands.Tail` and `Direct.Tail` do not open the process standard input. Pass the stream when an operand reads stdin.

`TailOptions.Measure` is a `TailMeasure`: a unit (lines or bytes), a count, and whether the count starts at the beginning. The default is the last 10 lines. `Headers` is `Auto` (a header only when there is more than one operand), `Never` (`-q`), or `Always` (`-v`). `ZeroTerminated` uses NUL as the line delimiter (`-z`). `Follow` polls files (`-f`). `SaveStream` defaults to on.

### Argument forms

The parser accepts:

- `-N`, `-n N`, `-nN`, `--lines N`, `--lines=N` for the last N lines. A bare `-N` or `+N` is digits only
- no arguments, which reads stdin and keeps the last 10 lines
- `-n +N`, `--lines=+N`, and a bare `+N` to start at line N (1-based). `+0` starts at the first line. A `+` token that is not a count, such as `+1a`, is a file name
- `-c N`, `-cN`, `--bytes N`, `--bytes=N`, and `-c +N` for bytes. A leading `-` on the number means from the end
- size suffixes: `b` is 512, `K` / `KiB` is 1024, `KB` is 1000, and the same pattern for M, G, T, P, E, Z, and Y. A count that overflows saturates
- `-q` / `--quiet` / `--silent`, `-v` / `--verbose`, `-z` / `--zero-terminated`
- `-f` / `--follow` / `--follow=descriptor`
- bundled short flags, such as `-fn 20` and `-qn 5`
- file operands before or after options. `-` is stdin. `--` ends options
- a repeated option keeps the last value

`--help` and `--version` set `TailOptions.Help` and `TailOptions.Version` and stop the parse. Tail does not print help or a version string.

An unknown option is an error that lists the supported forms. `-F`, `--follow=name`, `--pid`, and `-s` / `--sleep-interval` are errors that name the missing behavior. Nothing falls through to another `tail`.

### Slice

The slice is bytes. A final line with no newline counts as a line. Binary input is copied unchanged. A seekable stream is sliced from its current position through its end. For a last-N request, Tail seeks from the end of that range and does not read the range whole. A from-start request reads forward from the current position and does not buffer the skipped prefix.

Several operands print headers of the form `==> name <==`. A blank line separates a header from the previous operand. Stdin is named `standard input`. `-q` suppresses headers. `-v` prints them even for one operand. With no flag, headers appear only when there is more than one operand.

A missing file writes `tail: cannot open '<path>' for reading: No such file or directory` and continues. Permission denied and a directory use the same shape an agent expects from GNU tail. The exit code is 1 when any operand fails, and 0 otherwise.

`TailOutput.Stdout` and `Stderr` are the exact bytes. `StdoutText` is the UTF-8 view of those bytes.

### Saved stdin

When stdin cannot be seeked (a pipe), the request is the last N lines, `SaveStream` is on, and follow is off, Tail copies the pipe into `<temp>/tw-agent-streams/<user>/` while it builds the slice. The file name carries a UTC stamp, the process id, and a random suffix. On Unix, when this process creates the shared parent `<temp>/tw-agent-streams`, that directory is mode `01777` so each user can create a folder under it. The per-user folder is mode `0700` and the file is mode `0600`. Creating the folder, setting those modes, writing the cleanup stamp, and writing the copy are best effort. When one of them fails, Tail still prints the slice, deletes a partial copy, and writes no note. Closing the copy is part of that path: a flush during close does not become an error and does not skip later operands.

If lines were dropped, stderr gains one line:

```text
tail: showed last N of M lines; full output: <path>
```

stdout stays the slice alone, so a capture of stdout does not include the note. If nothing was dropped, the file is deleted and stderr is unchanged. The copy stops at `SaveLimit` (64 MiB unless the caller sets another positive limit) and the note then ends with `(stopped at 64 MiB)`. Files in that folder last written more than 24 hours ago are deleted, and that check runs at most once an hour. `StreamDirectory` overrides the folder.

### Follow

`-f` prints the initial slice, then polls each regular file until the cancellation token fires. Bytes appended after the slice are written. When headers are on and those bytes come from a different file than the previous output, Tail writes that file's header first. When a file shrinks, stderr gets `tail: <path>: file truncated` and the new contents are written from the start. The poll interval is `FollowInterval` (one second unless the caller changes it). The command line cannot set it. A pipe is read until it closes and is not polled. Follow of a file requires a cancellation token. Rotation (`-F`), `--pid`, and `-s` are rejected by the parser.
