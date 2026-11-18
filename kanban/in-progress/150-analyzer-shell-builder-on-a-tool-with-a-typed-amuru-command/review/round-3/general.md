# Round 3 — general
**Date:** 2026-11-18
**Scope reviewed:** Fix delta since round 2 head `92379d3` (`e4e9b37`). Product files: `source/timewarp-amuru-analyzers/git-helper-table.cs`, `source/timewarp-amuru-analyzers/shell-builder-analyzer.cs`, `tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs`. Recorded fix-pass gates in `## Results` were read and not re-run.

## Summary

M2, M3, and M4 are fixed. `GetDefaultBranchAsync` keeps the one argv in `git.get-default-branch.cs` (`symbolic-ref`, `refs/remotes/origin/HEAD`, `--short`). `SetUpstreamAsync` reports only when the third argument is a non-empty remote, a slash, and the fourth argument, for two constants or for an interpolation whose final hole is that same reference. `GetCommitsAheadAsync` accepts a `..HEAD` range and rejects a suffix of `...HEAD`. The new negative test locks each round-2 counterexample, and the existing true positives still name those helpers. No new defect on this delta. The fix-pass record covers it: `./bin/dev build` succeeded with 0 warnings, the analyzer runfile reported 24/24, and `ganda repo audit` exited 0.

## Resolved prior

### M1 — Severity: bug — Status: fixed
Unchanged on this delta. Round 2 already re-checked the listed counterexamples. This commit does not widen those rows.

### M2 — Severity: bug — Status: fixed
`Git.GetDefaultBranchAsync` builds `symbolic-ref`, `refs/remotes/origin/HEAD`, `--short` (`git.get-default-branch.cs`). The row is that one shape, and the label includes `--short`. `GitNearHelperShapes_Should_NotReport` expects no AMURU0003 for `symbolic-ref refs/remotes/origin/HEAD` and for `symbolic-ref --short refs/remotes/origin/HEAD`. `GitHelperShapes_Should_EachNameTheirHelper` still expects `GetDefaultBranchAsync` for the helper's order.

### M3 — Severity: bug — Status: fixed
`Git.SetUpstreamAsync` builds `branch --set-upstream-to {remote}/{branchName} {branchName}` (`git.set-upstream.cs`). `UpstreamNamesBranch` runs after that shape matches. Two constants match only when the third argument is longer than `/{fourth}` and ends with it, so the remote is non-empty. An interpolation matches only when a literal ending in `/` sits immediately before a final hole whose reference text equals the fourth argument (`ShellArgument.SlashTail` / `Reference`, from `SlashTail` and `ReferenceName`). `GitNearHelperShapes_Should_NotReport` covers `origin/main` with `feature`, `origin` with `main`, `/main` with `main`, `$"{remote}/{branch}"` with `other`, `$"/{branch}"`, `$"{remote}-{branch}"`, and a bare `remote`. The shapes test still has `$"{remote}/{branch}"` plus `branch`, and adds `origin/main` plus `main` and `$"origin/{branch}"` plus `branch`.

### M4 — Severity: bug — Status: fixed
`GetCommitsAheadAsync` builds `rev-list --count {branchName}..HEAD` (`git.get-commits-ahead.cs`). The slot is `Shaped("", "..HEAD", "...HEAD")`. `ArgumentSlot.Matches` still requires the known text to end with `..HEAD`, and it rejects a known suffix that ends with `...HEAD`. A constant `main...HEAD` uses the full text as its suffix, so it is rejected. `$"{branch}...HEAD"` has known suffix `...HEAD`, so it is rejected. `$"{branch}..HEAD` and `main..HEAD` do not end with `...HEAD` and still report. Both the rejection and the two-dot positives are in the runfile.

## Issues

<!-- No new issues. -->
