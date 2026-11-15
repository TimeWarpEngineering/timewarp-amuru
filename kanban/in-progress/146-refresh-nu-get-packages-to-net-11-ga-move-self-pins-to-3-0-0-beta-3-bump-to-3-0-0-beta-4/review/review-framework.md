# Review framework

## Budget (by-diff)

- Lines changed: 336
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 146

**Date:** 2026-11-15
**Host task:** kanban/in-progress/146-refresh-nu-get-packages-to-net-11-ga-move-self-pins-to-3-0-0-beta-3-bump-to-3-0-0-beta-4/
**Diff scope:** branch `task/146-refresh-nu-get-packages-to-net-11-ga-move-self` vs `master` (merge base). Product commit `63dcfe8`. Kanban move/land commits are out of findings. 19 files, +306/−30.
**Plan / brief:** refresh the ten outdated pins, move the githook self pins to 3.0.0-beta.3, bump `<Version>` to 3.0.0-beta.4, and add a beta.4 release note. SDK pin follows NetAnalyzers to 11.0.100.
**Effort:** 2 (by-diff budget), roster axis general
**Reviewer roster:** general
**Session IDs:** review oracle Grok 4.7, session `01a1de4d-53d4-7f01-8958-61359ae2d018`, max-turns 120

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
