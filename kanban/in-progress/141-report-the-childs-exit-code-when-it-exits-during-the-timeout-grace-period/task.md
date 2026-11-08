# Report the child's exit code when it exits during the timeout grace period

## Description

When a command's timeout fires, Amuru sends SIGINT and waits for `TimeoutGracePeriod`. Today the run is reported as a timeout the moment the graceful token fires: `TimeoutSession.TimedOut` is true, the `OperationCanceledException` catch in `CommandResult` returns `TimeoutExitCode` (124) with `TimedOut = true`, and strict validation throws `TimeoutException`. That holds even when the child handles the signal and exits on its own before the grace period elapses. `TtyPassthroughAsync` copies the same rule.

Task 138's oracle, `CommandCompletionMachine`, encodes the intended contract instead: `Interrupting` + `ChildExited` is `Exited` with the child's own code and `TimedOut` false. Only `Interrupting` + `GraceElapsed` is `TimedOut` with 124. The test `Interrupting_ChildExited_Should_ExitWithTheChildCode` in `tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs` passes against the oracle, and 138's Notes record that production disagrees. Steve confirmed the contract on 2026-11-08: a SIGINT that ends the child is a cooperative stop, not a timeout.

The cause is CliWrap. After the graceful token fires, `ExecuteAsync` throws `OperationCanceledException` when the process ends, whether the child exited by itself or the forceful token killed it. The exception does not carry the exit code, so the runner cannot tell the two cases apart from what CliWrap returns.

## Requirements

- Timeout fired, child exited before the grace period elapsed: result is the child's exit code, `TimedOut` is false, no `TimeoutException` under zero-exit validation. Zero-exit validation still throws `CommandExecutionException` for a non-zero code, as for any other run.
- Timeout fired, grace period elapsed, child killed: unchanged. Exit 124, `TimedOut` true, `TimeoutException` under zero-exit validation, the TTY path still does not throw.
- A child that dies from the SIGINT itself, without handling it, exits 130 on Unix. That reports as exit 130 and `TimedOut` false. Record this in the Design region so it is not filed as a bug.
- Caller cancellation is unchanged: `OperationCanceledException`, `TimedOut` false, `LastOutput` not written.
- Both runners agree: the CliWrap path (`TimeoutSession` plus `ListenWithTimeoutAsync`) and `TtyPassthroughAsync`.
- Pipelines: the stage that received SIGINT and exited reports its own code the same way. The pipeline's shared window is unchanged.
- Implementation reads the exit code from the process, not from CliWrap's exception. Acceptable routes: capture the exit code from the exit event before the cancellation surfaces, or run the timed-out wait through the handle-based child-process path task 083 added. Do not add a new package.
- Update the Design region of `source/timewarp-amuru/core/command-result.cs` and the public XML docs on `TimedOut`, `TimeoutExitCode`, and `WithTimeoutGracePeriod`.
- Remove the disagreement entry from 138's Notes only if the oracle test and the production test now agree. Otherwise leave it.
- Public API: no signature changes expected. If one is needed, `PublicAPI.Unshipped.txt` gets the line. `<Version>` stays `3.0.0-beta.1`.

### Version bump to 3.0.0-beta.2 (rolled into this task)

This task is also the version-bump PR for the next beta. Steve asked for it on 2026-11-09. Do the bump in the same branch, after the timeout change is green, as its own commit.

- `source/Directory.Build.props`: `<Version>` from `3.0.0-beta.1` to `3.0.0-beta.2`. The version is typed exactly once, here. No tag, no `dev release`, no publish in this task. Release is cut from master after merge with `/tw-release`.
- Public API roll-over, per AGENTS.md "Release": move every line in `source/timewarp-amuru/public-api/PublicAPI.Unshipped.txt` (the 083 JSON-RPC surface, 26 lines today, plus anything this task adds) into `PublicAPI.Shipped.txt`, and leave Unshipped as only `#nullable enable`. Do the same for `source/timewarp-amuru-tools/public-api/` (one line today). Build must stay clean under RS0016/RS0017.
- Release note `documentation/release-notes/3.0.0-beta.2.md`, same shape as `3.0.0-beta.1.md`. Sections: what is new (Amuru-native JSON-RPC client, `ShellBuilder.AsJsonRpcClient()`, `JsonRpcClientBuilder`, `JsonRpcClient`, source-generated serializer context, stderr observer; dev CLI telemetry via `UseTelemetry`), new dependency (`TimeWarp.Ramani` 1.0.0-beta.23 for the internal session machine, from nuget.org, depends only on `TimeWarp.Builder`), behavior change (this task: a child that exits during the timeout grace period reports its own exit code and `TimedOut` false; grace elapsed is still 124; exit 130 for an unhandled SIGINT), and unchanged (CliWrap stays the process library for ordinary commands; the JSON-RPC child uses the .NET 11 handle APIs). Mention tasks 083, 119, 138, 139, 140, 141 by number.
- `readme.md` and `documentation/overview.md`: if they state a version or a package list, update them. Do not add marketing text.
- Githook pins for `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` in `Directory.Packages.props` stay at the published version. They move only after `3.0.0-beta.2` is on the feed, in a later task.
- `./tools/dev-cli/dev.cs check-version` must report `3.0.0-beta.2` as not yet published.

## Checklist

- [ ] Production test: child handles SIGINT and exits 0 during grace, result exit 0 and `TimedOut` false, strict validation does not throw
- [ ] Production test: child handles SIGINT and exits 7 during grace, result exit 7, strict validation throws `CommandExecutionException`
- [ ] Production test: child ignores SIGINT, grace elapses, result 124 and `TimedOut` true (unchanged)
- [ ] Same three cases for `TtyPassthroughAsync` where a TTY is available, or recorded as skipped with the reason
- [ ] Pipeline stage case
- [ ] Design region and XML docs updated, including the exit 130 case
- [ ] 138 oracle and production now agree; Notes on 138 updated
- [ ] `<Version>` is `3.0.0-beta.2` in `source/Directory.Build.props`
- [ ] Unshipped lines moved to Shipped for both packable projects; Unshipped is header-only; RS0016/RS0017 clean
- [ ] `documentation/release-notes/3.0.0-beta.2.md` written
- [ ] `dev check-version` reports 3.0.0-beta.2 unpublished
- [ ] `./tools/dev-cli/dev.cs build` and `./tools/dev-cli/dev.cs test` pass
- [ ] `ganda repo audit` is clean

## Depends on

- 138

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)
- Version bump to 3.0.0-beta.2 rolled in: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)

## Notes

Filed from the 138 merge on 2026-11-09. Steve: "SIGINT ending something != timeout to me."

Why 124 was chosen originally: the runner only sees CliWrap's `OperationCanceledException` after the graceful token fires, so 124 was the only code it could report. The grace period was still useful because the child got to flush and clean up, but its answer was discarded.

Related: task 119 notes a possible CliWrap swap for a later release. This task does not need that swap, but if the exit-code capture turns out to need it, stop and record the finding rather than widening the task.
