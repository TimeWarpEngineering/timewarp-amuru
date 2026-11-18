# Review framework — task 150

**Date:** 2026-11-18
**Host task:** kanban/in-progress/150-analyzer-shell-builder-on-a-tool-with-a-typed-amuru-command/
**Diff scope (round 1):** branch `task/150-analyzer-shell-builder-on-a-tool-with-a-typed` vs `master` (`master...HEAD` at base `7efccb0`). Review was the implementation diff. The branch was 1 commit behind `master`; that commit was outside round 1.
**Diff scope (round 2):** delta since round 1 base `7efccb0e967f74d4a0c4261e21baac3ab3765968` through `92379d3`. Product change is `80ea920` (git helper shapes). Kitchen land commits are not product behavior.
**Diff scope (round 3):** delta since round 2 head `92379d3` through `e4e9b37`. Product change is `e4e9b37` (`git-helper-table.cs`, `shell-builder-analyzer.cs`, and the analyzer runfile). Kitchen edits in that commit are not product behavior.
**Plan / brief:** task.md requirements and `## Goal (implementer restatement)`. Ship AMURU0001–0003 in `TimeWarp.Amuru` for constant `Shell.Builder` / `Shell.Run` on `dotnet` and `git`. Round 3 checks the M2, M3, and M4 fix.
**Effort:** 2 (this walk). Round 1 ran at effort 3 on the full branch diff.
**Reviewer roster:** general
**Session IDs:** implementer session 939359cc-7424-4d0f-980c-364b061ae12d (task.md). Round 1 review session 01a1ec92-02b0-76a0-af9f-6e71341180db. Round 2 review session 01a1eca3-78dc-71d0-9c94-9992104206d8. Round 3 review session 01a1ecaf-97d5-7060-86a3-7dd884443dd5.

## Budget (by-diff)

- Lines changed: 279
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
