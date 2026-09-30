# Fix check-version exact-tag Tools version and clean wiring

## Description

Parent **111** round-1 findings **M16–M20**. Repo services used by every TimeWarp `dev-cli`. 098 delete-safety for `FindDirectories` still holds — do not reopen it. These are different holes.

Pinned tree: `fbd5d276fc5a936136a55d981fc121a23b991493`. Evidence: parent `review/round-1/merged.md`.

## Requirements

- **M16 (bug)** `repo/RepoCheckVersionService.cs:44-56` and `:160-187` — git-tag check compares only to the latest tag. 076 (done) asked for exact `git tag -l "v{version}"`; the defect is still present. Tags `v1.0.0` + `v1.1.0` with source `1.0.0` reports `IsNewVersion=true`. Keep latest tag for display; set `IsNewVersion` from exact tag existence.
- **M17 (bug)** `nu-get/nuget-package-service.cs:256-262` used by `RepoCheckVersionService.cs:92-112` — unlisted versions are excluded from the same list used for already-published. Republish of an unlisted version 409s. Include unlisted for existence; listed-only for latest.
- **M18 (bug)** `RepoCheckVersionService.cs:129-157` vs `timewarp-amuru-tools.csproj:12` — version is always read from core `source/Directory.Build.props`. Tools has its own `<Version>1.0.0-beta.2</Version>`. Per-package version, or pass version in.
- **M19 (bug)** `tools/dev-cli/endpoints/clean-command.cs:27-59` — `dev clean` does not call `RepoCleanService`. It `dotnet clean`s only the core csproj and `Directory.Delete(bin, recursive: true)` without 098 reparse/tracked-file guards, and deletes preserved `dev`/`dev.exe`. Delegate artifact cleanup to `RepoCleanService`.
- **M20 (suggestion)** `RepoCleanService.cs:146-199` — root-bin children skip `HasTrackedFilesAsync`. Apply the tracked-file and reparse guards.

## Checklist

- [x] M16 exact git tag existence for IsNewVersion
- [x] M17 unlisted versions count as already published
- [x] M18 Tools package version is the one compared to the feed
- [x] M19 `dev clean` uses RepoCleanService (keep nupkg/feed-cache extra)
- [x] M20 root-bin children get tracked-file + reparse guards
- [x] `## Results` + `### How to validate`

## Session

- Implementer: Grok session (2026-09-30)

## Results

M16–M20 are fixed in the library services the review named, plus this repo's `dev clean`.

- **M16.** `CheckGitTagVersionAsync` sets `IsNewVersion` from `git tag -l v{version}`. `LatestReleaseTag` stays the versionsort-latest tag, or the caller-supplied tag, for display. A failed tag lookup does not report the version as new. Tags `v1.0.0` + `v1.1.0` with source `1.0.0` now report `IsNewVersion=false` and `LatestReleaseTag=v1.1.0`.
- **M17.** `SearchAsync` keeps unlisted registration entries and marks `NuGetPackageVersion.Listed=false`. `GetLatestVersionsAsync` and the check-version "latest" field stay listed-only. Already-published matches any returned version, listed or not.
- **M18.** `CheckNuGetVersionAsync` compares each package to a literal `<Version>` on the csproj that declares its `PackageId`, otherwise `source/Directory.Build.props`. Task 117 removed Tools' `<Version>1.0.0-beta.2</Version>` (lockstep), so `TimeWarp.Amuru.Tools` now resolves to the props version. A csproj override is no longer ignored. `dev check-version` itself still comes from `TimeWarp.Nuru.DevCli` and reads the props version once; under lockstep that is the Tools version.
- **M19.** The shared DevCli `clean-command.cs` is excluded. `tools/dev-cli/endpoints/clean-command.cs` calls `RepoCleanService.CleanAsync` (reparse, tracked-file, and `dev`/`dev.exe` guards) and `CleanLocalFeedAsync` for `artifacts/packages` (`TimeWarp.Amuru.*.nupkg`, `timewarp.amuru`, `timewarp.amuru.tools`). The old core-only `dotnet clean` and `Directory.Delete(bin)` are gone.
- **M20.** Root `bin` files and child directories use the same reparse and tracked-file skips as named `bin`/`obj` directories.

`IRepoCleanService` gained `CleanLocalFeedAsync`. Callers of `CleanAsync` are unchanged. `NuGetPackageVersion` gained optional `Listed` (default true).

### How to validate

**Automated**

```bash
rm -rf ~/.local/share/dotnet/runfile/repo-check-version-service-* \
       ~/.local/share/dotnet/runfile/repo-clean-service-* \
       ~/.local/share/dotnet/runfile/nuget-package-service-*
dotnet run --file tests/timewarp-amuru/single-file-tests/repo-services/repo-check-version-service.cs
dotnet run --file tests/timewarp-amuru/single-file-tests/repo-services/repo-clean-service.cs
dotnet run --file tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.cs
dotnet build source/timewarp-amuru-tools/timewarp-amuru-tools.csproj --nologo
dotnet build tools/dev-cli/dev.cs --nologo
```

**Expect**

- repo-check-version-service: 13 passed, including older exact tag `v1.0.0` with latest `v1.1.0` → `IsNewVersion=false`, unlisted feed version → already published, csproj `<Version>1.0.0-beta.2</Version>` compared instead of props `1.0.0`.
- repo-clean-service: 5 passed. Root `bin` keeps `dev`, `dev.exe`, tracked paths, and symlinks; deletes only untracked non-reparse children. Local feed deletes `TimeWarp.Amuru.*.nupkg` and `timewarp.amuru` but keeps tracked nupkgs and symlink nupkgs.
- nuget-package-service: 31 passed. `Newtonsoft.Json` `6.0.1-beta1` is returned with `Listed=false`; latest stable/prerelease stay listed.
- Both builds: 0 warnings, 0 errors.

**Not in scope:** `dev check-version` against the live nuget.org feed (DevCli still uses the lockstep props version). Do not run `dev clean` on a checkout whose `artifacts/packages` or root `bin` you need to keep.

## Notes

Parent: **111**. Source: `review/round-1/tools-services.md`. 098/076 are prior art; this task is the remaining holes, not a reopen of 098’s FindDirectories walk.
