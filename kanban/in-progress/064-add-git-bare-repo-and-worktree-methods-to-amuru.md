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

### Related Work
- **Task 055**: Replace ghq and gwq with native git commands in timewarp-ganda
- **Consumer**: TimeWarp.Zana.GitRepoService and WorktreeService

### API Design Principles
- All methods should be `async Task<T>` (async-only)
- Keep low-level - no TimeWarp-specific path conventions here
- Zana will handle the high-level workflows (`~/repos/`, `~/worktrees/` structures)

### Porcelain Format Reference
```
worktree /path/to/worktree
HEAD abc123def456...
branch refs/heads/main

worktree /path/to/bare/repo.git
bare
```

### Target Release
- Version: 1.0.0-beta.18 (or next beta)
- Repository: TimeWarpEngineering/timewarp-amuru
