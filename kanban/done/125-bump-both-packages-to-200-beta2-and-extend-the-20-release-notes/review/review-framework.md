# Review framework

## Budget (by-diff)

- Lines changed: 640
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 125

**Date:** 2026-10-06
**Host task:** kanban/to-do/125-bump-both-packages-to-200-beta2-and-extend-the-20-release-notes/
**Diff scope:** branch `task/125-…` commit 85756f6 vs 9badcfc (version bump, PublicAPI promotion, release notes, doc literals)
**Plan / brief:** bump lockstep version to 2.0.0-beta.2, promote Unshipped → Shipped, add beta.2 release-notes section (task 122 pattern)
**Effort:** 2 (Budget.ByDiff)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, ganda task work 2026-10-06)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
