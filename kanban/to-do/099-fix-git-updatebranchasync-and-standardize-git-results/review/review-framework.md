# Review framework

## Budget (by-diff)

- Lines changed: 1804
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 099

**Date:** 2026-10-04
**Host task:** kanban/to-do/099-fix-git-updatebranchasync-and-standardize-git-results/
**Diff scope:** branch `task/099-fix-git-updatebranchasync-and-standardize-git-resu` vs `master` (merge-base), product code under `source/timewarp-amuru-tools/git-commands/`, tests under `tests/timewarp-amuru/`, docs (`documentation/developer/reference/git-commands.md`, `readme.md`, `skills/amuru/SKILL.md`)
**Plan / brief:** task.md checklist — fix checked-out `UpdateBranchAsync`, standardize git result records, DefaultBranch naming, repoPath overloads, WorktreeList failure result, relative gitdir resolution, leading `origin/` strip; real-execution tests.
**Effort:** 3 (by-diff budget), roster axes: general
**Reviewer roster:** general
**Session IDs:** review oracle (claude-opus-5-5, 2026-10-04); general reviewer subagent

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
