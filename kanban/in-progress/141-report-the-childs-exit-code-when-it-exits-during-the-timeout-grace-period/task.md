# Report the child's exit code when it exits during the timeout grace period

## Description

When a command's timeout fires, Amuru sends SIGINT and waits for `TimeoutGracePeriod`. Today the run is reported as a timeout the moment the graceful token fires: `TimeoutSession.TimedOut` is true, the `OperationCanceledException` catch in `CommandResult` returns `TimeoutExitCode` (124) with `TimedOut = true`, and strict validation throws `TimeoutException`. That holds even when the child handles the signal and exits on its own before the grace period elapses. `TtyPassthroughAsync` copies the same rule.

Task 138's oracle, `CommandCompletionMachine`, encodes the intended contract instead: `Interrupting` + `ChildExited` is `Exited` with the child's own code and `TimedOut` false. Only `Interrupting` + `GraceElapsed` is `TimedOut` with 124. The test `Interrupting_ChildExited_Should_ExitWithTheChildCode` in `tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs` passes against the oracle, and 138's Notes record that production disagrees. Steve confirmed the contract on 2026-11-08: a SIGINT that ends the child is a cooperative stop, not a timeout.

## Goal (implementer restatement)

A timeout still sends the graceful signal and then waits for the grace period. If the child exits during that wait, the run is that child's exit code and `TimedOut` is false. Zero-exit validation does not throw `TimeoutException` for that exit; a non-zero code still throws `CommandExecutionException`. If the child is still running when the grace period ends, the run stays exit 124 with `TimedOut` true, and strict validation still throws `TimeoutException`. The TTY runner reports that kill and does not throw. An unhandled SIGINT that kills the child is exit 130 on Unix and is not a timeout. Caller cancellation is still `OperationCanceledException` with `LastOutput` left unset. The CliWrap runner and `TtyPassthroughAsync` follow the same rule, and a pipeline stage that received the signal and exited reports its own code. The exit code comes from the process, because CliWrap's cancellation exception does not carry one. After that behavior is green, this same branch bumps the packages to `3.0.0-beta.2`, rolls the unshipped public API into Shipped, and writes the beta.2 release note. Nothing is tagged or published here.

Acceptance: the three grace outcomes (child exit 0, child exit 7, grace elapsed) hold for the CliWrap runner and for `TtyPassthroughAsync`, a pipeline stage that exits during grace keeps its code, the Design region and the `TimedOut` / `TimeoutExitCode` / `WithTimeoutGracePeriod` docs name the 130 case, task 138's disagreement note comes out once production matches the oracle, `<Version>` is `3.0.0-beta.2`, both Unshipped files are header-only, `dev check-version` shows that version unpublished, and build, test, and `ganda repo audit` are clean.

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

- [x] Production test: child handles SIGINT and exits 0 during grace, result exit 0 and `TimedOut` false, strict validation does not throw
- [x] Production test: child handles SIGINT and exits 7 during grace, result exit 7, strict validation throws `CommandExecutionException`
- [x] Production test: child ignores SIGINT, grace elapses, result 124 and `TimedOut` true (unchanged)
- [x] Same three cases for `TtyPassthroughAsync` where a TTY is available, or recorded as skipped with the reason
- [x] Pipeline stage case
- [x] Design region and XML docs updated, including the exit 130 case
- [x] 138 oracle and production now agree; Notes on 138 updated
- [x] `<Version>` is `3.0.0-beta.2` in `source/Directory.Build.props`
- [x] Unshipped lines moved to Shipped for both packable projects; Unshipped is header-only; RS0016/RS0017 clean
- [x] `documentation/release-notes/3.0.0-beta.2.md` written
- [x] `dev check-version` reports 3.0.0-beta.2 unpublished
- [x] `./tools/dev-cli/dev.cs build` and `./tools/dev-cli/dev.cs test` pass
- [x] `ganda repo audit` is clean

## Depends on

- 138

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)
- Version bump to 3.0.0-beta.2 rolled in: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)
- Implementation: 01a1bc56-2621-7af3-9055-ab81b04064d2 (2026-11-09). The grace-period growth tripped `source-file-line-cap`. The timeout session, exit capture, and timed stream moved to `command-result.timeout.cs`, the mode tests moved to `shell-builder.with-timeout.modes.cs`, and the `command-result.cs` baseline shrank from 1408 to 1354.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bc56-2621-7af3-9055-ab81b04064d2, max-turns 500, Done — 2026-11-08T18:43:25Z

## Notes

Filed from the 138 merge on 2026-11-09. Steve: "SIGINT ending something != timeout to me."

Why 124 was chosen originally: the runner only sees CliWrap's `OperationCanceledException` after the graceful token fires, so 124 was the only code it could report. The grace period was still useful because the child got to flush and clean up, but its answer was discarded.

Related: task 119 notes a possible CliWrap swap for a later release. This task does not need that swap, but if the exit-code capture turns out to need it, stop and record the finding rather than widening the task.

The exit code is read from the process `Exited` event. CliWrap stays the process library. A zero grace period is still exit 124, because the signal and the kill are the same instant. `TtyPassthroughAsync` reports exit 7 during grace and does not throw, which is the existing TTY validation exemption. `documentation/overview.md` states no package version, so it was left as it is. The githook pins in `Directory.Packages.props` stay at `2.0.0`.

## Results

A child that exits during the timeout grace period keeps its own exit code and `TimedOut` stays false. The grace-period kill is still exit 124. Unix exit 130 for an unhandled SIGINT is the child's code. Caller cancellation is unchanged. The version is `3.0.0-beta.2`. Both Unshipped files are header-only. Nothing was tagged or published.

`./tools/dev-cli/dev.cs build` succeeded with 0 warnings and packed `TimeWarp.Amuru.3.0.0-beta.2.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.2.nupkg`. `./tools/dev-cli/dev.cs test` reported 834 total, 833 passed, 1 skipped (`GetCommitsAheadOfDefaultBranch_Given_`, pre-existing). `./tools/dev-cli/dev.cs check-version` reported source `3.0.0-beta.2`, latest NuGet `3.0.0-beta.1`, and one prerelease increment ahead. The commit hook attested the line-cap commit and the version-bump commit.

### How to validate

**Smoke**

```bash
dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs
dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.modes.cs
./tools/dev-cli/dev.cs check-version
```

**Expect**

- The capture file reports 14 passed. Exit 0 keeps `TimedOut` false and strict validation does not throw. Exit 7 throws `CommandExecutionException` and leaves `LastOutput` null. Exit 130 keeps `TimedOut` false. A child that ignores SIGINT is exit 124 and `TimedOut` true. Caller cancellation leaves `LastOutput` null.
- The modes file reports 14 passed. `TtyPassthroughAsync` reports exit 0, exit 7 without throwing, and exit 130, and the ignored-SIGINT case is exit 124. The pipeline's last stage reports exit 7, and strict validation throws `CommandExecutionException`.
- `check-version` prints source `3.0.0-beta.2`, latest NuGet `3.0.0-beta.1`, and that the source version is not yet published.
