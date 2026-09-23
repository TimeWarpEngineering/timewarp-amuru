# Round 1 — merged findings
**Date:** 2026-09-23
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: .editorconfig:303-306
- Description: `dotnet_diagnostic.TW0007.filename = global-usings.cs` was inserted under the `#### Build Scripts Analyzer Settings ####` heading but before `[Scripts/*.cs]`, so it lands in `[*.cs]` (correct scope) while the heading implies the narrower Scripts section — a footgun for a future "fix" that would narrow it.
- Suggestion: Move the line up beside the `globalusingsanalyzer0001.filename` group and comment that it is intentionally repo-wide.
- Source: general
- Disposition notes: Moved into the Global Usings Analyzer group (still `[*.cs]`) with an explicit "repo-wide, not scoped to Scripts/" comment. Fixed by review oracle, commit on this task.

### M2 — Severity: nit — Status: fixed
- File: Directory.Packages.props:20, .memsearch.toml:50
- Description: Both files end without a trailing newline while `.editorconfig` `[*]` sets `insert_final_newline = true`.
- Suggestion: Append a trailing newline.
- Source: general
- Disposition notes: Trailing newline appended to both files. Fixed by review oracle, commit on this task.

## Duplicates / conflicts

- None (single reviewer).
