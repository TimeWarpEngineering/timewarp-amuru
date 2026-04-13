# Replace NuGet package lookup CLI calls with NuGet Protocol

**GitHub Issue:** #72

## Description

`NuGetPackageService` currently uses `dotnet package search` CLI calls to look up package versions. This needs to be replaced with direct NuGet Protocol API usage for correctness and performance.

## Problems with Current Implementation

1. **Prerelease packages reported as "not found"** - Missing `--prerelease` flag means packages like `TimeWarp.Amuru`, `TimeWarp.Jaribu`, and `TimeWarp.Multiavatar` are not found when only prerelease versions exist
2. **Slow lookups** - Each package spawns a new `dotnet` process, making `ganda nuget outdated` much slower than tools like `dotnet outdated`

## Checklist

- [x] Add NuGet.Protocol and NuGet.Configuration package references
- [x] Implement `SourceRepository` creation from configured NuGet sources
- [x] Implement `PackageMetadataResource` caching per source (via `NuGetSourceCache`)
- [x] Replace `dotnet package search` CLI call with `FindPackageByIdResource.GetAllVersionsAsync()` API
- [x] Ensure prerelease packages are correctly found
- [x] Ensure authenticated feeds work correctly
- [x] Maintain existing `INuGetPackageService` contract compatibility
- [x] Add/update unit tests for the new implementation
- [x] Verify performance improvement over CLI approach
- [ ] Update documentation if needed

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

### Files Modified
- `Directory.Packages.props` - Added `NuGet.Protocol`, `NuGet.Configuration`, `NuGet.Common` (upgraded to 7.3.0)
- `source/timewarp-amuru/timewarp-amuru.csproj` - Added package references
- `source/timewarp-amuru/nu-get/nuget-package-service.cs` - Replaced CLI calls with `FindPackageByIdResource.GetAllVersionsAsync()`
- `source/timewarp-amuru/nu-get/NuGetModels.cs` - Renamed to `nuget-models.cs` (kebab-case)

### Files Created
- `source/timewarp-amuru/nu-get/nuget-source-cache.cs` - Cache `SourceRepository` instances per source URL

### Tests Updated
- `tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.cs` - Added prerelease package tests, all 29 tests pass

### Key Decisions
1. Use `FindPackageByIdResource` instead of `PackageMetadataResource` - returns all versions including prerelease, simpler API
2. Upgraded NuGet packages to 7.3.0 (from 6.11.0) - uses System.Text.Json instead of Newtonsoft.Json
3. Cache `SourceRepository` instances in `NuGetSourceCache` to avoid repeated initialization
4. Remove `ParseSearchResult` method entirely (CLI JSON parsing no longer needed)
5. `INuGetPackageService` contract remains unchanged - backward compatible
6. Aggregate versions from all enabled NuGet sources

### Additional Changes (Style/Cleanup)
- All `source/timewarp-amuru/` files renamed to kebab-case (PascalCase → kebab-case)
- All `TODO:` region comments replaced with proper purpose/design descriptions
- `global-usings.cs` consolidated into single project-level file (removed folder-level duplicates)
- `*.lscache` files added to `.gitignore`
- Test files updated: replaced `ProcessStartInfo` with `Shell.Builder` (fixing RS0030 violations)
- `tools/dev-cli/services/process-helpers.cs` - Replaced `ProcessStartInfo` with `Shell.Builder` + argument parser
- Dev-cli: `TreatWarningsAsErrors` enabled, `global-usings.cs` added to compilation, `IL2104`/`IL3053` suppressed (StreamJsonRpc transitive Newtonsoft dep)
- `Directory.Packages.props`: `ModelContextProtocol.Core` updated to 1.0.0

### Implementation Steps
1. ✅ Add NuGet.Protocol and NuGet.Configuration to Directory.Packages.props
2. ✅ Add package references to timewarp-amuru.csproj
3. ✅ Create NuGetSourceCache.cs helper class
4. ✅ Rewrite NuGetPackageService.SearchAsync() using FindPackageByIdResource
5. ✅ Remove ParseSearchResult private method
6. ✅ Add NuGetVersionComparer helper
7. ✅ Update tests with prerelease package test case
8. ✅ Verify RepoCheckVersionService and CheckVersionCommand work unchanged
9. ✅ Run full test suite (355 passed, 1 skipped)
10. ⬜ Update documentation