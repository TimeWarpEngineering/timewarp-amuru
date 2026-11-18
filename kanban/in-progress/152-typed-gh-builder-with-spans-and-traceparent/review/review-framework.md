# Review framework — task 152

**Date:** 2026-11-18
**Host task:** kanban/in-progress/152-typed-gh-builder-with-spans-and-traceparent/
**Diff scope:** branch `task/152-typed-gh-builder-with-spans-and-traceparent` vs merge-base `ab35e18` (master). Product diff is commit `461c9d5` (3943 insertions, 115 deletions, 40 files). Kanban land commits are out of review scope.
**Plan / brief:** Typed `Gh` builders in TimeWarp.Amuru.Tools, one span per execution, TRACEPARENT from task 149, typed JSON for run list / auth status / pr view, AMURU0001 rows for the covered verbs, readme and `3.0.0-beta.9`.
**Effort:** 3 (by-diff, 4310 lines). Roster axes: general only.
**Reviewer roster:** general
**Session IDs:** 01a1ecec-1be6-70a3-8bba-e4d8e1927074 (Grok review oracle)

## Budget (by-diff)

- Lines changed: 115
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Round 2

**Date:** 2026-11-18
**Diff scope:** commit `2ff6e19` since round-1 base `751b122`. Product files: `tools/dev-cli/dev.cs`, `tools/dev-cli/telemetry.md`. The Budget block above is this round (115 lines, effort 1, general, turn cap 80). The header Effort line remains the round-1 record.
**Session IDs:** 01a1ecfa-efce-7a63-9496-75e0434fdbd3 (Grok review-2 oracle)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Goal check

The implementer restatement matches the original task. No misread. In-repo `workflow-command.cs` moves are the calls that became AMURU0001 errors in this solution. The org-wide 51 call sites stay out of scope.
