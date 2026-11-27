# Round 4 — merged findings
**Date:** 2026-11-28
**Sources:** general
**Scope:** fix delta `3e88f7a` and `e08614a`, checked against round-3 `merged.md` (open M7)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- Disposition notes: Fixed in round 3. This delta does not touch it.

### M2 — Severity: suggestion — Status: fixed
- Disposition notes: Fixed in round 2. This delta does not touch it.

### M3 — Severity: suggestion — Status: fixed
- Disposition notes: Fixed in round 2. This delta does not touch it.

### M4 — Severity: suggestion — Status: fixed
- Disposition notes: Fixed in round 2. This delta does not touch it.

### M5 — Severity: nit — Status: fixed
- Disposition notes: Fixed in round 2. This delta does not touch it.

### M6 — Severity: nit — Status: fixed
- Disposition notes: Fixed in round 2. This delta does not touch it.

### M7 — Severity: bug — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.full-store.cs:51-56, 165-197
- Disposition notes: Fixed in round 4. Before it runs the child, `PrivateTmpfsSkipReason()` checks for `unshare` on PATH. It then runs `unshare --user --map-root-user --mount` with a tmpfs mount and unmount, using `WithNoValidation()` and a 30 s timeout. A missing binary, a non-zero exit, a timeout, or a launch exception prints one `TestSkipped` line and returns. The probe uses the same flags and mount as the child. The child's assertions are unchanged.

## Duplicates / conflicts

- There was one reviewer and no new findings. All stable ids M1–M7 carry over as fixed.

## Evidence

- This launch listed no screenshots under `evidence/`, and none were viewed.
- No targeted command was run. No build, full test run, audit, or hook was run. The recorded gates in `task.md` `## Results` are `./bin/dev build`, `commands.tail.save.full-store.cs`, and `ganda repo audit`, all passing. The skip-branch checks used a PATH with a failing `unshare` and a PATH with no `unshare`. This review does not dispute that record.
