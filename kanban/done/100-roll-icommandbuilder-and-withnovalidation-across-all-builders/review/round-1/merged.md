# Round 1 — merged findings
**Date:** 2026-10-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 1 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: wontfix
- File: source/timewarp-amuru-tools/fzf-command/Fzf.cs:107
- Description: `FromCommand` path builds `Shell.Run(src, args, Options).Pipe("fzf", fzfArguments)`. Verified: `CommandResult.Pipe` creates the next stage via internal `CommandExtensions.Run(executable, arguments)` with default options, so `WithZeroExitCodeValidation()` (and working dir/env) apply to the source command only, not the fzf stage.
- Suggestion: Pipe overload taking options, or document.
- Source: general
- Disposition notes: Behaviour is pre-existing (master had the same `.Pipe("fzf", …)`); a fix needs a new public core API (`CommandResult.Pipe(executable, args, CommandOptions)`) in the TimeWarp.Amuru package, out of scope for this Tools-only task. Limitation documented in the Fzf.cs Design region and the `WithZeroExitCodeValidation` XML doc. Decided by: review oracle.

### M2 — Severity: suggestion — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/fzf-command/fzf-builder.validation.cs:20
- Description: Fzf validation test had no `[Timeout]`; only FromInput covered.
- Suggestion: Add Timeout; FromCommand case.
- Source: general
- Disposition notes: Added `[Timeout(30000)]`. FromCommand case not added — it would only pin the documented M1 limitation.

### M3 — Severity: suggestion — Status: fixed
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.validation.cs:120
- Description: No parent-then-child propagation case, no `WithNoValidation` after strict case.
- Suggestion: Add both.
- Source: general
- Disposition notes: Added `SlnParent_Should_PassValidationToChildCreatedAfterIt` and `NoValidationAfterStrict_Should_RestoreReportedFailure`; 12/12 pass.

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-amuru-tools/dot-net-commands/DotNet.DevCerts.cs:7
- Description: Sub-builders snapshot parent options at creation; undocumented for users.
- Suggestion: Note in dot-net.md.
- Source: general
- Disposition notes: Paragraph added to dot-net.md after the validation section.

## Duplicates / conflicts

- M2's FromCommand suggestion overlaps M1; folded into M1 disposition.
