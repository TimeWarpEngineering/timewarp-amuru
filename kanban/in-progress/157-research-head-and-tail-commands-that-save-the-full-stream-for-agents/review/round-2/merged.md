# Round 2 — merged findings
**Date:** 2026-11-27
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 4 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/research/proto-tail.sh:11
- Description: Q1 said `seq 3 | tail -5` left no file. Round 1's prototype always kept the `mktemp` file.
- Suggestion: Delete the stream file when nothing was cut, and make Q1 describe that prototype.
- Source: general
- Disposition notes: Fixed in `2587ab9`. Round 2 read the script. The file is removed when `awk` NR is not greater than N. The cut path still prints the stderr note and keeps the file.

### M2 — Severity: bug — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/research/slice-forms-scan.py:23
- Description: A cut was `count('\n')+1 >= N`, so 39 newline-terminated lines counted as a full slice of 40. The same test was in `rerun-after-cut-scan.py`.
- Suggestion: Count lines so a trailing newline ends the last line, re-run both scans, and replace the Q5 and Q8 figures.
- Source: general
- Disposition notes: Fixed in `2587ab9`. Both scripts use `str.splitlines`. Q5 and Q8 in `task.md` cite that run (`--until 2026-11-27`).

### M3 — Severity: bug — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/research/rerun-after-cut-scan.py:105
- Description: The strict re-run total was not what the script printed, and the Q1 `2>&1` counts were not produced by either script.
- Suggestion: Encode the strict predicate, print it, and add the Q1 counts to a script.
- Source: general
- Disposition notes: Fixed in `2587ab9`. The strict block prints `169 total 7169`, `139 after tail`, `30 after head`, and the Q8 rows. The form scan prints the Q1 counters. `### How to validate` names those lines. The Q8 case column sums to 169 and the seconds column sums to 7,169. Q9's 93 cases and 5,645 s are the `dev test`, `dev build`, `dev deps`, and `dotnet test/run` rows.

### M4 — Severity: bug — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:240
- Description: Q5's denominator did not match the Q3 piped-numeric rows or any counter in the form scan.
- Suggestion: Recompute the denominator from the same run as the Q3 table.
- Source: general
- Disposition notes: Fixed in `2587ab9`. Q5 uses `piped numeric slices` (59,301) and `tail piped numeric slices` (15,064) from that run. 15,064 is the three piped tail rows in the Q3 table.

### M5 — Severity: nit — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:216
- Description: The Q3 rows did not sum to the stated total of head/tail uses.
- Suggestion: Make the stated total the sum of the rows.
- Source: general
- Disposition notes: Fixed in `2587ab9`. The eleven rows sum to 65,725, and the answer says the script prints that total.

### M6 — Severity: nit — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:139
- Description: The evidence base said 20 `oracle-*.log` files. A find during round 1 printed 519.
- Suggestion: Replace 20 with the find count, and keep the statement that these files are oracle results.
- Source: general
- Disposition notes: Fixed in `2587ab9`. The answer says the same find printed 522 on 2026-11-27. Round 2 ran `find ~/.timewarp/ganda/walks -name 'oracle-*.log' | wc -l` and got 526. Walks were still writing logs, so the dated snapshot stands. The oracle-result statement is unchanged.

## Duplicates / conflicts

- No new findings. M1–M6 stay the round 1 ids. Nothing overlapped on this delta.

## Evidence

No screenshots were listed under `evidence/`. None were viewed.

M6 re-check, one targeted command, because the replacement count is the claim round 1 falsified: `find ~/.timewarp/ganda/walks -name 'oracle-*.log' | wc -l` printed 526. The answer's 522 is the same find earlier on 2026-11-27.
