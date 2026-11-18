# Round 2 — merged findings
**Date:** 2026-11-18
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 3 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:212
- Description: Round 1: AMURU0003 named a Git helper when a verb token was present, including command lines that helper does not run. The fix moves git rows to `GitHelperTable` and requires a full argument list. Re-checked each listed counterexample against the new matcher and `GitOtherOperations_Should_NotReport`. All of them stay silent. `rev-parse --show-toplevel` and `remote get-url origin` have no row. `worktree add` and `worktree add -b` name one helper each.
- Suggestion: Round 1 suggestion stands as the fix that landed for those cases.
- Source: general
- Disposition notes: fixed in `80ea920`. Round 2 re-checked the listed counterexamples; they stay silent. Remaining shape holes are M2, M3, and M4.

### M2 — Severity: bug — Status: open
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:242
- Description: `Git.GetDefaultBranchAsync` runs `symbolic-ref refs/remotes/origin/HEAD --short` only (`git.get-default-branch.cs:53`). The row also matches `symbolic-ref --short refs/remotes/origin/HEAD` and `symbolic-ref refs/remotes/origin/HEAD`. The two-argument form prints the full ref. The helper passes `--short` so `BranchFromOriginHead` can strip a leading `origin/`. A call without `--short` is told to use a helper that returns the short branch name. The positive test asserts only the argv the helper runs.
- Suggestion: Keep that one shape. Add Jaribu cases that expect no AMURU0003 for the two-argument form and for `--short` before the ref.
- Source: general
- Disposition notes:

### M3 — Severity: bug — Status: open
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:214
- Description: `Git.SetUpstreamAsync` runs `branch --set-upstream-to {remote}/{branchName} {branchName}` (`git.set-upstream.cs:67`). The row's two `Value()` slots accept any non-option arguments, so `git branch --set-upstream-to origin/main feature` and `git branch --set-upstream-to origin main` are reported as `SetUpstreamAsync`. The helper cannot emit those pairs. The true positive `$"{remote}/{branch}"` has empty known prefix and suffix, because both ends are holes, so the test does not lock the relationship.
- Suggestion: For constants, require `third == remote + "/" + fourth` with a non-empty remote. For an interpolation, accept a literal `/` immediately before a final hole that is the same expression as the fourth argument. Add the two constant counterexamples to `GitOtherOperations_Should_NotReport` and keep the existing true positive.
- Source: general
- Disposition notes:

### M4 — Severity: bug — Status: open
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:121
- Description: `GetCommitsAheadAsync` runs `rev-list --count {branchName}..HEAD`. `Shaped("", "..HEAD")` matches with `EndsWith("..HEAD")`, so the three-dot literal `main...HEAD` matches. That range is a different count from the two-dot range `GetCommitsAheadAsync("main")` builds.
- Suggestion: Accept an argument that ends with `..HEAD` and does not end with `...HEAD`, including `$"{branch}..HEAD"`. Reject `main...HEAD` in `GitOtherOperations_Should_NotReport`.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- One reviewer. M1 carried forward as fixed. M2, M3, and M4 are new on the fix delta.
