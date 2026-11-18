# Review framework

## Budget (by-diff)

- Lines changed: 1642
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-11-18
**Host task:** kanban/in-progress/153-add-with-timeout-and-missing-options-to-the-dot-net-build-run-and-test-builders/
**Diff scope:** branch `task/153-add-with-timeout-and-missing-options-to-the-dot` vs merge-base with `master` (commits bc938bd, fb7927c, plus kanban moves)
**Plan / brief:** task.md Requirements and `## Goal (implementer restatement)`: WithTimeout/WithTimeoutGracePeriod on Build/Run/Test, WithArtifactsPath on Build (and Run/Test/Publish/Pack), Test extra-argument pass-through, Run --no-cache/--nologo, PublicAPI.Unshipped entries.
**Effort:** 3 (budget by diff); roster axis general only
**Reviewer roster:** general (review oracle, Claude Opus 5.5)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Zero issues is a valid outcome; do not invent issues
- Re-verify claims by reading the diff and the kitchen `## Results` / `### How to validate`
- No full build, test run, audit, or hooks; one targeted command per suspected bug, named in the finding
- Prior rounds are immutable; new work goes in `round-(N+1)/`
