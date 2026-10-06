# Review framework

## Budget (by-diff)

- Lines changed: 2062
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 044

**Date:** 2026-10-06
**Host task:** kanban/to-do/044-add-command-timeout-support/
**Diff scope:** branch `task/044-add-command-timeout-support` vs `master` (commits 7a480db, 02cb7b9; kitchen rewrite f38be9f)
**Plan / brief:** task.md Design decisions + Results — per-command timeout on `CommandOptions`, graceful-then-forceful kill via CliWrap two-token `ExecuteAsync`, `TimedOut` + exit 124 result, strict validation throws `TimeoutException`, caller cancel propagates `OperationCanceledException`, tty/pipe/stream/mock coverage.
**Effort:** 3 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5, ganda task work headless); reviewer subagent (Claude, general)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
