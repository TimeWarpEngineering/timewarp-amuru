# Review framework — task 128

**Date:** 2026-11-15
**Host task:** kanban/in-progress/128-cscheck-pilot-quote-then-parse-round-trip-property-for-argument-escaping/
**Diff scope:** branch `task/128-cscheck-pilot-quote-then-parse-round-trip-property` vs merge-base `63a9997` (`master`). Four commits. Product delta is CsCheck 4.9.1 (central version only), the argv-echo child, the `WithArguments` round-trip runfile, the aggregate `Compile Remove`, and `CsCheck_Seed` on the CI step. Kitchen `task.md` moved to in-progress with Results. Worktree dirty only for this `review/` (untracked).
**Plan / brief:** task 128 — CsCheck pilot that sends random argument lists through `Shell.Builder(...).WithArguments(...)` and asserts a real child saw the same strings. Groom (2026-11-15) names CliWrap's escaping as the contract under test.
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** Grok `01a1dfe8-ed25-78c2-8d66-58b55ef32a20` (review); implementer session not reported (claude-opus-5-5, 2026-11-15T15:52:18Z). Ganda session 4171919.

## Budget (by-diff)

- Lines changed: 458
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
