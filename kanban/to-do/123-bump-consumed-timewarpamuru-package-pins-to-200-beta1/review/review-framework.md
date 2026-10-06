# Review framework

## Budget (by-diff)

- Lines changed: 114
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 123

**Date:** 2026-10-06
**Host task:** kanban/to-do/123-bump-consumed-timewarpamuru-package-pins-to-200-beta1/
**Diff scope:** branch `task/123-bump-consumed-timewarpamuru-package-pins-to-200-be` vs `origin/master` (HEAD `9c1677f`)
**Plan / brief:** Bump consumed `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` CPM pins to `2.0.0-beta.1` for the `.githooks` runfiles
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, 2026-10-06)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
