# Add WithTimeout and missing options to the DotNet Build Run and Test builders

## Goal (implementer restatement)

Jaribu's typed `dotnet` calls fall back to a raw argument list when Build, Run, or Test cannot express the options those calls pass. This task adds the missing options on the 3.0 prerelease line. The fix pass makes the headline build shape (`--artifacts-path`, `--nologo`, `--verbosity`) a documented typed call.

Acceptance criteria:

- Build, Run, and Test expose `WithTimeout` and `WithTimeoutGracePeriod`, and those limits reach command execution the same way they do on `DotNetBuilder`.
- Build, Run, Test, Publish, and Pack expose `WithArtifactsPath` and emit `--artifacts-path`.
- Test can append extra tokens, including Microsoft.Testing.Platform options such as `--coverage`, without a typed method for each one.
- Run can emit `--no-cache` and `--nologo`.
- Build emits `--nologo` through `WithNoLogo`, so `dotnet build <entry> --artifacts-path <dir> --nologo --verbosity minimal` is a typed call. That member is already in the shipped public API. The reference documents it, and a command-string test locks the flag set.
- Members added on this branch are listed in `PublicAPI.Unshipped.txt`. This walk does not publish the package.

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
- [x] Build `WithNoLogo` documented for the runfile-coverage shape (member already shipped)
- [ ] Released

## Session

- Created: 1894826 (2026-11-18)
- Implementation: 01a1ed52-de76-7240-9b2c-3a2dd9f90697 (2026-11-18)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1ed52-de76-7240-9b2c-3a2dd9f90697, max-turns 500, Done — 2026-11-18T06:52:32Z
- Review round 1: review oracle (claude, model claude-opus-5-5), effort 3, roster general, turn cap 200 — 2026-11-18. 1 open suggestion (M1); fix pass requested.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session a9cedddd-d43c-4462-8a7f-7306a5f8ddc3, max-turns 200, artifacts general.md, merged.md — 2026-11-18T06:54:39Z
- Fix pass: implement-fix-2 (grok-4.7) — 2026-11-18. M1's `WithNoLogo` was already on `DotNetBuildBuilder`. Documented it and locked the runfile-coverage command string. Bumped the Nuru pins so `package-pin-currency` passes.

## Results

Build, Run, and Test take `WithTimeout` and `WithTimeoutGracePeriod`. Those methods set `CommandOptions`, so the limit is enforced at execution and does not appear in the command string. Build, Run, Test, Publish, and Pack take `WithArtifactsPath` and emit `--artifacts-path`. Test `WithArguments` appends tokens in call order, including Microsoft.Testing.Platform options such as `--coverage`, and does not insert `--`. Run takes `WithNoCache` and `WithNoLogo`. `--nologo` is not a `dotnet run` option on SDK 11.0.100; the builder emits it before program arguments, and `dotnet run` forwards that token to the program.

The new members are in `PublicAPI.Unshipped.txt`. The package version stays `3.0.0-beta.9`. This walk does not publish the package, so Released stays open. Jaribu's call sites stay on their current builders until a follow-up moves them.

Fix pass for review M1: `DotNetBuildBuilder.WithNoLogo()` was already shipped and already emits `--nologo` (`PublicAPI.Shipped.txt`). A second `PublicAPI.Unshipped.txt` entry would duplicate that listing, so none was added. `dot-net.md` now names `WithNoLogo` on `DotNet.Build()`. The builder emits `--verbosity` before `--nologo`, so the runfile-coverage shape is `dotnet build entry.cs --artifacts-path artifacts/owned --verbosity minimal --nologo`. `Directory.Packages.props` pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at `4.0.0-beta.16` because `package-pin-currency` failed on `4.0.0-beta.15`.

The new methods pushed `dot-net.build.cs`, `dot-net.run.cs`, `dot-net.test.cs`, and `dot-net.publish.cs` over the TWA0038 line cap. Their option setters moved to `dot-net.build.options.cs`, `dot-net.run.options.cs`, `dot-net.test.options.cs`, and `dot-net.publish.options.cs`. Those four files are under 500 lines, so their entries left `.timewarp/line-cap-baseline.json` and `.editorconfig`.

`ganda repo audit` reported one advisory, `vscode-window-icon`, because `.vscode/settings.json` was missing. `ganda repo audit --fix` wrote that ignored file and set `window.title` to include `timewarp-amuru`. The audit inside that command then passed: 44 passed, 0 failed. The Windows taskbar icon was not applied; this machine has no Windows-side ganda.

The commit hook's `ganda repo attest` then refused to sign: the `.githooks` runfiles take `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` from `Directory.Packages.props`, and those pins were `3.0.0-beta.5` while nuget has `3.0.0-beta.9`. Both pins are now `3.0.0-beta.9`. The package version of this repo stays `3.0.0-beta.9`.

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
- `ganda repo audit` — rerun after the `.githooks` pin bump to `3.0.0-beta.9`: exit 0, Passed 44, Failed 0.

### How to validate (fix pass)

Smoke: `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build.cs`

Expect: 14 passed. The runfile-coverage case prints `dotnet build entry.cs --artifacts-path artifacts/owned --verbosity minimal --nologo`.

- `./bin/dev build` — succeeded, 0 warnings, 0 errors.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build.cs` — exit 0, 14 passed. Standalone runfile (`#if !JARIBU_MULTI`).
- `ganda repo audit` — first run exit 1, `package-pin-currency`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` `4.0.0-beta.15` behind `4.0.0-beta.16`.
- `dotnet run --file tools/dev-cli/dev.cs -- --help` — compiled against the bumped pins and printed help.
- `ganda repo audit` — rerun after the Nuru pin bump: exit 0, Passed 44, Failed 0.

### Review

- Round 1 (effort 3, roster general): 0 bug, 1 suggestion, 0 nit. M1 asked for `DotNetBuildBuilder.WithNoLogo()`. That method is already on the shipped builder. The fix pass documents it and adds the runfile-coverage command-string test. Round 2 dispositions M1.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Review

- Round 1: fix pass applied for M1; round 2 pending

## Notes

Jaribu call sites that move to the typed builders once this lands:
`tools/dev-cli/endpoints/{ci-test-suite,benchmark-suite,benchmark-in-process,benchmark-measure}.cs`,
`source/timewarp-jaribu/runfile-coverage.static.cs`, and the mtp / runfile-coverage /
telemetry single-file tests.
