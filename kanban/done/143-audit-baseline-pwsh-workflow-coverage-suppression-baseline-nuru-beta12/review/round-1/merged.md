# Round 1 — merged findings
**Date:** 2026-11-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 1 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: wontfix
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs:399
- Description: `Directory.EnumerateFileSystemEntries(toolPath, …)` throws `DirectoryNotFoundException` when the SDK does not create the `--tool-path` directory. That makes the "nothing installed" assertion depend on the SDK version. The predicate is also a single unparenthesized `||`/`&&` line.
- Suggestion: Guard with `!Directory.Exists(toolPath) || …`, and put the predicate in a named local.
- Source: general
- Disposition notes: wontfix (decided by the review oracle, 2026-11-11). This cannot produce a false pass. If the SDK behavior changes, the test fails loudly, just with a less clear message. It does not hide an installed package. On SDK 11.0.100, the version this worktree resolves, it passes (838 passed, 1 skipped). The task's open "CI green on the PR" checklist item covers the SDK that CI resolves. A fix pass plus a second review round would cost more than this hardening is worth on an audit-hygiene task. Reopen this if PR CI fails here with `DirectoryNotFoundException`.

## Duplicates / conflicts

- None (single reviewer).
