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

- [ ] `command-result.concurrency.cs`: shared-instance capture ×N, concurrent pipes, mixed modes, timeouts, mock AsyncLocal isolation
- [ ] `LastOutput` decision implemented and documented (PublicAPI Unshipped + release-note line if removed)
- [ ] Thread-safety contract written in the Design region and the execution doc
- [ ] `command-result.large-output.cs`: 2M-line capture, streaming memory ratio, 10 MB single line, interleaved ordering at volume, console no-deadlock
- [ ] Large-output contract written in the execution doc
- [ ] Any core fix found by the tests, with its own regression test
- [ ] `./bin/dev build` 0 warnings / 0 errors; full runner green; file runtime recorded; `ganda repo audit` clean
- [ ] Results: decision on `LastOutput`, measured memory ratios, test runtimes, any fixes

## Notes

- 002 archived 2026-10-07 as folded here.
- Reference: task 044 Results (why `LastOutput` exists), task 090 (result-vs-throw contract), task 121 (mock parity).
- Keep tests deterministic: no wall-clock assertions beyond `[Timeout]`, generous memory ratios, Linux-only commands are fine (CI is ubuntu) but guard with `OperatingSystem.IsLinux()` and skip otherwise.

## Session

- Created: 2025-12-12
- Rewritten and 002 folded in: 522eb63d (2026-10-07)
