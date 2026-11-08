# Disposition — task 139

**Date:** 2026-11-08
**Outcome:** accepted-exceptions
**Rounds:** 3
**Final open count:** 0

## Summary

Three review rounds, with general as the only reviewer. Round 1 used effort 3 and raised M1–M5. Round 2 used effort 3 and raised M6–M8. Round 3 used effort 2, by diff. Two fix passes on this task id closed eight findings: duplicate ids, notification and server-request frames, the Stopping completion, both test files running alone, a test race, keeping unread results, and moving `Release` onto the pump. Round 3 raised M9, a host-side lookup of the plain map that could run while the pump writes it. M9 is deferred to task 083 because that task owns the public client's read path. At c20e40a: `dev build` was clean, `dev test` passed 784 of 785 with 1 skipped and 0 failed, the session files passed alone on every repeated run, and `ganda repo audit` passed.

Final counts: 4 bugs fixed. 4 suggestions fixed and 1 wontfix. No nits.

## Exception log

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M9 | suggestion | Nothing in task 139 reads the map concurrently, because the tests are the only host and they serialize their reads. Task 083 builds the public client and chooses how a caller waits for a result (a concurrent map, a read on the pump, or an awaitable completion). The handoff is recorded in task.md Notes. | review oracle, round 3 |

## Escalations

- None.

## Artifacts

- `review/review-framework.md`
- `review/round-1/merged.md`, `review/round-2/merged.md`, `review/round-3/merged.md` (final ledger)
- `review/round-3/general.md`
