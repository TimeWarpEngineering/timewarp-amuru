# Add optional startPoint parameter to WorktreeAddNewBranchAsync

## Description

Add an optional `startPoint` parameter to `WorktreeAddNewBranchAsync` so that callers can create a new branch from a specific commit-ish / ref instead of always forking from HEAD.

GitHub Issue: https://github.com/TimeWarpEngineering/timewarp-amuru/issues/78

## Checklist

- [x] Add optional `string? startPoint = null` parameter to `WorktreeAddNewBranchAsync`
- [x] When `startPoint` is supplied, pass it as the final argument to `git worktree add -b`
- [x] Verify existing behavior is unchanged when `startPoint` is omitted
- [x] Update XML documentation and example in the method
- [x] Add tests for startPoint parameter (remote tracking branch and commit SHA scenarios)

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

## Results

- Added `string? startPoint = null` optional parameter to `WorktreeAddNewBranchAsync` (positioned before `CancellationToken` to maintain convention)
- When `startPoint` is provided, it is appended to the `git worktree add -b` arguments as the final positional argument
- When `startPoint` is null (omitted), behavior is unchanged — command is identical to before
- Updated XML doc summary, added `<param>` doc for `startPoint`, and expanded the `<example>` block with both usage patterns
- Added two new test methods: `StartPoint_Should_CreateWorktreeFromSpecifiedRef` (remote tracking branch) and `StartPoint_WithCommitSha_Should_CreateWorktreeFromCommit` (commit SHA)
- Build verified: `dotnet build source/timewarp-amuru/timewarp-amuru.csproj` succeeds with 0 warnings, 0 errors
- Note: Integration test runner has a pre-existing experimental feature flag issue unrelated to this change
