# Round 1 — general
**Date:** 2026-10-05
**Scope reviewed:** branch vs master, source/ and tests/

## Summary

The change tracks configured stdin on `CommandResult`, keeps it in `PassthroughAsync`, refuses it in `TtyPassthroughAsync`, records it on mock calls, and adds a `Pipe` overload that takes `CommandOptions`. The string-stdin path is correct and tested. One gap: stdin that comes from a pipe composition is still overwritten or silently dropped by both passthrough modes.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/core/command-result.cs:238 (PassthroughAsync), :308 (TtyPassthroughAsync), :524 (PipeCore)
- Description: CliWrap's `source | target` returns `target.WithStandardInputPipe(PipeSource.FromCommand(source))`, so a piped CommandResult's stdin is the upstream stage. `PipeCore` sets `ConfiguredStandardInput` only from the upstream string, which is null for `Shell.Run("ls").Pipe("fzf")` and for `Fzf.FromCommand(...)`. `PassthroughAsync` then replaces stdin with the console and the upstream stage never runs. `TtyPassthroughAsync` starts only the last stage's executable with inherited stdin, so the upstream stage is silently dropped. The task names `FromCommand` as an affected builder and requires that the drop is never silent.
- Suggestion: Mark pipe compositions as owning stdin. PassthroughAsync must not open console stdin for a pipeline. TtyPassthroughAsync must throw for a pipeline. Add tests for both.
- Status: open
