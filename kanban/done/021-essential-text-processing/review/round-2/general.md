# Round 2 — general
**Date:** 2026-10-06
**Scope reviewed:** commit 1842a0c + re-verification of M1–M11

## Summary

All 11 round-1 findings are fixed. All five text test files pass: bash-aliases 4/4, commands.replace-in-files 8/8, commands.select-string 12/12, direct.replace-in-files 22/22, direct.select-string 17/17.

I also ran a throwaway probe against the built library. It exercised relative and chained symlinks, UTF-16 with a BOM, odd-length UTF-16, UTF-16 without a BOM, an unreadable file, a dangling symlink, CR-only files, a lone CR, and a missing later path. All behaved as documented. Strict decoding does not break valid UTF-16 or UTF-32 files that have a BOM.

There are two new low-likelihood problems in the Commands walk. In both, an ArgumentException escapes the per-file handler, so output is lost even though a file may already have been rewritten (N1, N2). There are also two documentation nits (N3, N4).

## Prior findings
| ID | Status (fixed/open) | Evidence |
|----|--------|----------|
| M1 | fixed | `text-encoding.cs`: the UTF8, Unicode and UTF32 encodings now throw on invalid bytes. `IsBinary` checks for NUL in UTF-8/no-BOM files. `TextReplace.Decode` throws `InvalidDataException` naming the path, with a `ReasonKey`. Commands catch it through `IsFileError` and keep walking. Tests: `Latin1File_Should_ThrowAndKeepItsBytes`, `BinaryFile_Should_BeSkippedUntouched`, `InvalidText_Should_ReportThePathAndKeepWalking`. Probe: odd-length UTF-16 gives `ReplaceInFiles: <path>: not valid utf-16 text` with exit 1; a valid UTF-16 LE file with a BOM round-trips with its BOM and CRLF. |
| M2 | fixed | `NewlineStyle.Detect` picks CRLF/LF from the first `\n` and uses CR only when the file has no `\n`. In non-CR files `ToLineFeed` folds only `\r\n`. Probe: `a\rb\nc\n` becomes `a\rb\nZ\n`, and the CR-only file `a\rb\r` becomes `a\rX\rY\r`. Test `LoneCarriageReturn_Should_StayLiteral`. The `TextDiff` helpers were updated to match. |
| M3 | fixed | `TextFiles.Enumerate`/`WalkDirectory` now yield a `TextFileItem` with an `Error` for a missing input path or an unreadable directory (`IgnoreInaccessible = false`). Both `Search` and `Replace` report `Op: <path>: <reason>` and continue. Tests: `MissingLaterPath_Should_KeepEarlierOutput` (both commands) and `UnreadableDirectory_Should_KeepOtherHits`. Probe: an unreadable file and a dangling symlink in a walk are each reported with their path, and the other files' output is kept. See N2 for a remaining hole. |
| M4 | fixed | `MatchWindow.Start` increments `Started` once per line. `Search` and `Format` dedupe output by `LineNumber`. The options XML doc, Design region and reference docs now say "matching lines, like grep -m". Tests: `MaxMatches_Should_CountLinesNotHits` and `TwoHitsOnOneLine_Should_PrintTheLineOnce`. |
| M5 | fixed | `ResolveTarget` uses `ResolveLinkTarget(returnFinalTarget: true)`. The temp file is created in the target's directory and the mode is copied from the target. Probe: chain `l2 -> sub/l1 -> ../real/t.txt` (relative links) rewrote `real/t.txt`, both links stayed links, and no temp file was left. Test `Symlink_Should_RewriteTheTargetAndStayALink`. |
| M6 | fixed | The `text-replace.cs` Design region now says the whole file is rewritten in the detected style when the text changes, "so a mixed-newline file comes out uniform". |
| M7 | fixed | `MaxMatches_Should_StopPerFile` and `MaxReplacements_Should_ApplyToEachFile` now use two files and assert the limit per file. |
| M8 | fixed | Added `NoChange_Should_NotWriteABackup`, `IdentityReplacement_Should_CountButNotRewrite` and `CrlfFile_Should_WriteInsertedNewlinesAsCrlf`. |
| M9 | fixed | Added `Include_Should_SearchOnlyMatchingNames`, `CaseInsensitive_Should_MatchEitherCase`, `Singleline_Should_LetDotMatchNewline`, `Stream_Should_DecodeUtf8AndUseThePathLabel`, `GrepDirect_Should_StreamTextMatches` and `SedDirect_Should_StreamReplaceResults`. |
| M10 | fixed | `AtomicWrite_Should_NotLeaveATempFile` now asserts the exact directory listing `["notes.txt", "notes.txt.bak"]`. |
| M11 | fixed | `bash-aliases.cs` no longer uses `GetTempFileName`. It uses the isolated `NewDirectory()` helper (`amuru-text-<guid>`). |

## New issues
### N1 — Severity: suggestion
- File: source/timewarp-amuru/native/text/text-encoding.cs (`GetBytes` with a strict encoder), source/timewarp-amuru/native/text/commands.replace-in-files.cs (`Replace` catch filter), source/timewarp-amuru/native/text/text-command.cs (`IsFileError`)
- Description: The encoders now throw too (`throwOnInvalidBytes: true`). A replacement that leaves a lone surrogate therefore raises `EncoderFallbackException`, which is an `ArgumentException`. `IsFileError` does not match it, so it escapes the per-file handler in `Commands.ReplaceInFiles`. The outer catch then drops all stdout, ends the walk, and reports without a path, because `Fail` omits the path for `ArgumentException`. `Direct` throws an undocumented `ArgumentException` instead of `InvalidDataException`. Probe: pattern `\uD83D|zzz` on a directory holding `b.txt` (contains an emoji) and `a.txt`. Result: exit 1, empty stdout, stderr `ReplaceInFiles: Unable to translate Unicode character \uDE00 at index 3 to specified code page.`, and `a.txt` was never processed. You only hit this if the pattern can split a surrogate pair, but before this commit the same input wrote U+FFFD instead of aborting.
- Suggestion: In `Plan`, catch `EncoderFallbackException` and wrap it like `Decode` does: an `InvalidDataException` with the path and a `ReasonKey` such as "replacement produced invalid utf-8 text". It is then handled per file and keeps the path.
- Status: open

### N2 — Severity: suggestion
- File: source/timewarp-amuru/native/text/text-files.cs (`Enumerate(IEnumerable<string>, …)`, `RequireExisting`)
- Description: `RequireExisting` calls `ArgumentException.ThrowIfNullOrWhiteSpace`, but `Enumerate` only turns `FileNotFoundException` into an error item. An empty or whitespace entry in the `paths` list therefore throws out of the lazy iterator, and the outer catch in `Commands.ReplaceInFiles(…, IEnumerable<string>, …)` / `SelectString` drops the stdout gathered so far. Probe: `ReplaceInFiles("foo", "bar", [file, " "])` rewrote `file` (contents now `bar`) but returned exit 1, empty stdout, and stderr `ReplaceInFiles: The value cannot be an empty string…`. The caller gets no record that a file was changed. This is the M3 "earlier output lost" symptom, now caused by bad input.
- Suggestion: Either validate every path before the walk starts (fail fast before any write), or catch `ArgumentException` in `Enumerate` and yield it as an error item like a missing path.
- Status: open

### N3 — Severity: nit
- File: source/timewarp-amuru/native/text/text-replace.cs (`WriteBackup(path, …)`), documentation/developer/reference/native-text-commands.md
- Description: When a symlink is followed, the backup goes next to the link (`<link>.bak`) and is a regular-file copy of the target's old contents. The rewrite itself happens in the target's directory. Probe: `l2.bak` was created next to the link, nothing appeared next to `real/t.txt`, and `BackupPath` pointed at `l2.bak`. This is defensible, but neither the docs nor the Design region say it. They describe "sed --follow-symlinks" semantics, and a reader may expect the backup beside the target.
- Suggestion: Add one sentence to the docs and the Design region on where the backup goes for a symlink, or place it beside the resolved target.
- Status: open

### N4 — Severity: nit
- File: source/timewarp-amuru/native/text/text-encoding.cs (`IsBinary`), documentation/developer/reference/native-text-commands.md
- Description: A UTF-16 or UTF-32 file without a BOM contains NUL bytes, so it is classed as binary. This is safe: there is no corruption. But it is silent. Probe: `ReplaceInFiles` gives exit 0 with empty stdout and stderr, and `SelectString` gives exit 1 with nothing on stderr. The docs say "NUL means binary" but do not mention that BOM-less UTF-16 falls into that bucket. (A UTF-16 LE file with a BOM whose first character is U+0000 starts `FF FE 00 00` and is detected as UTF-32 LE. That behavior predates this commit and is very unlikely.)
- Suggestion: Add a note to the docs that UTF-16/32 files without a BOM are treated as binary and skipped.
- Status: open
