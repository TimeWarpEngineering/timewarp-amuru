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
