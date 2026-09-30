# Round 1 — merged findings
**Date:** 2026-09-30
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 3 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: skills/amuru/SKILL.md:187
- Description: Fzf samples call `WithInputItems` and `WithInputCommand`. The methods are `FromInput` and `FromCommand`.
- Suggestion: Rename the calls.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: skills/amuru/SKILL.md:17
- Description: Package section lists only `TimeWarp.Amuru`. DotNet, Git, and Fzf are in `TimeWarp.Amuru.Tools`.
- Suggestion: Name the Tools package and which types it holds.
- Source: general
- Disposition notes:

### M3 — Severity: bug — Status: open
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.md:178
- Description: Shared-members prose says `WithProperty` exists on every builder above. ListPackages, AddPackage, and RemovePackage do not have it.
- Suggestion: Limit `WithProperty` to Build, Clean, Restore, Run, Test, Publish, and Pack.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.md:25
- Description: `RunAndCaptureAsync` is also on the pack builder. The sentence omits it and says other builders do not have it.
- Suggestion: Include pack.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. One reviewer.
