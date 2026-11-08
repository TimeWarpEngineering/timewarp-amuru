# Round 2 — merged findings
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
- File: tests/timewarp-amuru/Directory.Build.props:35
- Description: (carried from round 1) The Ramani generator analyzer path typed out `1.0.0-beta.23` a second time, separately from the central PackageVersion.
- Source: general (round 1)
- Disposition notes: Re-verified in round 2 at 21b7a1f. The path comes from `@(PackageVersion->WithMetadataValue('Identity','TimeWarp.Ramani')->'%(Version)')`, lowercased. A missing dll raises an `<Error>` that names the path. A forced rebuild resolves the beta.23 analyzer, and the oracle runfile passes 19 of 19.

No new findings in the round-2 delta.

## Duplicates / conflicts

- None (single reviewer).
