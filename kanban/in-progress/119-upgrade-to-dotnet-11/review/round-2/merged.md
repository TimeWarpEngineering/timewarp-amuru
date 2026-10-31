# Round 2 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: the five char-search sites in `nuget-package-service.cs`, `file-system-walk.cs`, `glob-matcher.cs`, and `text-diff.cs` (×2)
- Description: CA1307 was suppressed because of a false "no StringComparison overload" claim.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Each site passes `StringComparison.Ordinal` on the char overload, and the pragmas are gone. `task.md` is corrected. Release build 0 warnings, tests green.

### M2 — Severity: nit — Status: fixed
- File: tools/dev-cli/Directory.Build.props
- Description: CA1812 was in the project-wide `NoWarn`.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. CA1812 is out of `NoWarn`. `WorkflowCommand.Handler`, `DevCli.ReleaseCommand.Handler`, and `DevCli.SelfInstallCommand.Handler` carry type-scoped suppressions. The dev-cli Release build is clean.

### M3 — Severity: nit — Status: fixed
- File: documentation/user/installation.md:81, :139
- Description: The doc said `dnx` and `#:package` came from .NET 11.
- Source: general (round 1)
- Disposition notes: fixed in `0b1cccd`. Both lines now say ".NET SDK (.NET 10 and later)".

## Duplicates / conflicts

- None. Round 2 raised no new findings.
