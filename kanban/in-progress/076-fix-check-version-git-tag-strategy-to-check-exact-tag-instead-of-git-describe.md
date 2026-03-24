# Fix check-version git-tag strategy to check exact tag instead of git describe

GitHub Issue: https://github.com/TimeWarpEngineering/timewarp-amuru/issues/57

## Description

Bug fix: `RepoCheckVersionService.CheckGitTagVersionAsync` uses `git describe --tags --abbrev=0` to find the "latest" tag, which finds the nearest ancestor tag reachable from HEAD — not the tag matching the current version. This causes incorrect "IsNew: True" results when the exact version tag exists but isn't in the current branch's direct ancestry (e.g., tag was created on master but you're on a feature branch).

**Impact:** This caused a release pipeline to report "safe to release" for a version already published on NuGet, leading to a failed NuGet push (409 Conflict).

## Checklist

- [ ] Locate `RepoCheckVersionService.CheckGitTagVersionAsync` implementation
- [ ] Replace `git describe --tags --abbrev=0` with `git tag -l "v{version}"` to check for the exact version tag
- [ ] Update logic: if tag exists (non-empty stdout), version is already released (IsNew: False); if tag doesn't exist, version is new (IsNew: True)
- [ ] Add/update tests for the fix
- [ ] Verify the fix with the reproduction scenario from the issue

## Notes

### Root Cause

`git describe --tags --abbrev=0` walks the commit ancestry to find the nearest reachable tag. On a feature branch that diverged before the version tag was created on master, it finds an older tag and incorrectly reports the version as new.

### Suggested Fix (from issue)

Replace:
```csharp
CommandOutput commandOutput = await Shell.Builder("git")
    .WithArguments("describe", "--tags", "--abbrev=0")
    .CaptureAsync(cancellationToken);
```

With:
```csharp
CommandOutput commandOutput = await Shell.Builder("git")
    .WithArguments("tag", "-l", $"v{version}")
    .CaptureAsync(cancellationToken);

if (commandOutput.ExitCode == 0 && !string.IsNullOrWhiteSpace(commandOutput.Stdout))
{
    // Tag exists — version already released
    resolvedTag = commandOutput.Stdout.Trim();
}
```

### Reproduction

```
# Version in Directory.Build.props: 3.0.0-beta.67
# Tag v3.0.0-beta.67 exists on remote (and locally after fetch)
# But current branch HEAD descends from v3.0.0-beta.66

dev check-version
# Output: Version: 3.0.0-beta.67, Tag: 3.0.0-beta.66, IsNew: True
# Expected: Version: 3.0.0-beta.67, Tag: v3.0.0-beta.67, IsNew: False
```
