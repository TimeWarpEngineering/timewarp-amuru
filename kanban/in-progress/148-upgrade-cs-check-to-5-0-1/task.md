# Upgrade CsCheck to 5.0.1

## Description

`ganda nuget outdated` reports one outdated package: CsCheck 4.9.1 -> 5.0.1 (major).
CsCheck is dev-only. Its single consumer is the property test
`tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs`.

Bump the pin in `Directory.Packages.props` and fix whatever the 5.x major breaks in that test.

## Requirements

- `Directory.Packages.props` pins CsCheck 5.0.1.
- The round-trip test compiles and passes on 5.0.1 and keeps its coverage: same generators,
  same iteration counts, no cases removed to make it pass.
- The pinned-seed behaviour still works. The test reads `CsCheck_Seed` (CI pins it in the
  workflow) and its failure message catches `CsCheckException` to report the seed. If 5.x
  renamed the environment variable, the exception type, or the seed format, update the test,
  its comments, and the CI workflow together.
- No release. Do not bump the package version or cut a tag: this is a test-only dependency
  and ships nothing to consumers (Steve 2026-11-17, "no need for a release").

## Goal (implementer restatement)

Move the dev-only CsCheck pin from 4.9.1 to 5.0.1 so `ganda nuget outdated` is clean, and keep
the one property test that uses it (`shell-builder.with-arguments.round-trip.cs`) working with
the same generators, iteration counts and cases.

Acceptance:
- `Directory.Packages.props` pins CsCheck 5.0.1.
- The round-trip test compiles and passes on 5.0.1 with unchanged coverage.
- The CI-pinned `CsCheck_Seed` still parses and is honoured, and the catch on `CsCheckException`
  still reports the seed. Any rename in 5.x is carried into the test, its comments and the workflow.
- No package version bump, no tag, no release.

## Checklist

- [x] Read the CsCheck 5.0.0 / 5.0.1 release notes for breaking changes
- [x] Bump CsCheck to 5.0.1 in `Directory.Packages.props`
- [x] Fix compile or behaviour breaks in `shell-builder.with-arguments.round-trip.cs`
- [x] Confirm `CsCheck_Seed` pinning and the `CsCheckException` seed message still work
- [x] `./bin/dev build` and `./bin/dev test` green
- [x] `ganda nuget outdated` reports nothing outdated

## Session

- Created: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-17)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session b6be8b1d-bef4-40f2-9b80-6b5757925221, max-turns 500, Done — 2026-11-17T10:27:07Z

## Results

- `Directory.Packages.props`: CsCheck 4.9.1 -> 5.0.1. No other file needed a change.
- CsCheck 5.0.0 breaking changes (from the package `PackageReleaseNotes`) checked against the test:
  net8 dropped, `Gen.UIntN` removed, `OneOfConst`/`FrequencyConst` folded, `Array`/`List` became
  extension properties, invalid `CsCheck_` env vars now throw, `CsCheckException.Message` no
  longer includes the inner exception's message. None affect the test: `Gen.Frequency`,
  `Gen.Char[...]`, `Gen.Int[...]`, `.Array[a, b]` and `SampleAsync(writeLine, threads, print)`
  compile unchanged. 5.0.1 only fixes WebAssembly type initializers and parallel ConformExhaustive.
- Seed: the `CsCheck_Seed` name and 12-char seed format are unchanged. The CI-pinned
  `012XdpNskFd2` still parses (5.x throws on an invalid `CsCheck_` value; it did not). A scratch
  probe on 5.0.1 showed a failing `Sample` throws `CsCheckException` with
  `Set seed: "..." or -e CsCheck_Seed=... to reproduce`, so the test's catch still reports the
  seed. The test, its comments and `.github/workflows/workflow.yml` stay as they are.
- Coverage unchanged: same generators, default iteration count (100), all cases kept.
- No version bump, no tag, no release.

### Gates

- `./bin/dev build`: succeeded, 0 warnings, 0 errors.
- `./bin/dev test`: All tests passed. 879 total, 878 passed, 1 skipped (the existing
  `GetCommitsAheadOfDefaultBranch` "Requires full git history" skip). `WithArgumentsRoundTrip_Given_` 4/4.
- `ganda repo audit`: passed with 1 advisory warning, `vscode-window-icon` (`.vscode/settings.json`
  missing in this worktree). `ganda repo audit --fix` created it; the file is gitignored, so no
  commit change. Rerun: "Repository passes all audit checks."
- `ganda nuget outdated`: "All packages are up to date!"

### How to validate

Smoke: `CsCheck_Seed=012XdpNskFd2 dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs`, then `ganda nuget outdated`.
Expect: 4/4 pass, including "Random Arguments_ Should_ Reach Child Unchanged" with "Passed 100 iterations."; the runfile's `obj/project.assets.json` resolves `CsCheck/5.0.1`; `ganda nuget outdated` prints "All packages are up to date!".

## Notes

- No package version bump and no release for this task.
