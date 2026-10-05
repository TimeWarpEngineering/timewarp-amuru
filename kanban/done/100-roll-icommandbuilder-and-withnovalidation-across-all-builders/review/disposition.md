# Disposition — task 100

**Date:** 2026-10-05
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

One general reviewer at effort 3 found 1 bug, 2 suggestions, and 1 nit. The two suggestions and the nit were fixed with new tests, a test timeout, and a dot-net.md note. The bug (Fzf `FromCommand` validation does not reach the piped fzf stage) predates this task. Fixing it needs a new core `CommandResult.Pipe` overload that takes options, so it is accepted as a documented limitation.

## Exception log

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M1 | bug | Pre-existing; fix requires a new public API in the core TimeWarp.Amuru package (out of scope for this Tools-only task); documented in Fzf.cs Design region and XML doc | review oracle |

## Escalations

- None.
