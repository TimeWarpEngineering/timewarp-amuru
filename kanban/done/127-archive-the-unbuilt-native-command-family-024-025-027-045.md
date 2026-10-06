# Archive the unbuilt Native command family (024, 025, 027, 045)

## Description

Board housekeeping after the 2.0.0-beta.2 release. Four native-command cards from 2025-12-12 were never started and have no callers or hand-rolled equivalents anywhere in the consumers (amuru tools/hooks/tests, ganda source, nuru dev-cli), unlike the text commands (021) which replaced 31 hand-rolled read-modify-write sites in ganda.

## Checklist

- [x] 024 Implement Native Process Commands (Ps/Kill) — no callers; `System.Diagnostics.Process` covers the rare need
- [x] 025 Implement Native System Info Commands — no callers; `PathResolver` (073) is the only piece that was ever wanted
- [x] 027 Implement Native Archive Commands — no callers
- [x] 045 Implement Native Tee Command — no callers; `StreamStdoutAsync` + file write covers it

## Notes

Decided 2026-10-07 after the 021 rewrite: keep Native scoped to file-system (112) and text (021). 083 (native JSON-RPC client) remains on the board as a separate decision. If any of these come back, open a new card with the concrete consumer that needs it.

## Session

- Created: 522eb63d (2026-10-07)
