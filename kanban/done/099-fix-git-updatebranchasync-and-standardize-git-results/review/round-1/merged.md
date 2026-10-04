# Round 1 — merged findings
**Date:** 2026-10-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 1 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-tools/git-commands/Git.BranchExists.cs:52
- Description: `git show-ref --verify` without `--quiet` exits 128 for a missing ref (verified, git 2.53.0), so a real missing branch returned `Success=false`. The unit test hid this by mocking exit 1.
- Suggestion: add `--quiet`, fix the mocks, add real-repo tests.
- Source: general
- Disposition notes: Fixed. Now runs `show-ref --verify --quiet`. Mocks updated. Added real-execution tests for an existing branch and a missing branch in `git.branch-exists.cs`.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-tools/git-commands/Git.UpdateBranch.cs:80-92
- Description: In a bare repository, `rev-parse --abbrev-ref HEAD` returned the HEAD branch, so the code ran `pull --ff-only`, which fails outside a work tree. This regressed from the old refspec fetch.
- Suggestion: skip the pull in bare repositories.
- Source: general
- Disposition notes: Fixed together with M3. Checked-out detection now uses `GetWorktreePathAsync`, which reads the porcelain worktree list. A bare entry has no branch line, so a bare repository takes the refspec fetch. Added test `BareRepository_*` in `git.update-branch.cs`.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru-tools/git-commands/Git.UpdateBranch.cs:77-95
- Description: A branch checked out in a different linked worktree still went to `fetch origin b:b`, which git refuses.
- Suggestion: find the branch's worktree and pull there.
- Source: general
- Disposition notes: Fixed. A branch checked out in any work tree, main or linked, is pulled there with `git -C <path> -c pull.rebase=false pull --ff-only`, and `BranchPath` is set to that path. Removed the `IsWorktree` delegation and `GetCheckedOutBranchAsync`. Added a linked-worktree test.

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-amuru-tools/git-commands/Git.UpdateWorktree.cs:48
- Description: `UpdateWorktreeAsync` used a plain `git pull`, unlike the fast-forward-only pull in `UpdateBranchAsync`.
- Suggestion: align the flags.
- Source: general
- Disposition notes: Fixed. Both methods now use the shared `PullFastForwardAsync` (`-c pull.rebase=false pull --ff-only`).

### M5 — Severity: nit — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/git-commands/git.worktree-remove.cs:58
- Description: The test changes the process-wide CWD.
- Suggestion: serialize the test or drop the CWD change.
- Source: general
- Disposition notes: Wontfix (orchestrator). The CWD change is the point of the regression test: it proves a relative `gitdir:` is not resolved against the process CWD. Six other test files already use `Directory.SetCurrentDirectory` with restore in `finally` (`commands.set-location.cs`, `script-context.*`, `repo-*-service.cs`), and the full runner passes with this pattern.

## Duplicates / conflicts

- M2 and M3 share one root cause (checked-out detection) and one fix.
