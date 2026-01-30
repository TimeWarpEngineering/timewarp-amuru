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

---

## Implementation Plan

### Files to Create/Modify

| Action | File Path |
|--------|-----------|
| **Create** | `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/cramer-2026-01-29-dev/Source/TimeWarp.Amuru/GitCommands/Git.BranchExists.cs` |
| **Create** | `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/cramer-2026-01-29-dev/tests/timewarp-amuru/single-file-tests/git-commands/git.branch-exists.cs` |
| **Update** | `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/cramer-2026-01-29-dev/Documentation/Developer/Reference/GitCommands.md` |

### 1. Create Git.BranchExists.cs

Implementation following patterns from Git.Fetch.cs:

```csharp
namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - BranchExistsAsync implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Checks if a branch exists in the repository.
  /// Uses 'git show-ref --verify refs/heads/{branch}' to verify branch existence.
  /// </summary>
  /// <param name="repoPath">The path to the repository.</param>
  /// <param name="branchName">The branch name to check.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>True if the branch exists, false otherwise.</returns>
  public static async Task<bool> BranchExistsAsync(
    string repoPath,
    string branchName,
    CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("show-ref", "--verify", $"refs/heads/{branchName}")
      .WithWorkingDirectory(repoPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    return result.Success;
  }
}
```

### 2. Create Unit Test

File: `tests/timewarp-amuru/single-file-tests/git-commands/git.branch-exists.cs`

Test cases:
- `ExistingBranch_Should_ReturnTrue`
- `NonExistingBranch_Should_ReturnFalse`
- `InvalidRepo_Should_ReturnFalse`

### 3. Update Documentation

Add `BranchExistsAsync` section to `Documentation/Developer/Reference/GitCommands.md` under "Branch Detection".
