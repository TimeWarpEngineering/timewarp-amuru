# Fix CommandMock pipe identity and path-override matching

## Description

Parent **111** round-1 findings **M6–M8**. Documented mock contracts are wrong at the Pipe boundary and at `CliConfiguration` path overrides.

Pinned tree: `fbd5d276fc5a936136a55d981fc121a23b991493`. Evidence: parent `review/round-1/merged.md`.

## Requirements

- **M6 (bug)** `core/command-result.cs:93` and `:413` — Pipe currently drops `MockExecutable`/`MockArguments` then falls back to last-stage CliWrap identity. Under Loose, `Setup("grep", "World")` can match `echo … | grep World` and skip the left stage. When mock identity is null (piped composition), do not fall back to CliWrap identity. Strict: pipe-specific throw. Loose: run the real pipeline. Regression test required.
- **M7 (suggestion)** `testing/CommandMock.cs:49` — disposing the scope from another async context cannot clear the originating `AsyncLocal`. Tombstone `MockState` on dispose so leftover AsyncLocal is ignored.
- **M8 (suggestion)** `core/command-extensions.cs:64-68` — capture logical mock executable **before** `CliConfiguration.GetCommandPath` so `Setup("git")` still matches a path-overridden git.

Do not reopen **089** (ordinary-path argument matching is fixed). M9 (Throws-then-Returns leftover Exception) is wontfix on 111.

## Checklist

- [x] M6 Pipe does not match last-stage setups; Loose runs the real pipeline; test
- [x] M7 disposed MockState is ignored even if AsyncLocal still points at it
- [x] M8 Setup uses the caller’s logical executable name, not the path override
- [x] `## Results` + `### How to validate`

## Notes

Parent: **111**. Source: `review/round-1/testing-mocks.md`.

## Session

- Implementer: grok session `01a0a78c-0a7e-7dd1-af56-eb562d4aa341` (2026-09-16)

## Results

M6–M8 from parent 111 round-1 `review/round-1/testing-mocks.md` / `merged.md`. Pipe mock identity and `CliConfiguration` path-override matching now match the documented CommandMock contract. **089** was not reopened. **M9** remains wontfix.

- **M6:** Piped `CommandResult` still has no mock identity (`new CommandResult(pipedCommand)`). `ResolveMockSetup` no longer falls back to last-stage CliWrap `TargetFilePath` / space-split `Arguments`. Strict throws a pipe-specific message. Loose returns null and runs the real pipeline.
- **M7:** `MockScope` dispose tombstones the captured `MockState`. `CommandMock.State` / `IsEnabled` / `Setup` / `VerifyCalled` / `CallCount` / `Reset` ignore disposed state even if the originating `AsyncLocal` still points at it. Same-context dispose still clears `AsyncLocal` when the pointer matches.
- **M8:** Logical mock executable is captured from the caller name before `CliConfiguration.GetCommandPath` (and before the `.cs` host rewrite). `Setup("git")` matches a path-overridden git; CliWrap still executes the override path when mocking is not intercepting.

**Files changed**

- `source/timewarp-amuru/core/command-result.cs`
- `source/timewarp-amuru/core/command-extensions.cs`
- `source/timewarp-amuru/testing/CommandMock.cs`
- `source/timewarp-amuru/testing/MockState.cs`
- `source/timewarp-amuru/testing/MockScope.cs`
- `tests/timewarp-amuru/single-file-tests/core/command-mock.cs`

**Decisions**

- Kept piped `CommandResult` without a mock identity rather than synthesizing a pipeline key. Composition is not a single executable; last-stage fallback was the bug.
- Tombstone the shared `MockState` object instead of trying to walk `ExecutionContext` to clear another context's `AsyncLocal`.
- Path-override test uses a real temp executable because CliWrap validates the override path at `Wrap` time, before mock interception.

**Tests:** `cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs` → **494 passed, 0 failed, 1 skipped** (pre-existing skip on `GetCommitsAheadOfDefaultBranch_Given_`). Targeted `command-mock.cs` and `shell-builder.pipe.cs` also passed.

### How to validate

**Smoke**

```bash
dotnet run tests/timewarp-amuru/single-file-tests/core/command-mock.cs -- --filter-method PipedCommand
dotnet run tests/timewarp-amuru/single-file-tests/core/command-mock.cs -- --filter-method DisposedFromOtherContext
dotnet run tests/timewarp-amuru/single-file-tests/core/command-mock.cs -- --filter-method PathOverride
```

**Expect**

- Loose piped `echo Hello\\nWorld\\nTest | grep World` with `Setup("grep", "World").Returns("MOCKED")`: stdout trims to `World` (real pipeline), not `MOCKED`.
- Strict piped same setup: throws `InvalidOperationException` whose message contains `Pipe compositions bypass mock matching` (does not return mocked stdout).
- Dispose Enable scope from `Task.Run`: leftover `AsyncLocal` is ignored (`Setup` throws not enabled; real `echo hello` returns `hello`; a new `Enable()` succeeds).
- `SetCommandPath("git", <existing temp file>)` + `Setup("git", "status")`: capture returns `On branch main` (logical name matches; override path is not the mock key).

**Automated**

```bash
cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs
# expect: Failed: 0 (this session: Passed 494, Skipped 1)
```

If a single-file run disagrees with source you just edited, clear the runfile cache: `rm -rf ~/.local/share/dotnet/runfile/<test-name>-*`.
