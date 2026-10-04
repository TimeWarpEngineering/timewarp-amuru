# Disposition — task 099

**Date:** 2026-10-04
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

The round 1 general reviewer (effort 3, roster axis general) raised 2 bugs, 1 suggestion and 2 nits. Fixed on this task:
- `BranchExistsAsync` now runs `show-ref --quiet`.
- `UpdateBranchAsync` uses the worktree list for checked-out detection, which covers bare repositories and linked worktrees.
- `UpdateWorktreeAsync` uses a fast-forward-only pull.

Round 2 re-verified these fixes and found nothing new. One nit is accepted as wontfix.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M5 | nit | The CWD change is the purpose of the relative-gitdir regression test. It follows the established repo pattern (restore in `finally`), and the full runner is green. | orchestrator (review oracle) |

## Escalations

- None.
