# Add Git bare repo and worktree methods to Amuru

## Description

Add low-level Git methods to the Amuru `Git` static class to support bare repository and worktree operations. These primitives will be used by higher-level services in TimeWarp.Zana for repository and worktree management workflows.

This is part of Task 055 in timewarp-ganda (replace ghq/gwq with native git commands).

## Checklist

- [x] Add `CloneBareAsync(url, targetPath)` - Bare clone without configuration
- [x] Add `ConfigureFetchRefspecAsync(repoPath)` - Set `+refs/heads/*:refs/remotes/origin/*`
- [x] Add `SetRemoteHeadAutoAsync(repoPath)` - Run `git remote set-head origin --auto`
- [x] Add `FetchAsync(repoPath, remote)` - Fetch from remote
- [x] Add `WorktreeListPorcelainAsync(repoPath)` - Return raw porcelain output
- [x] Add `WorktreeAddAsync(repoPath, worktreePath, branch)` - Add worktree for existing branch
- [x] Add `WorktreeAddNewBranchAsync(repoPath, worktreePath, branch)` - Add worktree with `-b`
- [x] Add `WorktreeRemoveAsync(worktreePath)` - Remove worktree
- [x] Add unit tests for all new methods
- [x] Add porcelain parsing utilities (internal)
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

## Results

### Implementation Complete

All 8 required git methods have been implemented in TimeWarp.Amuru:

1. **CloneBareAsync(url, targetPath)** - Git.CloneBare.cs
2. **ConfigureFetchRefspecAsync(repoPath)** - Git.FetchRefspec.cs
3. **SetRemoteHeadAutoAsync(repoPath)** - Git.RemoteHead.cs
4. **FetchAsync(repoPath, remote)** - Git.Fetch.cs
5. **WorktreeListPorcelainAsync(repoPath)** - Git.WorktreeList.cs
6. **WorktreeAddAsync(repoPath, worktreePath, branch)** - Git.WorktreeAdd.cs
7. **WorktreeAddNewBranchAsync(repoPath, worktreePath, branch)** - Git.WorktreeAddNewBranch.cs
8. **WorktreeRemoveAsync(worktreePath)** - Git.WorktreeRemove.cs

### Files Created

**Source Files (9):**
- Source/TimeWarp.Amuru/GitCommands/Git.CloneBare.cs
- Source/TimeWarp.Amuru/GitCommands/Git.FetchRefspec.cs
- Source/TimeWarp.Amuru/GitCommands/Git.RemoteHead.cs
- Source/TimeWarp.Amuru/GitCommands/Git.Fetch.cs
- Source/TimeWarp.Amuru/GitCommands/Git.WorktreeList.cs
- Source/TimeWarp.Amuru/GitCommands/Git.WorktreeAdd.cs
- Source/TimeWarp.Amuru/GitCommands/Git.WorktreeAddNewBranch.cs
- Source/TimeWarp.Amuru/GitCommands/Git.WorktreeRemove.cs
- Source/TimeWarp.Amuru/GitCommands/Git.WorktreePorcelainParser.cs

**Test File (1):**
- Tests/Integration/GitCommands/Git.BareAndWorktree.cs (15 tests)

### New Result Types

- GitCloneResult (bool Success, string? Path, string? ErrorMessage)
- GitWorktreeAddResult (bool Success, string? WorktreePath, string? ErrorMessage)
- GitWorktreeRemoveResult (bool Success, string? ErrorMessage)
- WorktreeEntry (string Path, string? HeadCommit, string? BranchRef, bool IsBare)

### Test Results

- 15/15 tests passed
- All methods tested with both success and failure scenarios
- Porcelain parser tests cover multiple worktrees, bare repos, and edge cases

### Key Decisions

- Made WorktreePorcelainParser public (not internal) to enable testing
- Added helper method to find main repository from worktree path
- Used IReadOnlyList for parser return type to satisfy analyzer rules
