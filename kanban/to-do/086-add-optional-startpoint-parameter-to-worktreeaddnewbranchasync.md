# Add optional startPoint parameter to WorktreeAddNewBranchAsync

## Description

Add an optional `startPoint` parameter to `WorktreeAddNewBranchAsync` so that callers can create a new branch from a specific commit-ish / ref instead of always forking from HEAD.

GitHub Issue: https://github.com/TimeWarpEngineering/timewarp-amuru/issues/78

## Checklist

- [ ] Add optional `string? startPoint = null` parameter to `WorktreeAddNewBranchAsync`
- [ ] When `startPoint` is supplied, pass it as the final argument to `git worktree add -b`
- [ ] Verify existing behavior is unchanged when `startPoint` is omitted
- [ ] Update XML documentation and example in the method
- [ ] Validate valid start points (remote tracking branches, local branches, commit SHAs/ranges)

## Notes

### Current Behavior

```
await Git.WorktreeAddNewBranchAsync(repoPath, worktreePath, "feature-x");
```
Produces: `git worktree add -b feature-x <worktreePath>` (always from HEAD)

### Proposed Behavior

When `startPoint` is provided:
```
git worktree add -b <newBranchName> <worktreePath> <startPoint>
```

Valid start points include:
- Remote tracking branches: `origin/develop`
- Local branches: `develop`, `main`
- Commit SHAs / ranges: `abc1234`, `HEAD~3`, `origin/feature-y`

### File to Modify

- `source/timewarp-amuru/git-commands/Git.WorktreeAddNewBranch.cs`

### Related Work

- timewarp-ganda kanban task 143: "Add --from option to worktree add to create new branches from a specific start-point"
- The corresponding change in ganda will be made after a new beta of `TimeWarp.Amuru` containing this overload is published
