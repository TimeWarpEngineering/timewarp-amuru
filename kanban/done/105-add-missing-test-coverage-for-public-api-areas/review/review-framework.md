# Review framework

## Budget (by-diff)

- Lines changed: 822
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 105

**Date:** 2026-10-05
**Host task:** kanban/to-do/105-add-missing-test-coverage-for-public-api-areas/
**Diff scope:** branch task/105-add-missing-test-coverage-for-public-api-areas vs master (435c08f, 49c1d47)
**Plan / brief:** Add tests for `dotnet tool` builders (dot-net.tool.cs + cli-smoke additions) and Direct.GetLocation/SetLocation.
**Effort:** 3
**Reviewer roster:** general
**Session IDs:** claude review oracle (ganda task work, 2026-10-05)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
