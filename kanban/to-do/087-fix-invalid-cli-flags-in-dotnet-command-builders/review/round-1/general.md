# Round 1 — general
**Date:** 2026-10-04
**Scope reviewed:** same as framework (source/timewarp-amuru-tools/dot-net-commands/*, tests/.../dot-net-commands/*)

## Summary

The change fixes every checklist item: `--tl:<mode>` as one token on six builders, `WithCollect(string)`,
pack `WithFramework` removed, dev-certs export emits only `--export-path` (and throws without a path),
`nuget why` positional project before the package id, `nuget delete` drops `--configfile` and gains
`--non-interactive` (with an interactive/non-interactive conflict guard), watch drops unsupported
include/exclude/property, and `dotnet run` guards project+file. Risk is low; breaking surface is
intentional and documented in Results and dot-net.md. Verified: `dot-net.cli-smoke.cs` 10/10 pass
against the pinned SDK; `ganda repo audit` passes. No remaining callers of removed APIs in the repo.

## Issues

### Issue 1 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs:118
- Description: The dev-certs smoke runs only `https --check`; it does not execute the `--export-path` form that was the actual fix. The export form is covered by the string snapshot in dot-net.dev-certs.cs only.
- Suggestion: Optionally add an export smoke to a temp path.
- Status: open
