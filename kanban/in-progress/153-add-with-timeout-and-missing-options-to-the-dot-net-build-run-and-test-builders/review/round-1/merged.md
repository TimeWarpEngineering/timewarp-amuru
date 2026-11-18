# Round 1 — merged findings
**Date:** 2026-11-18
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.build.options.cs:1
- Description: `DotNetBuildBuilder` has no `WithNoLogo()`. The headline Jaribu call site in the task Notes (`runfile-coverage.static.cs`: `dotnet build <entry> --artifacts-path <dir> --nologo --verbosity minimal`) and `benchmark-measure.cs` build calls pass `--nologo`, so they still cannot move to the typed Build builder. Pack and Test builders already have `WithNoLogo()`.
- Suggestion: Add `DotNetBuildBuilder.WithNoLogo()` emitting `--nologo`, a PublicAPI.Unshipped entry, a command-string test in `tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build.cs`, and a `dot-net.md` mention. Record the test run in `## Results`.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
