# Disposition — task 136

**Date:** 2026-11-07
**Outcome:** accepted-exceptions
**Rounds:** 1
**Final open count:** 0

## Summary

The registration cache meets every requirement in task 136. There is one walk per package id per process, shared by `SearchAsync`, `GetLatestVersionsAsync`, and in-flight callers. Latest selection stays listed-only. Search still includes unlisted versions. A 404 or a failure is not cached, and callers that joined that attempt share its outcome. Public method signatures are unchanged, and the only API addition is the handler constructor. The implementer's goal restatement matches the task.

The round-1 general review raised one nit (M1): a partial list is cached after an external page 404. The review oracle accepted it as wontfix, with the rationale below. Tests verified by the reviewer:

- Registration-cache tests: 9/9 passed.
- Aggregate runner: 741 passed, 1 skipped (existing skip), 0 failed.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M1 | nit | Page-404 tolerance is pre-existing. The requirement targets the index 404 and thrown failures, and both are handled. The consumer is a short-lived CLI process. Cache freshness belongs to ganda task 538. | review oracle (claude-opus-5-5) |

## Escalations

- None.
