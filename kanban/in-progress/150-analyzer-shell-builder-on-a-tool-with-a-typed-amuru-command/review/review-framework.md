# Review framework — task 150

**Date:** 2026-11-18
**Host task:** kanban/in-progress/150-analyzer-shell-builder-on-a-tool-with-a-typed-amuru-command/
**Diff scope:** branch `task/150-analyzer-shell-builder-on-a-tool-with-a-typed` vs `master` (`master...HEAD`, 4 commits, 23 files). Review is the implementation diff. The branch is 1 commit behind `master`; that commit is outside this review.
**Plan / brief:** task.md requirements and `## Goal (implementer restatement)`. Ship AMURU0001–0003 in `TimeWarp.Amuru` for constant `Shell.Builder` / `Shell.Run` on `dotnet` and `git`.
**Effort:** 3
**Reviewer roster:** general
**Session IDs:** implementer session 939359cc-7424-4d0f-980c-364b061ae12d (task.md). Review oracle: Grok, 2026-11-18 (this walk). No separate session id was available to record.

## Budget (by-diff)

- Lines changed: 1465
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
