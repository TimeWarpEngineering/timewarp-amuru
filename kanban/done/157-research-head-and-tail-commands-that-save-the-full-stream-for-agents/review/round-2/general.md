# Round 2 — general
**Date:** 2026-11-27
**Scope reviewed:** delta `1240328..2587ab9` (fix pass for round 1 M1–M6). `research/proto-tail.sh`, `research/slice-forms-scan.py`, `research/rerun-after-cut-scan.py`, `research/readme.md`, and the Q1, Q3, Q5, Q8, and Q9 numbers in `task.md`.

## Summary

The fix pass keeps the same recommendation (a small POSIX `tail` shim, drop `head`) and replaces the evidence round 1 could not reproduce. `proto-tail.sh` now deletes its stream file when nothing was cut. Both scans count lines with `str.splitlines`, take `--until 2026-11-27`, and print the counters `### How to validate` names. The Q3 rows sum to 65,725, the Q5 tail denominator is the same run's piped-numeric tail count (15,064), and the Q8 table sums to 169 cases and 7,169 s.

No new defect on that delta. The recorded fix-pass results cover it: `./bin/dev build` exit 0, both scans exit 0 with the numbers written above, and `ganda repo audit` exit 0. No C# changed, and no test file was added.

## Prior findings

### M1 — fixed
`proto-tail.sh` keeps `stream.*` only when `awk` `NR` is greater than N. Otherwise it `rm`s the file before exit. That matches the Q1 `seq 3 | tail -5` and `seq 100 | tail -1` claims. A last line with no newline is included in `NR`.

### M2 — fixed
Both scripts use `len(s.splitlines())`. A trailing newline ends the last line, so 39 newline-terminated lines are not treated as a full slice of 40.

### M3 — fixed
`rerun-after-cut-scan.py` prints a `-- strict` block (total, seconds, `after tail` / `after head`, month counts, and one row per Q8 upstream) and a `-- loose` block. Strict means the upstream matches a table row and, after the loose substring match, one `&&` / `;` statement of the next command starts with that upstream. `slice-forms-scan.py` prints the Q1 `2>&1`, capture, and `pipefail` counters. `### How to validate` names those counters (`169 total 7169`, `139 after tail`, `30 after head`, `880 loose rerun`).

### M4 — fixed
Q5's denominator is `piped numeric slices` from the same script as the Q3 table: 59,301, of which `tail piped numeric slices` is 14,821 + 210 + 33 = 15,064.

### M5 — fixed
The eleven Q3 rows sum to 65,725, the script's `total head/tail uses`. The pipe forms with `-N`, `-n N`, or no args sum to 59,301.

### M6 — fixed
The evidence base cites `find ~/.timewarp/ganda/walks -name 'oracle-*.log' | wc -l` as 522 on 2026-11-27, and it still says those files are oracle results rather than a second bash corpus. A re-check of that find during this round printed 526. Walks were still finishing, so the dated snapshot stands.

## Issues

None.
