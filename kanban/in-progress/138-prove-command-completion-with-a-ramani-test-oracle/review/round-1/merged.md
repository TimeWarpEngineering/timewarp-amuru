# Round 1 — merged findings
**Date:** 2026-11-08
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: tests/timewarp-amuru/Directory.Build.props:123
- Description: The Ramani generator analyzer path types out `1.0.0-beta.23`, a second copy of the version in `Directory.Packages.props`. After a central version bump (for example the audit's package-freshness fix), a clean CI cache does not have beta.23. The generator is then missing and every test runfile fails to compile, because each one includes `command-completion/*.cs`. A warm cache runs a stale generator without any warning.
- Suggestion: Build the path from the `PackageVersion` item for `TimeWarp.Ramani`, lowercased, and add an `<Error Condition="!Exists(...)">` with a clear message.
- Source: general
- Disposition notes: `AddCommandCompletionRamaniGenerator` sets `RamaniPackageVersion` from `@(PackageVersion->WithMetadataValue('Identity', 'TimeWarp.Ramani')->'%(Version)')`, lowercases it, and builds the cache path from that. `<Error Condition="!Exists('$(RamaniGeneratorPath)')">` names the version and the path. A rebuild of the oracle runfile passes `.../timewarp.ramani/1.0.0-beta.23/analyzers/dotnet/cs/TimeWarp.Ramani.Generators.dll` to csc. A probe with version `0.0.0-missing` fails with that error.

## Duplicates / conflicts

- None (single reviewer).
