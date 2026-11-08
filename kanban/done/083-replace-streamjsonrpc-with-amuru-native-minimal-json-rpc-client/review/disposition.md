# Disposition — task 083

**Date:** 2026-11-08
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 raised M1 through M8 (2 bugs, 4 suggestions, 2 nits). The fix pass (commits 2afada3 and b47b9bb) closed all eight. Round 2 re-verified each one against the code and re-ran the tests: the client file passed 20 of 20, including the real ModelContextProtocol 2.4.0 stdio server; the machine file passed 31 of 31; the guard file passed 14 of 14. The library builds Release with 0 warnings. Round 2 raised two new nits, M9 and M10. Both are marked wontfix because neither changes product behavior. Final counts are bug 2 fixed, suggestion 4 fixed, and nit 2 fixed plus 2 wontfix.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M9 | nit | The `Task.Yield` loop on start is bounded at 2 s and covers only the pump's synchronous step from effect to state assignment. It is not worth a fix pass. | review oracle (round 2) |
| M10 | nit | The MCP server build cache is test-only. The Windows `.exe` name only causes a rebuild. A stale build needs a props edit on a developer machine, and touching the source or deleting the temp directory fixes it. | review oracle (round 2) |

## Escalations

- None.
