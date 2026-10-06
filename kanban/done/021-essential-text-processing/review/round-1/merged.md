# Round 1 — merged findings
**Date:** 2026-10-06
**Sources:** general, general-tests

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 4 | 0 | 0 |
| suggestion | 4 | 0 | 0 |
| nit | 3 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-amuru/native/text/text-encoding.cs:76-96, text-replace.cs:65-87
- Description: No-BOM files are decoded lossily (U+FFFD), so a replace silently corrupts non-UTF-8 bytes and binary files.
- Suggestion: Throwing decoders; per-file failure (Direct throws, Commands reports path on stderr, exit 1, keeps walking); skip NUL-containing (binary) files; tests.
- Source: general (Issue 1)
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/timewarp-amuru/native/text/text-encoding.cs:22-57
- Description: A lone CR before the first LF flips the whole file to CR line endings.
- Suggestion: Detect CRLF vs LF only (CR only when no `\n` at all); normalize only `\r\n`; leave lone `\r` literal; test.
- Source: general (Issue 2)
- Disposition notes:

### M3 — Severity: bug — Status: open
- File: commands.replace-in-files.cs:69-77,152-167; commands.select-string.cs:67-76,189-213
- Description: Enumeration errors (missing later path, unreadable subdir) discard accumulated stdout and lose the path in stderr.
- Suggestion: Handle per-path / per-directory errors inside the walk, append `Op: <path>: <reason>`, continue.
- Source: general (Issue 3)
- Disposition notes:

### M4 — Severity: bug — Status: open
- File: source/timewarp-amuru/native/text/text-search.cs:171,218-229; select-string-options.cs:46
- Description: MaxMatches counts regex matches, not lines (claims grep -m); Commands prints a line once per match.
- Suggestion: Count lines for MaxMatches; Commands prints each matching line once; docs.
- Source: general (Issue 4)
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/native/text/text-replace.cs:89-125
- Description: Atomic move replaces a symlink with a regular file and leaves the target untouched.
- Suggestion: Write to the resolved link target, or document; test.
- Source: general (Issue 5)
- Disposition notes:

### M6 — Severity: nit — Status: open
- File: source/timewarp-amuru/native/text/text-replace.cs:66-69
- Description: Design region claims mixed newlines stay put; only true with zero replacements.
- Suggestion: Reword.
- Source: general (Issue 6)
- Disposition notes:

### M7 — Severity: suggestion — Status: open
- File: tests/.../native/text/direct.select-string.cs:130, direct.replace-in-files.cs:65
- Description: Max-limit tests use one file; cannot distinguish per-file from global.
- Suggestion: Two files, assert per-file limit.
- Source: general-tests (Issue 1)
- Disposition notes:

### M8 — Severity: suggestion — Status: open
- File: tests/.../native/text/direct.replace-in-files.cs:128
- Description: Untested documented behaviors: no `.bak` without change; identity replacement `Changed == false`; CRLF style for inserted newlines.
- Suggestion: Three Direct cases.
- Source: general-tests (Issue 2)
- Disposition notes:

### M9 — Severity: suggestion — Status: open
- File: tests/.../native/text/direct.select-string.cs:166
- Description: Include, replace CaseInsensitive/Singleline, Stream overload, GrepDirect/SedDirect untested.
- Suggestion: One-assert cases.
- Source: general-tests (Issue 3)
- Disposition notes:

### M10 — Severity: nit — Status: open
- File: tests/.../native/text/direct.replace-in-files.cs:215
- Description: Atomic-write test depends on `.tmp` naming.
- Suggestion: Assert exact directory listing.
- Source: general-tests (Issue 4)
- Disposition notes:

### M11 — Severity: nit — Status: open
- File: tests/.../native/text/bash-aliases.cs:229
- Description: Uses shared temp root via GetTempFileName.
- Suggestion: Isolated temp dir like the other files.
- Source: general-tests (Issue 5)
- Disposition notes:

## Duplicates / conflicts

- None overlapping. M6 is partly mooted by M2's fix but kept separately for the Design-region wording.
