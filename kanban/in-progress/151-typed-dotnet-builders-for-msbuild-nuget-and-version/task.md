# Typed dotnet builders for msbuild, nuget, and version

## Description

Task 150 adds an analyzer that reports `Shell.Builder("dotnet")`. Its warning case
(`AMURU0002`) is a dotnet verb with no typed builder. Scanning the org's master worktrees on
2026-11-18 found these verbs used through a raw shell:

| Verb | Call sites | Example use | Typed builder today |
|------|-----------:|-------------|---------------------|
| msbuild | 5 | `dotnet msbuild <proj> -t:Target -p:Name=Value -getProperty:X` | none |
| nuget | 3 | `dotnet nuget push`, `dotnet nuget verify`, `dotnet nuget locals` | `DotNet.NuGet()` has `Push` and `Locals`; no `Verify` |
| --version / --info | 1 | SDK version probe | `DotNet.WithVersion()` / `DotNet.WithInfo()` |

Groom on 2026-11-18 checked `source/timewarp-amuru-tools/dot-net-commands`: `dot-net.nu-get.cs`
already ships `DotNet.NuGet()` with `Push`, `Delete`, `Locals`, `Why`, and the source verbs, and
`dot-net.base.cs` ships `WithVersion()` and `WithInfo()`. The real gaps are `msbuild` and
`nuget verify`. This task adds those two.

## Requirements

- `DotNet.MsBuild()`: project or solution path, `-t:` targets (repeatable), `-p:` properties
  (repeatable), `-getProperty:` / `-getItem:` / `-getTargetResult:`, verbosity, `-restore`,
  `-nologo`, binary log path, working directory, environment variables. Same fluent shape and
  `Options` plumbing as `dot-net.build.cs`, so build telemetry tags and `TRACEPARENT`
  propagation (task 149) apply. New file `dot-net.ms-build.cs`.
- `DotNet.NuGet().Verify(packagePath)` in `dot-net.nu-get.cs`, following the existing
  sub-builders there: certificate fingerprint (repeatable), `--all`, verbosity, config file.
- The existing `DotNet.NuGet().Push()` api key (and symbol api key) never goes into telemetry
  tags or logs. Add the test; fix it if the test finds a leak.
- Do not add `DotNet.Version()` / `DotNet.Info()`. `WithVersion()` / `WithInfo()` already cover
  the probe and a second spelling would only split the call sites.
- Each builder follows the existing conventions in `source/timewarp-amuru-tools/dot-net-commands`
  (`WithEnvironmentVariable`, `WithWorkingDirectory`, `WithNoValidation`,
  `CaptureAsync` / `RunAsync`).
- Public API files updated (`source/timewarp-amuru-tools/public-api`).
- `dot-net.md` lists the new builders.
- Tests: Jaribu runfiles asserting the argument string each new builder produces, including
  repeatable `-t:` and `-p:` ordering, and that the push api key is not in any tag.
- Version bump.

## Out of scope

- `dotnet fixie` (third-party tool, repo-specific to crunchit).
- Builders for `gh`, `ganda`, or `herdr`.
- Moving the call sites in other repos; each repo does that under its own task when it bumps.

## Goal (implementer restatement)

Close the two real typed-builder gaps the org still reaches through a raw `Shell.Builder("dotnet")`:
`dotnet msbuild` and `dotnet nuget verify`, so task 150's analyzer has no msbuild/nuget entry in its
known-gap list.

Acceptance:

- `DotNet.MsBuild()` / `DotNet.MsBuild(project)` in new `dot-net.ms-build.cs`: project/solution
  positional, repeatable `-t:` targets (call order kept), repeatable `-p:` properties (insertion
  order kept), `-getProperty:`, `-getItem:`, `-getTargetResult:`, `-verbosity:`, `-restore`,
  `-nologo`, `-bl:<path>`, plus `WithWorkingDirectory`, `WithEnvironmentVariable`,
  `WithNoValidation`, `WithZeroExitCodeValidation`, `RunAsync`/`CaptureAsync` etc. It calls
  `DotNetBuildTelemetry.Tag` like `dotnet build` and runs through `Shell.Run(..., Options)`, so
  build tags and TRACEPARENT propagation apply.
- `DotNet.NuGet().Verify(packagePath)`: `--all`, repeatable `--certificate-fingerprint`,
  `--verbosity`, `--configfile`, same sub-builder shape as the other nuget sub-builders.
- A test proves `Push().WithApiKey(...).WithSymbolApiKey(...)` puts neither key into any
  `Activity.Current` tag (fix if it leaks).
- No `DotNet.Version()` / `DotNet.Info()`.
- Public API (Unshipped) and `dot-net.md` updated; Jaribu tests assert argument strings;
  package version bumped (3.0.0-beta.6 -> 3.0.0-beta.7).

## Checklist

- [x] DotNet.MsBuild()
- [x] DotNet.NuGet().Verify()
- [x] Push api key absent from telemetry tags (test)
- [x] Public API files and dot-net.md
- [x] Tests
- [x] Version bump

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session
- Groomed: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-18), scope cut to the builders that do not exist yet

- Implemented: implement oracle (2026-11-18). `dot-net.nu-get.cs` sat at its TWA0038 line limit
  (1801), so `DotNetNuGetBuilder` became `partial` and `Verify()` plus `DotNetNuGetVerifyBuilder`
  live in `dot-net.nu-get.verify.cs`. The push api key test found no leak: no nuget sub-builder
  tags `Activity.Current`, and the strict-exit exception message carries no argv.
- Audit: one advisory warning (`vscode-window-icon`, ignored `.vscode/settings.json` missing in
  this worktree) fixed with `ganda repo audit --fix`; nothing tracked changed.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 403bd4f0-1397-412c-b3ef-37e513e541ea, max-turns 500, Done — 2026-11-17T20:33:56Z

## Results

Files:

- `source/timewarp-amuru-tools/dot-net-commands/dot-net.ms-build.cs` (new): `DotNet.MsBuild()`,
  `DotNet.MsBuild(project)`, `DotNetMsBuildBuilder` (`WithTarget`, `WithProperty`,
  `WithGetProperty`, `WithGetItem`, `WithGetTargetResult`, `WithVerbosity`, `WithRestore`,
  `WithNoLogo`, `WithBinaryLog()` / `WithBinaryLog(path)`, shared options, run modes). Calls
  `DotNetBuildTelemetry.Tag("msbuild", ...)` and `Shell.Run("dotnet", ..., Options)`.
- `source/timewarp-amuru-tools/dot-net-commands/dot-net.nu-get.verify.cs` (new):
  `DotNetNuGetBuilder.Verify(packagePath)` and `DotNetNuGetVerifyBuilder` (`WithAll`,
  `WithCertificateFingerprint` repeatable, `WithVerbosity`, `WithConfigFile`).
- `dot-net.nu-get.cs`: `DotNetNuGetBuilder` is now `partial` (no other change).
- `dot-net.build-telemetry.cs`: comments list msbuild as a tagged command.
- `public-api/PublicAPI.Unshipped.txt`, `dot-net.md`, `documentation/release-notes/3.0.0-beta.7.md`.
- `source/Directory.Build.props`: 3.0.0-beta.6 -> 3.0.0-beta.7.
- Tests: new `tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.ms-build.cs`
  (12 tests: argv for every switch, repeatable `-t:` / `-p:` / `-get*` order, property replace in
  place, msbuild span tags, no paths or property values in tags); `dot-net.nuget.cs` gains 4
  tests (verify argv, fingerprint order, parent options, push api key and symbol api key absent
  from every Activity tag, event tag, and display name).

Gates:

- `./bin/dev build` — pass (first run failed TWA0038 on `dot-net.nu-get.cs`; fixed by the
  partial-file split; rerun pass).
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.ms-build.cs` — pass, 12/12.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.nuget.cs` — pass, 16/16.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build-telemetry.cs` — pass, 14/14 (covers the edited telemetry file).
- `ganda repo audit` — pass, 1 advisory (`vscode-window-icon`) then fixed with `--fix`; clean.

### How to validate

Smoke:

```bash
./bin/dev build
dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.ms-build.cs
dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.nuget.cs
```

Expect: build succeeds; ms-build runfile reports 12 passed, nuget runfile 16 passed. For example
`DotNet.MsBuild("app.csproj").WithTarget("Clean").WithTarget("Build").Build().ToCommandString()`
is `dotnet msbuild app.csproj -t:Clean -t:Build`, and
`DotNet.NuGet().Verify("p.nupkg").WithAll().Build().ToCommandString()` is
`dotnet nuget verify p.nupkg --all`.

## Notes

Related: 150 (analyzer). This task runs first, so the analyzer's known-gap list starts with no
`msbuild` or `nuget` entry.
