# Fix Git UpdateBranchAsync and standardize git results

## Description

The git-commands module (Tools package, `source/timewarp-amuru-tools/git-commands/`) has consistent result records and correct flags, but its most common operation fails in a normal repo, and result/naming conventions drift across members.

**Decision (2026-10-04):** 1.1.1 already shipped, so the API consistency items below are breaking changes to the Tools package. Take them now: task 087 already removed Tools members in this release cycle, and one version bump should carry all of the breaking changes together. Record every removed or renamed public member in Results so the release notes can list them.

**Scope added from task 105:** the git-commands members have no tests for `FindRoot`, `GetRepositoryNameAsync`, `GetWorktreePathAsync`, `IsWorktreeAsync`, `UpdateBranchAsync`, `UpdateWorktreeAsync`. Add real-execution tests (temporary git repos under a temp directory) for those as part of this task; the `UpdateBranchAsync` test must reproduce the checked-out-branch failure before the fix.

## Checklist

### Bug
- [x] `git-commands/Git.UpdateBranch.cs:53-56` — non-worktree path runs `git fetch origin <branch>:<branch>`, which git REFUSES for the currently checked-out branch. In a normal repo on `master`, `UpdateBranchAsync("master")` — the most common call — always fails. Use `git pull` (or `--update-head-ok` semantics) for the checked-out case

### API consistency (breaking; see Decision above)
- [x] `FetchAsync`, `BranchExistsAsync`, `ConfigureFetchRefspecAsync`, `SetRemoteHeadAutoAsync` return bare `bool` (error info lost) while siblings return `Git*Result` records with `Success`+`ErrorMessage`. Standardize on result records
- [x] `GetMasterWorktreePathAsync`/`UpdateMasterWorktreeAsync` hardcode "master" (`Git.GetWorktreePath.cs:28` defaults `branchName = "master"`) while `GetDefaultBranchAsync`/`UpdateDefaultBranchAsync` exist. Standardize on "DefaultBranch" naming
- [x] CWD inconsistency: `GetDefaultBranchAsync`, `GetCommitsAheadAsync`, `GetWorktreePathAsync`, `UpdateBranchAsync` operate on process CWD with no `repoPath` parameter while `BranchExistsAsync`/`FetchAsync`/`WorktreeAddAsync` take explicit paths. Align

### Minor
- [x] `git-commands/Git.WorktreeList.cs:44` — `WorktreeListPorcelainAsync` returns `string.Empty` on failure, indistinguishable from "no worktrees" (same unchecked-result family as task 070)
- [x] `git-commands/Git.WorktreeRemove.cs:106-111` — `FindMainRepositoryFromWorktree` resolves relative `gitdir:` paths (git 2.48+ relative worktrees) against process CWD → bogus path; also returns `<repo>/.git` instead of `<repo>`
- [x] `Git.GetDefaultBranch.cs:44` — `Replace("origin/", "")` mangles a branch containing "origin/" mid-name (edge case)

### Tests (from task 105)
- [x] Real-execution tests for `FindRoot`, `GetRepositoryNameAsync`, `GetWorktreePathAsync`, `IsWorktreeAsync`, `UpdateBranchAsync` (checked-out branch and non-checked-out branch), `UpdateWorktreeAsync`
- [x] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`

## Notes

Found by multi-agent release review (2026-07-04). Verified clean: worktree add `-b <branch> <path> [<start>]` ordering, `WorktreePorcelainParser` logic, `Git.WorktreeRemoveAsync` result checking (task 070's actual bug is in the ganda repo's Zana, not here). Paths relative to `source/timewarp-amuru-tools/` (git-commands moved to the Tools package in the 094 split). Verify member names against current source before editing; some names in the checklist may have drifted since 2026-07-04. Tests live under `tests/timewarp-amuru/single-file-tests/git-commands/`.

## Results

`UpdateBranchAsync` looks up the branch in the porcelain worktree list (`GetWorktreePathAsync`).

- **Checked out in any work tree (main or linked):** it runs `git -C <path> -c pull.rebase=false pull --ff-only origin <branch>` there and sets `BranchPath` to that path. Before, `BranchPath` was null for a normal repository.
- **Not checked out:** it runs `git fetch origin <branch>:<branch>`, which also covers the HEAD branch of a bare repository.

`UpdateWorktreeAsync` uses the same fast-forward-only pull. Before, it ran a plain `git pull`, so it could create a merge or rebase depending on config.

`GetDefaultBranchAsync` strips only a leading `origin/` prefix. `FindMainRepositoryFromWorktree` resolves a relative `gitdir:` against the worktree directory and returns the repository directory (the parent of `.git` for a non-bare repo).

Package `<Version>` is still `1.1.1`. The release that ships this commit needs a version greater than `1.1.1` so these breaks are not published as `1.1.1`.

### Removed or renamed public members

- Removed `Git.GetMasterWorktreePathAsync`. Replacement: `Git.GetDefaultWorktreePathAsync` (detects the default branch, then finds its worktree).
- Removed `Git.UpdateMasterWorktreeAsync`. Replacement: `Git.UpdateDefaultWorktreeAsync`.
- `Git.GetWorktreePathAsync`, `Git.UpdateBranchAsync`, and `Git.UpdateWorktreeAsync` no longer default `branchName` to `"master"`. The branch name is required. Working-directory overloads remain: `(string branchName, CancellationToken cancellationToken = default)`.
- Added repository-path overloads (null path means process working directory) for `GetDefaultBranchAsync`, `UpdateDefaultBranchAsync`, `GetCommitsAheadAsync`, `GetCommitsAheadOfDefaultBranchAsync`, `GetWorktreePathAsync`, `GetDefaultWorktreePathAsync`, `UpdateBranchAsync`, `UpdateDefaultBranchAsync`, `UpdateWorktreeAsync`, and `UpdateDefaultWorktreeAsync`. Existing zero-argument and `(CancellationToken)` calls of the default-branch helpers still compile.
- `FetchAsync` returns `GitFetchResult` instead of `bool`.
- `BranchExistsAsync` returns `GitBranchExistsResult` (`Success`, `Exists`, `ErrorMessage`) instead of `bool`. It runs `git show-ref --verify --quiet`. Exit code 1 (missing ref) is `Success` true and `Exists` false. Other failures set `Success` false and keep the git message.
- `ConfigureFetchRefspecAsync` returns `GitConfigureFetchRefspecResult` instead of `bool`.
- `SetRemoteHeadAutoAsync` returns `GitSetRemoteHeadResult` instead of `bool`.
- `WorktreeListPorcelainAsync` returns `GitWorktreeListResult` (`Success`, `Porcelain`, `ErrorMessage`) instead of `string`. Failure is `Success` false and `Porcelain` null, not `string.Empty`.
- `GetCommitsAheadAsync(string branchName = "master", ...)` still defaults the comparison branch to `"master"`. `GetCommitsAheadOfDefaultBranchAsync` remains the default-branch entry point.

### How to validate

**Smoke**

```bash
dotnet run tests/timewarp-amuru/single-file-tests/git-commands/git.update-branch.cs
dotnet run tests/timewarp-amuru/single-file-tests/git-commands/git.update-worktree.cs
dotnet run tests/timewarp-amuru/single-file-tests/git-commands/git.worktree-remove.cs
dotnet run tests/timewarp-amuru/single-file-tests/git-commands/git.get-default-branch.cs -- --filter-method OriginInsideBranchName
dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs
```

**Expect**

- `UpdateBranch_Given_.CheckedOutBranch_Should_FastForwardFromOrigin`: exit 0. A clone whose checked-out `master` is behind a local origin fast-forwards to that origin commit. (`git fetch origin master:master` refuses this case.)
- `UpdateBranch_Given_.OtherBranch_Should_UpdateRefWithoutLeavingMaster`: exit 0. `HEAD` stays on `master` and `refs/heads/feature` matches origin.
- `WorktreeRemove_Given_.RelativeGitdirAndForeignWorkingDirectory_Should_RemoveWorktree`: exit 0. Removal succeeds while process CWD is not the repository, after the worktree `.git` file is rewritten to a relative `gitdir:`.
- `SymbolicRefWithOriginInsideBranchName_Should_StripOnlyLeadingPrefix`: branch name is `feature/origin/topic`.
- `UpdateBranch_Given_.BareRepository_Should_UpdateHeadBranchByFetch` and `.BranchCheckedOutInLinkedWorktree_Should_PullInThatWorktree`: exit 0.
- `BranchExists_Given_.RealMissingBranch_Should_ReturnFalseWithoutError`: exit 0.
- Full runner: Failed 0. After the review fixes: Passed 551, Skipped 1, Total 552.

### Review disposition

- **Process:** effort 3 (by-diff budget, 1804 lines). Roster: general. 2 rounds.
- **Final counts:**
  - bug: 2 fixed
  - suggestion: 1 fixed
  - nit: 1 fixed, 1 wontfix
  - open: 0
- **Disposition:** `accepted-exceptions`. M5 (a test changes the process CWD) is wontfix because changing the CWD is what the relative-gitdir regression test checks. It follows the existing repo pattern and restores the CWD in `finally`.
- **Fixes made on this task:**
  - `BranchExistsAsync` adds `--quiet`. Without it, real git exits 128 for a missing ref.
  - `UpdateBranchAsync` handles bare repositories and branches checked out in another linked worktree.
  - The worktree pull is fast-forward only.
- **Artifacts:** `review/review-framework.md`, `review/round-1/{general,merged}.md`, `review/round-2/{general,merged}.md`, `review/disposition.md`.
- `ganda repo audit`: passes.

## Session

- Kitchen refresh: 522eb63d (2026-10-04)
- Implementation: grok 01a1076b-f96a-78c1-b878-cca5d62d7d80 (2026-10-04)
- Review oracle: claude-opus-5-5 (2026-10-04). General reviewer and fix pass ran as Claude Sonnet subagents. Effort 3, 2 rounds, accepted-exceptions.
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-04T15:23:25Z
