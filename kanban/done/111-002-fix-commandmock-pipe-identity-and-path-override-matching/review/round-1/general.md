# Round 1 — general
**Date:** 2026-09-16
**Scope reviewed:** branch `task/111-002-fix-commandmock-pipe-identity-and-path-override-ma` vs `origin/feature/overnight-amuru` (product: `source/timewarp-amuru/core/{command-result,command-extensions}.cs`, `source/timewarp-amuru/testing/{CommandMock,MockState,MockScope}.cs`, tests in `tests/timewarp-amuru/single-file-tests/core/command-mock.cs`)

## Summary

Parent 111 findings **M6–M8** are fixed with low residual risk. Piped `CommandResult` still carries no mock identity (`command-result.cs:435`); `ResolveMockSetup` no longer falls back to last-stage CliWrap identity and instead Strict-throws a pipe-specific message or Loose-runs the real pipeline (`command-result.cs:91-107`). `MockScope` dispose tombstones `MockState` so leftover `AsyncLocal` is ignored (`CommandMock.cs:29`, `:55-61`; `MockState.cs:46-54`). Logical mock executable is captured before `GetCommandPath` and before the `.cs` rewrite (`command-extensions.cs:64-70`). Targeted `command-mock.cs` regressions for PipedCommand / DisposedFromOtherContext / PathOverride all passed.

## Issues

## Verified-clean notes

- **M6:** `InternalCommand == null` short-circuits before the pipe branch, so `NullCommandResult` does not throw the pipe message (`command-result.cs:91-94`). Loose test asserts real `World` (not `MOCKED`); Strict asserts message contains `Pipe compositions bypass mock matching`.
- **M7:** `State` / `IsEnabled` / `Setup` / `VerifyCalled` / `CallCount` / `Reset` all go through disposed-aware `State`. Cross-context dispose test covers Setup-not-enabled, real execution, and re-`Enable`.
- **M8:** Path-override test with a real temp executable + `Setup("git", "status")` returns the mocked stdout under Strict, proving the override path is not the mock key.
- Pre-existing missing final newlines on `CommandMock.cs` / `MockState.cs` / `MockScope.cs` are unchanged vs `origin/feature/overnight-amuru`; not introduced here.
- Did not reopen 089 or M9; siblings 111-003…111-005 out of scope.
