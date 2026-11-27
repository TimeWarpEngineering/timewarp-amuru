# Round 3 — merged findings
**Date:** 2026-11-28
**Sources:** general
**Scope:** fix delta `0e6183c` and `3223388` vs round-2 `merged.md` (open M1)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 1 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru/native/text/tail-store.cs:65, 186-203; source/timewarp-amuru/native/text/tail-engine.cs:217-231
- Disposition notes: Round 3. `CopyStream.Dispose(bool)` swallows `IOException` and `UnauthorizedAccessException`. The buffered strategy closes the handle in its own `finally`, so the exception stays inside `saved?.Dispose()` and `File.Delete(path)` runs. A later operand is still read. Covered by `FullStore_Should_StillPrintTheSliceAndRemoveTheFile` for `Commands.Tail` and `Direct.Tail` (see M7 for where that test can run).

### M2 — Severity: suggestion — Status: fixed
- Disposition notes: Fixed in round 2; untouched by this delta.

### M3 — Severity: suggestion — Status: fixed
- Disposition notes: Fixed in round 2; untouched by this delta.

### M4 — Severity: suggestion — Status: fixed
- Disposition notes: Fixed in round 2; untouched by this delta.

### M5 — Severity: nit — Status: fixed
- Disposition notes: Fixed in round 2; untouched by this delta.

### M6 — Severity: nit — Status: fixed
- Disposition notes: Fixed in round 2; untouched by this delta.

### M7 — Severity: bug — Status: open
- File: tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.full-store.cs:44-61
- Description: The new full-disk test runs on every Linux host. It calls `unshare --user --map-root-user --mount`, and its child calls `mount -t tmpfs`. It asserts exit 0, with no check that the host allows unprivileged user and mount namespaces. Those namespaces are often unavailable:
  - Stock Ubuntu 23.10+, including the 24.04 `ubuntu-latest` CI runner, sets `kernel.apparmor_restrict_unprivileged_userns=1`.
  - Docker's default seccomp profile blocks them.
  - Some distros set `user.max_user_namespaces=0`.

  On those hosts the test fails instead of skipping. The CI multi-file aggregate compiles every file under `single-file-tests/**`, so CI and `./bin/dev test` can go red for an environment reason. The recorded pass is from the implementer's WSL2 machine only.
- Suggestion: Probe first (`unshare --user --map-root-user --mount true`, plus a tmpfs mount if needed). When the probe fails or `unshare` is missing, return early with a printed skip reason, the same way the test returns on non-Linux. Optionally add namespace-free coverage of the close path.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. M1 carries its stable id and is now fixed. M7 is new on the fix delta.

## Evidence

- No screenshots were listed under `evidence/` for this launch, and none were viewed.
- One targeted command: `gh run list --repo TimeWarpEngineering/timewarp-amuru --branch task/158-native-tail-command-with-its-own-argument-parser --limit 3`. Reason: to check whether CI on the runner already covered M7's test. It returned no runs.
- No build, full test run, audit, or hook was run. The recorded gates in `task.md` `## Results` are build, `commands.tail.save.full-store.cs`, `commands.tail.save.cs`, and `ganda repo audit`, all listed as passing. No finding disputes that record for the implementer's machine.
