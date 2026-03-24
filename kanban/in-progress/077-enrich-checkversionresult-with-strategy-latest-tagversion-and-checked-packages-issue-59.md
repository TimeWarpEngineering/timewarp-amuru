# Enrich CheckVersionResult with strategy, latest tag/version, and checked packages (Issue #59)

**Priority:** High
**GitHub Issue:** https://github.com/TimeWarpEngineering/timewarp-amuru/issues/59
**Related:** Issue #57

## Description

The `CheckVersionResult` record needs additional fields so consuming CLIs can display rich, descriptive output per strategy. The service currently writes directly to the terminal during checks — display should be the consumer's responsibility.

## Checklist

### Data Model
- [ ] Add `Strategy` field (string) to `CheckVersionResult`
- [ ] Add `LatestReleaseTag` field (string?) to `CheckVersionResult`
- [ ] Add `LatestNuGetVersion` field (string?) to `CheckVersionResult`
- [ ] Add `CheckedPackages` field (List<string>?) to `CheckVersionResult`

### Strategy Updates
- [ ] Update git-tag strategy to find and return the latest release tag
- [ ] Update nuget-search strategy to return latest NuGet version and checked packages list

### Separation of Concerns
- [ ] Remove terminal writes from the version check service — return data only
- [ ] Ensure consumers are responsible for display/formatting

### Verification
- [ ] Verify backward compatibility with existing consumers
- [ ] Add/update integration tests for enriched result

## Notes

- This task enriches the return type so consuming CLIs (e.g., dev-cli version check endpoints) can render strategy-specific output without the service dictating display format.
- The git-tag strategy should populate `Strategy` and `LatestReleaseTag`.
- The nuget-search strategy should populate `Strategy`, `LatestNuGetVersion`, and `CheckedPackages`.
- Removing terminal writes from the service is a separation-of-concerns improvement — the service layer should be pure data, with the CLI layer handling presentation.
- Related to issue #57 which also involves version check improvements.

## Implementation Plan

### Files to Change (in order)

1. **`source/timewarp-amuru/repo/IRepoCheckVersionService.cs`** — Update `CheckVersionResult` record: add `Strategy`, `LatestReleaseTag`, `LatestNuGetVersion`, `CheckedPackages` fields; remove `ResolvedTag`
2. **`source/timewarp-amuru/repo/RepoCheckVersionService.cs`** — Remove `ITerminal` dependency, remove all terminal writes, add `GetLatestGitTagAsync` helper, populate new fields in both strategies
3. **`tools/dev-cli/endpoints/check-version-command.cs`** — Replace duplicated logic with service call + rich display formatting
4. **`tests/timewarp-amuru/single-file-tests/repo-services/repo-check-version-service.cs`** — Update constructor calls, assertions, add mocks for new git command

### Key Design Decisions
- `LatestReleaseTag` replaces `ResolvedTag` with different semantics: returns globally latest tag, not the matched tag
- Use `git tag --sort=-v:refname` to find latest release tag
- Strategy is non-nullable string; error paths use `string.Empty`
- Service becomes pure data — no terminal dependency
- CLI endpoint handles all display formatting
