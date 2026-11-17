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

## Checklist

- [ ] DotNet.MsBuild()
- [ ] DotNet.NuGet().Verify()
- [ ] Push api key absent from telemetry tags (test)
- [ ] Public API files and dot-net.md
- [ ] Tests
- [ ] Version bump

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session
- Groomed: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-18), scope cut to the builders that do not exist yet

## Notes

Related: 150 (analyzer). This task runs first, so the analyzer's known-gap list starts with no
`msbuild` or `nuget` entry.
