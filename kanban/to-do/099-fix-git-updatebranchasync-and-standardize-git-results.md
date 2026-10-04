# Fix Git UpdateBranchAsync and standardize git results

## Description

The git-commands module (Tools package, `source/timewarp-amuru-tools/git-commands/`) has consistent result records and correct flags, but its most common operation fails in a normal repo, and result/naming conventions drift across members.

**Decision (2026-10-04):** 1.1.1 already shipped, so the API consistency items below are breaking changes to the Tools package. Take them now: task 087 already removed Tools members in this release cycle, and one version bump should carry all of the breaking changes together. Record every removed or renamed public member in Results so the release notes can list them.

**Scope added from task 105:** the git-commands members have no tests for `FindRoot`, `GetRepositoryNameAsync`, `GetWorktreePathAsync`, `IsWorktreeAsync`, `UpdateBranchAsync`, `UpdateWorktreeAsync`. Add real-execution tests (temporary git repos under a temp directory) for those as part of this task; the `UpdateBranchAsync` test must reproduce the checked-out-branch failure before the fix.

## Checklist

### Bug
- [ ] `git-commands/Git.UpdateBranch.cs:53-56` — non-worktree path runs `git fetch origin <branch>:<branch>`, which git REFUSES for the currently checked-out branch. In a normal repo on `master`, `UpdateBranchAsync("master")` — the most common call — always fails. Use `git pull` (or `--update-head-ok` semantics) for the checked-out case

### API consistency (breaking; see Decision above)
- [ ] `FetchAsync`, `BranchExistsAsync`, `ConfigureFetchRefspecAsync`, `SetRemoteHeadAutoAsync` return bare `bool` (error info lost) while siblings return `Git*Result` records with `Success`+`ErrorMessage`. Standardize on result records
- [ ] `GetMasterWorktreePathAsync`/`UpdateMasterWorktreeAsync` hardcode "master" (`Git.GetWorktreePath.cs:28` defaults `branchName = "master"`) while `GetDefaultBranchAsync`/`UpdateDefaultBranchAsync` exist. Standardize on "DefaultBranch" naming
- [ ] CWD inconsistency: `GetDefaultBranchAsync`, `GetCommitsAheadAsync`, `GetWorktreePathAsync`, `UpdateBranchAsync` operate on process CWD with no `repoPath` parameter while `BranchExistsAsync`/`FetchAsync`/`WorktreeAddAsync` take explicit paths. Align

### Minor
- [ ] `git-commands/Git.WorktreeList.cs:44` — `WorktreeListPorcelainAsync` returns `string.Empty` on failure, indistinguishable from "no worktrees" (same unchecked-result family as task 070)
- [ ] `git-commands/Git.WorktreeRemove.cs:106-111` — `FindMainRepositoryFromWorktree` resolves relative `gitdir:` paths (git 2.48+ relative worktrees) against process CWD → bogus path; also returns `<repo>/.git` instead of `<repo>`
- [ ] `Git.GetDefaultBranch.cs:44` — `Replace("origin/", "")` mangles a branch containing "origin/" mid-name (edge case)

### Tests (from task 105)
- [ ] Real-execution tests for `FindRoot`, `GetRepositoryNameAsync`, `GetWorktreePathAsync`, `IsWorktreeAsync`, `UpdateBranchAsync` (checked-out branch and non-checked-out branch), `UpdateWorktreeAsync`
- [ ] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`

## Notes

Found by multi-agent release review (2026-07-04). Verified clean: worktree add `-b <branch> <path> [<start>]` ordering, `WorktreePorcelainParser` logic, `Git.WorktreeRemoveAsync` result checking (task 070's actual bug is in the ganda repo's Zana, not here). Paths relative to `source/timewarp-amuru-tools/` (git-commands moved to the Tools package in the 094 split). Verify member names against current source before editing; some names in the checklist may have drifted since 2026-07-04. Tests live under `tests/timewarp-amuru/single-file-tests/git-commands/`.

## Session

- Kitchen refresh: 522eb63d (2026-10-04)
