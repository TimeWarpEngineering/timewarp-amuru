# Extend INuGetPackageService with version comparison helpers

**GitHub Issue:** #66

## Description

The `INuGetPackageService` currently only has `SearchAsync` which returns a raw list of versions. Ganda (and potentially other consumers) need higher-level helpers for version analysis including semantic version comparison, parsing, and update type detection.

## Proposed API Additions

### New Type: `PackageVersionInfo`

```csharp
public sealed record PackageVersionInfo
(
  string? StableVersion,
  string? PrereleaseVersion
);
```

### New Methods on `INuGetPackageService`

```csharp
public interface INuGetPackageService
{
  // Existing
  Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken);
  
  // New
  Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken);
  NuGetVersion? ParseVersion(string version);
  int CompareVersions(string version1, string version2);
  string GetUpdateType(string currentVersion, string latestVersion);
}
```

## Checklist

### Design
- [ ] Create `PackageVersionInfo` record in `NuGetModels.cs`
- [ ] Add new method signatures to `INuGetPackageService`
- [ ] Consider using `NuGet.Versioning` package for semantic version handling (recommended)
- [ ] Design `GetUpdateType` return values (enum vs string - suggest enum)

### Implementation
- [ ] Implement `GetLatestVersionsAsync` - query NuGet and separate stable from prerelease
- [ ] Implement `ParseVersion` - handle leading 'v', validate semver format
- [ ] Implement `CompareVersions` - semantic version comparison (not string comparison)
- [ ] Implement `GetUpdateType` - return "major", "minor", "patch", or "prerelease"
- [ ] Update `NuGetPackageService` with all new method implementations
- [ ] Consider caching strategy for `GetLatestVersionsAsync` results

### Testing
- [ ] Add unit tests for `ParseVersion` with various inputs:
  - [ ] Standard semver: "1.2.3"
  - [ ] With leading 'v': "v1.2.3"
  - [ ] Prerelease: "1.2.3-beta.1"
  - [ ] Invalid versions should return null
- [ ] Add unit tests for `CompareVersions`:
  - [ ] Major version differences
  - [ ] Minor version differences
  - [ ] Patch version differences
  - [ ] Prerelease vs stable comparison
- [ ] Add unit tests for `GetUpdateType`:
  - [ ] Major update detection
  - [ ] Minor update detection
  - [ ] Patch update detection
  - [ ] Prerelease detection
- [ ] Add integration tests for `GetLatestVersionsAsync` with real packages

### Documentation
- [ ] Add XML documentation for all new methods
- [ ] Update `NuGetModels.cs` with documentation for `PackageVersionInfo`
- [ ] Add usage examples in comments

### Refactoring (Optional)
- [ ] Update `RepoCheckVersionService.CheckNuGetVersionAsync` to use new methods
  - Currently uses `string.Compare` which is not semantically correct
  - Should use `CompareVersions` for proper semantic comparison

## Notes

### Current State
- `INuGetPackageService` has only `SearchAsync` method
- `NuGetSearchResult` contains `PackageId` and list of `NuGetPackageVersion`
- `RepoCheckVersionService` does naive string comparison (line 175) which doesn't handle semantic versioning correctly

### Use Case
Ganda's `repo check-version` command needs to:
1. Check if a version is already published (stable or prerelease)
2. Compare versions to determine update type (major/minor/patch)
3. Parse version strings (handling leading 'v', etc.)

### Recommended Approach
Use the `NuGet.Versioning` NuGet package which provides:
- `NuGetVersion` class for parsing and comparing semantic versions
- Handles prerelease versions correctly
- Handles leading 'v' prefix
- Well-tested and maintained

### Files to Modify
- `source/timewarp-amuru/nu-get/NuGetModels.cs` - add `PackageVersionInfo`
- `source/timewarp-amuru/nu-get/INuGetPackageService.cs` - add new method signatures
- `source/timewarp-amuru/nu-get/NuGetPackageService.cs` - implement new methods
- `tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.cs` - add tests

### Context
- Discovered during migration from ganda's local `INuGetPackageService` to Amuru's version
- The local ganda implementation had these methods, but Amuru's public interface doesn't expose them
