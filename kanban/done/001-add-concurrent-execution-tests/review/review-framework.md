# Review framework

## Budget (by-diff)

- Lines changed: 676
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 001

**Date:** 2026-10-07
**Host task:** kanban/to-do/001-add-concurrent-execution-tests/
**Diff scope:** branch task/001-add-concurrent-execution-tests vs master (commit 1c7c239)
**Plan / brief:** task.md Parts A (concurrency) and B (large output); LastOutput decision (a)
**Effort:** 2 (by-diff budget); roster axes: general
**Reviewer roster:** general (Claude Sonnet subagent), merge and verification by review oracle (Claude Opus 5.5)
**Session IDs:** general subagent a1bae4f113a3a928e

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
