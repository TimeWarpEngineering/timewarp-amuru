# Review framework

## Budget (by-diff)

- Lines changed: 210
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-11-07
**Host task:** kanban/in-progress/137-update-nuget-packages-from-ganda-nuget-outdated/
**Diff scope:** branch `task/137-update-nuget-packages-from-ganda-nuget-outdated` vs merge-base with `master` (commits 422127a, eadc66c, 7f2ca00)
**Plan / brief:** bump the three Roslynator analyzer pins 5.0.0 → 5.0.1 that `ganda nuget outdated` reports; boy-scout audit fixes (`.vscode/settings.json` untracked, first `.timewarp/line-cap-baseline.json`)
**Effort:** 2 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, claude-opus-5-5), headless `ganda task work`

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
