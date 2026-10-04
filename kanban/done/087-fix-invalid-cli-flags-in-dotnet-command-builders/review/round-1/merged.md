# Round 1 — merged findings
**Date:** 2026-10-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs:118
- Description: dev-certs smoke does not execute the `--export-path` form.
- Suggestion: Add an export smoke to a temp path.
- Source: general
- Disposition notes: wontfix (orchestrator). `dotnet dev-certs https -ep` creates a dev certificate when none exists, mutating the user cert store on dev machines and CI. The smoke still proves the builder no longer emits the rejected `--export` switch, and the snapshot test pins the `--export-path` emission.

## Duplicates / conflicts

- None (single reviewer).
