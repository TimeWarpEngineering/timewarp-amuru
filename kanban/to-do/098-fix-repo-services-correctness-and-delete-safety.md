# Fix repo services correctness and delete safety

## Description

Two real bugs in `repo/` services, one of which can delete source-controlled files. Per the 094 decision record (2026-07-05): `repo/` and `nu-get/` stay PUBLIC and move to the `TimeWarp.Amuru.Tools` package (every TimeWarp repo's dev-cli consumes them; public repos can't depend on private Zana). Fix the bugs here — they ride along in the 094-003 move.

## Checklist

- [ ] `repo/RepoCleanService.cs:57,96` — recursively deletes **every** directory named `obj`/`bin` under the repo root with no tracked-file guard and no dry-run: a repo with tracked `tools/bin/*.sh` (or an assets dir named `obj`) loses source-controlled files; `Directory.GetDirectories(..., AllDirectories)` also follows directory symlinks, so a symlink inside the repo can cause deletion of `bin`/`obj` dirs OUTSIDE the repo. Add a tracked-file guard (prefer `git clean -Xdf`-style logic), skip reparse points, and consider a dry-run option
- [ ] `repo/RepoCheckVersionService.cs:107-114` — `CheckNuGetVersionAsync` marks a package "already published" only if current version equals the **latest** feed version (`Versions[0]`): props at 1.2.0 with feed at 1.2.0+1.3.0 → reports `IsNewVersion=true`, release pipeline proceeds and `nuget push` 409s. Use `Versions.Contains(current)`
- [ ] `repo/RepoCheckVersionService.cs:160-167` — `GetLatestGitTagAsync` runs without `WithNoValidation`, so non-zero git exit throws before the `ExitCode != 0` check (dead error handling; throws outside a repo instead of returning null). Also `git tag --sort=-v:refname` without `versionsort.suffix` ranks `v1.0.0-beta.34` above `v1.0.0`
- [ ] `nu-get/nuget-package-service.cs:241-262` — Design-region claim "Registration metadata excludes unlisted packages" is wrong: nuget.org registration blobs include unlisted versions (flagged via `catalogEntry.listed`, never read), so `GetLatestVersionsAsync` can report a delisted version as latest. Filter on `listed`
- [ ] Tests for each fix (delete-safety test with a tracked `bin` file; version-contains test; unlisted-version test)

## Notes

Found by multi-agent release review (2026-07-04). The version-check bug directly affects the release pipeline this board is driving toward. Paths relative to `source/timewarp-amuru/`.
