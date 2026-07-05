# Fix repo services correctness and delete safety

## Description

Two real bugs in `repo/` services, one of which can delete source-controlled files. Per the 094 decision record (2026-07-05): `repo/` and `nu-get/` stay PUBLIC and move to the `TimeWarp.Amuru.Tools` package (every TimeWarp repo's dev-cli consumes them; public repos can't depend on private Zana). Fix the bugs here — they ride along in the 094-003 move.

## Checklist

- [x] `RepoCleanService` hardened (2026-07-05): enumeration is a manual walk that never follows reparse points, and any candidate directory containing git-TRACKED files (`git ls-files`) is skipped with a warning — fail-safe skips when git itself errors
- [x] `CheckNuGetVersionAsync` now matches the current version against ALL feed versions, not just the latest (2026-07-05)
- [x] `GetLatestGitTagAsync`: throw-on-nonzero was fixed globally by 090 (default validation is now None, so the ExitCode check works); added `-c versionsort.suffix=-` so pre-release tags sort before their release (2026-07-05)
- [x] `NuGetPackageService` now skips versions with `catalogEntry.listed == false`; design region corrected (2026-07-05)
- [ ] Tests for each fix (delete-safety test with a tracked `bin` file; version-contains test; unlisted-version test) — MOVED to task 105 (test-coverage sweep); existing repo-services suite passes against the new behavior

## Notes

Found by multi-agent release review (2026-07-04). The version-check bug directly affects the release pipeline this board is driving toward. Paths relative to `source/timewarp-amuru/`.
