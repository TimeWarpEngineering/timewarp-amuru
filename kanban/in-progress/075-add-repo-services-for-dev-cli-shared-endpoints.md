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

- [ ] Create `IRepoCleanService` interface
- [ ] Create `RepoCleanService` implementation
  - Delete all `obj` directories recursively
  - Delete all `bin` directories recursively except root `./bin`
  - Selectively clean root `./bin` (preserve `dev` and `dev.exe`)
  - Handle locked files with warnings
- [ ] Create `IRepoCheckVersionService` interface
- [ ] Create `RepoCheckVersionService` implementation
  - Two strategies: `git-tag` and `nuget-search`
  - Read version from `source/Directory.Build.props`
  - `git-tag`: Compare against resolved git tag
  - `nuget-search`: Check NuGet for existing published versions
  - Read default strategy from repo config
- [ ] Move `IRepoConfigService` from ganda
- [ ] Move `RepoConfigService` from ganda
- [ ] Move `RepoConfig` models from ganda
- [ ] Register services in DI
- [ ] Add unit tests
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
