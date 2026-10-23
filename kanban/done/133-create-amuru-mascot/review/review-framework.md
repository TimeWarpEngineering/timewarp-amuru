# Review framework

## Budget (by-diff)

- Lines changed: 181
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-23
**Host task:** kanban/to-do/133-create-amuru-mascot/
**Diff scope:** branch `task/133-create-amuru-mascot` vs `master` (commit e96789b)
**Plan / brief:** copy the five Amuru kit PNGs from `/home/steve/masoko/mascot-kit/` into `assets/mascot/` byte for byte, add a centered hero at the top of `readme.md`, tick checklist items 1-5. Also `ganda repo audit --fix` output (Nuru 4.0.0-beta.5 pin, `feature/**` PR trigger, gitignore lines).
**Effort:** 1 (general only)
**Reviewer roster:** general (review oracle, Claude Opus 5.5, headless `ganda task work`)
**Session IDs:** review oracle session not reported by host

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
