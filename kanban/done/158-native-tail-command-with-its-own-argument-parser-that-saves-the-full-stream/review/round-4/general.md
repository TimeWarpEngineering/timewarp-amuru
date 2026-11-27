# Round 4 — general
**Date:** 2026-11-28
**Scope reviewed:** fix delta `3e88f7a` (test change in `tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.full-store.cs`) and `e08614a` (kanban note only), checked against round-3 `merged.md` (open M7).

## Summary

The fix pass adds `PrivateTmpfsSkipReason()` to the full-disk test. Before it runs the child, the test checks for `unshare` on PATH. It then runs `unshare --user --map-root-user --mount sh -c '<mktemp; mount -t tmpfs; umount; rm>'` with `WithNoValidation()` and a 30 s timeout. A missing binary, a non-zero exit, a timeout, or a launch exception (`Win32Exception`, `IOException`, `CliWrapException`) prints one `TestSkipped` line and returns. When the probe exits 0, the full-disk child runs unchanged. This closes M7.

## M7 check

- The probe uses the same namespace flags as the child (`--user --map-root-user --mount`) and the same tmpfs mount (`-t tmpfs -o size=8192`). A host that cannot run the child fails the probe the same way. On Ubuntu 24.04 with `kernel.apparmor_restrict_unprivileged_userns=1`, the uid_map write or the mount is denied, so `unshare` exits non-zero and the test skips. A Docker seccomp denial or `user.max_user_namespaces=0` also gives a non-zero exit, so the test skips there too.
- `WithNoValidation()` keeps a non-zero probe exit from throwing. `TimedOut` is a real `CommandOutput` property (`source/timewarp-amuru/core/command-output.cs:49`). `TestHelpers.TestSkipped(TimeWarpTerminal.Default, …)` follows the existing pattern (`core/command-result.large-output.cs:257`). `CliWrap` is referenced by `tests/timewarp-amuru/Directory.Build.props:24`.
- The probe cleans up its own temporary directory, and the parent's `root` is created only after the probe passes, so a skip leaves nothing behind.
- The skip is gated by the environment and is documented in `task.md` (`## Requirements` and `### How to validate`). The child's assertions are unchanged, so this does not weaken the check.
- Recorded gates for this pass are build, the touched runfile, and audit, all passing. The implementer also ran the test with a PATH where `unshare` exits 1 and with no `unshare` on PATH, and both printed a skip. That covers the skip branches.

## Observation (not a finding)

On a runner that blocks namespaces, including the likely `ubuntu-latest` CI runner, this test now always skips. The M1 close path is then covered only on hosts that allow namespaces, such as the implementer's WSL2 machine. Round 3 already called namespace-free coverage optional. This review does not open a finding for it.

## Issues

None.
