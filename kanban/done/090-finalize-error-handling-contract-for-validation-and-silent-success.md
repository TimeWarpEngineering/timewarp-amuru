# Finalize error handling contract for validation and silent success

## Description

The 1.0 error-handling contract was self-contradictory: docs promised validation defaulted to `None` while CliWrap's `ZeroExitCode` default actually applied (making `CommandOutput.Success`/`ExitCode` unreachable dead API), and never-ran commands reported success.

**Resolved 2026-07-05.** Contract decided and implemented:

1. **Default validation is `None`** — non-zero exit codes are reported via `ExitCode`/`Success`, never thrown. `CommandOptions.ApplyTo` now always applies the resolved validation so CliWrap's internal default can't leak in.
2. **Strict validation is opt-in** via `WithZeroExitCodeValidation()` on both `CommandOptions` and `ShellBuilder` (implements task 041's plan).
3. **Never-ran commands report failure**: `CommandResult.NeverRanExitCode` (`-1`) via `RunAsync`, `CaptureAsync`/`RunAndCaptureAsync` (`Success == false`), and `PassthroughAsync`/`TtyPassthroughAsync` (`IsSuccess == false`). A typo'd/empty command or failed `Pipe` composition is no longer mistaken for success; composition still never throws.
4. **`TtyPassthroughAsync`** documented as exempt from validation (raw process boundary; inspect the result).
5. **Mock/real alignment**: with the default now `None`, mocked non-zero exit codes match real behavior. (Mocks still ignore opt-in strict validation — tracked as part of task 089.)

## Checklist

- [x] Default = `None`, applied explicitly in `CommandOptions.ApplyTo`
- [x] `WithZeroExitCodeValidation()` on `CommandOptions` and `ShellBuilder`
- [x] Never-ran contract: `NeverRanExitCode` constant; all null-command paths report failure
- [x] `Pipe` failures distinguishable (NullCommandResult now reports failure when executed)
- [x] `TtyPassthroughAsync` validation exemption documented
- [x] XML docs updated (`command-options.cs`, `command-result.cs` design regions)
- [x] Tests: default no-throw, strict opt-in throw, never-ran failure for Capture/Passthrough/Tty (suite 358/359 green, 1 known env skip)
- [ ] readme validation examples — rides with task 102 (readme rewrite), noted there

## Notes

Found by multi-agent release review (2026-07-04); decision per the advice round approved 2026-07-05. Task 041 is implemented by this work. Paths relative to `source/timewarp-amuru/`.
