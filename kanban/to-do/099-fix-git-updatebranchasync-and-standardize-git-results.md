# Fix Git UpdateBranchAsync and standardize git results

## Description

The git-commands module is the closest to 1.0-ready (consistent result records, correct flags), but its most common operation fails in a normal repo, and result/naming conventions drift across members — breaking to fix after 1.0.

## Checklist

### Bug
- [ ] `git-commands/Git.UpdateBranch.cs:53-56` — non-worktree path runs `git fetch origin <branch>:<branch>`, which git REFUSES for the currently checked-out branch. In a normal repo on `master`, `UpdateBranchAsync("master")` — the most common call — always fails. Use `git pull` (or `--update-head-ok` semantics) for the checked-out case

### API consistency (breaking to change post-1.0)
- [ ] `FetchAsync`, `BranchExistsAsync`, `ConfigureFetchRefspecAsync`, `SetRemoteHeadAutoAsync` return bare `bool` (error info lost) while siblings return `Git*Result` records with `Success`+`ErrorMessage`. Standardize on result records
- [ ] `GetMasterWorktreePathAsync`/`UpdateMasterWorktreeAsync` hardcode "master" (`Git.GetWorktreePath.cs:28` defaults `branchName = "master"`) while `GetDefaultBranchAsync`/`UpdateDefaultBranchAsync` exist. Standardize on "DefaultBranch" naming
- [ ] CWD inconsistency: `GetDefaultBranchAsync`, `GetCommitsAheadAsync`, `GetWorktreePathAsync`, `UpdateBranchAsync` operate on process CWD with no `repoPath` parameter while `BranchExistsAsync`/`FetchAsync`/`WorktreeAddAsync` take explicit paths. Align

### Minor
- [ ] `git-commands/Git.WorktreeList.cs:44` — `WorktreeListPorcelainAsync` returns `string.Empty` on failure, indistinguishable from "no worktrees" (same unchecked-result family as task 070)
- [ ] `git-commands/Git.WorktreeRemove.cs:106-111` — `FindMainRepositoryFromWorktree` resolves relative `gitdir:` paths (git 2.48+ relative worktrees) against process CWD → bogus path; also returns `<repo>/.git` instead of `<repo>`
- [ ] `Git.GetDefaultBranch.cs:44` — `Replace("origin/", "")` mangles a branch containing "origin/" mid-name (edge case)

## Notes

Found by multi-agent release review (2026-07-04). Verified clean: worktree add `-b <branch> <path> [<start>]` ordering, `WorktreePorcelainParser` logic, `Git.WorktreeRemoveAsync` result checking (task 070's actual bug is in the ganda repo's Zana, not here). Paths relative to `source/timewarp-amuru/`.
