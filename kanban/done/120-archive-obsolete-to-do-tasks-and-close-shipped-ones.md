# Archive obsolete to-do tasks and close shipped ones

## Description

Board housekeeping after a full review of the 33 to-do tasks against master at 1.1.1 (2026-10-04). Archive tasks whose premise no longer exists in this repo, and move to done tasks whose work already shipped.

## Checklist

### Archived (obsolete)

- [x] 004 Claude fluent API — modeled on Gwq, removed in 067; per-tool builders abandoned
- [x] 022 Build process commands — duplicates 024/025; DotNet builders cover it
- [x] 026 Native interactive commands — Fzf builder covers selection
- [x] 031 Create Kijamii library — no Kijamii code in this repo (Amuru/Zana/Kijamii split)
- [x] 033 Extract Kijamii to own repo — nothing to extract here
- [x] 040 Fix Nuru routing in multiavatar — multiavatar extracted in 032; recheck on that repo's board
- [x] 042 Expose StreamJsonRpc features — StreamJsonRpc removed in 084
- [x] 043 Evaluate extension methods — Ghq/Gwq removed in 067
- [x] 070 WorktreeService.RemoveAsync — bug lives in ganda; Amuru side already checks the result

### Done (already shipped)

- [x] 005 Git fluent API — Git.FindRoot and ~18 Git methods exist in Tools with tests
- [x] 050 Git native commands — FindRoot and GetRepositoryNameAsync exist with tests
- [x] 107 Cut 1.0.0 release — 1.0.0 shipped 2026-07-05; 1.1.0 and 1.1.1 on 2026-09-23

## Notes

Remaining to-do after this pass, ordered by value: 087, 088, 099, 094-004, 100, then 093, 105, 044, 001, 002, 083, 119 (blocked until .NET 11 GA 2026-11-10).

Owner decisions still pending: the unbuilt Native command family (021, 023, 024, 025, 027, 045) and whether to merge 106 into 082 as its own cosmetic-rename release.

## Session

- Created: 563546 (2026-10-03)
