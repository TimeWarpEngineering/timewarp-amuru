# Review framework

## Budget (by-diff)

- Lines changed: 851
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Round 1 framing

**Date:** 2026-10-31
**Host task:** `kanban/in-progress/119-upgrade-to-dotnet-11/`
**Diff scope:** branch `task/119-upgrade-to-dotnet-11` (HEAD `75b0006`) against merge base `53950d7` with `origin/master`. Product commits are `89f5da6` (audit fixes) and `6bdf9c2` (net11.0 retarget). Kanban commits are in scope only to check claims.
**Plan / brief:** `task.md` `## Goal (implementer restatement)` and the checklist: hard-cut to `net11.0`, SDK `11.0.100-rc.3.26522.148`, NetAnalyzers on the same RC build, docs say .NET 11, `<Version>` becomes `3.0.0`.
**Effort:** 2 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle, Claude Code (claude-opus-5-5), headless `ganda task work` print mode. The harness exposes no vendor session id.

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the call sites around it. Re-check claims that can be falsified against the repo and the installed SDK.
- Whatever `ganda repo audit --fix` changes is policy and is not a finding (`.githooks/pre-push.cs`, `glossary.md` scaffold, Nuru pins).
- A round is frozen once the next one opens. New work goes in `round-(N+1)/`.

## Round 2 framing

**Date:** 2026-10-31
**Diff scope:** the fix delta `75b0006..0b1cccd` (commit `0b1cccd`). Round 1 base is in `round-1/base.sha`.
**Plan / brief:** close round-1 M1, M2, and M3 (`round-1/merged.md`).
**Effort:** 1 (by-diff, 188 lines)
**Reviewer roster:** general
**Session IDs:** review oracle, Claude Code (claude-opus-5-5), headless `ganda task work` print mode. The harness exposes no vendor session id.

## Round 3 framing

**Date:** 2026-10-31
**Diff scope:** product delta `0b1cccd..445f7ad` (commits `bd1f3c4` sample monikers to `net11.0`, `bc3b773` executable bit restored). A tree-wide sweep for current .NET 10 references was also in scope, because the open work asks for every current reference to show .NET 11.
**Plan / brief:** `task.md` `## Open work (Steve, 2026-10-31)` and `## Goal (implementer restatement)`.
**Effort:** 3 (by-diff budget, 851 lines, the full branch against merge base `53950d7`)
**Reviewer roster:** general
**Session IDs:** review oracle, Claude Code (claude-opus-5-5), headless `ganda task work` print mode. The harness exposes no vendor session id.
