## Resolution

**Status:** ✅ RESOLVED (2026-03-18)

**Fix:** Updated `TimeWarp.Jaribu` from `1.0.0-beta.8` to `1.0.0-beta.12`.

The newer Jaribu version is binary-compatible with `TimeWarp.Terminal 1.0.0-beta.7`.

**Verification:** `dev test` now completes successfully with all tests passing.

---

## Symptom

Running `dev test` crashes during test result rendering with:

`System.MissingMethodException: Method not found: 'Void TimeWarp.Terminal.TerminalTableExtensions.WriteTable(TimeWarp.Terminal.ITerminal, TimeWarp.Terminal.Table)'`

Stack trace shows failure in:
- `TimeWarp.Jaribu.TestHelpers.PrintResultsTable(...)`
- called from `TimeWarp.Jaribu.TestRunner.RunAllTests(...)`
- from `tests/timewarp-amuru/multi-file-runners/run-tests.cs:9`

## Root Cause

Binary compatibility mismatch between `TimeWarp.Jaribu` and the resolved `TimeWarp.Terminal` assembly at runtime.

- `TimeWarp.Jaribu 1.0.0-beta.8` calls `TerminalTableExtensions.WriteTable(ITerminal, Table)` with **void** return signature.
- Resolved runtime `TimeWarp.Terminal 1.0.0-beta.7` exposes `WriteTable(ITerminal, Table)` with **ITerminal** return signature.

Because return type is part of CLR method signature, the runtime cannot bind Jaribu’s callsite, producing `MissingMethodException`.

## Evidence Chain

1. **Observed runtime failure**
   - `dev test` output includes exact `MissingMethodException` for `WriteTable(ITerminal, Table)` with expected return type `Void`.

2. **Project resolves these package versions for tests**
   - `Directory.Packages.props`:
     - `TimeWarp.Jaribu` = `1.0.0-beta.8`
     - `TimeWarp.Terminal` = `1.0.0-beta.7`
   - Runtime deps file from generated test runner confirms same:
     - `run-tests.deps.json` lines 14-15, 324-329, 337-343.

3. **Jaribu runtime call expects `void WriteTable(...)`**
   - IL/decompilation inspection (via subagent): Jaribu callsite is `call void ... TerminalTableExtensions::WriteTable(...)`.

4. **Resolved Terminal assembly has non-void signature**
   - API inspection (via subagent): Terminal method is `ITerminal WriteTable(ITerminal terminal, Table table)`.

5. **Signature mismatch explains exact exception**
   - CLR method binding fails when callsite expects `void` but target method returns `ITerminal`.

## Affected Scope

- Any code path in this repo that executes Jaribu’s table-printing helpers with current package combination.
- Likely affects all `dev test` runs (local and CI) until package/API compatibility is aligned.

## Reproduction Steps

1. From repo root, run `dev test`.
2. Tests begin and some pass.
3. Crash occurs during summary table output with `MissingMethodException` for `TerminalTableExtensions.WriteTable`.

## Contributing Factors

- Central package management pins `TimeWarp.Terminal` to `1.0.0-beta.7` while Jaribu binary expects older Terminal API shape.
- Pre-release package ecosystem with rapid API changes increases ABI drift risk.

## Related History

- `Directory.Packages.props` currently pins:
  - `TimeWarp.Jaribu` `1.0.0-beta.8`
  - `TimeWarp.Terminal` `1.0.0-beta.7`
- `tests/timewarp-amuru/Directory.Build.props` references both packages for test execution.
