# Add Base Branch Auto-Detection

> GitHub Issue: #41

## Description

Add auto-detection of the default/base branch (master/main/dev) and deprecate the existing hardcoded `*Master*` method variants in the Git helper class.

## Background

The `Git` helper class currently has methods that hardcode "master":
- `Git.UpdateMasterAsync()`
- `Git.GetCommitsAheadOfMasterAsync()`

This doesn't work for repositories using `main` or `dev` as their default branch.

## Requirements

- Auto-detect the default branch from the repository
- Provide new methods that use the detected default branch
- Deprecate existing hardcoded master methods with `[Obsolete]` attribute

## Checklist

### Implementation
- [x] Implement `Git.GetDefaultBranchAsync()` - Auto-detect the default branch
  - Check `git symbolic-ref refs/remotes/origin/HEAD --short`
  - Fallback: check existence of common branch names (main, master, dev)
- [x] Implement `Git.UpdateDefaultBranchAsync()` - Update the detected default branch
- [x] Implement `Git.GetCommitsAheadOfDefaultBranchAsync()` - Get commits ahead of detected default branch
- [x] Remove `Git.UpdateMasterAsync()` (replaced by `UpdateDefaultBranchAsync`)
- [x] Remove `Git.GetCommitsAheadOfMasterAsync()` (replaced by `GetCommitsAheadOfDefaultBranchAsync`)
- [x] Add integration tests for new methods

### Documentation
- [ ] Update API documentation for Git commands

## Notes

Detection strategy for `GetDefaultBranchAsync()`:

```csharp
public static async Task<string?> GetDefaultBranchAsync()
{
  // Option 1: Check symbolic ref
  var result = await Shell.Builder("git")
    .WithArguments("symbolic-ref", "refs/remotes/origin/HEAD", "--short")
    .CaptureAsync();
  
  if (result.Success)
    return result.Stdout.Trim().Replace("origin/", "");

  // Option 2: Check common branch names
  foreach (var branch in new[] { "main", "master", "dev" })
  {
    var exists = await Shell.Builder("git")
      .WithArguments("show-ref", "--verify", "--quiet", $"refs/remotes/origin/{branch}")
      .CaptureAsync();
    
    if (exists.Success)
      return branch;
  }

  return null;
}
```

## Consumer

This is needed by `timewarp-ganda` for the `repo base` and `repo base sync` commands.

## Related

- timewarp-ganda task 026: Create unified `repo` command group
