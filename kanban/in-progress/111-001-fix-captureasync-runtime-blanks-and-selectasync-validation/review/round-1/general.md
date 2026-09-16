# Round 1 — general
**Date:** 2026-09-16
**Scope reviewed:** product `source/timewarp-amuru/core/{command-result,command-output,command-options}.cs` + tests under `tests/timewarp-amuru/single-file-tests/core/` (vs `origin/feature/overnight-amuru`)

## Summary

Parent 111 findings M1–M5 are implemented cleanly: capture paths set `RunTime`, the string constructor and `SplitMockLines` share `SplitLines` so interior blanks survive, `SelectAsync` rethrows `CommandExecutionException` while keeping graceful degradation for unexpected failures, all six `CommandTask` awaits use `ConfigureAwait(false)`, and the three previously aliasing `With*` methods copy `EnvironmentVariables`. Smoke coverage for those fixes passed; CaptureAsync’s real `PipeTarget.ToDelegate` path still preserves interior blanks via the existing GetLines test. Residual risk is low — one EditorConfig nit on a touched file.

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-amuru/core/command-options.cs:166
- Description: The file still has no trailing newline after the closing brace (`insert_final_newline = true` in root `.editorconfig`). Pre-existing on the base branch, but this change appends `CopyEnvironmentVariables` at EOF and leaves the violation in place; `git diff` reports `\ No newline at end of file`.
- Suggestion: Add the final newline when landing the follow-up (or as a one-line hygiene fix on this branch).
- Status: open
