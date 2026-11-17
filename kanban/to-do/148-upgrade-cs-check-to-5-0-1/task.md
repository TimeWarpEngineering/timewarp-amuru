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

## Checklist

- [ ] Read the CsCheck 5.0.0 / 5.0.1 release notes for breaking changes
- [ ] Bump CsCheck to 5.0.1 in `Directory.Packages.props`
- [ ] Fix compile or behaviour breaks in `shell-builder.with-arguments.round-trip.cs`
- [ ] Confirm `CsCheck_Seed` pinning and the `CsCheckException` seed message still work
- [ ] `./bin/dev build` and `./bin/dev test` green
- [ ] `ganda nuget outdated` reports nothing outdated

## Session

- Created: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-17)

## Notes

- No package version bump and no release for this task.
