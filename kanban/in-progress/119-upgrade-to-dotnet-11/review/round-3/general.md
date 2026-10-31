# Round 3 — general
**Date:** 2026-10-31
**Scope reviewed:** product delta `0b1cccd..445f7ad` (commits `bd1f3c4` and `bc3b773`): the sample moniker pass. I also swept the whole tree for current .NET 10 references, because the task asks for every current reference to show .NET 11.

## Summary

The pass swaps `net10.0` for `net11.0` in the builder XML docs, `dot-net.md`, the dotnet-command tests and their expected command strings, both 1.0.0 blips, and the migration-analysis `<TargetFramework>` sample. Each edit is a one-token swap, and every `WithFramework` / `WithTemplateArg` edit matches its expected string. `bc3b773` puts back the executable bit that the rewrite dropped, so the net mode change from `0b1cccd` is none. Shipped records were left alone as instructed. One current .NET 10 reference is still there: the readme badge. That readme ships inside both `3.0.0` nupkgs.

Verification on this host, SDK `11.0.100-rc.3.26522.148`:

- `dotnet build timewarp-amuru.slnx -c Release -warnaserror`: 0 warnings, 0 errors.
- `dotnet run --file tests/timewarp-amuru/multi-file-runners/run-tests.cs`: Total 733, Passed 732, Skipped 1, exit 0.
- `rg 'net10\.0|\.NET 10'` outside `kanban/`, `bin/`, `obj/`, and `artifacts/` finds only the strings the task keeps (release notes 2.0.0 and 3.0.0, `installation.md:81` and `:139`).

## Issues

### Issue 1 — Severity: bug
- File: readme.md:14
- Description: The shields badge still says `dotnet-10.0` (`https://img.shields.io/badge/dotnet-10.0-blue`). Steve's open-work note says "update every current reference so it shows .NET 11", and Requirements say to update README mentions. Checklist 5 says `readme.md` was spot-checked and now says .NET 11. Both product csprojs pack `$(RepositoryRoot)readme.md` as `PackageReadmeFile`. The `3.0.0` package page on NuGet would show a .NET 10 badge for a package that targets only `net11.0`, which is the TFM break `3.0.0` exists to announce. The `rg` smoke under Results checks only the listed paths, so it does not catch this.
- Suggestion: Change the badge to `dotnet-11.0` (`https://img.shields.io/badge/dotnet-11.0-blue`). Add `readme.md` to the `rg` smoke under How to validate.
- Status: open

## Checked, no finding

- `cliwrap-exit-code-tests/readme.md:34` (`.NET Version: 10.0.100-preview.5`) records the environment where those exit-code tests ran. That is history, not a current requirement. Leave it.
- `dot-net.timeout.cs:48` mocks `dotnet --version` returning `10.0.0`. It is an arbitrary mock payload for a timeout test and does not claim support. No finding.
- The 1.0.0 blips are announcements for an old version, but Steve listed them by name in the open work, so the edit follows the task.
- The cli-smoke Design note now says the 11.0 feature band. That matches the `global.json` pin.
