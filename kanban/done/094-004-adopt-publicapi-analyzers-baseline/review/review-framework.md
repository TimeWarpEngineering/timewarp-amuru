# Review framework

## Budget (by-diff)

- Lines changed: 1895
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 094-004

**Date:** 2026-10-05
**Host task:** kanban/to-do/094-004-adopt-publicapi-analyzers-baseline/
**Diff scope:** branch `task/094-004-adopt-publicapi-analyzers-baseline` vs `master` (5eb25d6, 0c5d0f8)
**Plan / brief:** adopt Microsoft.CodeAnalysis.PublicApiAnalyzers on the two packable projects; baseline current master surface into Shipped; document workflow
**Effort:** 3 (by-diff budget; ~1740 of the 1895 lines are generated PublicAPI baseline entries)
**Reviewer roster:** general
**Session IDs:** review oracle (claude-opus-5-5), 2026-10-05

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
