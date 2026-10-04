# Review framework

## Budget (by-diff)

- Lines changed: 589
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 088

**Date:** 2026-10-04
**Host task:** kanban/to-do/088-fix-or-remove-fzf-selectwithfzf-stub-and-fragile-input-methods/
**Diff scope:** branch task/088-fix-or-remove-fzf-selectwithfzf-stub-and-fragile-i vs master (commit 1fd028e)
**Plan / brief:** Implement `ExtractFzfArguments`; FromInput via stdin; FromFiles in-process; FromCommand quote-aware split; align label-pos overloads; real-execution test.
**Effort:** 2 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** claude review oracle (task-work 2026-10-04)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
