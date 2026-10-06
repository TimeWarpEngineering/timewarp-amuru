# Round 1 — merged findings
**Date:** 2026-10-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru-tools/git-commands/git.worktree-porcelain-parser.cs:7
- Description: The Design region misstated where records are flushed. It said the last record is flushed "rather than on a blank separator", but a whitespace-only line branch still flushes.
- Suggestion: Describe the next-`worktree` flush, the whitespace-only line flush, and the post-loop flush.
- Source: general
- Disposition notes: The comment was rewritten to name all three flush points. Comment only; no code change.

### M2 — Severity: nit — Status: fixed
- File: source/timewarp-amuru-tools/git-commands/git.worktree-add.cs:7
- Description: The comment "failure carries stderr" omitted the fixed fallback message used when stderr is blank.
- Suggestion: Mention the fallback.
- Source: general
- Disposition notes: The comment now says the failure carries trimmed stderr, or a fixed message when stderr is blank.

## Duplicates / conflicts

- None.
