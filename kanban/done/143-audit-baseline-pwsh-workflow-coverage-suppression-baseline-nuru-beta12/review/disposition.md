# Disposition — task 143

**Date:** 2026-11-11
**Outcome:** accepted-exceptions
**Rounds:** 1
**Final open count:** 0

## Summary

Round 1 used effort 2 with the general reviewer. It found no bugs. The pwsh rewrite of "Run CI Pipeline" keeps the release, break-glass, and default branching. The rewrite of "Keep last two Packages artifacts" keeps the listing, the skip-if-not-listed check, the ordinal newest-first order, and keep-2. All pwsh `run:` blocks parse cleanly. The ci-code-coverage opt-out is the one the task sanctions, under `[ganda.audit]` with a reason and the task id. The audit `--fix` output is policy. One suggestion was raised, M1 (the tool-path assertion depends on the SDK version), and it was accepted as wontfix.

## Exception log

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M1 | suggestion | It cannot cause a false pass: if the SDK changes, the test fails loudly. It passes on SDK 11.0.100. The PR CI checklist item covers the CI SDK. Reopen if CI hits `DirectoryNotFoundException`. | review oracle |

## Escalations

- None.
