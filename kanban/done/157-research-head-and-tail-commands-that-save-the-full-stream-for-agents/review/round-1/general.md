# Round 1 — general
**Date:** 2026-11-27
**Scope reviewed:** branch `task/157-research-head-and-tail-commands-that-save-the-full` vs `master`. Research answer in `task.md`, `research/proto-tail.sh`, `research/slice-forms-scan.py`, `research/rerun-after-cut-scan.py`, plus the audit pin and line-cap edits.

## Summary

The answer recommends a small POSIX `tail` shim on the agent `BASH_ENV` PATH and drops `head`. That shape matches the nine questions: stdout identity, early exit, which forms, finding the real binary, save location, whose PATH, startup, whether re-runs pay, and what task 683 plus Claude's large-output save already cover. The split (nothing in Amuru, shim and `BASH_ENV` in timewarp-ganda, hand-launched hooks in timewarp-flow) is argued and the ganda/flow citations that were checked match the current sources.

The blocking gaps are in the evidence the recommendation rests on. `proto-tail.sh` leaves the save file when nothing was cut, which Q1 says it does not. Both scan scripts treat N−1 newline-terminated lines as a full slice. The strict re-run total of 106 is not what `### How to validate` prints. Q3's stated total and Q5's denominator do not match the rows beside them. Gates for the pin bump and the line-cap baseline are recorded (build, listed single-file tests, `dev self-install`, audit). Those gates were not re-run. The five new baseline lengths match `wc -l`.

## Issues

### Issue 1 — Severity: bug
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:151
- Description: Q1 says `seq 3 | tail -5` leaves no file and no note. The note is absent. The file is not. `research/proto-tail.sh` always `mktemp`s, `cat`s the stream into that file, and never removes it. Targeted check, because Q1 names this command as the proof: symlink `proto-tail.sh` onto `PATH` as `tail`, then `bash -c 'seq 3 | tail -5'`. A new `${TMPDIR:-/tmp}/stream.*` file remained. `seq 100 | tail -1` did print `[100]` on stdout and the stderr note, and `tail -3 in.txt` did reach the real binary.
- Suggestion: Make Q1 describe the prototype that is in the tree (file kept either way; note only when `wc -l` is greater than N). Keep deletion as Q5's design for the follow-up shim, or implement that deletion in the prototype and re-check the same command.
- Status: open

### Issue 2 — Severity: bug
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/research/slice-forms-scan.py:68
- Description: A cut is `out.count('\n')+1 >= N`. For newline-terminated tool output that counts N−1 lines as a full slice. Thirty-nine lines ending in newlines have 39 newlines, and `39+1 >= 40` is true. The same test is in `research/rerun-after-cut-scan.py:50`. Q5's "26,601 of 38,798" and Q8's re-run totals are computed from this test. Checked with a three-line Python comparison of a 39-line and a 40-line terminated string against N=40. The 39-line string matches the cut test.
- Suggestion: Count lines so a trailing newline ends the last line, compare that count with N, re-run both scans, and replace the Q5 and Q8 figures from the new output.
- Status: open

### Issue 3 — Severity: bug
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/research/rerun-after-cut-scan.py:65
- Description: Q8's strict total is 106 cases whose next command starts with the same expensive upstream, plus the 88/18 tail/head split and the 5,107 s table. `### How to validate` says running `rerun-after-cut-scan.py` yields a strict total close to 106. The script records a re-run when `up in nc` (the upstream string appears anywhere in a later command) and prints cheap/exp counters. It has no "starts with" filter and it does not print 106. Q1's `2>&1` counts (5, 4, 1), the 39,535 piped-slice commands, the 2,107 captures, and the 65 `pipefail` commands are also absent from both scripts.
- Suggestion: Put the strict predicate in `rerun-after-cut-scan.py` and point How to validate at the counter it prints. Add the Q1 transcript counts to a script, or cite the command that produced them.
- Status: open

### Issue 4 — Severity: bug
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:230
- Description: Q5 says 38,798 piped `-N` slices. The Q3 rows for piped numeric forms sum to 46,591 (`| head -N` 31,326 + `| tail -N` 15,006 + `| tail -n N` 210 + `| head -n N` 49). Literal piped `-N` alone is 46,332. Both figures are attributed to `slice-forms-scan.py`, which uses one counter for numeric piped slices. The denominator is 7,793 below that counter's inputs, and the script has no second filter that would print 38,798.
- Suggestion: Recompute the Q5 denominator from the same run as the Q3 table, or name the extra filter and the counter key that yields 38,798.
- Status: open

### Issue 5 — Severity: nit
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:208
- Description: Q3 says the pipe forms are 60,166 of 66,714 uses. The pipe rows do sum to 60,166, and the tail share 15,249 matches its three rows. The eleven rows in the table sum to 66,593, which is 121 below 66,714.
- Suggestion: Make the stated total the sum of the rows, or add the missing row.
- Status: open

### Issue 6 — Severity: nit
- File: kanban/in-progress/157-research-head-and-tail-commands-that-save-the-full-stream-for-agents/task.md:138
- Description: The evidence base says `~/.timewarp/ganda/walks` has only 20 `oracle-*.log` files. Targeted check, because that count is the reason the walk logs were left out of the form tally: `find ~/.timewarp/ganda/walks -name 'oracle-*.log' | wc -l` printed 519. One opened file (`oracle-implement-*.log` under task 706-005) is the oracle result JSON plus `ORACLE_EXIT`, so leaving these logs out of the bash-form count still holds. The number 20 does not.
- Suggestion: Replace 20 with the count from that find, and keep the statement that these files are oracle results rather than a second bash corpus.
- Status: open
