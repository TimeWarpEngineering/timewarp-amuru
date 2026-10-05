# Round 1 — merged findings
**Date:** 2026-10-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: readme.md:49
- Description: Install commands resolve to stable 1.1.1 while the readme documents the 2.0 API. The release notes say to use `--prerelease`.
- Suggestion: Add a prerelease note that links the release notes.
- Source: general
- Disposition notes: Fixed on the task branch. The readme now says it documents 2.0 and to add `--prerelease` until GA, and links `documentation/release-notes/2.0.0.md`.

### M2 — Severity: nit — Status: fixed
- File: documentation/release-notes/2.0.0.md:27
- Description: `WithExport()` entries were not type-qualified.
- Suggestion: Use `DotNetDevCertsHttpsBuilder.WithExport()`.
- Source: general
- Disposition notes: Fixed in the Breaking changes and Upgrade guide entries.

## Duplicates / conflicts

- None.
