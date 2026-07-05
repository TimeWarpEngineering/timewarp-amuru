# Delete dead public surface

## Description

Phase 1 of task 094: delete public types with zero callers (verified across amuru AND ganda repos, 2026-07-04 review + Grok cross-check). One PR, no behavior change for consumers of kept API. Do this before 093 (XML docs) and 096 (AOT) — every deleted type is doc/trim work avoided.

## Checklist

- [x] Delete `native/utilities/Post.cs` — no-op stub returning success; canonical implementation is ganda `services/post/` (→ Kijamii later, tasks 031/033)
- [x] Delete `native/utilities/GenerateColor.cs` — zero callers; ganda/Zana reimplemented inline
- [x] Delete `native/utilities/ConvertTimestamp.cs` — zero callers; ganda reimplemented inline
- [x] Delete `Installer.cs` — zero callers; ganda uses `Zana.Installer` (its security fixes belong to Zana's copy, ganda board)
- [x] Delete `extensions/CommandBuilderExtensions.cs` — zero usage outside its own XML examples
- [x] Internalize `git-commands/Git.WorktreePorcelainParser.cs` (keep `WorktreeEntry` public); amuru tests that referenced the parser directly switch to the public `Git` APIs or internal access within the test project setup
- [x] Remove corresponding items from task 105 coverage checklist (already noted there)
- [x] Build + full suite green (357/358, 1 known env skip)

## Notes

Deleting `CommandBuilderExtensions` re-scopes task 100 (done 2026-07-05: 100 is now WithNoValidation consistency only). Paths relative to `source/timewarp-amuru/`.

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)
