# Review framework

## Budget (by-diff)

- Lines changed: 201
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 142

**Date:** 2026-11-09
**Host task:** kanban/in-progress/142-update-nuget-packages-move-the-self-pins-to-300-beta2-and-bump-to-300-beta3/
**Diff scope:** branch `task/142-update-nuget-packages-move-the-self-pins-to-300-be` vs `master` (merge base); product commits `8da7577` (pins) and `0bdfcad` (version bump); kanban commits excluded from findings
**Plan / brief:** refresh outdated NuGet pins, move the githook self pins to 3.0.0-beta.2, bump `<Version>` to 3.0.0-beta.3 with a release note; no behavior change, no publish
**Effort:** 2 (by-diff budget), roster axis general
**Reviewer roster:** general
**Session IDs:** review oracle claude-opus-5-5 (Claude Code, headless `ganda task work`)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
