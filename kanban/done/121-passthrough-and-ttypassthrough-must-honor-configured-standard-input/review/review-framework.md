# Review framework

## Budget (by-diff)

- Lines changed: 659
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 121

**Date:** 2026-10-05
**Host task:** kanban/to-do/121-passthrough-and-ttypassthrough-must-honor-configured-standard-input/
**Diff scope:** branch task/121-passthrough-and-ttypassthrough-must-honor-configur vs master (commit f9b29c4)
**Plan / brief:** PassthroughAsync keeps configured stdin; TtyPassthroughAsync refuses configured stdin (option b); mock records stdin; Pipe overload with CommandOptions; fzf passes options to the pipe stage.
**Effort:** 2 (by-diff budget), roster axes: general
**Reviewer roster:** general
**Session IDs:** claude review oracle (ganda task work, 2026-10-05)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
