# Review framework — task 148

**Date:** 2026-11-17
**Host task:** kanban/in-progress/148-upgrade-cs-check-to-5-0-1/
**Diff scope:** branch `task/148-upgrade-cs-check-to-5-0-1` vs merge-base `938fc26` (`master`). Product delta is `Directory.Packages.props` only (CsCheck 4.9.1 → 5.0.1). Kitchen `task.md` moved to in-progress with Results. Worktree dirty only for this `review/` (untracked).
**Plan / brief:** Upgrade the dev-only CsCheck pin to 5.0.1 and keep `tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs` working with the same generators, iteration counts, and cases. No package release.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok `01a1e90b-ef78-7ed0-a718-53de22f825da` (2026-11-17). Implementer session is on `task.md`.

## Budget (by-diff)

- Lines changed: 124
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
