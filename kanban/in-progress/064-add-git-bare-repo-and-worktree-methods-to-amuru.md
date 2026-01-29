# Add Git bare repo and worktree methods to Amuru

## Description

Add low-level Git methods to the Amuru `Git` static class to support bare repository and worktree operations. These primitives will be used by higher-level services in TimeWarp.Zana for repository and worktree management workflows.

This is part of Task 055 in timewarp-ganda (replace ghq/gwq with native git commands).

## Checklist

- [ ] Add `CloneBareAsync(url, targetPath)` - Bare clone without configuration
- [ ] Add `ConfigureFetchRefspecAsync(repoPath)` - Set `+refs/heads/*:refs/remotes/origin/*`
- [ ] Add `SetRemoteHeadAutoAsync(repoPath)` - Run `git remote set-head origin --auto`
- [ ] Add `FetchAsync(repoPath, remote)` - Fetch from remote
- [ ] Add `WorktreeListPorcelainAsync(repoPath)` - Return raw porcelain output
- [ ] Add `WorktreeAddAsync(repoPath, worktreePath, branch)` - Add worktree for existing branch
- [ ] Add `WorktreeAddNewBranchAsync(repoPath, worktreePath, branch)` - Add worktree with `-b`
- [ ] Add `WorktreeRemoveAsync(worktreePath)` - Remove worktree
- [ ] Add unit tests for all new methods
- [ ] Add porcelain parsing utilities (internal)
- [ ] Update documentation

## Notes

## Implementation Plan

### File Structure

Create 9 new source files in `Source/TimeWarp.Amuru/GitCommands/`:
1. `Git.CloneBare.cs` - CloneBareAsync method
2. `Git.FetchRefspec.cs` - ConfigureFetchRefspecAsync method
3. `Git.RemoteHead.cs` - SetRemoteHeadAutoAsync method
4. `Git.Fetch.cs` - FetchAsync method
5. `Git.WorktreeList.cs` - WorktreeListPorcelainAsync method
6. `Git.WorktreeAdd.cs` - WorktreeAddAsync method
7. `Git.WorktreeAddNewBranch.cs` - WorktreeAddNewBranchAsync method
8. `Git.WorktreeRemove.cs` - WorktreeRemoveAsync method
9. `Git.WorktreePorcelainParser.cs` - Internal parsing utilities

Create 1 test file:
- `Tests/Integration/GitCommands/Git.BareAndWorktree.cs`

### Result Types to Add

1. `GitCloneResult` - (bool Success, string? Path, string? ErrorMessage)
2. `GitWorktreeAddResult` - (bool Success, string? WorktreePath, string? ErrorMessage)
3. `GitWorktreeRemoveResult` - (bool Success, string? ErrorMessage)
4. `WorktreeEntry` - (string Path, string? HeadCommit, string? BranchRef, bool IsBare)

### Implementation Pattern

All methods follow existing patterns:
- Use `Shell.Builder("git").WithArguments(...).WithNoValidation().CaptureAsync()`
- Parse stdout/stderr appropriately
- Return strongly-typed results
- Support CancellationToken

### Testing Pattern

Use existing CommandMock pattern with Shouldly assertions.

### Design Decisions

1. Keep methods low-level - no path conventions
2. Return raw porcelain string, provide internal parser for Zana to use
3. Use simple ErrorMessage (not structured) - keeps API minimal
4. Use existing file naming convention (Git.BareAndWorktree.cs not .Tests.cs)

### Related Work
- **Task 055**: Replace ghq and gwq with native git commands in timewarp-ganda
- **Consumer**: TimeWarp.Zana.GitRepoService and WorktreeService

### API Design Principles
- All methods should be `async Task<T>` (async-only)
- Keep low-level - no TimeWarp-specific path conventions here
- Zana will handle the high-level workflows (`~/repos/`, `~/worktrees/` structures)

### Target Release
- Version: 1.0.0-beta.18 (or next beta)
- Repository: TimeWarpEngineering/timewarp-amuru
