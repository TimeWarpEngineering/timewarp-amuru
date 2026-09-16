# Fix CaptureAsync RunTime blanks and SelectAsync validation

## Description

Parent **111** round-1 findings **M1–M5**. Core capture/select paths drift from the documented `CommandOutput` contract.

Pinned tree: `fbd5d276fc5a936136a55d981fc121a23b991493`. Evidence lives in parent `kanban/in-progress/111-complete-detailed-code-review-of-timewarpamuru/review/round-1/merged.md`.

## Requirements

- **M1 (bug)** `core/command-result.cs:520-526` and `:569-570` — set `CommandOutput.RunTime` from CliWrap’s result on `RunAndCaptureAsync` and `CaptureAsync` (passthrough/TTY already do). Add a capture-path assertion.
- **M2 (bug)** `core/command-output.cs:172-188` — string constructor drops whitespace-only lines; `SplitMockLines` (`command-result.cs:127-128`) uses `RemoveEmptyEntries`. Align with `SplitLines` so `RunAndCaptureAsync` and mocks preserve interior blanks like `CaptureAsync`.
- **M3 (bug)** `core/command-result.cs:349-363` — `SelectAsync` catch-all swallows `WithZeroExitCodeValidation()` failures. Let validation/command-execution failures propagate; only TTY is documented as exempt.
- **M4 (suggestion)** six `ExecuteAsync` awaits on `CommandTask` omit `ConfigureAwait(false)` (`command-result.cs:199,351,465,520,569,731`). CA2007 does not see `CommandTask`.
- **M5 (suggestion)** `command-options.cs:60,111,126` — copy `EnvironmentVariables` in every `With*` (do not alias the dictionary).

Do not reopen 090–092 / 097 unless the defect is still present after these fixes.

## Checklist

- [x] M1 RunTime on CaptureAsync / RunAndCaptureAsync + test
- [x] M2 string ctor + SplitMockLines preserve interior blanks + test
- [x] M3 SelectAsync does not swallow zero-exit-code validation
- [x] M4 ConfigureAwait(false) on CommandTask awaits
- [x] M5 EnvironmentVariables copied in With* methods
- [x] `## Results` + `### How to validate`
- [x] Implementation review (effort 1 general): round 1 + fix M1 newline + round 2 + disposition clean

## Notes

Parent: **111**. Source: `review/round-1/core-engine.md`.

### Implementation review 2026-09-16

Kitchen: `review/`. Effort 1 general, 2 rounds, disposition **clean**. See Results.

## Session

- Implementer: grok session `01a0a77a-093e-7771-b995-bd2c2994cb99` (2026-09-16)
- Review oracle: grok session `01a0a781-c37f-71b3-8b10-84ee1e2fc14b` (2026-09-16)

## Results

M1–M5 from parent 111 round-1 `review/round-1/core-engine.md` / `merged.md`. Capture and select paths now match the documented `CommandOutput` contract. 090–092 / 097 were not reopened.

- **M1:** `CaptureAsync` and `RunAndCaptureAsync` set `CommandOutput.RunTime` from CliWrap’s result (mocks and `Empty` stay at zero).
- **M2:** String constructor and `SplitMockLines` use `SplitLines`, so interior blanks (including whitespace-only lines) are preserved on `RunAndCaptureAsync` and mock capture/stream, matching `CaptureAsync`.
- **M3:** `SelectAsync` rethrows `CliWrap.Exceptions.CommandExecutionException` (zero-exit-code validation / command-execution failure). Cancellation still propagates. Unexpected runtime failures still degrade to `""`. Only `TtyPassthroughAsync` remains validation-exempt.
- **M4:** All six `CommandTask.ExecuteAsync` awaits use `.ConfigureAwait(false)`.
- **M5:** `WithWorkingDirectory`, `WithNoValidation`, and `WithZeroExitCodeValidation` copy `EnvironmentVariables` instead of aliasing the dictionary.

**Files changed**

- `source/timewarp-amuru/core/command-result.cs`
- `source/timewarp-amuru/core/command-output.cs`
- `source/timewarp-amuru/core/command-options.cs`
- `tests/timewarp-amuru/single-file-tests/core/shell-builder.capture-async.cs`
- `tests/timewarp-amuru/single-file-tests/core/shell-builder.run-and-capture-async.cs` (new)
- `tests/timewarp-amuru/single-file-tests/core/command-output.get-lines.cs`
- `tests/timewarp-amuru/single-file-tests/core/command-mock.cs`
- `tests/timewarp-amuru/single-file-tests/core/shell-builder.select-async.cs`
- `tests/timewarp-amuru/single-file-tests/core/command-options.environment-variables.cs` (new)

**Decisions**

- Shared `CommandOutput.SplitLines` (now `internal`) rather than duplicating split logic in `SplitMockLines`.
- Kept SelectAsync graceful-degradation `catch` for unexpected failures; only validation/execution exceptions and cancellation propagate (core-engine Issue 3 first option, not a new TTY-style exemption).

**Tests:** `cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs` → **490 passed, 0 failed, 1 skipped** (pre-existing skip on `GetCommitsAheadOfDefaultBranch_Given_`). Targeted single-file runs for the new assertions also passed.

### Review disposition

- **Outcome:** clean
- **Rounds:** 2
- **Effort / roster:** 1, general only
- **Final counts:** bug 0/0/0 open/fixed/wontfix; suggestion 0/0/0; nit 0 open, 1 fixed, 0 wontfix
- **Final open count:** 0
- Round 1: parent 111 M1–M5 confirmed; **M1** missing trailing newline on `command-options.cs` (`insert_final_newline`)
- Fix on this task id (no sibling apply-review task): append LF to `command-options.cs`
- Round 2: M1 confirmed fixed; no new findings
- Paths: `review/review-framework.md`, `review/round-1/{general,merged}.md`, `review/round-2/{general,merged}.md`, `review/disposition.md`

### How to validate

**Smoke**

```bash
dotnet run tests/timewarp-amuru/single-file-tests/core/shell-builder.capture-async.cs -- --filter-method CaptureRunTime
dotnet run tests/timewarp-amuru/single-file-tests/core/shell-builder.run-and-capture-async.cs
dotnet run tests/timewarp-amuru/single-file-tests/core/command-output.get-lines.cs -- --filter-method StringConstructor
dotnet run tests/timewarp-amuru/single-file-tests/core/command-mock.cs -- --filter-method InteriorBlank
dotnet run tests/timewarp-amuru/single-file-tests/core/shell-builder.select-async.cs -- --filter-method ZeroExitCodeValidation
dotnet run tests/timewarp-amuru/single-file-tests/core/command-options.environment-variables.cs
```

**Expect**

- CaptureAsync / RunAndCaptureAsync sleep tests: exit 0 and `RunTime.TotalMilliseconds > 50`.
- String constructor, mock stream/capture, and RunAndCaptureAsync interior-blank tests: three lines with an empty (or whitespace-only) middle line.
- SelectAsync + `WithZeroExitCodeValidation()` on a failing `ls`: throws `CliWrap.Exceptions.CommandExecutionException` (does not return `""`).
- CommandOptions With* tests: mutating the original `EnvironmentVariables` dictionary does not change the derived instance.

**Automated**

```bash
cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs
# expect: Failed: 0 (this session: Passed 490, Skipped 1)
```

If a single-file run disagrees with source you just edited, clear the runfile cache: `rm -rf ~/.local/share/dotnet/runfile/<test-name>-*`.
