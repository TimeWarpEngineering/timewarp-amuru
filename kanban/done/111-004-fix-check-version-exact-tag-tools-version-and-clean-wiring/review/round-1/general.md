# Round 1 — general
**Date:** 2026-09-30
**Scope reviewed:** `task/111-004-…` vs `origin/feature/overnight-amuru` (`452dd14`). M16–M20 in `RepoCheckVersionService`, `NuGetPackageService`, `RepoCleanService`, and `tools/dev-cli` clean.

## Summary

M16–M18 and M20 match the brief. `IsNewVersion` comes from `git tag -l v{version}` while the latest tag stays display-only; unlisted registration entries stay in `SearchAsync` and are excluded from latest; a literal csproj `<Version>` overrides `Directory.Build.props`; root-bin children go through the reparse and tracked-file guards. `dev clean` calls `RepoCleanService` and still clears the local feed. The new feed walk breaks the cleaner's "never follow reparse points" rule and can delete files outside the repo.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru-tools/repo/RepoCleanService.cs:71
- Description: `CleanLocalFeedAsync` enumerates with `SearchOption.AllDirectories`. That API follows directory symlinks. `TryDeletePathAsync` only refuses to delete the reparse point itself. A directory link under `artifacts/packages` to a folder outside the repo is walked. `git ls-files` on the path through the link exits 0 with empty output, so the tracked-file guard treats the target as untracked. `File.Delete` / `Directory.Delete` then remove the outside `TimeWarp.Amuru.*.nupkg` or `timewarp.amuru` directory. Reproduced: a symlink to an outside directory, `git ls-files` empty, deleting the enumerated path removed the outside nupkg. This contradicts the service Design rule that enumeration never follows reparse points (098).
- Suggestion: Enumerate only the current directory. Do not descend into reparse points. If `artifacts/packages` itself is a reparse point, skip it. Keep the existing per-path tracked-file and reparse delete guards.
- Status: open
