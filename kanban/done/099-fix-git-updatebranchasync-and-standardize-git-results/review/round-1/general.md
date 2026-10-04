# Round 1 — general
**Date:** 2026-10-04
**Scope reviewed:** `git diff master...HEAD` — source/timewarp-amuru-tools/git-commands/*.cs, tests/timewarp-amuru/git-repository-fixture.cs, tests/timewarp-amuru/single-file-tests/git-commands/*.cs, documentation/developer/reference/git-commands.md, readme.md, skills/amuru/SKILL.md; repo-wide grep for old bool/string signatures and removed members (no remaining callers outside git-commands and tests). Behaviors checked against git 2.53.0.

## Summary

The checked-out-branch fix, relative gitdir resolution, leading `origin/` strip, null-repoPath handling and cancellation propagation look correct, and the Results list of removed/renamed members matches the source. Two real defects: `BranchExistsAsync` relies on exit code 1 for a missing ref, but real git returns 128 for `show-ref --verify` without `--quiet` (the unit test masks this with a wrong mock), and `UpdateBranchAsync` now fails on bare repositories whose HEAD branch is the requested branch. No stale callers or doc drift found.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru-tools/git-commands/Git.BranchExists.cs:52
- Description: `git show-ref --verify refs/heads/nope` (no `--quiet`) exits 128 with `fatal: 'refs/heads/nope' - not a valid ref`, not 1 (verified on git 2.53.0; only `--verify --quiet` exits 1). So for a real missing branch `BranchExistsAsync` returns `Success=false` with an error message, not `Success=true, Exists=false`. This contradicts the documented/Results contract ("Exit code 1 (missing ref) is Success true and Exists false"). The code comment in the Design region ("exits 1 when it does not") is likewise wrong.
- Suggestion: Add `--quiet` to the show-ref arguments (then 0 = exists, 1 = missing, other = real failure), or use `git rev-parse --verify --quiet`. Update the mock in git.branch-exists.cs and add a real-repo test (existing branch and missing branch) via GitRepositoryFixture; the current NonExistingBranch test mocks exit 1 which real git never produces for this command line.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-amuru-tools/git-commands/Git.UpdateBranch.cs:80-92
- Description: `GetCheckedOutBranchAsync` uses `rev-parse --abbrev-ref HEAD`, which in a bare repository returns the HEAD branch (e.g. `master`). The code then runs `git pull --ff-only`, which fails with `fatal: this operation must be run in a work tree`. Before this change `fetch origin master:master` worked in a bare repo (verified). This library has bare-repo workflows (CloneBare, worktrees), so `UpdateBranchAsync`/`UpdateDefaultBranchAsync` regress for the bare repo's HEAD branch.
- Suggestion: Skip the pull path when `git rev-parse --is-bare-repository` is true (use the refspec fetch). Add a bare-repo test.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-amuru-tools/git-commands/Git.UpdateBranch.cs:77-95
- Description: Only the current HEAD is treated as "checked out". A branch checked out in a different linked worktree, while `repositoryPath` is the main repo, still goes to `fetch origin b:b`, which git refuses (the same class of failure the task fixes). Detached HEAD (falls through to fetch) is fine.
- Suggestion: Optionally call `GetWorktreePathAsync(branchName, repositoryPath)` and delegate to `UpdateWorktreeAsync` when the branch is checked out elsewhere. At minimum document the limitation.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-amuru-tools/git-commands/Git.UpdateWorktree.cs:48
- Description: `UpdateWorktreeAsync` runs plain `git pull origin <branch>` (no `--ff-only`, no `pull.rebase=false`), while the checked-out path in `UpdateBranchAsync` forces ff-only and rebase off. A linked-worktree repo therefore gets different (merge/rebase-config-dependent) semantics from a normal repo for the same call. Pre-existing, but the Design region of UpdateBranch claims consistent ff-only behavior.
- Suggestion: Either align the worktree pull flags or note the difference in the docs.
- Status: open

### Issue 5 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/git-commands/git.worktree-remove.cs:58
- Description: The relative-gitdir test mutates process-wide `Directory.SetCurrentDirectory`; in the multi-file runner other tests may run in parallel and see the changed CWD. Restoration is in `finally`, so it is safe sequentially only.
- Suggestion: If the runner runs tests concurrently, tag/serialize this test or drop the CWD change (the `Path.Combine(worktreePath, gitdir)` fix is already exercised by the relative gitdir itself).
- Status: open
