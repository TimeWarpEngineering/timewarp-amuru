# Review framework

## Budget (by-diff)

- Lines changed: 704
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 087

**Date:** 2026-10-04
**Host task:** kanban/to-do/087-fix-invalid-cli-flags-in-dotnet-command-builders/
**Diff scope:** branch task/087-fix-invalid-cli-flags-in-dotnet-command-builders vs master (ea980e6, c6a49b0)
**Plan / brief:** task.md checklist — fix flags the dotnet SDK rejects/ignores in the DotNet builders; add real-SDK smoke tests
**Effort:** 2 (by-diff budget); roster axes: general
**Reviewer roster:** general
**Session IDs:** claude review oracle (ganda task-work, 2026-10-04)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
