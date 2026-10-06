# Review framework

## Budget (by-diff)

- Lines changed: 171
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 124

**Date:** 2026-10-06
**Host task:** kanban/to-do/124-upgrade-timewarpnuru-to-300-beta79-and-migrate-dev-cli-endpoints/
**Diff scope:** branch task/124-upgrade-timewarpnuru-to-300-beta79-and-migrate-dev vs master (excluding kanban/)
**Plan / brief:** Bump TimeWarp.Nuru / Nuru.DevCli to 3.0.0-beta.79; migrate tools/dev-cli to TimeWarp.Mediator contracts (Task<Unit>, Unit.Value, public endpoints).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** claude review oracle (2026-10-06)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
