# Review framework

## Budget (by-diff)

- Lines changed: 3712
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 021

**Date:** 2026-10-06
**Host task:** kanban/to-do/021-essential-text-processing/
**Diff scope:** branch `task/021-essential-text-processing` vs `master` (commits 4bcdf28, aef9afc; product code under `source/timewarp-amuru/native/text/`, `native/aliases/bash.cs`, tests under `tests/timewarp-amuru/single-file-tests/native/text/`, docs)
**Plan / brief:** task.md Requirements — native `SelectString` (grep) and `ReplaceInFiles` (sed -i), Commands + Direct + Bash aliases
**Effort:** 3 (by-diff budget)
**Reviewer roster:** general (two independent general passes: `general` = correctness/contracts, `general-tests` = test coverage vs requirements)
**Session IDs:** review oracle Claude Code session (headless ganda task work)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
