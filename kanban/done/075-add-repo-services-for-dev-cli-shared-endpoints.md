# Add repo services for dev-cli shared endpoints

**GitHub Issue:** [#52](https://github.com/TimeWarpEngineering/timewarp-amuru/issues/52)

## Description

Add services needed by shared dev-cli endpoints:
- `IRepoCleanService` / `RepoCleanService` — bin/obj directory cleaning
- `IRepoCheckVersionService` / `RepoCheckVersionService` — version checking logic
- `IRepoConfigService` / `RepoConfigService` — per-repo configuration (moved from ganda)

## Background

We're refactoring dev-cli to use shared endpoints from TimeWarp.Nuru. The shared endpoints need services that must live in a public repo (Amuru) to avoid private build-time dependencies.

## Checklist

- [x] Create `IRepoCleanService` interface
- [x] Create `RepoCleanService` implementation
  - Delete all `obj` directories recursively
  - Delete all `bin` directories recursively except root `./bin`
  - Selectively clean root `./bin` (preserve `dev` and `dev.exe`)
  - Handle locked files with warnings
- [x] Create `IRepoCheckVersionService` interface
- [x] Create `RepoCheckVersionService` implementation
  - Two strategies: `git-tag` and `nuget-search`
  - Read version from `source/Directory.Build.props`
  - `git-tag`: Compare against resolved git tag
  - `nuget-search`: Check NuGet for existing published versions
  - Read default strategy from repo config
- [x] Move `IRepoConfigService` from ganda
- [x] Move `RepoConfigService` from ganda
- [x] Move `RepoConfig` models from ganda
- [x] Register services in DI
- [x] Add unit tests
- [ ] Publish new version of TimeWarp.Amuru

## Notes

### Service Interfaces

**IRepoCleanService:**
```csharp
public interface IRepoCleanService
{
  Task<CleanResult> CleanAsync(string repoPath, CancellationToken cancellationToken = default);
}

public record CleanResult(int ObjDeleted, int BinDeleted, int FilesCleaned);
```

**IRepoCheckVersionService:**
```csharp
public interface IRepoCheckVersionService
{
  Task<CheckVersionResult> CheckAsync(string repoPath, string? strategy = null, string? package = null, string? tag = null, CancellationToken cancellationToken = default);
}
```

**IRepoConfigService:**
```csharp
public interface IRepoConfigService
{
  Task<RepoConfig> GetConfigAsync(CancellationToken cancellationToken = default);
  Task SetConfigAsync(RepoConfig config, CancellationToken cancellationToken = default);
}

public record RepoConfig
{
  public RepoCheckVersionConfig? CheckVersion { get; set; }
}

public record RepoCheckVersionConfig
{
  public string? Strategy { get; set; }
  public string? Packages { get; set; }
}
```

### Reference Implementations

- Clean logic: ganda task #116 (`tools/dev-cli/endpoints/clean-command.cs`)
- Check version logic: ganda `source/timewarp-ganda/endpoints/repo/repo-check-version-command.cs`
- Config service: ganda `source/timewarp-ganda/services/repo-config-service.cs`

### Related

- ganda task #117: Refactor dev-cli shared endpoints to TimeWarp.Nuru
- TimeWarp.Nuru issue: Add shared dev-cli endpoints

## Implementation Plan

### Key Decisions

1. **repoRoot**: Services call `Git.FindRoot()` internally - no parameter needed
2. **INuGetPackageService**: Move interface and implementation to Amuru
3. **DI Registration**: No helper in Amuru - consumers register services directly

### File Structure

```
source/TimeWarp.Amuru/
├── Repo/
│   ├── IRepoCleanService.cs           (NEW)
│   ├── RepoCleanService.cs            (NEW)
│   ├── IRepoCheckVersionService.cs    (NEW)
│   ├── RepoCheckVersionService.cs     (NEW)
│   ├── IRepoConfigService.cs          (NEW)
│   ├── RepoConfigService.cs           (NEW)
│   ├── RepoConfig.cs                  (NEW - models)
│   └── RepoConfigJsonContext.cs       (NEW - source-generated JSON)
├── NuGet/
│   ├── INuGetPackageService.cs        (NEW - moved from ganda)
│   ├── NuGetPackageService.cs         (NEW - moved from ganda)
│   ├── NuGetModels.cs                 (NEW)
│   └── NuGetJsonContext.cs            (NEW - source-generated JSON)
├── GlobalUsings.cs                     (UPDATE)
└── TimeWarp.Amuru.csproj              (UPDATE)
```

### Implementation Steps

1. Update project dependencies (add TimeWarp.Terminal)
2. Update GlobalUsings.cs (add System.Xml.Linq, TimeWarp.Terminal)
3. Create NuGet service (moved from ganda)
4. Create IRepoCleanService interface
5. Create RepoCleanService implementation
6. Create IRepoConfigService interface
7. Create RepoConfigService implementation
8. Create RepoConfig models
9. Create RepoConfigJsonContext
10. Create IRepoCheckVersionService interface
11. Create RepoCheckVersionService implementation
12. Add tests in `tests/timewarp-amuru/single-file-tests/repo-services/`

### Dependencies

| Service | Dependencies |
|---------|--------------|
| `IRepoCleanService` | `ITerminal`, `Git` (static) |
| `IRepoConfigService` | `Git` (static), `File` I/O |
| `INuGetPackageService` | `Shell` (static) |
| `IRepoCheckVersionService` | `ITerminal`, `INuGetPackageService`, `IRepoConfigService`, `Git` (static) |

### Consumer DI Registration

```csharp
services.AddSingleton<ITerminal>(TimeWarpTerminal.Default);
services.AddSingleton<INuGetPackageService, NuGetPackageService>();
services.AddSingleton<IRepoConfigService, RepoConfigService>();
services.AddSingleton<IRepoCleanService, RepoCleanService>();
services.AddSingleton<IRepoCheckVersionService, RepoCheckVersionService>();
```

### Open Questions

1. Should we preserve `ganda.jsonc` config file name? → Keep for backward compatibility
2. Should `RepoCheckVersionService` throw exceptions or return error results? → Return result with error info

## Results

### What was implemented

Added four services to TimeWarp.Amuru for dev-cli shared endpoints:

1. **IRepoCleanService** - Cleans bin/obj directories
   - Deletes all `obj` directories recursively
   - Deletes all `bin` directories recursively except root `./bin`
   - Selectively cleans root `./bin` (preserves `dev`, `dev.exe`)
   - Handles locked files with warnings

2. **IRepoCheckVersionService** - Version checking with two strategies
   - `git-tag`: Compare against resolved git tag
   - `nuget-search`: Check NuGet for existing published versions
   - Reads version from `source/Directory.Build.props`

3. **IRepoConfigService** - Per-repo configuration
   - Config path: `.timewarp/ganda.jsonc`
   - AOT-compatible JSON serialization via source generators

4. **INuGetPackageService** - NuGet package operations
   - Uses `dotnet package search` CLI command
   - Returns latest version for a package

### Files Changed

**New Files (source/TimeWarp.Amuru/):**
- `NuGet/NuGetModels.cs`
- `NuGet/INuGetPackageService.cs`
- `NuGet/NuGetPackageService.cs`
- `Repo/IRepoCleanService.cs`
- `Repo/RepoCleanService.cs`
- `Repo/IRepoConfigService.cs`
- `Repo/RepoConfigService.cs`
- `Repo/RepoConfig.cs`
- `Repo/RepoConfigJsonContext.cs`
- `Repo/IRepoCheckVersionService.cs`
- `Repo/RepoCheckVersionService.cs`

**New Files (tests/):**
- `single-file-tests/repo-services/nuget-package-service.cs`
- `single-file-tests/repo-services/repo-config-service.cs`
- `single-file-tests/repo-services/repo-clean-service.cs`
- `single-file-tests/repo-services/repo-check-version-service.cs`

**Updated Files:**
- `Directory.Packages.props` - Added TimeWarp.Terminal
- `source/TimeWarp.Amuru/TimeWarp.Amuru.csproj` - Added TimeWarp.Terminal
- `source/TimeWarp.Amuru/GlobalUsings.cs` - Added System.Xml.Linq, TimeWarp.Terminal
- `tests/timewarp-amuru/Directory.Build.props` - Added TimeWarp.Terminal

### Key Decisions

1. Services call `Git.FindRoot()` internally - no repoRoot parameter needed
2. INuGetPackageService moved to Amuru (from ganda)
3. No DI helper in Amuru - consumers register services directly
4. Config path uses `.timewarp/ganda.jsonc` for backward compatibility
5. AOT-compatible JSON serialization via source-generated contexts

### Test Outcomes

All 13 tests passed:
- repo-config-service.cs: 3/3 ✓
- nuget-package-service.cs: 4/4 ✓
- repo-clean-service.cs: 3/3 ✓
- repo-check-version-service.cs: 3/3 ✓
