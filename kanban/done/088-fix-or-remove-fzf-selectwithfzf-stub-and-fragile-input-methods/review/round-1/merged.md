# Round 1 — merged findings
**Date:** 2026-10-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 2 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: suggestion — Status: wontfix
- File: source/timewarp-amuru/core/command-extensions.cs:97
- Description: Empty `standardInput` now means immediate EOF instead of inherited stdin. This is a core-package behavior change.
- Suggestion: Note it in the changelog.
- Source: general
- Disposition notes: The change is intended and documented in the Design region of command-extensions.cs. An explicit `WithStandardInput("")` meaning EOF is the more correct contract. The release-notes mention belongs to the version-bump/release PR, not this change. Decided by: review oracle.

### M2 — Severity: suggestion — Status: wontfix
- File: source/timewarp-amuru/core/command-result.cs:210
- Description: Passthrough/TtyPassthrough ignore the configured stdin, so FzfBuilder input sources are dropped in those modes. This is pre-existing.
- Suggestion: Follow-up task.
- Source: general
- Disposition notes: Not a regression: the old echo pipeline had the same problem. The fix belongs in core CommandResult execution modes, outside 088's scope. SelectAsync, the primary fzf path, works. Decided by: review oracle.

### M3 — Severity: nit — Status: wontfix
- File: source/timewarp-amuru-tools/fzf-command/Fzf.cs:88-93
- Description: The `UseStdin` branch duplicates the fallthrough.
- Suggestion: Collapse it.
- Source: general
- Disposition notes: I tried collapsing it. `UseStdin` then becomes assigned-but-unread, and CS0414 fails the build under warnings-as-errors. The explicit branch keeps the field meaningful and documents FromStdin intent. Decided by: review oracle.

## Duplicates / conflicts

- None.
