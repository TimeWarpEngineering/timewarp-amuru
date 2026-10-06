# Round 1 — general
**Date:** 2026-10-06
**Scope reviewed:** `git diff master...HEAD` product code: `source/timewarp-amuru/native/text/*.cs` (commands, direct, options, results, text-encoding, text-files, text-patterns, text-replace, text-search, text-diff, text-command) and `source/timewarp-amuru/native/aliases/bash.cs`, against `kanban/to-do/021-essential-text-processing/task.md`. I checked the claims below by reading the code. I also ran throwaway probe runfiles in `/tmp/rv021` against a build of the branch in a temp folder. No repo files were changed.

## Summary
The shape, the exit-code contracts, atomic temp-file-plus-move, skip-unchanged, dry run, backup, BOM detection (UTF-8/16/32 LE), literal escaping, streaming search, and context ordering all work as specified, and probes confirmed them. Two real data-integrity bugs are in the ReplaceInFiles preservation path: undecodable bytes are silently replaced with U+FFFD, and a lone CR before the first LF switches the whole file to CR line endings. The Commands wrappers also drop already-produced output when a later path or subdirectory fails. MaxMatches counts regex matches, not lines, which differs from the grep `-m` contract it claims.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/native/text/text-encoding.cs:76-96 (also text-replace.cs:65-87)
- Description: A file without a BOM is decoded with `new UTF8Encoding(false)`, whose default decoder replaces invalid bytes with U+FFFD. When the pattern matches anywhere in the file, the whole file is re-encoded. Every non-UTF-8 byte elsewhere in it becomes `EF BF BD`. Probe: bytes `63 61 66 E9 0A 66 6F 6F 0A` (Latin-1 "café\nfoo\n"), `ReplaceInFiles("foo","bar")` exits 0, and the file becomes `63 61 66 EF BF BD 0A 62 61 72 0A`. The "é" is destroyed silently. A recursive walk with no Include visits binary files too, such as `.git/index`, images, and `.dll`. Any binary file whose lossy decode happens to match the pattern is rewritten and corrupted. That breaks the "keep the file's encoding" requirement and the purpose of a safe replacement for the ganda fixers.
- Suggestion: Decode with `new UTF8Encoding(false, throwOnInvalidBytes: true)` and the throwing UTF-16/32 variants. Treat `DecoderFallbackException` as a per-file failure: Direct throws, or skips with a flag. Commands reports `path: invalid/binary content` on stderr, exits 1, and keeps walking. It also needs a catch per file, because `DecoderFallbackException` is an `ArgumentException` and today would abort the whole walk. Optionally skip files that contain NUL bytes, as grep and sed do for binary files. Add a test with a Latin-1 byte and a NUL-containing file.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-amuru/native/text/text-encoding.cs:22-57
- Description: `NewlineStyle.Detect` treats any `\r` that comes before the first `\n` as CR style. `ToLineFeed` turns every lone `\r` into `\n`, including a stray CR inside a line. Probe: an LF file `a\rb\nfoo\n` with `ReplaceInFiles("foo","bar")` becomes `a\rb\rbar\r` (bytes `61 0D 62 0D 62 61 72 0D`). Every LF in the file was rewritten as CR, so the file effectively turns into one line for most tools. The same thing happens to a CRLF file with a stray CR before its first CRLF. A stray lone CR in an otherwise LF file is common: pasted terminal output, `\r` progress lines, literal CRs in test fixtures. A single one should not change the line endings of the whole file.
- Suggestion: Detect only CRLF vs LF; treat CR as the style only if the file has no `\n` at all. Normalize only `\r\n` to `\n`, and leave a lone `\r` as a literal character. A simpler and more faithful option is to skip normalization altogether: match on the original text and translate only `\n` that the replacement inserts. Add a test with a stray `\r` in an LF file.
- Status: open

### Issue 3 — Severity: bug
- File: source/timewarp-amuru/native/text/commands.replace-in-files.cs:69-77 and 152-167; commands.select-string.cs:67-76 and 189-213
- Description: Per-file errors are caught only around the read or replace of a file that has already been enumerated. An error thrown by the enumerator escapes the `foreach` in `Replace`/`Search` and is caught by the outer `catch` in the public method, which builds a fresh `CommandOutput` and throws away all accumulated stdout. Such errors come from `RequireExisting` on a later path in the `IEnumerable<string>` overload, and from `UnauthorizedAccessException` on an unreadable subdirectory, because `FileSystemWalk` uses `IgnoreInaccessible=false`. Probes:
  - `Commands.ReplaceInFiles("foo","bar", [a.txt, missing])` rewrites `a.txt` but returns stdout `""` and stderr `ReplaceInFiles: No such file or directory`, which names neither path because `Fail` gets `path: null`.
  - `Commands.SelectString("foo", dir)`, where `dir` holds a matching `a.txt` and a mode-000 subdirectory, returns stdout `""` and exit 2. grep prints the hit, reports the unreadable directory, and exits 2.

  For ReplaceInFiles, the caller is never told which files were already modified. The Design region says "Changed files already written stay written", but the caller has no record of them.
- Suggestion: Move the per-path and per-directory error handling into the walk. Catch around `RequireExisting` for each input path, and around each directory enumeration step, or wrap `MoveNext` in try/catch inside the `Search`/`Replace` loops. Append `Op: <path>: <reason>` to stderr, set the error flag, and continue. Keep the outer catch only for argument and regex failures that happen before any work starts.
- Status: open

### Issue 4 — Severity: bug
- File: source/timewarp-amuru/native/text/text-search.cs:171 and 218-229; select-string-options.cs:46 (Design region line 7: "MaxMatches is per file, matching grep -m")
- Description: `Started` is incremented once per regex `Match`, not once per matching line. `MaxMatches` therefore limits matches, while grep `-m NUM` stops after NUM matching lines. Probe: `"foo\nboo\n"`, pattern `o`, `MaxMatches=2` returns two `TextMatch` for line 1 and never reaches line 2. grep `-m 2 o` returns both lines. A related problem: `Commands.SelectString` prints a line once per match, so `foo` with pattern `o` prints `-:1:foo` twice. grep prints a matching line once. The Results section treats "two hits on one line" as intended, but that conflicts with "stdout is `path:line:text` per match (grep format)" and with the grep `-m` claim in the Design region.
- Suggestion: Pick one behavior and make the docs say it. Either count lines for `MaxMatches` and dedupe by line in the Commands formatter, which matches grep, or keep counting per match and change the XML doc and Design region to "maximum regex matches per file (unlike grep -m, which counts lines)". Do the same for the Commands output.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-amuru/native/text/text-replace.cs:89-125
- Description: The atomic write moves the temp file onto `path`. If `path` is a symlink, the link is replaced by a regular file and the target is left untouched. Probe: `link.txt -> target.txt` ends with `link.txt` as a regular file containing `bar`, and `target.txt` still contains `foo`. Hard links are split the same way. Ownership/ACLs and extended attributes are not carried over either; only the Unix mode is copied. GNU `sed -i` has the same default, but a repo-wide fixer will hit symlinked files, such as shared `.editorconfig` or `Directory.Build.props` links. This should at least be a documented decision.
- Suggestion: Resolve `FileInfo.LinkTarget`/`ResolveLinkTarget(true)` and write to the final target, as `sed --follow-symlinks` does. If not, document in the Design region and docs that symlinks are replaced by regular files. Add a test either way.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-amuru/native/text/text-replace.cs:66-69
- Description: In a mixed-newline file, `changed` compares the newline-normalized output with the original. A match whose replacement equals itself, such as replacing `foo` with `foo`, still sets `Changed=true` and rewrites the whole file with uniform newlines. Probe: `foo\r\nx\ny\n` with any change becomes `...\r\nx\r\ny\r\n`. That is consistent with "newline style detected from the first newline", but it does not match the Design region's claim that "mixed newlines and the original bytes stay put" in general. It only holds when there are zero replacements.
- Suggestion: Reword the Design region. If Issue 2 moves to matching on the original text, this goes away.
- Status: open
