# Review framework

## Budget (by-diff)

- Lines changed: 69
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope (task 135)

**Date:** 2026-11-01
**Host task:** kanban/in-progress/135-bump-both-packages-to-300-beta1/
**Diff scope:** branch `task/135-bump-both-packages-to-300-beta1` vs merge-base `4ba051b` with `master` (product commit `211d72d`; master is ahead by kanban-only landing commits)
**Plan / brief:** replace unpublished `3.0.0` with `3.0.0-beta.1` in `source/Directory.Build.props`, rename the release note, update `AGENTS.md`; no publish, no retarget
**Effort:** 1 (general only)
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle under `ganda task work` (headless print mode)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
