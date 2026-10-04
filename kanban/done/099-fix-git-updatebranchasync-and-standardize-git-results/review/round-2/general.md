# Round 2 — general (re-review of fix delta)
**Date:** 2026-10-04
**Scope reviewed:** uncommitted fix delta after round 1: `Git.BranchExists.cs`, `Git.UpdateBranch.cs`, `Git.UpdateWorktree.cs`, `git.branch-exists.cs`, `git.update-branch.cs`, `documentation/developer/reference/git-commands.md`

## Summary

Re-checked M1–M4 against the fix delta. `show-ref --verify --quiet` exits 1 for a missing ref (verified against real git), and the real-repo tests cover both outcomes. `UpdateBranchAsync` now chooses between pull and fetch from the porcelain worktree list, which correctly handles normal, bare, and linked-worktree layouts. A failed worktree list falls through to the refspec fetch, which then reports the git error. Behavior change to record in Results: when the branch is checked out, `BranchPath` is now the work-tree path (it was null for a normal repo). Verification: changed runfiles 5/5, 4/4 and 1/1 passed. Full runner: Passed 551, Skipped 1, Total 552, Failed 0. `ganda repo audit` passes.

## Issues

<!-- None. M5 wontfix carried from round 1. -->
