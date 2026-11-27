# Review framework — task 157

**Date:** 2026-11-27
**Host task:** kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/
**Diff scope:** branch `task/157-research-head-and-tail-commands-that-save-the-full` vs `master` (research answer, `research/` scans and `proto-tail.sh`, `Directory.Packages.props` pin bumps, `.timewarp/line-cap-baseline.json`)
**Plan / brief:** Research only. Recommend whether to ship PATH `head`/`tail` that save the full stream. Do not build the commands. Answer the nine questions under `## Results`.
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** implementer claude `16672e49-ce38-4da3-942e-119c5e4ec6c4`; reviewer cursor `0c6c085c-dbed-434b-85c1-7c18bacafc0b`

## Budget (by-diff)

- Lines changed: 530
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

## Round 2

**Date:** 2026-11-27
**Diff scope:** `1240328..2587ab9` (fix pass for M1–M6): `research/proto-tail.sh`, `research/slice-forms-scan.py`, `research/rerun-after-cut-scan.py`, `research/readme.md`, and the Q1/Q3/Q5/Q8/Q9 numbers in `task.md`. Round 1 files were not rewritten.
**Plan / brief:** Re-verify M1–M6 on that delta. Scan the fix for new defects.
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** reviewer session not reported (grok-4.7). The budget line above (530) is this delta.
