# Round 1 — merged findings
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
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:151
- Description: Q1 says `seq 3 | tail -5` leaves no file and no note. The note is absent. The file is not. `research/proto-tail.sh` always `mktemp`s and never removes the file. Targeted check: symlink `proto-tail.sh` onto `PATH` as `tail`, then `bash -c 'seq 3 | tail -5'`. A new `${TMPDIR:-/tmp}/stream.*` file remained. `seq 100 | tail -1` printed `[100]` on stdout with the stderr note, and `tail -3 in.txt` reached the real binary.
- Suggestion: Make Q1 describe the prototype in the tree. Keep deletion as the Q5 design, or implement it in the prototype and re-check.
- Source: general
- Disposition notes: proto-tail.sh removes its stream file when nothing was cut; Q1 re-checked with an empty TMPDIR.

### M2 — Severity: bug — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/research/slice-forms-scan.py:68
- Description: A cut is `out.count('\n')+1 >= N`, and the same test is in `rerun-after-cut-scan.py:50`. Thirty-nine newline-terminated lines satisfy `>= 40`. Q5's 26,601 / 38,798 and Q8's re-run totals use this test. Checked with a three-line Python comparison of a 39-line and a 40-line terminated string against N=40.
- Suggestion: Count lines so a trailing newline ends the last line, re-run both scans, and replace the Q5 and Q8 figures from that output.
- Source: general
- Disposition notes: Both scripts count lines with str.splitlines; scans re-run, Q5 and Q8 figures replaced.

### M3 — Severity: bug — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/research/rerun-after-cut-scan.py:65
- Description: How to validate says the script's strict re-run total is close to 106. The script matches `up in nc` and prints cheap/exp counters. It does not require the next command to start with the upstream, and it does not print 106, the 88/18 split, or the 5,107 s table. Q1's `2>&1` counts (5, 4, 1), 39,535 piped-slice commands, 2,107 captures, and 65 `pipefail` commands are not produced by either script.
- Suggestion: Encode the strict predicate and point How to validate at the counter it prints. Add the Q1 counts to a script, or cite the command that produced them.
- Source: general
- Disposition notes: rerun-after-cut-scan.py prints the strict block (total, tail/head, month, rows); slice-forms-scan.py prints the Q1 counters; How to validate names them.

### M4 — Severity: bug — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:230
- Description: Q5's denominator is 38,798 piped `-N` slices. The Q3 piped numeric rows sum to 46,591 (literal piped `-N` alone is 46,332). `slice-forms-scan.py` has one counter for numeric piped slices and no filter that prints 38,798.
- Suggestion: Recompute the denominator from the same run as the Q3 table, or name the extra filter and the counter key.
- Source: general
- Disposition notes: Q5 uses the `piped numeric slices` counter from the same run as Q3 (59,301; tail 15,064).

### M5 — Severity: nit — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:208
- Description: The Q3 pipe rows sum to the stated 60,166, and the tail share 15,249 matches its rows. The eleven table rows sum to 66,593, which is 121 below the stated 66,714 uses.
- Suggestion: Make the stated total the sum of the rows, or add the missing row.
- Source: general
- Disposition notes: Q3 rows now sum to the stated total, 65,725 (script prints it).

### M6 — Severity: nit — Status: fixed
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:138
- Description: The evidence base says `~/.timewarp/ganda/walks` has only 20 `oracle-*.log` files. `find ~/.timewarp/ganda/walks -name 'oracle-*.log' | wc -l` printed 519. An opened implement log is oracle result JSON plus `ORACLE_EXIT`, so excluding these files from the bash-form count still holds. The number 20 does not.
- Suggestion: Replace 20 with the find count, and keep the statement that these files are oracle results rather than a second bash corpus.
- Source: general
- Disposition notes: Replaced 20 with the find count, 522; kept the oracle-result statement.

## Duplicates / conflicts

- No overlapping findings. M2, M3, and M4 are separate defects in the same evidence chain (line test, strict filter, denominator).

## Evidence

No screenshots were listed under `evidence/`. None were viewed.
