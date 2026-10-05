# Round 1 — merged findings
**Date:** 2026-10-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-amuru/core/command-result.cs:238, :308, :524
- Description: Passthrough modes ignore pipe-sourced stdin. PassthroughAsync replaces the upstream stage with console stdin, and TtyPassthroughAsync runs only the last stage. Both affect `Fzf.FromCommand` and any `Pipe(...)` composition.
- Suggestion: Track pipeline-owned stdin on CommandResult. Skip console stdin in PassthroughAsync and throw in TtyPassthroughAsync. Add tests.
- Source: general
- Disposition notes: Fixed on task 121. CommandResult.IsPipeline is set by PipeCore. PassthroughAsync keeps the upstream stage as stdin. TtyPassthroughAsync throws "TtyPassthroughAsync cannot run a pipeline; the upstream stage would be dropped". New tests: PassthroughAsync Pipeline_Should_FeedTheUpstreamStage and TtyPassthroughAsync Pipeline_Should_Throw.

## Duplicates / conflicts

- None.
