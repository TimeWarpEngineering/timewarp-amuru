# Review framework — task 111-003

**Date:** 2026-09-30
**Host task:** kanban/to-do/111-003-fix-scriptcontext-leak-deadlock-and-native-force-extras/
**Diff scope:** branch `task/111-003-fix-scriptcontext-leak-deadlock-and-native-force-e` vs `origin/master` (commits `d925b37`, `c8fc9f7`)
**Plan / brief:** Parent 111 findings M10–M14. M10/M11 ScriptContext leak and OnExit deadlock. M12/M13 native force extras already folded into task 104 (`Direct.RemoveItem`); this task verifies them. M14 Unix PathResolver execute-bit check and narrowed docs.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Grok review oracle (2026-09-30); general reviewer subagent (2026-09-30)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Do not treat sibling 111-004/111-005 scope, or 104 items listed as out of scope in task.md, as findings
- M12/M13 are in scope only as verification that task 104's `Direct.RemoveItem` behavior still matches the requirement (no reimplementation expected)
