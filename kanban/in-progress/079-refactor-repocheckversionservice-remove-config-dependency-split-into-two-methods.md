# Refactor RepoCheckVersionService - remove config dependency, split into two methods

## Description

Remove config-related files from Amuru (public repo should not reference private "ganda" branding) and refactor `IRepoCheckVersionService` to expose two focused methods instead of one strategy-switching method.

## Checklist

- [ ] Delete config-related files from Amuru
  - [ ] `source/timewarp-amuru/repo/IRepoConfigService.cs`
  - [ ] `source/timewarp-amuru/repo/RepoConfigService.cs`
  - [ ] `source/timewarp-amuru/repo/RepoConfig.cs`
  - [ ] `source/timewarp-amuru/repo/RepoConfigJsonContext.cs`
- [ ] Rewrite `IRepoCheckVersionService.cs`
  - [ ] Remove `CheckVersionResult` record
  - [ ] Add `GitTagCheckResult` record
  - [ ] Add `NuGetCheckResult` record
  - [ ] Replace `CheckAsync` with `CheckGitTagVersionAsync(string? tag = null, CancellationToken)`
  - [ ] Add `CheckNuGetVersionAsync(IReadOnlyList<string> packages, CancellationToken)`
- [ ] Rewrite `RepoCheckVersionService.cs`
  - [ ] Remove `IRepoConfigService` dependency from constructor
  - [ ] Remove config reading from `CheckAsync`
  - [ ] Split into two methods matching the interface
  - [ ] `CheckGitTagVersionAsync` — no package param, optional tag
  - [ ] `CheckNuGetVersionAsync` — required `IReadOnlyList<string> packages`
- [ ] Update tests in `tests/timewarp-amuru/single-file-tests/repo-services/`
  - [ ] Delete `MockRepoConfigService` from test file
  - [ ] Update tests to call `CheckGitTagVersionAsync` or `CheckNuGetVersionAsync` directly
  - [ ] Update assertions for new result types

## Notes

### Problem Statement

Amuru is a **public** repo but contains references to "ganda" (a private internal tool):
- `RepoConfigService` reads from `.timewarp/ganda.jsonc`
- This leaks private branding into public code

Additionally, the config service is an **application concern**, not a library concern. Amuru provides primitives (process execution, shell commands) but shouldn't dictate how applications configure themselves.

### Design Decision: Two Methods vs Strategy Switching

**Before:**
```csharp
Task<CheckVersionResult> CheckAsync(string? strategy, string? package, string? tag, ...);
// String-based dispatch, params that only apply to one strategy
```

**After:**
```csharp
Task<GitTagCheckResult> CheckGitTagVersionAsync(string? tag = null, CancellationToken = default);
Task<NuGetCheckResult> CheckNuGetVersionAsync(IReadOnlyList<string> packages, CancellationToken = default);
// Direct method call, type-safe params, focused result types
```

### New Result Types

```csharp
public sealed record GitTagCheckResult
(
  bool IsNewVersion,
  string Version,
  string? LatestReleaseTag
);

public sealed record NuGetCheckResult
(
  bool IsNewVersion,
  string Version,
  string? LatestNuGetVersion,
  IReadOnlyList<string> CheckedPackages,
  IReadOnlyList<string>? AlreadyPublishedPackages
);
```

### Caller Example

```csharp
// Git tag check
GitTagCheckResult result = await CheckVersionService.CheckGitTagVersionAsync(cancellationToken: ct);

// NuGet check
string[] packages = ["TimeWarp.Amuru", "TimeWarp.Nuru"];
NuGetCheckResult result = await CheckVersionService.CheckNuGetVersionAsync(packages, ct);
```

### Files to Delete

| File | Reason |
|------|--------|
| `IRepoConfigService.cs` | Config policy doesn't belong in Amuru |
| `RepoConfigService.cs` | Hardcoded `.timewarp/ganda.jsonc` path |
| `RepoConfig.cs` | Application-level config schema |
| `RepoConfigJsonContext.cs` | Serialization for app config |

### Follow-up Sequence

This task is step 1 of a larger migration:

```
1. Refactor Amuru (this task)
       │
       ▼
2. Release new Amuru version
       │
       ▼
3. Nuru.DevCli updates to use new Amuru
       │
       ▼
4. Amuru's dev-cli updates to use Nuru.DevCli shared endpoints
```

**NOT in this task:**
- Updating Amuru's `tools/dev-cli/endpoints/check-version-command.cs` (step 4)
- Updating Nuru dev-cli (step 3)

The dev-cli updates happen after Nuru.DevCli consumes the new Amuru release.
