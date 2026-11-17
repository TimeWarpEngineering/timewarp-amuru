# Typed dotnet builders for msbuild, nuget, and version

## Description

Task 150 adds an analyzer that reports `Shell.Builder("dotnet")`. Its warning case
(`AMURU0002`) is a dotnet verb with no typed builder. Scanning the org's master worktrees on
2026-11-18 found these verbs used through a raw shell with no `DotNet.*` equivalent:

| Verb | Call sites | Example use |
|------|-----------:|-------------|
| msbuild | 5 | `dotnet msbuild <proj> -t:Target -p:Name=Value -getProperty:X` |
| nuget | 3 | `dotnet nuget push`, `dotnet nuget verify`, `dotnet nuget locals` |
| --version / --info | 1 | SDK version probe |

Add typed builders so those call sites can move off `Shell.Builder` and the analyzer's known-gap
list for these verbs can be removed.

## Requirements

- `DotNet.MsBuild()`: project or solution path, `-t:` targets (repeatable), `-p:` properties
  (repeatable), `-getProperty:` / `-getItem:` / `-getTargetResult:`, verbosity, `-restore`,
  `-nologo`, binary log path, working directory, environment variables. Same fluent shape and
  `Options` plumbing as `dot-net.build.cs`, so build telemetry tags and `TRACEPARENT`
  propagation (task 149) apply.
- `DotNet.NuGet()` with `Push` (package path, source, api key, skip-duplicate, timeout),
  `Verify` (package path, certificate fingerprint), and `Locals` (cache name, clear or list).
  Api key never goes into telemetry tags or logs.
- `DotNet.Version()` and `DotNet.Info()` returning the trimmed stdout.
- Each builder follows the existing conventions in `source/timewarp-amuru-tools/dot-net-commands`
  (one file per verb, `WithEnvironmentVariable`, `WithWorkingDirectory`, `WithNoValidation`,
  `CaptureAsync` / `RunAsync`).
- Public API files updated (`source/timewarp-amuru-tools/public-api`).
- Tests: Jaribu runfiles asserting the argument string each builder produces, including
  repeatable `-t:` and `-p:` ordering and that the api key is not in any tag.
- Version bump.

## Out of scope

- `dotnet fixie` (third-party tool, repo-specific to crunchit).
- Builders for `gh`, `ganda`, or `herdr`.
- Moving the call sites in other repos; each repo does that under its own task when it bumps.

## Checklist

- [ ] DotNet.MsBuild()
- [ ] DotNet.NuGet() Push / Verify / Locals
- [ ] DotNet.Version() / DotNet.Info()
- [ ] Public API files
- [ ] Tests
- [ ] Version bump

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session

## Notes

Related: 150 (analyzer). Either can land first. When 151 lands after 150, remove these verbs
from the analyzer's known-gap list in the same change.
