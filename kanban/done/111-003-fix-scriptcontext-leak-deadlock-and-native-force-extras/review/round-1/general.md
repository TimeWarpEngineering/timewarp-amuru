# Round 1 — general
**Date:** 2026-09-30
**Scope reviewed:** branch task/111-003 vs origin/master (ScriptContext, PathResolver, their tests; Direct.RemoveItem M12/M13 verification)

## Summary

ScriptContext factories call `SetCurrentDirectory` before `PushLive`. A throw leaves `LiveContexts` unchanged, does not return the instance, and does not run that context's `onExit`; a push that fails afterward is popped by `AbandonIfPushed` with the same guarantee. Dispose and process-exit unwind pop and mark contexts under `SyncLock`, then restore directories and invoke `onExit` only from `FinishUnwind`, so a nested `ScriptContext` can take the lock. `Direct.RemoveItem` (unchanged versus `origin/master`) skips attribute clears on file reparse points and clears the root directory read-only bit after children while still skipping reparse points. Unix `PathResolver` requires a user, group, or other execute bit and the public summary says it approximates `which` / `where` rather than matching them.

## Issues

None.
