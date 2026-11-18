# Add WithTimeout and missing options to the DotNet Build Run and Test builders

## Description

timewarp-jaribu task 077 moved its `Shell.Builder("dotnet")` calls off AMURU0001. Most of
them could not use the typed builder and went to the base `DotNet.Builder().WithArguments(…)`
instead, because the typed builders are missing options those calls need:

- `DotNetBuildBuilder`, `DotNetRunBuilder`, `DotNetTestBuilder`: no `WithTimeout`
  (`DotNetBuilder` has one). Jaribu's benchmark endpoints, test harnesses, and fixture
  builds all set a timeout.
- `DotNetBuildBuilder`: no `--artifacts-path` (Jaribu runfile coverage builds the
  runner into an owned artifacts directory).
- `DotNetTestBuilder`: no way to pass extra arguments, such as Microsoft.Testing.Platform
  extension options (`--coverage --coverage-output-format cobertura --coverage-output …`).
- `DotNetRunBuilder`: no `--no-cache` and no `--nologo`.

## Requirements

- Add `WithTimeout` (and `WithTimeoutGracePeriod`) to the Build, Run, and Test builders.
- Add `WithArtifactsPath` to the Build builder (and to Run/Test/Publish/Pack if they share it).
- Add a pass-through for extra arguments on the Test builder (MTP extension options).
- Ship them in the next 3.0.0 beta.

## Checklist

- [ ] WithTimeout on Build/Run/Test
- [ ] WithArtifactsPath on Build
- [ ] Extra-argument pass-through on Test
- [ ] Released

## Session

- Created: 1894826 (2026-11-18)

## Notes

Jaribu call sites that move to the typed builders once this lands:
`tools/dev-cli/endpoints/{ci-test-suite,benchmark-suite,benchmark-in-process,benchmark-measure}.cs`,
`source/timewarp-jaribu/runfile-coverage.static.cs`, and the mtp / runfile-coverage /
telemetry single-file tests.
