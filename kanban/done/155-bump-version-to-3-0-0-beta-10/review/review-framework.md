# Review framework — task 155

**Date:** 2026-11-18
**Host task:** kanban/in-progress/155-bump-version-to-3-0-0-beta-10/
**Diff scope:** branch `task/155-bump-version-to-3-0-0-beta-10` vs `master` (three-dot). Product change is `<Version>` in `source/Directory.Build.props`. Tip `2ff481b`. Merge-base `15ab788`. Ahead 3, behind 1 (`0db026a`, the same kanban rename).
**Plan / brief:** Bump `<Version>` from `3.0.0-beta.9` to `3.0.0-beta.10`. That line is the only product edit. `./bin/dev build` and `./bin/dev check-version` are the checklist gates. Release (`dev release`) is out of scope.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** implementer `a2bee1e1-1468-47a2-95c8-e3d2a1982976`; review oracle grok `01a1edf1-b9e5-7a13-a8a3-0bea78e54917`

## Budget (by-diff)

- Lines changed: 89
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
