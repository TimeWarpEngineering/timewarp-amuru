# Round 1 — general
**Date:** 2026-10-07
**Scope reviewed:** branch task/001-add-concurrent-execution-tests vs master

## Summary

The tests cover each Part A and Part B requirement, and the doc and Design-region claims I checked match the code. Capture uses arrival order, `RunAndCaptureAsync` builds `CommandOutput` from separate strings (stdout then stderr), `RunAsync` and `PassthroughAsync` do not buffer, and `LastOutput` is written by `Remember` and `RememberTimeout`. I found no incorrect behavior. I found one flakiness risk in the capture-size baseline and two small test-fidelity points.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs:209
- Description: `CaptureTwoMillionAsync` measures the capture delta with `GC.GetTotalMemory(true)` after the last use of `output` and `lines`. Nothing keeps them rooted. In an optimized (Release) JIT build the JIT may treat both as dead at that point. The collection could then reclaim them, and `captureDelta` could come out near zero. That would fail `captureDelta.ShouldBeGreaterThan(1_000_000)` or make the 25% ratio meaningless. The Results say the delta was measured "while rooted", but the code does not guarantee that. The 400 MB measured so far suggests the current build config keeps them alive.
- Suggestion: Add `GC.KeepAlive(output); GC.KeepAlive(lines);` immediately after `long after = GC.GetTotalMemory(true);`.
- Status: open

### Issue 2 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs:38
- Description: `InterleavedScript` writes all stdout lines and then all stderr lines, and the test uses `RunAndCaptureAsync`. The test name and Design comment say "interleaved", but the output is not interleaved, and `RunAndCaptureAsync` orders stdout then stderr by construction (`CommandOutput(string, string, int)`). The test only proves volume and the documented ordering. It does not check that `CaptureAsync` at volume keeps both streams complete. The task requirement ("all stdout then all stderr" at ~200k each) is met.
- Suggestion: Alternate stdout and stderr writes in the script so the test is truly interleaved. Optionally add a `CaptureAsync` count check for stdout and stderr at the same volume. Otherwise rename the test, for example `RunAndCapture_Should_OrderStdoutBeforeStderrAtVolume`.
- Status: open

### Issue 3 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/core/command-result.large-output.cs:163
- Description: The Linux guard covers only the OS check. These tests also need `python3` (long-line and interleaved tests) and `seq` (coreutils). A Linux host without `python3` fails with a launch error instead of skipping. This is not a problem on the ubuntu CI image.
- Suggestion: Either accept it as a documented assumption in the Design region, or fall back to a shell child such as `head -c 10000000 /dev/zero | tr '\0' x`.
- Status: open
