# Review framework — task 111-001

**Date:** 2026-09-16
**Host task:** `kanban/in-progress/111-001-fix-captureasync-runtime-blanks-and-selectasync-validation/`
**Diff scope:** branch `task/111-001-fix-captureasync-runtime-blanks-and-selectasync-va` vs `origin/feature/overnight-amuru` (product: `source/timewarp-amuru/core/{command-result,command-output,command-options}.cs` + tests under `tests/timewarp-amuru/single-file-tests/core/`)
**Plan / brief:** Parent 111 round-1 findings **M1–M5**. Align capture/select paths with the documented `CommandOutput` contract: set `RunTime` on CaptureAsync/RunAndCaptureAsync; preserve interior blanks in the string constructor and `SplitMockLines`; let `SelectAsync` propagate `CommandExecutionException`; `ConfigureAwait(false)` on the six `CommandTask` awaits; copy `EnvironmentVariables` in every `With*`. Do not reopen 090–092 / 097 unless the defect is still present.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** implementer grok `01a0a77a-093e-7771-b995-bd2c2994cb99` (2026-09-16); review-oracle grok `01a0a781-c37f-71b3-8b10-84ee1e2fc14b` (2026-09-16)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Do not treat sibling 111-002…111-005 scope (mocks pipe identity, ScriptContext, tools, docs/CI) as findings for this task
- Do not reopen 090–092 / 097 unless the defect is still present after these fixes
