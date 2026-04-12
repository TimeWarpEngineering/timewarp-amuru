# Replace NuGet package lookup CLI calls with NuGet Protocol

**GitHub Issue:** #72

## Description

`NuGetPackageService` currently uses `dotnet package search` CLI calls to look up package versions. This needs to be replaced with direct NuGet Protocol API usage for correctness and performance.

## Problems with Current Implementation

1. **Prerelease packages reported as "not found"** - Missing `--prerelease` flag means packages like `TimeWarp.Amuru`, `TimeWarp.Jaribu`, and `TimeWarp.Multiavatar` are not found when only prerelease versions exist
2. **Slow lookups** - Each package spawns a new `dotnet` process, making `ganda nuget outdated` much slower than tools like `dotnet outdated`

## Checklist

- [ ] Add NuGet.Protocol and NuGet.Configuration package references
- [ ] Implement `SourceRepository` creation from configured NuGet sources
- [ ] Implement `PackageMetadataResource` caching per source
- [ ] Replace `dotnet package search` CLI call with `GetMetadataAsync()` API
- [ ] Ensure prerelease packages are correctly found
- [ ] Ensure authenticated feeds work correctly
- [ ] Maintain existing `INuGetPackageService` contract compatibility
- [ ] Add/update unit tests for the new implementation
- [ ] Verify performance improvement over CLI approach
- [ ] Update documentation if needed

## Reference Implementation Pattern

From `dotnet-outdated`:

```csharp
var sourceRepository = new SourceRepository(enabledSource, Repository.Provider.GetCoreV3());
var metadata = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
var versions = await metadata.GetMetadataAsync(packageId, includePrerelease, ...);
```

## Session

- Created: ses_27dd18c7effe1K4rnFRhnQezjn (2026-04-12)

## Notes

- Current implementation location: `NuGetPackageService`
- Current CLI command: `dotnet package search {packageId} --exact-match --format json`
- The new implementation should:
  - correctly find prerelease packages
  - be much faster (avoid process startup overhead)
  - work with configured NuGet sources and authenticated feeds
  - preserve the existing `INuGetPackageService` contract
- Repro observed: `ganda nuget outdated --update` shows packages as "(not found on NuGet)" even though they exist as prerelease

---

## Implementation Plan

### Files to Modify
- `Directory.Packages.props` - Add `NuGet.Protocol` 6.11.0 and `NuGet.Configuration` 6.11.0
- `source/timewarp-amuru/timewarp-amuru.csproj` - Add package references
- `source/timewarp-amuru/nu-get/NuGetPackageService.cs` - Major rewrite: replace CLI calls with `FindPackageByIdResource.GetAllVersionsAsync()`

### Files to Create
- `source/timewarp-amuru/nu-get/NuGetSourceCache.cs` - Cache `SourceRepository` instances per source URL

### Tests to Update
- `tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.cs` - Add prerelease package test, verify existing tests pass

### Key Decisions
1. Use `FindPackageByIdResource` instead of `PackageMetadataResource` - returns all versions including prerelease, simpler API
2. Match `NuGet.Protocol`/`NuGet.Configuration` version to existing `NuGet.Versioning` 6.11.0
3. Cache `SourceRepository` instances in `NuGetSourceCache` to avoid repeated initialization
4. Remove `ParseSearchResult` method entirely (CLI JSON parsing no longer needed)
5. `INuGetPackageService` contract remains unchanged - backward compatible
6. Aggregate versions from all enabled NuGet sources

### Implementation Steps
1. Add NuGet.Protocol and NuGet.Configuration to Directory.Packages.props
2. Add package references to timewarp-amuru.csproj
3. Create NuGetSourceCache.cs helper class
4. Rewrite NuGetPackageService.SearchAsync() using FindPackageByIdResource
5. Remove ParseSearchResult private method
6. Add NuGetVersionComparer helper
7. Update tests with prerelease package test case
8. Verify RepoCheckVersionService and CheckVersionCommand work unchanged
9. Run full test suite
10. Update documentation
