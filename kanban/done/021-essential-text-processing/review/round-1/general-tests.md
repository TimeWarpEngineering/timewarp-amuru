# Round 1 — general-tests
**Date:** 2026-10-06
**Scope reviewed:** tests/timewarp-amuru/single-file-tests/native/text/*.cs (5 files) against task.md "Non-functional > Tests" and Results; multi-file runner pickup (tests/timewarp-amuru/multi-file-runners/run-tests.cs + Directory.Build.props); documentation/developer/reference/native-text-commands.md, readme.md, documentation/overview.md, documentation/conceptual/architectural-layers.md; Purpose/Design regions and PublicAPI.Unshipped.txt for source/timewarp-amuru/native/text/.

## Summary
Every test the task requires exists and asserts real behavior. The mtime test first sets the stamp to 2020, so a rewrite would fail it. BOM, CRLF and UTF-8 checks compare exact bytes. The atomic-write test matches the `.tmp` suffix the implementation uses. Each case gets its own GUID temp directory, cleaned up in `finally`, and nothing depends on timing. Single-file runs: bash-aliases 2/2, commands.replace-in-files 6/6, commands.select-string 10/10, direct.replace-in-files 12/12, direct.select-string 13/13 (43/43, every process exit 0). The multi-file runner picks up all five classes (recursive `../single-file-tests/**/*.cs` glob): 663 passed, 1 skipped (an unrelated git-history test), 0 failed. Docs match the source. Every native/text source file has Purpose and Design regions. Bash `Grep`/`GrepDirect`/`Sed`/`SedDirect` are in PublicAPI.Unshipped.txt. The issues below are coverage gaps and nits, not bugs.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/native/text/direct.select-string.cs:130
- Description: `MaxMatches_Should_StopPerFile` (and `MaxReplacements_Should_StopInsideTheFile` at direct.replace-in-files.cs:65) use only one file. They cannot tell a per-file limit (the documented behavior: "a per-file match limit", "per-file replacement limit") from a global limit across all inputs.
- Suggestion: Use two files that each have 2+ hits, with `MaxMatches = 1` / `MaxReplacements = 1`. Assert one hit per file (2 total) and that each file has one replacement.
- Status: open

### Issue 2 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/native/text/direct.replace-in-files.cs:128
- Description: native-text-commands.md documents several ReplaceInFiles behaviors that no test covers:
  - (a) "A backup is `path.bak`, and only when the text changes". No test runs `Backup = true` on a non-matching file and asserts that no `.bak` is created.
  - (b) "`Changed` is false when the replacement leaves the text identical". text-replace.cs:84 makes this a separate case where `ReplacementCount > 0` but `Changed == false` (for example, replacing `a` with `a`). It is untested, and the Commands output depends on it.
  - (c) "Newlines that the replacement itself inserts are written in that same style". No CRLF test has a replacement that contains `\n`.
- Suggestion: Add three short Direct cases: a no-match run with backup, which leaves no `.bak` and `BackupPath` null; an identity replacement, which gives count > 0, `Changed` false, and the same mtime; and a CRLF file where the replacement inserts `\n`, with the output bytes containing `\r\n`.
- Status: open

### Issue 3 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/native/text/direct.select-string.cs:166
- Description: Some public options and overloads have no test: `SelectStringOptions.Include` (only `Exclude` is tested); `ReplaceInFilesOptions.CaseInsensitive` and `Singleline` (only `Multiline` is tested); the `Stream` overload of SelectString (only `TextReader` is tested); and the `GrepDirect` / `SedDirect` aliases. These are all thin wrappers, so the risk is low. The task's required test list does not name any of them.
- Suggestion: Add one-assert cases for `Include`, replace `CaseInsensitive`/`Singleline`, a `MemoryStream` input, and `GrepDirect`/`SedDirect` in bash-aliases.cs.
- Status: open

### Issue 4 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/native/text/direct.replace-in-files.cs:215
- Description: `AtomicWrite_Should_NotLeaveATempFile` only checks that no file name contains `.tmp`. That works with today's temp name (`.<name>.<guid>.tmp`, text-replace.cs:152), but the test would pass silently if the temp naming changed (for example, to `~` or `.partial`).
- Suggestion: Assert the exact directory listing: the only files should be `notes.txt` and `notes.txt.bak`. That does not depend on the temp naming.
- Status: open

### Issue 5 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/native/text/bash-aliases.cs:229
- Description: bash-aliases.cs uses `Path.GetTempFileName()` directly in the shared system temp root. The other text tests use an isolated `amuru-text-<guid>` directory. `Sed` writes its atomic temp sibling into the shared temp root, so a failure there would leave a stray `.tmpXXXX.tmp.<guid>.tmp` file that the `finally` does not clean up.
- Suggestion: Use the same `NewDirectory()` + `Directory.Delete(recursive: true)` pattern as the other four files.
- Status: open
