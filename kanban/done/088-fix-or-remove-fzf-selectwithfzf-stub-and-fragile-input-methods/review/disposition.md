# Disposition — task 088

**Date:** 2026-10-04
**Outcome:** accepted-exceptions
**Rounds:** 1
**Final open count:** 0

## Summary

A general reviewer (effort 2) found no bugs. The stub is fixed, the input methods are portable, and the label-pos overloads are aligned without breaking the API. A real-execution test proves fzf receives the configured options. Three findings were raised and all are wontfix: a core empty-stdin behavior change (intended, documented, belongs in release notes); a pre-existing gap where Passthrough/TtyPassthrough drop configured stdin (core scope, not a regression); and a redundant branch whose removal breaks the build with CS0414.

## Exception log

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M1 | suggestion | Intended contract change, documented in Design region; changelog belongs to release PR | review oracle |
| M2 | suggestion | Pre-existing, core execution-mode scope; SelectAsync path works | review oracle |
| M3 | nit | Collapsing it triggers CS0414 (UseStdin unread) | review oracle |

## Escalations

- None.
