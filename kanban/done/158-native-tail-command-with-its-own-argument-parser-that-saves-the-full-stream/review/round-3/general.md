# Round 3 — general
**Date:** 2026-11-28
**Scope reviewed:** fix delta `0e6183c` and `3223388` vs round-2 `merged.md` (open M1). Files: `source/timewarp-amuru/native/text/tail-store.cs`, `source/timewarp-amuru/native/text/tail-engine.cs`, `tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.full-store.cs`, `documentation/developer/reference/native-text-commands.md`, `task.md`.

## Summary

The fix adds a private `CopyStream : FileStream` whose `Dispose(bool)` swallows `IOException` and `UnauthorizedAccessException`. `TailStore.TryCreate` returns it as the saved copy. That closes M1's residual. `BufferedFileStreamStrategy.Dispose` closes the handle in its own `finally` before the flush exception reaches the override. So the handle is closed, the exception stays inside `saved?.Dispose()`, and the `File.Delete(path)` in `TailEngine`'s `finally` runs. A later operand is then read. `capture?.Dispose()` cannot throw, because `TailCaptureStream` does not dispose the streams it borrows. Docs, the Design regions, and `task.md` match the change.

The new regression test drives the real failure (a full tmpfs) for `Commands.Tail` and `Direct.Tail`. The cleanup stamp is zero bytes, so `TryCreate` still succeeds on the full mount and the test does reach the close path. One problem: the test needs unprivileged user and mount namespaces. When the host cannot create them, the test fails instead of skipping. That host includes stock Ubuntu 23.10+ and the `ubuntu-latest` CI runner, where AppArmor restricts unprivileged user namespaces, and most containers.

## Issues

### Issue 1 — Severity: bug
- File: tests/timewarp-amuru/single-file-tests/native/text/commands.tail.save.full-store.cs:44-61
- Description: `FullStore_Should_StillPrintTheSliceAndRemoveTheFile` runs on every Linux host. It checks `OperatingSystem.IsLinux()` and nothing else, then runs `unshare --user --map-root-user --mount dotnet run --file …`, and the child runs `mount -t tmpfs`. It asserts `child.ExitCode.ShouldBe(0)`.
  - Unprivileged user namespaces are often unavailable. Ubuntu 23.10+ ships `kernel.apparmor_restrict_unprivileged_userns=1`, and `ubuntu-latest` is Ubuntu 24.04. Docker's default seccomp profile blocks `unshare`. Some distros set `user.max_user_namespaces=0`. On those hosts `unshare` fails (uid_map write or mount denied) or `unshare` is missing.
  - The test then fails. It does not skip.
  - CI's `dev workflow` runs the multi-file aggregate, which compiles every file under `single-file-tests/**`. That aggregate includes this test. So CI and `./bin/dev test` can go red for an environment reason, not a product defect.
  - The recorded pass in `## Results` was on the implementer's machine (WSL2), where namespaces are allowed. There is no CI run on the branch yet (`gh run list --branch task/158-… --limit 3` returned nothing), so the record does not cover the runner.
- Suggestion: Before the real run, probe the capability, for example `unshare --user --map-root-user --mount true`, plus a tmpfs mount in the probe if needed. When the probe fails or `unshare` is missing, return early and print a one-line skip reason, the same way the test already returns on non-Linux. Optionally keep CI coverage of the close path with a check that does not need namespaces.
- Status: open

## Resolved prior

- M1 (bug) — fixed. See the summary above. The write-failure and close path no longer fails the tail, and the partial file is removed.
- M2–M6 were fixed in round 2. This delta does not touch them.

## Evidence / commands

- One targeted command: `gh run list --repo TimeWarpEngineering/timewarp-amuru --branch task/158-native-tail-command-with-its-own-argument-parser --limit 3`. Reason: to see whether a CI run on the runner already covers the namespace-dependent test. It returned no runs.
- No build, full test run, audit, or hook was run.
