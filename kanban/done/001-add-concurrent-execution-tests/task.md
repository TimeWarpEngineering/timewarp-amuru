# Concurrency and large-output contracts for command execution

## Description

Prove and document two contracts of `CommandResult` / `CommandOutput` (`source/timewarp-amuru/core/`) that the library claims but does not test. Task **002** (long output tests) is folded into this card and archived with a pointer.

**Rewritten 2026-10-07.** The original cards predate the Amuru rename (`TimeWarp.Cli`, `GetStringAsync`, "caching"). Current facts: there is no result caching in core. `CommandResult` is documented as reusable and thread-safe by immutability, but task 044 (2026-10-06) added `CommandResult.LastOutput { get; private set; }`, written at the end of every execution (`command-result.cs:134`, set at ~315/328). That is per-instance mutable state: two concurrent executions of the same instance race on it, last writer wins, undocumented. `CommandOutput` buffers all lines in a `List<OutputLine>`, so capture is O(output); `StreamStdoutAsync` / `StreamStderrAsync` exist to avoid that; nothing proves either side. The only concurrency test today covers `CliConfiguration` statics.

## Part A: concurrency

### Requirements
- Tests under `tests/timewarp-amuru/single-file-tests/core/command-result.concurrency.cs`:
  - N (e.g. 32) concurrent `CaptureAsync` on **one** `CommandResult` instance (a cheap command like `printf` with a per-call distinguishing env var is not possible on a shared instance, so use a command whose output is deterministic and assert every result is identical and complete).
  - Concurrent `Pipe(...)` compositions built from one shared source `CommandResult`, each producing a distinct downstream; assert isolation.
  - Mixed modes concurrently on one instance: `CaptureAsync`, `RunAsync`, `StreamStdoutAsync` consumer; assert no exceptions and correct outputs.
  - Concurrent `WithTimeout` executions where some time out and some do not; assert each caller's own `CommandOutput` is right regardless of `LastOutput`.
  - `CommandMock` under concurrency: `AsyncLocal<MockState>` isolation across parallel async flows (two concurrent flows with different mock setups do not see each other's setups).
- **Decision on `LastOutput`:** pick one and implement: (a) document it as "the most recent execution on this instance; not meaningful when the same instance executes concurrently; prefer the returned `CommandOutput`"; or (b) remove it in favor of the returned result (it was added in 044 mainly so strict-validation timeouts could expose the partial result; if removing, carry that information on the `TimeoutException` instead). Recommendation: (b) if nothing in the repo reads it besides tests; otherwise (a). Record the choice, and if (b), the `*REMOVED*` line in `public-api/PublicAPI.Unshipped.txt` and the release-note line.
- Document the thread-safety contract in the `command-result.cs` Design region and the execution reference doc: instances are immutable aside from the decision above; execution methods may be called concurrently.

## Part B: large output

### Requirements
- Define the contract and write it down (`documentation/developer/reference/command-execution.md`): `CaptureAsync` / `RunAndCaptureAsync` buffer the full output in memory (lines + combined strings); `StreamStdoutAsync` / `StreamStderrAsync` are constant-memory per line; `RunAsync` / `PassthroughAsync` do not buffer. State that there is no size limit, only memory.
- Tests under `tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs` (Linux CI; generate output with `seq` or a tiny C#/python child, no fixtures on disk):
  - Capture of ~2 million lines (`seq 1 2000000`, ~15 MB): completes, `GetLines().Length` is exact, first/last lines correct, exit 0.
  - Streaming the same output: completes, count exact, and peak managed memory (`GC.GetTotalMemory` / `GC.GetTotalAllocatedBytes` delta, or `Process.GetCurrentProcess().WorkingSet64` sampled) stays well below the captured size (assert a generous ratio, e.g. < 25% of the capture's delta, to avoid flakiness).
  - One very long single line (~10 MB without newline): capture returns it intact; stream yields one line.
  - Interleaved stdout+stderr at volume (`OutputLines` ordering contract: all stdout then all stderr) holds at ~200k lines each.
  - `RunAsync` with ~2 million lines to the console does not deadlock (redirect test stdout to a null sink or run the child with output to `/dev/null` via the shell to keep CI logs small).
  - Mark the heavy cases with the suite's `[Timeout]` attribute; keep the whole file under ~30 s on CI.
- No behavior change expected. If a test exposes quadratic string building or a deadlock, fix it in core and add it to Results.

## Checklist

- [x] `command-result.concurrency.cs`: shared-instance capture ×N, concurrent pipes, mixed modes, timeouts, mock AsyncLocal isolation
- [x] `LastOutput` decision implemented and documented (PublicAPI Unshipped + release-note line if removed)
- [x] Thread-safety contract written in the Design region and the execution doc
- [x] `command-result.large-output.cs`: 2M-line capture, streaming memory ratio, 10 MB single line, interleaved ordering at volume, console no-deadlock
- [x] Large-output contract written in the execution doc
- [x] Any core fix found by the tests, with its own regression test
- [x] `./bin/dev build` 0 warnings / 0 errors; full runner green; file runtime recorded; `ganda repo audit` clean
- [x] Results: decision on `LastOutput`, measured memory ratios, test runtimes, any fixes

## Notes

- 002 archived 2026-10-07 as folded here.
- Reference: task 044 Results (why `LastOutput` exists), task 090 (result-vs-throw contract), task 121 (mock parity).
- Keep tests deterministic: no wall-clock assertions beyond `[Timeout]`, generous memory ratios, Linux-only commands are fine (CI is ubuntu) but guard with `OperatingSystem.IsLinux()` and skip otherwise.

## Results

Choice (a): `CommandResult.LastOutput` stays. Nothing outside tests and docs reads it, but `StreamStdoutAsync`, `StreamStderrAsync`, `StreamCombinedAsync`, and `StreamToFileAsync` do not return `CommandOutput`. `LastOutput` is their completion record (exit code, timeout, runtime), and strict timeouts still publish the partial result there before `TimeoutException`. Removing it would drop that contract. No `*REMOVED*` line and no release-note line.

The property is the most recent execution on the instance. It is not meaningful when that instance executes concurrently (last writer wins). Callers use the returned `CommandOutput`. The instance is otherwise immutable, and execution methods may run concurrently. That contract is in the `command-result.cs` Design region, the type and property docs, and `documentation/developer/reference/command-execution.md`.

`CaptureAsync` keeps arrival order. `RunAndCaptureAsync` rebuilds `OutputLines` from separate strings, so those lines are all stdout, then all stderr. The volume test asserts that path at 200,000 lines on each stream.

No core behavior change. The tests did not show quadratic string building or a deadlock.

Memory for `seq 1 2000000`, standalone `dotnet run` of `command-result.large-output.cs` (second run):

- Capture retained delta: 400,062,728 bytes (`GC.GetTotalMemory(true)` while the `CommandOutput` and `GetLines()` array were rooted)
- Stream retained delta after `GC.GetTotalMemory(true)`: 104,728 bytes, ratio 0.0003
- Stream unforced peak (`GC.GetTotalMemory(false)`): 49,913,400 bytes, ratio 0.1248
- The assertion is retained delta under 25% of the capture delta. The unforced peak on that run was also under 25%. The full suite run measured capture 416,717,264, retained 0, unforced ratio 0.0146

Runtimes, standalone `dotnet run` wall clock:

- `command-result.concurrency.cs`: 0.79 s (5 passed; slowest test 0.36 s)
- `command-result.large-output.cs`: 5.65 s (4 passed; capture+stream 4.10 s, 10 MB line 0.25 s, interleaved 0.56 s, `RunAsync` 0.43 s)

Full runner `./bin/dev test`: 732 passed, 1 skipped (pre-existing `GetCommitsAheadOfDefaultBranch`), 0 failed. `Concurrency_Given_` 5/5, `LargeOutput_Given_` 4/4. `./bin/dev build`: 0 warnings, 0 errors. `ganda repo audit`: 31 passed, 0 failed.

### Review disposition

- Effort 2 (by-diff), roster: general (Sonnet subagent) plus oracle verification; 2 rounds.
- Final counts: bug 0; suggestion 1 fixed; nit 1 fixed, 1 wontfix; 0 open.
- Outcome: **accepted-exceptions**. M1 fixed (`GC.KeepAlive` on the capture baseline so the JIT cannot collect it before the second sample). M2 fixed (the interleaved child now alternates stdout/stderr writes). M3 wontfix (no `python3`/`seq` probe; the brief allows them on Linux CI and a missing binary should fail loudly).
- After fixes: large-output file 4 passed (7.9 s), `./bin/dev build` 0 warnings, `./bin/dev test` 732 passed / 1 skipped / 0 failed, `ganda repo audit` clean.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

1. `dotnet run tests/timewarp-amuru/single-file-tests/core/command-result.concurrency.cs`
2. `dotnet run tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs`
3. `./bin/dev build`
4. `./bin/dev test`

Expect:

- Concurrency file: 5 passed. Shared capture, pipes, mixed modes, timeouts, and mock flows each return that caller's own output.
- Large-output file: 4 passed in well under 30 s. The printed `retainedRatio` is under 0.25. `seq` capture length is 2000000, the 10 MB line length is 10000000, interleaved `OutputLines` are stdout then stderr, and `RunAsync` exits 0.
- Build: 0 warnings, 0 errors.
- Full runner: 0 failed. The one skip is `GetCommitsAheadOfDefaultBranch_Given_`.

## Session

- Created: 2025-12-12
- Rewritten and 002 folded in: 522eb63d (2026-10-07)
- Implementation: 2026-10-07
- Review: 2026-10-07, effort 2, general reviewer subagent a1bae4f113a3a928e; disposition accepted-exceptions
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-07T13:07:12Z
