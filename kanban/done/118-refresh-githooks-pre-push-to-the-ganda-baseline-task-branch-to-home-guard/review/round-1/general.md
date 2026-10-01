# Round 1 — general
**Date:** 2026-10-01
**Scope reviewed:** commit 31ed911 (`.githooks/pre-push.cs`, task.md)

## Summary

The change adds a `taskToHome` guard to the pre-push hook. It refuses `refs/heads/task/*` → `refs/heads/master|main`
before the existing HEAD-is-home check, so it applies from any HEAD. Raw-sha sources (kanban publish)
and task→task pushes are still allowed. The guard reuses the existing `IsHomeBranchDest` helper and
the same pragma/stderr idiom, and the diff is additive. I re-verified it: `ganda repo audit` passes all checks;
stdin smoke gives task/x→master exit=1, raw sha→master exit=0, task/x→task/x exit=0. Risk is low.

## Issues

None.
