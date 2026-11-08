# Report the child's exit code when it exits during the timeout grace period

## Description

When a command's timeout fires, Amuru sends SIGINT and waits for `TimeoutGracePeriod`. Today the run is reported as a timeout the moment the graceful token fires: `TimeoutSession.TimedOut` is true, the `OperationCanceledException` catch in `CommandResult` returns `TimeoutExitCode` (124) with `TimedOut = true`, and strict validation throws `TimeoutException`. That holds even when the child handles the signal and exits on its own before the grace period elapses. `TtyPassthroughAsync` copies the same rule.

Task 138's oracle, `CommandCompletionMachine`, encodes the intended contract instead: `Interrupting` + `ChildExited` is `Exited` with the child's own code and `TimedOut` false. Only `Interrupting` + `GraceElapsed` is `TimedOut` with 124. The test `Interrupting_ChildExited_Should_ExitWithTheChildCode` in `tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs` passes against the oracle, and 138's Notes record that production disagrees. Steve confirmed the contract on 2026-11-08: a SIGINT that ends the child is a cooperative stop, not a timeout.

## Goal (implementer restatement)

A timeout still sends the graceful signal and then waits for the grace period. If the child exits during that wait, the run is that child's exit code and `TimedOut` is false. Zero-exit validation does not throw `TimeoutException` for that exit; a non-zero code still throws `CommandExecutionException`. If the child is still running when the grace period ends, the run stays exit 124 with `TimedOut` true, and strict validation still throws `TimeoutException`. The TTY runner reports that kill and does not throw. An unhandled SIGINT that kills the child is exit 130 on Unix and is not a timeout. Caller cancellation is still `OperationCanceledException` with `LastOutput` left unset. The CliWrap runner and `TtyPassthroughAsync` follow the same rule, and a pipeline stage that received the signal and exited reports its own code. The exit code comes from the process, because CliWrap's cancellation exception does not carry one. After that behavior is green, this same branch bumps the packages to `3.0.0-beta.2`, rolls the unshipped public API into Shipped, and writes the beta.2 release note. Nothing is tagged or published here.

Acceptance: the three grace outcomes (child exit 0, child exit 7, grace elapsed) hold for the CliWrap runner and for `TtyPassthroughAsync`, a pipeline stage that exits during grace keeps its code, the Design region and the `TimedOut` / `TimeoutExitCode` / `WithTimeoutGracePeriod` docs name the 130 case, task 138's disagreement note comes out once production matches the oracle, `<Version>` is `3.0.0-beta.2`, both Unshipped files are header-only, `dev check-version` shows that version unpublished, and build, test, and `ganda repo audit` are clean.

A slow stream reader does not turn a grace-period exit into a timeout, and a descendant does not erase the wall clock. When the direct child exits during a positive grace period, the forceful deadline moves from `Timeout + TimeoutGracePeriod` to `Timeout` plus twice the grace period. That extra grace is the only added drain. A reader that finishes inside it receives every line the child wrote, the result keeps the child's exit code, and `TimedOut` stays false. The deadline stays armed. If a descendant still holds stdout or stderr when that deadline fires, the run ends then with the same child code and `TimedOut` false. It does not wait for the descendant. An exit before the graceful signal leaves the original `Timeout + TimeoutGracePeriod` deadline, and a pipe that stays open is cut off there on the same rule. A child that is still running when its deadline fires is still exit 124 with `TimedOut` true. A zero grace period does not move the deadline. An exit before that signal still records the child, so a descendant that holds a pipe is cut off at `Timeout` with the child's exit code and `TimedOut` false. An exit at or after the zero-grace signal, and a child that is still running when that deadline fires, stay exit 124 with `TimedOut` true. The caller token and an abandoned enumerator still cancel the drain. The slow-reader test outlasts `Timeout + TimeoutGracePeriod`, finishes inside the doubled grace, reads every line, reports the child's code, and leaves `TimedOut` false. The descendant test ends near the armed deadline, reports the child's code, and leaves `TimedOut` false. The zero-grace descendant test ends near `Timeout`, reports exit 0, and leaves `TimedOut` false.

Review round 1 left two open suggestions, and this pass closes them. A timed stream has to push back on the child the same way CliWrap's one-slot listen channel does: one waiting line, and the next write waits. A consumer that reads slower than the child must not collect the whole run in memory, and a kill or an abandoned enumerator must cancel a write that is waiting so the child does not stay stuck. `ListenWithTimeoutAsync` still sends a timeout into that path. Everything after that branch is only the no-timeout listen loop. Caller cancellation still propagates, the exit code still comes from the exited event, `Remember` still stores it, and disposing the enumerator still swallows the second cancellation. The comment that described a timeout on that loop goes away, and the `command-result.cs` line-cap baseline drops to the new line count.

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
- [x] M1: timed stream keeps one line and waits on `WriteAsync` with CliWrap's pipe token
- [x] M2: `ListenWithTimeoutAsync` remainder is the no-timeout listen loop, and the line-cap baseline is 1339
- [x] M3: a child that exits during a positive grace period with a slow reader keeps every line, the child's code, and `TimedOut` false
- [x] M4: a timed command whose descendant holds stdout still ends at a bounded time after the child exits, with a test, and the docs match the rule
- [x] M5: with a zero grace period, a child that exits before the timeout while a descendant holds a pipe keeps the child's code and `TimedOut` false, and the deadline stays at `Timeout`

## Depends on

- 138

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)
- Version bump to 3.0.0-beta.2 rolled in: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-09)
- Implementation: 01a1bc56-2621-7af3-9055-ab81b04064d2 (2026-11-09). The grace-period growth tripped `source-file-line-cap`. The timeout session, exit capture, and timed stream moved to `command-result.timeout.cs`, the mode tests moved to `shell-builder.with-timeout.modes.cs`, and the `command-result.cs` baseline shrank from 1408 to 1354.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bc56-2621-7af3-9055-ab81b04064d2, max-turns 500, Done — 2026-11-08T18:43:25Z
- Review oracle round 1: Claude Opus 5.5 (claude-opus-5-5), body tw-implementation-review, effort 3, roster general, turn cap 200 — 2026-11-09. 2 suggestions open (M1, M2), fix pass requested. See `review/round-1/merged.md`.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 8ac9a6f8-44df-45a3-ab90-5653488e918a, max-turns 200, artifacts general.md, merged.md — 2026-11-08T18:49:47Z
- Fix pass: implement-fix-2 on 2026-11-09. M1 bounds the timed stream to one line. M2 drops the dead timeout branch in `ListenWithTimeoutAsync` and lowers the `command-result.cs` baseline from 1354 to 1339.
- Review oracle round 2: Claude Opus 5.5 (claude-opus-5-5), body tw-implementation-review, effort 2, roster general, turn cap 120 — 2026-11-09. Scope: delta `f7aed63..fa56fb5`. M1 and M2 are verified fixed. Bug M3 is open, and a fix pass is requested. See `review/round-2/merged.md`.
- Fix pass: implement-fix-3 on 2026-11-09, session 01a1bc92-3c8b-7913-a974-2d52b9a29552. M3 stops the forceful deadline when the process exits during a positive grace period, so a slow reader finishes the lines the child wrote.
- Review oracle round 3: Claude Opus 5.5 (claude-opus-5-5), body tw-implementation-review, effort 1, roster general, turn cap 80, on 2026-11-09. The scope was the delta `fa56fb5..003885a`. M3 is verified fixed. Bug M4 is open: the deadline stop leaves no bound on a timed run when a descendant holds the pipe. A fix pass is requested. See `review/round-3/merged.md`.
- Fix pass: implement-fix-4 on 2026-11-09, session 01a1bca4-a073-70f0-9fc5-eb59e8cecc55. M4 keeps a forceful deadline after the direct child exits. An exit during grace moves that deadline to `Timeout` plus two grace periods. An earlier exit leaves `Timeout` plus one grace period. A later cancel reports the child's code with `TimedOut` false.
- Review oracle round 4: Claude Opus 5.5 (claude-opus-5-5), body tw-implementation-review, effort 2, roster general, turn cap 120, on 2026-11-09. The scope was the delta `003885a..93204a3`. M4 is verified fixed for a positive grace period. Bug M5 is open: with a zero grace period, a child that exits before the timeout while a descendant holds a pipe still reports 124. The new docs say that case keeps the child's code. A fix pass is requested. See `review/round-4/merged.md`.
- Fix pass: implement-fix-5 on 2026-11-09, session 01a1bcbb-0ceb-7130-b375-ebef6e8ad2ca. M5 records an exit before the zero-grace signal and does not move the deadline. A descendant that holds a pipe is cut off at `Timeout` with the child's code and `TimedOut` false. An exit at or after that signal stays exit 124.
- Review oracle round 5: Claude Opus 5.5 (claude-opus-5-5), body tw-implementation-review, effort 2, roster general, turn cap 120, on 2026-11-09. The scope was the delta `93204a3..e33d046`. M5 is verified fixed. No new findings. Disposition is `clean`. See `review/round-5/merged.md` and `review/disposition.md`.

## Review

- Round 1: M1 and M2 fixed on this branch.
- Round 2: M3 was open. The implement-fix-3 pass stopped the forceful deadline on that exit.
- Round 3: M3 is fixed. M4 was open. This pass keeps a bounded deadline after the child exits.
- Round 4: M4 is fixed. M5 was open. This pass records an exit before the zero-grace signal and leaves the deadline at `Timeout`.
- Round 5: Done
## Notes

Filed from the 138 merge on 2026-11-09. Steve: "SIGINT ending something != timeout to me."

Why 124 was chosen originally: the runner only sees CliWrap's `OperationCanceledException` after the graceful token fires, so 124 was the only code it could report. The grace period was still useful because the child got to flush and clean up, but its answer was discarded.

Related: task 119 notes a possible CliWrap swap for a later release. This task does not need that swap, but if the exit-code capture turns out to need it, stop and record the finding rather than widening the task.

The exit code is read from the process `Exited` event. CliWrap stays the process library. A zero grace period is still exit 124 when the child is still running, because the signal and the kill are the same instant. An exit before that signal keeps the child's code. `TtyPassthroughAsync` reports exit 7 during grace and does not throw, which is the existing TTY validation exemption. `documentation/overview.md` states no package version, so it was left as it is. The githook pins in `Directory.Packages.props` stay at `2.0.0`.

## Results

A child that exits during the timeout grace period keeps its own exit code and `TimedOut` stays false. The grace-period kill is still exit 124. Unix exit 130 for an unhandled SIGINT is the child's code. Caller cancellation is unchanged. The version is `3.0.0-beta.2`. Both Unshipped files are header-only. Nothing was tagged or published.

A timed stream keeps one unread line and waits to write the next. The write uses CliWrap's pipe token, so a kill or an abandoned reader unblocks it. `ListenWithTimeoutAsync` after the timeout branch is only the no-timeout listen loop. The `command-result.cs` line-cap baseline is 1339.

A positive grace period moves that deadline to `Timeout` plus two grace periods when the direct child exits during grace. A slow reader that finishes inside the window receives every line the child wrote, the result keeps the child's exit code, and `TimedOut` stays false. An exit before the graceful signal leaves the original `Timeout` plus one grace period. A descendant that still holds a pipe is cut off at the armed deadline with the child's code and `TimedOut` false. Caller cancellation and an abandoned enumerator still cancel the drain. A zero grace period does not move the deadline. When the direct child exits before that signal, the same cut-off keeps the child's code and `TimedOut` stays false. A child that is still running when its deadline fires is still exit 124.

The three descendant probes live in `shell-builder.with-timeout.descendants.cs`. The capture file stays at 442 lines, under the 500-line cap. `command-result.cs` is still 1339 lines.

`./tools/dev-cli/dev.cs build` succeeded with 0 warnings and packed `TimeWarp.Amuru.3.0.0-beta.2.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.2.nupkg`. `./tools/dev-cli/dev.cs test` reported 839 total, 838 passed, 1 skipped (`GetCommitsAheadOfDefaultBranch_Given_`, pre-existing). `./tools/dev-cli/dev.cs check-version` reported source `3.0.0-beta.2`, latest NuGet `3.0.0-beta.1`, and one prerelease increment ahead. `ganda repo audit` passed 39, failed 0, skipped 2. The `command-result.cs` line count is still 1339. The capture file is 442 lines.

### How to validate

**Smoke**

```bash
dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.cs
dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.descendants.cs
dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-timeout.modes.cs
./tools/dev-cli/dev.cs check-version
```

**Expect**

- The capture file reports 16 passed. Exit 0 keeps `TimedOut` false and strict validation does not throw. Exit 7 throws `CommandExecutionException` and leaves `LastOutput` null. Exit 130 keeps `TimedOut` false. A child that ignores SIGINT is exit 124 and `TimedOut` true. Caller cancellation leaves `LastOutput` null. The slow-consumer test reads `START`, waits, and the done marker is still absent. After the stream ends the marker exists, the exit code is 0, and `TimedOut` is false. `StreamStdout_ChildExitsDuringGrace_Should_KeepEveryLineAndTheChildCode` reads `LINE 00` through `LINE 39`, uses a 1s timeout and a 4s grace, takes longer than that 5s deadline and less than the 9s drain bound, reports exit 0, and leaves `TimedOut` false.
- The descendants file reports 3 passed. `Capture_DescendantHoldsStdoutBeforeTimeout_Should_EndAtTheOriginalDeadline` runs `sleep 15 & echo started` with a 1s timeout and a 1s grace, ends in about 2s with stdout `started`, exit 0, and `TimedOut` false. `Capture_DescendantHoldsStdoutWithZeroGrace_Should_KeepTheChildCodeAtTimeout` uses the same command with grace 0, ends in about 1s, which is at `Timeout` and short of the 15s sleep, with stdout `started`, exit 0, and `TimedOut` false. `Capture_DescendantHoldsStdoutDuringGrace_Should_EndAtTheDrainBound` ends in about 3s, which is past the 2s original deadline and at the 3s drain bound, with exit 0 and `TimedOut` false.
- The modes file reports 14 passed. `TtyPassthroughAsync` reports exit 0, exit 7 without throwing, and exit 130, and the ignored-SIGINT case is exit 124. The pipeline's last stage reports exit 7, and strict validation throws `CommandExecutionException`.
- `check-version` prints source `3.0.0-beta.2`, latest NuGet `3.0.0-beta.1`, and that the source version is not yet published.

### Review

Round 1 ran with effort 3 and a general reviewer. The fix pass closed both suggestions on the round-1 ledger. Ledger counts after that pass: bug 0, suggestion 2 fixed, nit 0.

- M1 fixed: the timed stream uses a one-slot channel and `WriteAsync` with CliWrap's pipe token.
- M2 fixed: `ListenWithTimeoutAsync` below the timeout branch is only `ListenAsync(cancellationToken)`. The `command-result.cs` baseline is 1339.

The reviewer re-ran both timeout test files (14/14 each) before the fix. After the fix, those files report 15/15 and 14/14, `ganda repo audit` is clean, and `check-version` still shows beta.2 unpublished. Paths: `review/review-framework.md` and `review/round-1/merged.md`.

Round 2 ran with effort 2 and a general reviewer, on the fix delta `f7aed63..fa56fb5`. M1 and M2 are re-verified as fixed. The capture test file reported 15/15 before this pass, and the `command-result.cs` line count matches the 1339 baseline. A dropped enumerator unblocks a waiting write within about 1s and leaves no child behind. M3 was open: a slow reader turned a child's exit during grace into exit 124. This fix pass stops the forceful deadline on that exit. The capture file now reports 16/16, including the slow-reader grace test at about 7s. Path: `review/round-2/merged.md`.

Round 3 ran with effort 1 and a general reviewer, on the fix delta `fa56fb5..003885a`. M3 is re-verified as fixed: the capture file reports 16/16 and the modes file 14/14. M4 is a new open bug. `StopForcefulDeadline` runs on any exit of the direct child, including one before the timeout, and in every timed mode. After that, nothing bounds the run.

Probe: `sh -c "sleep 15 & echo started"` with timeout 1s and grace 1s. At `fa56fb5` it ends in 2.1s with exit 124. Before this pass it ends in 15.0s with exit 0, and it never ends if the descendant never exits.

This pass keeps the deadline. The same probe ends in about 2.1s with exit 0 and `TimedOut` false. A descendant started from the grace-period handler ends at `Timeout` plus two grace periods, about 3.1s here, with the child's code and `TimedOut` false. The slow-reader test still reads all 40 lines. Path: `review/round-3/merged.md`.

Round 4 ran with effort 2 and a general reviewer, on the fix delta `003885a..93204a3`. M4 is re-verified as fixed for a positive grace period:

- The capture file reports 18/18 and the modes file reports 14/14.
- The round-3 probe ends in 2.1s with exit 0 and `TimedOut` false.
- Strict capture, strict non-zero, and `StreamStdoutAsync` with a descendant each end at about 2.1s with the child's result.
- `command-result.cs` is still 1339 lines.

M5 is a new open bug. With a zero grace period, `NoteChildExit` returns before it records the exit. A child that exits with 0 before the timeout, while a descendant holds stdout, reports exit 124 with `TimedOut` true at 1.1s. The new `command-execution.md` row, the Design region, and the Goal restatement say that cut-off keeps the child's code.

Ledger counts:

| Severity | Open | Fixed | Wontfix |
| --- | --- | --- | --- |
| Bug | 1 | 2 | 0 |
| Suggestion | 0 | 2 | 0 |
| Nit | 0 | 0 | 0 |

Disposition is not written because M5 is open. Path: `review/round-4/merged.md`.

This pass fixes M5. `NoteChildExit` records an exit that happens before the zero-grace signal and does not call `CancelAfter`. The deadline stays at `Timeout`. The same probe with grace 0 ends in about 1.1s with stdout `started`, exit 0, and `TimedOut` false. A child that is still running at that deadline is still exit 124. The descendant probes moved to `shell-builder.with-timeout.descendants.cs` because the capture file had crossed the 500-line cap. Round 5 has not run, so disposition is not written.

Ledger counts after this pass:

| Severity | Open | Fixed | Wontfix |
| --- | --- | --- | --- |
| Bug | 0 | 3 | 0 |
| Suggestion | 0 | 2 | 0 |
| Nit | 0 | 0 | 0 |

Round 5 ran with effort 2 and a general reviewer, on the fix delta `93204a3..e33d046`. M5 is re-verified as fixed and the round raised no new findings.

- The descendants file reports 3/3. The zero-grace case took 1.18s.
- The capture file reports 16/16, the modes file 14/14, and `command-result.concurrency.cs` 5/5.
- These probes used grace 0:
  - `StreamStdoutAsync` and strict `CaptureAsync` on `sleep 15 & echo started` each ended in 1.1s with exit 0 and `TimedOut` false.
  - A `sleep 15` child that was still running at the deadline ended with exit 124 and `TimedOut` true.
- `command-result.cs` is still 1339 lines.
- The new runfile has its executable bit set.

The Goal restatement matches the original task and the rolled-in version bump, and no misread was found.

**Review disposition: `clean`.** Five rounds ran, each with a general reviewer (Claude Opus 5.5) at effort 3, 2, 1, 2, and 2. Final counts are bug 3 fixed, suggestion 2 fixed, nit 0, with 0 open and 0 wontfix. There were no escalations. Paths: `review/review-framework.md`, `review/round-5/merged.md`, and `review/disposition.md`.
