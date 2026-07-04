# Finalize error handling contract for validation and silent success

## Description

The 1.0 error-handling contract is self-contradictory today, and whichever way it's resolved becomes a breaking change the day 1.0 ships — it must be decided now.

1. **The documented default validation is a lie.** Docs on `core/command-options.cs:36-38` say "defaults to CommandResultValidation.None for graceful error handling", but `CommandExtensions.Run` never sets validation and `CommandOptions.ApplyTo` only applies it when `Validation.HasValue`, so the effective default is CliWrap's `ZeroExitCode` — any non-zero exit throws `CommandExecutionException`. `RunAsync`'s `Task<int>` return and `CommandOutput.Success`/`ExitCode` are unreachable dead API unless the caller knows to add `WithNoValidation()`. Example: `Shell.Builder("grep").WithArguments("x", "file").RunAsync()` throws when grep exits 1 (no match).
2. **Silent-success degradation.** Null/invalid commands report success: null command `RunAsync` returns 0, `CaptureAsync` returns `Success == true` (`core/command-result.cs:315-318,447-450`), and `Pipe` has a bare `catch` swallowing all exceptions into `NullCommandResult` which also "succeeds" (`core/command-extensions.cs:52-60`). A typo'd executable or failed pipeline stage reports success while doing nothing.

**BLOCKER for 1.0** — contract decision, related to task 041 (which already tracks exposing `WithZeroExitCodeValidation` and making `None` the true default).

## Checklist

- [ ] Decide: default validation = `None` (matching docs and the `Success`/`ExitCode` API shape) or `ZeroExitCode` (current effective behavior). Recommendation from review: make `None` the true default per task 041's plan
- [ ] Implement the decided default explicitly in `CommandOptions.ApplyTo`/`CommandExtensions.Run` so it no longer depends on CliWrap's internal default
- [ ] Implement task 041's `WithZeroExitCodeValidation()` on `CommandOptions` and `ShellBuilder` so strict validation stays opt-in-able
- [ ] Decide and document the invalid/null-command contract: keep "graceful degradation" (document loudly) or reserve a distinct failure signal (`Success == false`, distinct exit code) for never-ran commands
- [ ] Fix `Pipe`'s bare `catch` so pipeline construction failures are at least distinguishable from success (`core/command-extensions.cs:52-60`)
- [ ] `TtyPassthroughAsync` ignores configured `Validation` entirely (`core/command-result.cs:139-211`) — align or document
- [ ] Align mock exit-code semantics with the decided contract (see task 089)
- [ ] Update docs (`command-options.cs` XML docs, readme) to match the decided contract
- [ ] Tests asserting the default and both opt-in paths

## Notes

Found by multi-agent release review (2026-07-04). Supersedes/extends the inconsistency already noted in task 041 — do them together. Paths relative to `source/timewarp-amuru/`.
