# Review framework

## Budget (by-diff)

- Lines changed: 513
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

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
