# Fix ScriptContext leak deadlock and native force extras

## Description

Parent **111** round-1 findings **M10–M14**. New native/ScriptContext defects **not** already tracked by **104**.

Pinned tree: `fbd5d276fc5a936136a55d981fc121a23b991493`. Evidence: parent `review/round-1/merged.md`.

## Requirements

- **M10 (bug)** `ScriptContext.cs:73-76` and `:97-100` — factory pushes onto `LiveContexts` before `SetCurrentDirectory`. A throw leaks a live stack entry the caller cannot dispose. Change cwd before push, or pop/cleanup on failure.
- **M11 (bug)** `ScriptContext.cs:143` — `OnExit` runs while `SyncLock` is held; nested ScriptContext in `onExit` deadlocks. Invoke `OnExit` outside the lock.
- **M12 (bug)** `native/file-system/direct/Direct.RemoveItem.cs:21-32` — **different from 104** (104 is directory `SearchOption.AllDirectories`). `force` on a *file* symlink can clear read-only on the target, then delete only the link. Delete the link without mutating target attributes.
- **M13 (bug)** `Direct.RemoveItem.cs:63-80` — `RemoveReadOnlyAttribute` never clears read-only on the **root** directory itself, so `force`+`recursive` can still fail on Windows. Clear the root bit after children; still skip reparse points (104).
- **M14 (suggestion)** `PathResolver.cs:166-175` — Unix search is `File.Exists` only while docs claim `which` equivalence. Check execute bits or narrow the docs.

Do **not** clone 104 items (dir-symlink AllDirectories walk, force not-found, EnumeratorCancellation, Commands sync-over-async, Cd global cwd, Rm twin bools, IAsyncEnumerable naming). GetChildItem `Task.Yield` per entry folds into 104.

## Checklist

- [x] M10 failed SetCurrentDirectory does not leak LiveContexts
- [x] M11 OnExit not under SyncLock
- [x] M12 force on file symlink does not mutate the target
- [x] M13 force+recursive clears root directory read-only
- [x] M14 PathResolver Unix execute-bit or docs
- [x] `## Results` + `### How to validate`

## Notes

Parent: **111**. Source: `review/round-1/native-fs.md`. Coordinate with **104** if both touch `Direct.RemoveItem`.

M12 and M13 were already folded into **104** (`Direct.RemoveItem` skips file-symlink attribute changes and clears the root directory read-only bit after children, still skipping reparse points). This task verified that code and its tests; it did not edit `Direct.RemoveItem`.

## Session

- Implementer: Grok task-work oracle (2026-09-29)

## Results

- **M10:** `ScriptContext` changes the working directory before pushing onto `LiveContexts`. A failed `SetCurrentDirectory` leaves the stack unchanged, restores the captured directory if the change had already happened, and does not run `onExit`.
- **M11:** Dispose and process-exit unwind pop and mark contexts under `SyncLock`, then restore directories and invoke `onExit` outside the lock. A nested `ScriptContext` inside `onExit` no longer deadlocks. Handlers are unregistered before `onExit`, so a nested context can register them again.
- **M12 / M13:** Already present from task 104. Re-ran `direct.remove-item` (6 passed), including file-symlink force (target stays read-only) and read-only root directory force+recursive.
- **M14:** Unix `PathResolver` matches only files with a user, group, or other execute bit. Docs no longer claim exact `which` / `where` equivalence; Windows still uses PATHEXT plus a bare name. Execute-bit check is mode bits, not `access(2)`.

### Files changed

- `source/timewarp-amuru/ScriptContext.cs`
- `source/timewarp-amuru/native/PathResolver.cs`
- `tests/timewarp-amuru/single-file-tests/script-support/script-context.from-entry-point.cs`
- `tests/timewarp-amuru/single-file-tests/script-support/script-context.from-relative-path.cs`
- `tests/timewarp-amuru/single-file-tests/native/path-resolver.cs`

### Key decisions / deviations

- M12/M13 were not reimplemented. Task 104 already owned `Direct.RemoveItem`.
- Unix lookup checks execute mode bits rather than P/Invoke `access(2)`. A file the caller cannot execute (owner bits deny execute while group/other allow it) can still be returned. The public summary says "approximating which", not equivalent.

### Test outcomes

- `script-context.from-entry-point.cs`: 7 passed
- `script-context.from-relative-path.cs`: 2 passed
- `path-resolver.cs`: 12 passed
- `direct.remove-item.cs`: 6 passed
- Aggregate `tests/timewarp-amuru/multi-file-runners/run-tests.cs`: **499 passed, 1 skipped, 0 failed** (500 total)
- Roslynk diagnostics on `timewarp-amuru.slnx`: 0 errors, 0 warnings

### How to validate

**Smoke**

```bash
cd /path/to/timewarp-amuru
dotnet run tests/timewarp-amuru/single-file-tests/script-support/script-context.from-entry-point.cs
dotnet run tests/timewarp-amuru/single-file-tests/script-support/script-context.from-relative-path.cs
dotnet run tests/timewarp-amuru/single-file-tests/native/path-resolver.cs
dotnet run tests/timewarp-amuru/single-file-tests/native/file-system/direct.remove-item.cs
```

**Expect**

- `from-entry-point`: 7 passed, including `OnExitNestedContext_Should_NotDeadlock` (completes, does not hang) and `OnExit_Should_ObserveRestoredDirectory`
- `from-relative-path`: 2 passed, including `MissingTarget_Should_NotLeakLiveContext` (`DirectoryNotFoundException`, `onExit` of the failed context does not run)
- `path-resolver`: 12 passed, including `NonExecutableEarlierOnPath_Should_SkipToExecutable` (non-executable earlier PATH hit is skipped; direct path to that file is null)
- `direct.remove-item`: 6 passed, including `FileSymlinkWithForce_Should_DeleteLinkWithoutMutatingTarget` and `ReadOnlyRootDirectory_ForceRecursive_Should_Succeed`

**Automated gate**

```bash
cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs
# expect: Grand Total Passed: 499, Failed: 0, Skipped: 1, Total: 500
```

If a standalone `dotnet run <test>.cs` disagrees with the source you just changed, clear the runfile cache (`ganda runfile cache --clear` or `rm -rf ~/.local/share/dotnet/runfile/<test-name>-*`) and re-run.

**Not in scope:** 104 items (directory-symlink `AllDirectories` walk, force not-found, `EnumeratorCancellation`, Commands sync-over-async, Cd global cwd, Rm twin bools, `IAsyncEnumerable` naming). Unix `access(2)` credential check.
