# Round 2 — merged findings
**Date:** 2026-09-30
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 3 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: skills/amuru/SKILL.md
- Description: Fzf samples called `WithInputItems` and `WithInputCommand`.
- Suggestion: Use `FromInput` and `FromCommand`.
- Source: general
- Disposition notes: Fixed. Calls match `Fzf.InputMethods`.

### M2 — Severity: bug — Status: fixed
- File: skills/amuru/SKILL.md
- Description: Package section listed only `TimeWarp.Amuru`.
- Suggestion: Name `TimeWarp.Amuru.Tools` and which types it holds.
- Source: general
- Disposition notes: Fixed. Tools package directive plus the type split.

### M3 — Severity: bug — Status: fixed
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.md
- Description: Shared-members prose said `WithProperty` existed on every builder above.
- Suggestion: Limit it to the builders that define it.
- Source: general
- Disposition notes: Fixed. ListPackages, AddPackage, and RemovePackage are called out as not having it.

### M4 — Severity: suggestion — Status: fixed
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.md
- Description: `RunAndCaptureAsync` omitted the pack builder.
- Suggestion: Include pack.
- Source: general
- Disposition notes: Fixed.

## Duplicates / conflicts

- None. Prior M1–M4 carried forward. No new IDs.
