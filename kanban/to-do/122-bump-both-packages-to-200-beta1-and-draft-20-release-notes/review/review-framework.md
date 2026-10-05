# Review framework

## Budget (by-diff)

- Lines changed: 486
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 122

**Date:** 2026-10-05
**Host task:** kanban/to-do/122-bump-both-packages-to-200-beta1-and-draft-20-release-notes/
**Diff scope:** branch task/122-bump-both-packages-to-200-beta1-and-draft-20-relea vs master (439b62f, 8961e3a)
**Plan / brief:** version 1.1.1 → 2.0.0-beta.1, PublicAPI Unshipped → Shipped promotion, 2.0 release notes draft, readme/AGENTS/skill prose
**Effort:** 2 (by-diff budget), roster axis general
**Reviewer roster:** general (review oracle, Opus 5.5)
**Session IDs:** review oracle session 2026-10-05 (ganda task work review node)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
