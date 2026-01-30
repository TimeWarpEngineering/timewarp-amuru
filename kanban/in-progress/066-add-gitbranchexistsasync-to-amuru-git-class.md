# Add Git.BranchExistsAsync to Amuru Git class

## Description

Add a low-level `BranchExistsAsync` method to the Amuru `Git` static class for checking if a branch exists in a repository. This will be used by higher-level services like TimeWarp.Zana.WorktreeService.

## Checklist

- [ ] Add `Git.BranchExistsAsync(string repoPath, string branchName)` method
- [ ] Returns `Task<bool>` - true if branch exists, false otherwise
- [ ] Uses `git show-ref --verify refs/heads/{branch}` internally
- [ ] Handle errors gracefully (return false on any error)
- [ ] Add unit tests
- [ ] Update documentation

## Notes

### Usage Pattern
```csharp
bool branchExists = await Git.BranchExistsAsync(repoPath, "feature-branch");
```

### Implementation Reference
Current implementation in Zana (to be moved to Amuru):
```csharp
private static async Task<bool> BranchExistsAsync(string repoPath, string branch)
{
  CommandOutput result = await Shell.Builder("git")
    .WithArguments("show-ref", "--verify", $"refs/heads/{branch}")
    .WithWorkingDirectory(repoPath)
    .WithNoValidation()
    .CaptureAsync();
  return result.Success;
}
```

### Related Work
- TimeWarp.Zana.WorktreeService currently has this as a private helper
- Moving to Amuru aligns with the layered architecture (low-level in Amuru, high-level in Zana)
- This method will be used when creating worktrees to determine if branch already exists
