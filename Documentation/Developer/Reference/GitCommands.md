# Git Commands Reference

Complete reference for TimeWarp.Amuru Git helper methods.

## Overview

The `Git` static class provides convenient methods for common Git operations, integrating seamlessly with the TimeWarp.Amuru shell execution framework.

## Branch Detection

### GetDefaultBranchAsync

Auto-detects the default branch of the repository.

**Signature:**
```csharp
public static async Task<GitDefaultBranchResult> GetDefaultBranchAsync(
    CancellationToken cancellationToken = default)
```

**Returns:** `GitDefaultBranchResult`
- `Success` - True if detection succeeded
- `BranchName` - The detected branch name (null if failed)
- `ErrorMessage` - Error message if failed (null if succeeded)

**Detection Strategy:**
1. First tries `git symbolic-ref refs/remotes/origin/HEAD --short`
2. Falls back to checking for common branch names: `main`, `master`, `dev`

**Example:**
```csharp
GitDefaultBranchResult result = await Git.GetDefaultBranchAsync();
if (result.Success)
{
    Console.WriteLine($"Default branch: {result.BranchName}");
}
else
{
    Console.WriteLine($"Failed to detect: {result.ErrorMessage}");
}
```

## Branch Updates

### UpdateBranchAsync

Updates a specific branch from origin, handling both worktree and regular repository configurations.

**Signature:**
```csharp
public static async Task<GitBranchUpdateResult> UpdateBranchAsync(
    string branchName = "master",
    CancellationToken cancellationToken = default)
```

**Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `branchName` | string | "master" | The branch name to update |
| `cancellationToken` | CancellationToken | default | Cancellation token |

**Returns:** `GitBranchUpdateResult`
- `Success` - True if the update succeeded
- `BranchPath` - Path to the branch's worktree (null if not using worktrees)
- `ErrorMessage` - Error message if failed (null if succeeded)

**Behavior:**
- If in a worktree configuration, updates the branch in its worktree directory
- Otherwise, uses `git fetch origin branch:branch` to update the local ref

**Example:**
```csharp
// Update master branch
GitBranchUpdateResult result = await Git.UpdateBranchAsync("master");
if (result.Success)
{
    Console.WriteLine(result.BranchPath != null
        ? $"Updated master at: {result.BranchPath}"
        : "Updated master");
}

// Update a feature branch
GitBranchUpdateResult featureResult = await Git.UpdateBranchAsync("feature-branch");
```

### UpdateDefaultBranchAsync

Updates the default branch (main/master/dev) from origin with auto-detection.

**Signature:**
```csharp
public static async Task<GitBranchUpdateResult> UpdateDefaultBranchAsync(
    CancellationToken cancellationToken = default)
```

**Returns:** `GitBranchUpdateResult`
- `Success` - True if the update succeeded
- `BranchPath` - Path to the branch's worktree (null if not using worktrees)
- `ErrorMessage` - Error message if failed (null if succeeded)

**Behavior:**
1. Auto-detects the default branch using `GetDefaultBranchAsync()`
2. Updates the detected branch using `UpdateBranchAsync()`

**Example:**
```csharp
GitBranchUpdateResult result = await Git.UpdateDefaultBranchAsync();
if (result.Success)
{
    Console.WriteLine(result.BranchPath != null
        ? $"Updated default branch at: {result.BranchPath}"
        : "Updated default branch");
}
else
{
    Console.WriteLine($"Failed: {result.ErrorMessage}");
}
```

## Commit Comparisons

### GetCommitsAheadAsync

Gets the number of commits the current branch is ahead of a specified branch.

**Signature:**
```csharp
public static async Task<GitCommitCountResult> GetCommitsAheadAsync(
    string branchName = "master",
    CancellationToken cancellationToken = default)
```

**Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `branchName` | string | "master" | The branch to compare against |
| `cancellationToken` | CancellationToken | default | Cancellation token |

**Returns:** `GitCommitCountResult`
- `Success` - True if the count succeeded
- `Count` - Number of commits ahead (0 if failed or equal)
- `ErrorMessage` - Error message if failed (null if succeeded)

**Example:**
```csharp
// Compare against master
GitCommitCountResult result = await Git.GetCommitsAheadAsync("master");
if (result.Success)
{
    Console.WriteLine($"Commits ahead of master: {result.Count}");
}

// Compare against a different branch
GitCommitCountResult devResult = await Git.GetCommitsAheadAsync("develop");
```

### GetCommitsAheadOfDefaultBranchAsync

Gets the number of commits the current branch is ahead of the default branch with auto-detection.

**Signature:**
```csharp
public static async Task<GitCommitCountResult> GetCommitsAheadOfDefaultBranchAsync(
    CancellationToken cancellationToken = default)
```

**Returns:** `GitCommitCountResult`
- `Success` - True if the count succeeded
- `Count` - Number of commits ahead (0 if failed or equal)
- `ErrorMessage` - Error message if failed (null if succeeded)

**Behavior:**
1. Auto-detects the default branch using `GetDefaultBranchAsync()`
2. Counts commits using `GetCommitsAheadAsync()`

**Example:**
```csharp
GitCommitCountResult result = await Git.GetCommitsAheadOfDefaultBranchAsync();
if (result.Success)
{
    Console.WriteLine($"Commits ahead of default branch: {result.Count}");
}
else
{
    Console.WriteLine($"Failed: {result.ErrorMessage}");
}
```

## Repository Information

### FindRoot

Finds the root directory of the current Git repository.

**Signature:**
```csharp
public static string? FindRoot(string? startPath = null)
```

**Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `startPath` | string? | null | Starting directory (current directory if null) |

**Returns:** The repository root path, or null if not in a Git repository.

**Example:**
```csharp
string? gitRoot = Git.FindRoot();
if (gitRoot != null)
{
    Console.WriteLine($"Repository root: {gitRoot}");
}
```

### GetRepositoryNameAsync

Gets the name of the repository from the remote origin URL.

**Signature:**
```csharp
public static async Task<string?> GetRepositoryNameAsync(
    string? gitRoot = null,
    CancellationToken cancellationToken = default)
```

**Returns:** The repository name extracted from the origin URL, or null if unavailable.

**Example:**
```csharp
string? repoName = await Git.GetRepositoryNameAsync();
if (repoName != null)
{
    Console.WriteLine($"Repository: {repoName}");
}
```

## Worktree Support

### IsWorktree

Checks if the current directory is within a Git worktree.

**Signature:**
```csharp
public static bool IsWorktree()
```

**Returns:** True if in a worktree, false otherwise.

### GetWorktreePathAsync

Gets the path to a specific branch's worktree.

**Signature:**
```csharp
public static async Task<string?> GetWorktreePathAsync(
    string branchName,
    CancellationToken cancellationToken = default)
```

**Returns:** The worktree path for the specified branch, or null if not found.

## Result Types

### GitDefaultBranchResult

```csharp
public record GitDefaultBranchResult(
    bool Success,
    string? BranchName,
    string? ErrorMessage);
```

### GitBranchUpdateResult

```csharp
public record GitBranchUpdateResult(
    bool Success,
    string? BranchPath,
    string? ErrorMessage);
```

### GitCommitCountResult

```csharp
public record GitCommitCountResult(
    bool Success,
    int Count,
    string? ErrorMessage);
```

## Common Usage Patterns

### Sync with Default Branch

```csharp
// Update the default branch and check how far ahead we are
GitBranchUpdateResult updateResult = await Git.UpdateDefaultBranchAsync();
if (updateResult.Success)
{
    GitCommitCountResult countResult = await Git.GetCommitsAheadOfDefaultBranchAsync();
    if (countResult.Success)
    {
        Console.WriteLine($"Branch is {countResult.Count} commits ahead of default branch");
    }
}
```

### Repository Information Script

```csharp
string? root = Git.FindRoot();
if (root == null)
{
    Console.WriteLine("Not in a Git repository");
    return;
}

string? name = await Git.GetRepositoryNameAsync(root);
GitDefaultBranchResult defaultBranch = await Git.GetDefaultBranchAsync();

Console.WriteLine($"Repository: {name}");
Console.WriteLine($"Root: {root}");
Console.WriteLine($"Default branch: {defaultBranch.BranchName ?? "unknown"}");
Console.WriteLine($"Is worktree: {Git.IsWorktree()}");
```

## See Also

- [Shell Commands Reference](ShellCommands.md) - General shell command reference
- [How to Use ScriptContext](../HowToGuides/HowToUseScriptContext.md) - Script context usage
