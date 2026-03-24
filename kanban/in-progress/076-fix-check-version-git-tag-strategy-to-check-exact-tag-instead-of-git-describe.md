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

## Implementation Plan

### Files to Modify

1. **`source/timewarp-amuru/repo/RepoCheckVersionService.cs`** (lines ~107-114) — Replace `git describe --tags --abbrev=0` with `git tag -l "v{version}"` and update the result-handling logic.
2. **`tools/dev-cli/endpoints/check-version-command.cs`** (lines ~75-78) — Same fix for duplicated logic in the dev CLI command.
3. **`tests/.../repo-check-version-service.cs`** — Add 4 new test methods for the fix.

### Key Design Decisions

- Use `git tag -l "v{version}"` instead of `git describe --tags --abbrev=0` — searches ALL tags globally, not just commit ancestry.
- `git tag -l` always returns exit code 0; empty stdout means tag doesn't exist, non-empty means it exists.
- Both the service and the dev CLI endpoint have duplicated logic and both need the fix.
- Tag naming uses `v` prefix convention consistent with existing repo tags.

### Verification Steps

1. `dotnet build` to verify compilation
2. `./Tests/RunTests.cs` to run all tests
3. `./tools/dev-cli/dev.cs check-version` to verify dev CLI behavior

## Results

### What was implemented
Fixed the check-version git-tag strategy to use exact tag lookup (`git tag -l "v{version}"`) instead of `git describe --tags --abbrev=0`. The old approach found the nearest ancestor tag reachable from HEAD, which gave wrong results on feature branches. The new approach checks if the exact version tag exists globally, regardless of commit ancestry.

### Files changed
- `source/timewarp-amuru/repo/RepoCheckVersionService.cs` — Replaced `git describe` with `git tag -l` in `CheckGitTagVersionAsync`
- `tools/dev-cli/endpoints/check-version-command.cs` — Same fix applied to duplicated logic in dev CLI
- `tests/timewarp-amuru/single-file-tests/repo-services/repo-check-version-service.cs` — Added 2 new test methods with CommandMock
- `tests/timewarp-amuru/multi-file-runners/Directory.Build.props` — Added comment noting repo-services test exclusion

### Key decisions
- `git tag -l` always returns exit code 0; empty stdout = no match, non-empty = match
- Kept existing `GITHUB_REF_NAME` env var check and explicit `tag` parameter paths unchanged
- Both service and dev CLI endpoint had duplicated logic; both were fixed

### Test results
- 5/5 repo-check-version-service tests passed (including 2 new tests)
- Library build succeeded with 0 warnings, 0 errors
