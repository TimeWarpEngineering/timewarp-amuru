# Round 2 — general
**Date:** 2026-10-05
**Scope reviewed:** fix delta for M1 (command-result.cs IsPipeline, passthrough and tty tests)

## Summary

`PipeCore` now marks compositions with `IsPipeline`. `PassthroughAsync` opens console stdin only when no string was configured and the command is not a pipeline. `TtyPassthroughAsync` refuses pipelines with a clear message. The Design header and XML docs describe both cases. The new tests pass: `printf | sort -o` through PassthroughAsync writes the sorted upstream output, and `echo | cat` through TtyPassthroughAsync throws. The full suite passes (567 passed, 1 skipped). No new issues found.

## Issues

None.
