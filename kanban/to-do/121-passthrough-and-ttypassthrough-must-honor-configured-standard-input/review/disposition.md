# Disposition — task 121

**Date:** 2026-10-05
**Outcome:** clean
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 (general, effort 2) found one bug (M1): both passthrough modes ignored stdin that came from a `Pipe` composition, which affected `Fzf.FromCommand`. It was fixed on this task with an `IsPipeline` flag and two new tests. Round 2 confirmed the fix and found no new issues. Build: 0 warnings. Suite: 567 passed, 1 skipped, 0 failed.

## Exception log (if accepted-exceptions)

None.

## Escalations

- None.
