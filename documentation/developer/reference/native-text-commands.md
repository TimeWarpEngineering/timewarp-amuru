# Native text commands

`SelectString` and `ReplaceInFiles` live in `TimeWarp.Amuru.Native.Text`. They follow the file-system split: `Direct` returns typed results and throws, `Commands` returns `CommandOutput` and does not throw. Bash aliases are `Grep` / `GrepDirect` and `Sed` / `SedDirect`.

Input is one path, a sequence of paths, or a directory plus a glob. Globs go through `FindItem` and `FindCriteria`. A name-only glob matches the file name. A glob that contains a slash or `**` matches the relative path. `Include` and `Exclude` on the options are extra filters of the same kind.

`SelectString` also accepts a `TextReader` or a `Stream`. There is no string-content overload, because a string is already a path. Pass a `StringReader` for in-memory text. A reader or stream with no path is labeled `-`. Include and exclude globs are ignored for those inputs.

## SelectString

The pattern is a regular expression unless `SelectStringOptions.SimpleMatch` is set. Matching is line by line, so `^` and `$` mean the line. Options cover case, invert, context before and after, a per-file limit on matching lines (`MaxMatches`, like `grep -m`), and include/exclude globs. Line numbers are always on the result.

`Direct.SelectString` returns `IAsyncEnumerable<TextMatch>` (`Path`, `LineNumber`, `Line`, `Match`, `ContextBefore`, `ContextAfter`). It reads one line at a time. An inverted hit still carries a `Match`, and that match is unsuccessful. A line with two hits yields two results. `MaxMatches` counts lines, so both hits on a counted line are returned.

A file whose first 8000 bytes contain a NUL byte, and that has no UTF-16/32 BOM, is treated as binary and skipped with no results. Unlike grep, no "Binary file matches" line is written. `TextReader` and `Stream` inputs are not checked.

`Commands.SelectString` writes `path:line:text` once per matching line, even when the line has several hits, and uses grep's exit codes: 0 when something matched, 1 when nothing matched, 2 on a regex or I/O error. A missing input path, an unreadable directory, or an unreadable file is written to stderr as `SelectString: <path>: <reason>`. The walk continues, output already produced is kept, and the exit code is 2. Context lines stay on `TextMatch`. They are not written to stdout.

## ReplaceInFiles

The replacement honors `$1` and `${name}`. Options cover a literal pattern, dry run, `.bak` backup, a per-file replacement limit, case, and `RegexOptions.Multiline` / `Singleline`.

The newline style is CRLF when the first `\n` follows a `\r`, otherwise LF. A file with no `\n` at all that contains `\r` uses CR. The file is matched after `\r\n` is folded to `\n` (or, for a CR file, `\r` to `\n`), then written back in the detected style. A lone `\r` in an LF or CRLF file stays a literal character, so a pattern can match it. When the text changes, the whole file is written in the detected style, so a file with mixed newlines comes out uniform. A byte-order mark is kept. A file with no BOM is written as UTF-8 without one. A trailing newline is kept when the file had one. Newlines that the replacement itself inserts are written in the detected style.

Decoding is strict. A file that is not valid text in its detected encoding (for example Latin-1 bytes in a file with no BOM) is never rewritten: `Direct.ReplaceInFiles` throws `InvalidDataException` with the path in the message, and `Commands.ReplaceInFiles` writes `ReplaceInFiles: <path>: not valid utf-8 text` to stderr, sets exit code 1, and keeps walking. A UTF-8 or no-BOM file that contains a NUL byte is binary and is skipped: zero replacements, `Changed` false, no write.

`Direct.ReplaceInFiles` returns one `ReplaceResult` per file, including files that did not change: `Path`, `ReplacementCount`, `Changed`, `Preview`, `BackupPath`. `Changed` is false when the replacement leaves the text identical, so the file is not rewritten and its mtime stays. Dry run sets `Preview` to unified-diff lines, writes nothing, and does not create a backup. A backup is `path.bak`, and only when the text changes.

The write is atomic: a temp file in the same directory, then a move over the original. The temp file is removed if the move fails. A symlink is followed to its final target, like `sed --follow-symlinks`: the target is rewritten (temp file in the target's directory) and the link stays a link. A hard link is split, because the move replaces the directory entry. Only the Unix mode is copied to the new file; owner, ACLs, and extended attributes are not.

`Commands.ReplaceInFiles` writes `path: N replacement(s)` for each changed file, including a dry run. Exit code 0 means the walk finished, even when nothing matched. Exit code 1 means a regex or I/O error. A missing input path, an unreadable directory, or a file that cannot be read or decoded is written to stderr as `ReplaceInFiles: <path>: <reason>`. The walk continues, and stdout still lists every file already changed.
