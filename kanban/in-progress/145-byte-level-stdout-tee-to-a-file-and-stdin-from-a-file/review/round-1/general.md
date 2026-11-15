# Round 1 — general
**Date:** 2026-11-15
**Scope reviewed:** branch `task/145-byte-level-stdout-tee-to-a-file-and-stdin-from-a` vs `master` (merge-base `06ae239`). `ShellBuilder.WithStandardOutputTee` and `WithStandardInputFile`, the `CommandResult` execution paths, the two new runfiles, docs, and the `3.0.0-beta.5` bump.

## Summary

`WithStandardOutputTee` copies the child's stdout bytes into a file as each chunk arrives, and the execution method still returns or streams its usual result. `WithStandardInputFile` attaches `PipeSource.FromFile`. Stderr is never given the tee target. `TtyPassthroughAsync` and `AsJsonRpcClient` refuse both options. CliWrap 3.10.5 `PipeTarget.Merge` replicates each chunk onto a separate substream per target, and `ListenAsync` merges the command's existing stdout pipe with its line observer, so the tee stays in front of the line-based modes (`CaptureAsync`, `RunAsync`, `RunAndCaptureAsync`, `SelectAsync`, `StreamToFileAsync`, `PassthroughAsync`, and both listen paths, including the timeout stream). A chunk already read is written and flushed with `CancellationToken.None`. The new runfiles sit under the multi-file runner glob, so the recorded `./bin/dev test` (875 total, 874 passed, 1 skipped) covers them, along with build (0 warnings) and a passing audit. No product defect.

## Issues

None.
