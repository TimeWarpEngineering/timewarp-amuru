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
