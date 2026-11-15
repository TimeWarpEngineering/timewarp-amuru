# Review framework — task 145

**Date:** 2026-11-15
**Host task:** kanban/in-progress/145-byte-level-stdout-tee-to-a-file-and-stdin-from-a-file/
**Diff scope:** branch `task/145-byte-level-stdout-tee-to-a-file-and-stdin-from-a` vs `master` (merge-base `06ae239`). Product change is `5897887` plus the audit-fix commit `ad6e875`. 1216 lines, 19 files.
**Plan / brief:** Byte-level stdout tee and stdin from a file on `ShellBuilder`, so ganda can stop wrapping agent launches in `bash -c '… | tee -- out'` (ganda task 648).
**Effort:** 3 (general only)
**Reviewer roster:** general
**Session IDs:** review grok `01a1df82-cbe2-7311-986d-67e45db0fcde` (2026-11-15). Implementer claude `1f2f70cc-65f8-4a6c-a0be-04a5eafbfb27`.

## Budget (by-diff)

- Lines changed: 1216
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
