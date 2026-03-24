# Extend INuGetPackageService with version comparison helpers

**GitHub Issue:** #66

## Description

The `INuGetPackageService` currently only has `SearchAsync` which returns a raw list of versions. Ganda needs higher-level helpers for version analysis.

**Approach:** Amuru should use `NuGet.Versioning` internally and expose a clean API. Consumers shouldn't need to reference `NuGet.Versioning` directly.

## Proposed API Additions

### New Type: `PackageVersionInfo`

```csharp
public sealed class PackageVersionInfo
{
  public string? StableVersion { get; set; }
  public string? PrereleaseVersion { get; set; }
}
```

### New Methods on `INuGetPackageService`

```csharp
public interface INuGetPackageService
{
  // Existing
  Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken);
  
  // New - uses NuGet.Versioning internally, exposes simple string-based APIs
  Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken);
  string? ParseVersion(string version);
  int CompareVersions(string version1, string version2);
  string GetUpdateType(string currentVersion, string latestVersion);
}
```

**Note:** `ParseVersion`, `CompareVersions`, and `GetUpdateType` use `NuGet.Versioning` internally but expose simple string-based APIs for consumers.

## Checklist

### Setup
- [ ] Add `NuGet.Versioning` package reference to Amuru
- [ ] Publish dev-cli with AOT to validate `NuGet.Versioning` compatibility
- [ ] Address any AOT warnings/errors if they arise

### Design
- [ ] Create `PackageVersionInfo` class in `NuGetModels.cs`
- [ ] Add new method signatures to `INuGetPackageService`
- [ ] Design `GetUpdateType` return values: "major", "minor", "patch", "stable", or "none"

### Implementation
- [ ] Implement `GetLatestVersionsAsync` - calls `SearchAsync` and extracts latest stable/prerelease from versions list
- [ ] Implement `ParseVersion` - wraps `NuGetVersion.TryParse()`, handles leading 'v', returns normalized string or null
- [ ] Implement `CompareVersions` - wraps `NuGetVersion.CompareTo()`, returns -1, 0, or 1
- [ ] Implement `GetUpdateType` - uses `Major`/`Minor`/`Patch`/`IsPrerelease` to determine update type
- [ ] Update `NuGetPackageService` with all new method implementations

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
  - [ ] No update (same version)
- [ ] Add integration tests for `GetLatestVersionsAsync` with real packages

### Documentation
- [ ] Add XML documentation for all new methods
- [ ] Update `NuGetModels.cs` with documentation for `PackageVersionInfo`
- [ ] Add usage examples in comments

### Refactoring
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

### Implementation Details

| Method | Implementation |
|--------|----------------|
| `ParseVersion` | Wraps `NuGetVersion.TryParse()`, handles leading 'v', returns normalized string or null |
| `CompareVersions` | Wraps `NuGetVersion.CompareTo()`, returns -1, 0, or 1 |
| `GetUpdateType` | Uses `Major`/`Minor`/`Patch`/`IsPrerelease` to determine "major", "minor", "patch", "stable", or "none" |
| `GetLatestVersionsAsync` | Calls `SearchAsync` and extracts latest stable/prerelease from the versions list |

### AOT Validation Strategy
- dev-cli has `PublishAot=true` and references Amuru
- Publishing dev-cli with AOT will validate `NuGet.Versioning` compatibility
- If AOT compilation succeeds, `NuGet.Versioning` is compatible
- If AOT compilation fails, we'll see specific warnings/errors to address

### Files to Modify
- `source/timewarp-amuru/timewarp-amuru.csproj` - add `NuGet.Versioning` package reference
- `source/timewarp-amuru/nu-get/NuGetModels.cs` - add `PackageVersionInfo`
- `source/timewarp-amuru/nu-get/INuGetPackageService.cs` - add new method signatures
- `source/timewarp-amuru/nu-get/NuGetPackageService.cs` - implement new methods
- `tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.cs` - add tests

### Context
- Discovered during migration from ganda's local `INuGetPackageService` to Amuru's version
- Ganda's local implementation had these methods using `NuGet.Versioning` internally
- Moving to Amuru keeps all NuGet-related logic in one place

## Implementation Plan

### Phase 1: Setup & AOT Validation
1. Add NuGet.Versioning package reference to Directory.Packages.props and timewarp-amuru.csproj
2. Validate AOT compatibility by publishing dev-cli

### Phase 2: Design - Data Models
1. Add PackageVersionInfo record to NuGetModels.cs

### Phase 3: Design - Interface Updates
1. Update INuGetPackageService with 4 new method signatures

### Phase 4: Implementation - SearchAsync Enhancement
1. Verify if dotnet package search returns all versions
2. May need to use NuGet API directly if not

### Phase 5: Implementation - New Methods
1. ParseVersion - wraps NuGetVersion.TryParse(), handles leading 'v'
2. CompareVersions - wraps NuGetVersion.CompareTo()
3. GetUpdateType - determines "major", "minor", "patch", "stable", or "none"
4. GetLatestVersionsAsync - extracts latest stable/prerelease from versions

### Phase 6: Testing
1. Unit tests for ParseVersion, CompareVersions, GetUpdateType
2. Integration tests for GetLatestVersionsAsync

### Phase 7: Refactoring
1. Update RepoCheckVersionService to use CompareVersions

### Phase 8: Documentation
1. Add XML documentation for all new methods

### Risk: SearchAsync Version Discovery
The current `dotnet package search --exact-match --format json` may only return the latest version. May need to query NuGet API directly: `https://api.nuget.org/v3-flatcontainer/{packageId}/index.json`

### Execution Order
1. Add NuGet.Versioning package, validate AOT
2. Add PackageVersionInfo record
3. Update INuGetPackageService interface
4. Implement ParseVersion, CompareVersions, GetUpdateType
5. Update SearchAsync to return all versions
6. Implement GetLatestVersionsAsync
7. Add tests
8. Refactor RepoCheckVersionService
9. Add documentation
