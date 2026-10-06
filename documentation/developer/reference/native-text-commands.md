# Native text commands

`SelectString` and `ReplaceInFiles` live in `TimeWarp.Amuru.Native.Text`. They follow the file-system split: `Direct` returns typed results and throws, `Commands` returns `CommandOutput` and does not throw. Bash aliases are `Grep` / `GrepDirect` and `Sed` / `SedDirect`.

Input is one path, a sequence of paths, or a directory plus a glob. Globs go through `FindItem` and `FindCriteria`. A name-only glob matches the file name. A glob that contains a slash or `**` matches the relative path. `Include` and `Exclude` on the options are extra filters of the same kind.

`SelectString` also accepts a `TextReader` or a `Stream`. There is no string-content overload, because a string is already a path. Pass a `StringReader` for in-memory text. A reader or stream with no path is labeled `-`. Include and exclude globs are ignored for those inputs.

## SelectString

The pattern is a regular expression unless `SelectStringOptions.SimpleMatch` is set. Matching is line by line, so `^` and `$` mean the line. Options cover case, invert, context before and after, a per-file match limit, and include/exclude globs. Line numbers are always on the result.

`Direct.SelectString` returns `IAsyncEnumerable<TextMatch>` (`Path`, `LineNumber`, `Line`, `Match`, `ContextBefore`, `ContextAfter`). It reads one line at a time. An inverted hit still carries a `Match`, and that match is unsuccessful. A line with two hits yields two results.

`Commands.SelectString` writes `path:line:text` and uses grep's exit codes: 0 when something matched, 1 when nothing matched, 2 on a regex or I/O error. Context lines stay on `TextMatch`. They are not written to stdout.

## ReplaceInFiles

The replacement honors `$1` and `${name}`. Options cover a literal pattern, dry run, `.bak` backup, a per-file replacement limit, case, and `RegexOptions.Multiline` / `Singleline`.

The file is matched after newlines are normalized to LF, then written back in the style of the first newline (`\n`, `\r\n`, or `\r`). A byte-order mark is kept. A file with no BOM is written as UTF-8 without one. A trailing newline is kept when the file had one. Newlines that the replacement itself inserts are written in that same style. A pattern that looks for a literal `\r` does not see one, because the match runs on the normalized text.

`Direct.ReplaceInFiles` returns one `ReplaceResult` per file, including files that did not change: `Path`, `ReplacementCount`, `Changed`, `Preview`, `BackupPath`. `Changed` is false when the replacement leaves the text identical, so the file is not rewritten and its mtime stays. Dry run sets `Preview` to unified-diff lines, writes nothing, and does not create a backup. A backup is `path.bak`, and only when the text changes.

The write is atomic: a temp file in the same directory, then a move over the original. The temp file is removed if the move fails.

`Commands.ReplaceInFiles` writes `path: N replacement(s)` for each changed file, including a dry run. Exit code 0 means the walk finished, even when nothing matched. Exit code 1 means a regex or I/O error.
