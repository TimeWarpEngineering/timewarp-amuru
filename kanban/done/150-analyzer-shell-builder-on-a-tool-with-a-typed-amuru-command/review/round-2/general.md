# Round 2 — general
**Date:** 2026-11-18
**Scope reviewed:** Fix delta since round 1 base `7efccb0e967f74d4a0c4261e21baac3ab3765968` (`80ea920` plus the kitchen land `92379d3`). Product files: `source/timewarp-amuru-analyzers/git-helper-table.cs`, `shell-builder-analyzer.cs`, `typed-builder-table.cs`, `tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs`, `readme.md`. Recorded fix-pass gates in `## Results` were read and not re-run.

## Summary

M1's listed counterexamples stay unmatched, and the new tests lock each of them. The dotnet path still keys off the leading constants, now across every chained `WithArguments`. Three git rows still name a helper for a command line that helper does not run: `GetDefaultBranchAsync` has two extra shapes, `SetUpstreamAsync` treats the upstream and the branch as independent values, and `GetCommitsAheadAsync` treats a three-dot range as `{branch}..HEAD`. The fix-pass record covers this delta: `./bin/dev build` succeeded, the analyzer runfile reported 23/23, and `ganda repo audit` exited 0.

## Resolved prior

### M1 — Severity: bug — Status: fixed
Re-checked against `GitHelperTable` and `GitOtherOperations_Should_NotReport`. Each round-1 counterexample is a different argument list from the helper that used to be named: wrong ref, missing `--quiet`, `rev-list --count` without `..HEAD`, `remote get-url`, `set-head` without `origin --auto`, a different fetch refspec, `fetch --all` / a refspec / no remote, a `symbolic-ref` set, `rev-parse --show-toplevel`, `worktree add --detach`, `worktree add -b` with too few arguments, `worktree list` without `--porcelain`, `worktree remove <path> --force`, and `fetch -- origin`. `rev-parse --show-toplevel` and `remote get-url origin` have no row. `worktree add` and `worktree add -b` are separate rows. The message change in `GitWithHelper_Should_ReportInfo` names one helper and the new label; the same change records why.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:242
- Description: `Git.GetDefaultBranchAsync` runs one command: `symbolic-ref refs/remotes/origin/HEAD --short` (`git.get-default-branch.cs:53`). The row also matches `symbolic-ref --short refs/remotes/origin/HEAD` and `symbolic-ref refs/remotes/origin/HEAD`. The two-argument form prints the full ref (`refs/remotes/origin/main`). The helper passes `--short` because `BranchFromOriginHead` only strips a leading `origin/`. A call that omits `--short` is told to switch to a helper that returns the short branch name. The reordered `--short` form is a second argv the helper never runs. `GitHelperShapes_Should_EachNameTheirHelper` only asserts the three-argument order the helper uses, so these two shapes are untested.
- Suggestion: Keep the single shape `symbolic-ref`, `refs/remotes/origin/HEAD`, `--short`. Add Jaribu cases that expect no AMURU0003 for `symbolic-ref refs/remotes/origin/HEAD` and for `symbolic-ref --short refs/remotes/origin/HEAD`.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:214
- Description: `Git.SetUpstreamAsync` runs `branch --set-upstream-to {remote}/{branchName} {branchName}` (`git.set-upstream.cs:67`). The row is `branch`, `--set-upstream-to`, `Value()`, `Value()`. `Value()` accepts any argument that does not start with `-`, so the upstream and the branch are not required to be the same branch. `git branch --set-upstream-to origin/main feature` matches and is reported as `SetUpstreamAsync`. That helper cannot emit `origin/main` together with `feature`; it would emit `origin/feature feature` or `origin/main main`. `git branch --set-upstream-to origin main` (no slash) matches for the same reason. The true positive in `GitHelperShapes_Should_EachNameTheirHelper` is `$"{remote}/{branch}"` plus `branch`. Both ends of that interpolation are holes, so the known prefix and suffix are empty and the test does not lock the `{remote}/{branch}` relationship.
- Suggestion: For two constants, require a non-empty remote such that the third argument equals `remote + "/" + fourth`. For an interpolation, accept the helper's form: a literal `/` immediately before a final hole that is the same expression as the fourth argument. Reject `origin/main` with `feature`, and `origin` with `main`, in `GitOtherOperations_Should_NotReport`. Keep the existing `$"{remote}/{branch}"` true positive.
- Status: open

### Issue 3 — Severity: bug
- File: source/timewarp-amuru-analyzers/git-helper-table.cs:121
- Description: `GetCommitsAheadAsync` runs `rev-list --count {branchName}..HEAD` (`git.get-commits-ahead.cs:63`). The slot is `Shaped("", "..HEAD")`, and `ArgumentSlot.Matches` uses `EndsWith("..HEAD")`. The literal `main...HEAD` ends with `..HEAD` (the last six characters are `.`, `.`, `H`, `E`, `A`, `D`) and is longer than the suffix, so `git rev-list --count main...HEAD` matches. Git's three-dot range is a different count from the two-dot range the helper builds. `GetCommitsAheadAsync("main")` produces `main..HEAD`, not `main...HEAD`. The round-1 counterexamples (`HEAD`, and a bare variable) stay silent; this literal is not one of them.
- Suggestion: Treat the suffix as a boundary. Accept an argument that ends with `..HEAD` and does not end with `...HEAD`, including `$"{branch}..HEAD"`. Reject the literal `main...HEAD` in `GitOtherOperations_Should_NotReport`.
- Status: open
