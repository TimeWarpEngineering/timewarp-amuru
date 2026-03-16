# Add Git.SetUpstreamAsync method for branch tracking

## Description

[Brief description of the task]

## Checklist

- [ ] Item 1
- [ ] Item 2

## Notes

[Additional context]

## Results

- Created `Git.SetUpstreamAsync` method in `Source/TimeWarp.Amuru/GitCommands/Git.SetUpstream.cs`
- Parameters: `worktreePath`, `branchName`, `remote = "origin"`, `fetchFirst = false`, `CancellationToken`
- `fetchFirst: true` fetches from remote before setting upstream (ensures remote branch ref exists)
- `GitSetUpstreamResult` record with `Success` and `ErrorMessage`
- 5 unit tests in `tests/timewarp-amuru/single-file-tests/git-commands/git.set-upstream.cs`
- All 355 tests passed
- Committed in: `bfec64c feat: add Git.SetUpstreamAsync for branch tracking (closes #49)`

**Files created:**
- `Source/TimeWarp.Amuru/GitCommands/Git.SetUpstream.cs`
- `tests/timewarp-amuru/single-file-tests/git-commands/git.set-upstream.cs`
