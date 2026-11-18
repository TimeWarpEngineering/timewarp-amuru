# Round 3 — merged findings
**Date:** 2026-11-18
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 4 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:212
- Description: Round 1: AMURU0003 named a Git helper when a verb token was present, including command lines that helper does not run. Round 2 re-checked the listed counterexamples; they stay silent.
- Suggestion: Round 1 suggestion stands as the fix that landed for those cases.
- Source: general
- Disposition notes: fixed in `80ea920`. Not reopened on the round 3 delta.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:288
- Description: Round 2: `GetDefaultBranchAsync` also matched `symbolic-ref --short refs/remotes/origin/HEAD` and `symbolic-ref refs/remotes/origin/HEAD`. The helper runs only `symbolic-ref refs/remotes/origin/HEAD --short`.
- Suggestion: Keep that one shape, and add no-report cases for the other two.
- Source: general
- Disposition notes: fixed in `e4e9b37`. Round 3 re-checked the helper source, the single row, and `GitNearHelperShapes_Should_NotReport`.

### M3 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:333
- Description: Round 2: `SetUpstreamAsync` accepted any two non-option arguments, so `origin/main` with `feature` and `origin` with `main` were reported. The helper runs `{remote}/{branchName}` and that same `branchName`.
- Suggestion: Require a non-empty remote and the same branch on both sides, for constants and for an interpolation ending `/{x}`.
- Source: general
- Disposition notes: fixed in `e4e9b37`. Round 3 re-checked `UpstreamNamesBranch`, `SlashTail`, and the positive and negative runfile cases.

### M4 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:282
- Description: Round 2: `Shaped("", "..HEAD")` treated `main...HEAD` as `{branch}..HEAD`. The three-dot range is a different count from the range `GetCommitsAheadAsync` builds.
- Suggestion: Accept a `..HEAD` suffix that is not a `...HEAD` suffix, and reject `main...HEAD`.
- Source: general
- Disposition notes: fixed in `e4e9b37`. Round 3 re-checked the excluded suffix in `ArgumentSlot.Matches` and the two-dot and three-dot runfile cases.

## Duplicates / conflicts

- One reviewer. M1 carried forward as fixed. M2, M3, and M4 are fixed on this delta. No new findings.
