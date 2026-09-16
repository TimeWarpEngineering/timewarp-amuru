# Review framework — task 111-002

**Date:** 2026-09-16
**Host task:** `kanban/in-progress/111-002-fix-commandmock-pipe-identity-and-path-override-matching/`
**Diff scope:** branch `task/111-002-fix-commandmock-pipe-identity-and-path-override-ma` vs `origin/feature/overnight-amuru` (product: `source/timewarp-amuru/core/{command-result,command-extensions}.cs`, `source/timewarp-amuru/testing/{CommandMock,MockState,MockScope}.cs`, tests in `tests/timewarp-amuru/single-file-tests/core/command-mock.cs`)
**Plan / brief:** Parent 111 round-1 findings **M6–M8**. Pipe mock identity and `CliConfiguration` path-override matching must match the documented CommandMock contract. Do not reopen **089**. **M9** remains wontfix.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** implementer grok `01a0a78c-0a7e-7dd1-af56-eb562d4aa341` (2026-09-16); review-oracle grok `01a0a793-ccde-7931-8e31-1f9715361667` (2026-09-16)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Do not treat sibling 111-003…111-005 scope (ScriptContext, tools/version, docs/CI) as findings for this task
- Do not reopen **089** (ordinary-path argument matching) unless the defect is still present
- Do not reopen **M9** (Throws-then-Returns leftover Exception); it is wontfix on 111
