# Round 2 — merged findings
**Date:** 2026-10-06
**Sources:** general (re-review of commit 1842a0c)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 4 | 0 |
| suggestion | 2 | 4 | 0 |
| nit | 2 | 3 | 0 |

## Resolved prior (round 1)

| ID | Severity | Status | Fix |
|----|----------|--------|-----|
| M1 | bug | fixed | strict decoders, per-file InvalidDataException, NUL = binary skip (1842a0c) |
| M2 | bug | fixed | CRLF/LF detection, only `\r\n` normalized (1842a0c) |
| M3 | bug | fixed | per-path/per-directory errors handled in walk (1842a0c) |
| M4 | bug | fixed | MaxMatches counts lines; Commands prints a line once (1842a0c) |
| M5 | suggestion | fixed | symlinks followed to final target (1842a0c) |
| M6 | nit | fixed | Design region reworded (1842a0c) |
| M7 | suggestion | fixed | two-file per-file limit tests (1842a0c) |
| M8 | suggestion | fixed | no-.bak, identity, CRLF-insert tests (1842a0c) |
| M9 | suggestion | fixed | Include/CaseInsensitive/Singleline/Stream/GrepDirect/SedDirect tests (1842a0c) |
| M10 | nit | fixed | exact directory listing (1842a0c) |
| M11 | nit | fixed | isolated temp dir (1842a0c) |

## Issues

### M12 — Severity: suggestion — Status: open
- File: text-encoding.cs (`GetBytes`), commands.replace-in-files.cs, text-command.cs (`IsFileError`)
- Description: EncoderFallbackException from a lone surrogate escapes the per-file handler, drops stdout, no path.
- Suggestion: Wrap as InvalidDataException with the path in `Plan`.
- Source: general (N1)
- Disposition notes:

### M13 — Severity: suggestion — Status: open
- File: text-files.cs (`Enumerate`, `RequireExisting`)
- Description: Empty/whitespace path entry throws out of the lazy walk, dropping earlier stdout after a file was rewritten.
- Suggestion: Validate all paths before the walk, or yield as a per-path error.
- Source: general (N2)
- Disposition notes:

### M14 — Severity: nit — Status: open
- File: text-replace.cs (`WriteBackup`), native-text-commands.md
- Description: Backup location for a followed symlink undocumented.
- Suggestion: Document.
- Source: general (N3)
- Disposition notes:

### M15 — Severity: nit — Status: open
- File: text-encoding.cs (`IsBinary`), native-text-commands.md
- Description: BOM-less UTF-16/32 silently skipped as binary; undocumented.
- Suggestion: Document.
- Source: general (N4)
- Disposition notes:

## Duplicates / conflicts

- None.
