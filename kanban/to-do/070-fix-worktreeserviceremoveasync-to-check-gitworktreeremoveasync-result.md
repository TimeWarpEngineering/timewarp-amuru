# Fix WorktreeService.RemoveAsync to check Git.WorktreeRemoveAsync result

## Description

Zana's `WorktreeService.RemoveAsync()` in timewarp-ganda calls `Git.WorktreeRemoveAsync()` from TimeWarp.Amuru but ignores the returned `GitWorktreeRemoveResult`. The result has a `Success` property that indicates whether the removal actually succeeded, but this is never checked.

This causes worktree removal failures to be silently ignored, making ganda report success when the worktree was never actually removed.

## Location

- **Zana (ganda repo)**: `/source/timewarp-zana/git/worktree-service.cs` lines 148-158
- **Amuru (this repo)**: `/Source/TimeWarp.Amuru/GitCommands/Git.WorktreeRemove.cs` lines 29-56

## Current Code (Zana)

```csharp
public static async Task RemoveAsync(string worktreePath)
{
  ArgumentException.ThrowIfNullOrEmpty(worktreePath);

  if (!Directory.Exists(worktreePath))
  {
    throw new DirectoryNotFoundException($"Worktree not found: {worktreePath}");
  }

  // BUG: Result is ignored - failure is silent!
  await Git.WorktreeRemoveAsync(worktreePath, CancellationToken.None).ConfigureAwait(false);
}
```

## Checklist

- [ ] Update Zana's `WorktreeService.RemoveAsync()` to check `GitWorktreeRemoveResult.Success`
- [ ] Throw appropriate exception when `Success` is false
- [ ] Include error message from `GitWorktreeRemoveResult.ErrorMessage` in exception
- [ ] Consider adding `--force` flag option for removing unclean worktrees
- [ ] Add test to verify failure cases are properly reported

## Notes

**Related issues:**
- ganda worktree remove reports success but worktree remains
- Cache is not cleared after failed removal (separate issue)

**The Amuru API returns:**
```csharp
public record GitWorktreeRemoveResult(bool Success, string? ErrorMessage);
```

**Proper usage should be:**
```csharp
GitWorktreeRemoveResult result = await Git.WorktreeRemoveAsync(worktreePath, cancellationToken);
if (!result.Success)
{
  throw new InvalidOperationException($"Failed to remove worktree: {result.ErrorMessage}");
}
```
