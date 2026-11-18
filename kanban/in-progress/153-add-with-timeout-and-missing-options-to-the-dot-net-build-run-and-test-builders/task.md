# Add WithTimeout and missing options to the DotNet Build Run and Test builders

## Goal (implementer restatement)

Jaribu's typed `dotnet` calls need options the Build, Run, and Test builders do not expose, so those calls fall back to a raw argument list. This task adds the missing options on the 3.0 prerelease line.

Acceptance criteria:

- `DotNetBuildBuilder`, `DotNetRunBuilder`, and `DotNetTestBuilder` each expose `WithTimeout` and `WithTimeoutGracePeriod`, and those limits reach command execution the same way they do on `DotNetBuilder`.
- `WithArtifactsPath` emits `--artifacts-path` on Build. Run, Test, Publish, and Pack get it too, because those verbs accept the same flag.
- `DotNetTestBuilder` can append extra tokens, including Microsoft.Testing.Platform options such as `--coverage`, without a typed method for each one.
- `DotNetRunBuilder` can emit `--no-cache` and `--nologo`, which the same Jaribu calls pass today.
- The new members are recorded in `PublicAPI.Unshipped.txt` so the next 3.0.0 beta can ship them. This walk does not publish the package.

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

- [x] WithTimeout on Build/Run/Test
- [x] WithArtifactsPath on Build
- [x] Extra-argument pass-through on Test
- [ ] Released

## Session

- Created: 1894826 (2026-11-18)
- Implementation: 01a1ed52-de76-7240-9b2c-3a2dd9f90697 (2026-11-18)

## Results

Build, Run, and Test take `WithTimeout` and `WithTimeoutGracePeriod`. Those methods set `CommandOptions`, so the limit is enforced at execution and does not appear in the command string. Build, Run, Test, Publish, and Pack take `WithArtifactsPath` and emit `--artifacts-path`. Test `WithArguments` appends tokens in call order, including Microsoft.Testing.Platform options such as `--coverage`, and does not insert `--`. Run takes `WithNoCache` and `WithNoLogo`. `--nologo` is not a `dotnet run` option on SDK 11.0.100; the builder emits it before program arguments, and `dotnet run` forwards that token to the program.

The new members are in `PublicAPI.Unshipped.txt`. The package version stays `3.0.0-beta.9`. This walk does not publish the package, so Released stays open. Jaribu's call sites stay on their current builders until a follow-up moves them.

The new methods pushed `dot-net.build.cs`, `dot-net.run.cs`, `dot-net.test.cs`, and `dot-net.publish.cs` over the TWA0038 line cap. Their option setters moved to `dot-net.build.options.cs`, `dot-net.run.options.cs`, `dot-net.test.options.cs`, and `dot-net.publish.options.cs`. Those four files are under 500 lines, so their entries left `.timewarp/line-cap-baseline.json` and `.editorconfig`.

`ganda repo audit` reported one advisory, `vscode-window-icon`, because `.vscode/settings.json` was missing. `ganda repo audit --fix` wrote that ignored file and set `window.title` to include `timewarp-amuru`. The audit inside that command then passed: 44 passed, 0 failed. The Windows taskbar icon was not applied; this machine has no Windows-side ganda.

### How to validate

Smoke: `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.test.cs`

Expect: 16 passed. The coverage case prints `dotnet test tests.csproj --configuration Release --results-directory artifacts/coverage --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml`. The slow-test case finishes in about 0.2s with `TimedOut` and exit code 124.

- `./bin/dev build` — succeeded, 0 warnings, 0 errors. The first attempt failed TWA0038 on the four builder files; this line is the rerun after the partial split.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build.cs` — exit 0, 13 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.run.cs` — exit 0, 13 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.test.cs` — exit 0, 16 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.publish.cs` — exit 0, 10 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.pack.cs` — exit 0, 10 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.timeout.cs` — exit 0, 2 passed.
- `ganda repo audit` — exit 0, Passed 43, Failed 1 advisory (`vscode-window-icon`).
- `ganda repo audit --fix` — wrote the ignored `.vscode/settings.json`; the audit it ran passed, Passed 44, Failed 0.

## Notes

Jaribu call sites that move to the typed builders once this lands:
`tools/dev-cli/endpoints/{ci-test-suite,benchmark-suite,benchmark-in-process,benchmark-measure}.cs`,
`source/timewarp-jaribu/runfile-coverage.static.cs`, and the mtp / runfile-coverage /
telemetry single-file tests.
