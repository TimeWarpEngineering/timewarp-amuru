# Review framework — task 149

**Date:** 2026-11-18
**Host task:** kanban/in-progress/149-propagate-traceparent-to-child-processes-from-activity-current/
**Diff scope:** branch `task/149-propagate-traceparent-to-child-processes-from` vs `master` (merge-base `5c514e59`). Commits `886284d`..`c2d8f32` (781 lines). Product diff plus this untracked `review/` kitchen.
**Plan / brief:** `task.md` original requirements and `## Goal (implementer restatement)`: propagate `TRACEPARENT` / `TRACESTATE` from `Activity.Current` at child start, keep explicit caller values (including null), document opt-out, bump to `3.0.0-beta.6`.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** implementer claude `3a66e76a-b33f-4dca-854a-60aedc886c76`; reviewer grok `01a1eb03-0dd8-7a53-806d-4a05467179d7`

## Budget (by-diff)

- Lines changed: 781
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
