# Review framework

## Budget (by-diff)

- Lines changed: 230
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope (task 141)

**Date:** 2026-11-09
**Host task:** kanban/in-progress/141-report-the-childs-exit-code-when-it-exits-during-the-timeout-grace-period/
**Diff scope:** branch `task/141-report-the-childs-exit-code-when-it-exits-during-t` vs merge-base `61ea2ee` with master (commits `d0b2f37`, `caefe3e`, `356479f`, plus kanban commits). 21 files, +1049 / -451.
**Plan / brief:** `## Goal (implementer restatement)` in task.md. A child that exits during the timeout grace period keeps its own exit code and `TimedOut` is false. A grace-elapsed kill is still 124. Then the version bumps to 3.0.0-beta.2, the public API rolls over, and the beta.2 release note is written.
**Effort:** 3 (by-diff budget). Roster axes: general only.
**Reviewer roster:** general (review oracle, Claude Opus 5.5)
**Session IDs:** review oracle under `ganda task work` (Claude Code headless print mode; no vendor session id exposed to the oracle)

## Round 2 scope

**Date:** 2026-11-09
**Diff scope:** fix delta `f7aed63..fa56fb5` (commit `6ce685d`, which fixes the stream backpressure, plus the kanban record). The scope is the round-1 M1/M2 fixes and new defects in that delta.
**Effort:** 2 (by-diff budget, 230 lines). Roster axes: general only.
**Reviewer roster:** general (review oracle, Claude Opus 5.5)
**Session IDs:** review oracle under `ganda task work` (Claude Code headless print mode; no vendor session id exposed)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
