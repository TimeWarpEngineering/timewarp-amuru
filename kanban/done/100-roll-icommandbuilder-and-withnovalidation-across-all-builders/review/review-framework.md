# Review framework

## Budget (by-diff)

- Lines changed: 2037
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 100

**Date:** 2026-10-05
**Host task:** kanban/to-do/100-roll-icommandbuilder-and-withnovalidation-across-all-builders/
**Diff scope:** branch task/100-roll-icommandbuilder-and-withnovalidation-across-a vs master (2037 lines)
**Plan / brief:** task.md Decisions — both validation controls on every Tools builder; WithConfig→WithConfigFile, WithTargetFramework→WithFramework; PublicAPI bookkeeping
**Effort:** 3 (budget by-diff); roster axes: general
**Reviewer roster:** general (Claude Sonnet subagent); orchestrator merge + verification by review oracle (Claude Opus 5.5)
**Session IDs:** review oracle — ganda task-work review node 2026-10-05; general subagent accaaa8e773eeec7f

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
