# Review framework — task 132

**Date:** 2026-10-14
**Host task:** kanban/in-progress/132-marketing-for-the-timewarpamuru-200-release/
**Diff scope:** branch `task/132-marketing-for-the-timewarpamuru-200-release` vs `master` (commit `2764ba6`, `documentation/posts/**` plus the task file move)
**Plan / brief:** Unpublished launch pack for TimeWarp.Amuru 2.0.0: launch and follow-up blogs, X post and reply, Nostr note, follow-up blip, 1200×675 card, video script, posts readme with FreezeTeam stub. Every claim must match shipped 2.0.0 behavior and the release notes.
**Effort:** 2 (Budget.ByDiff: 683 lines changed; turn cap 120)
**Reviewer roster:** general (orchestrator review plus a delegated source fact-check of every technical claim against `source/` at `v2.0.0` and `documentation/release-notes/2.0.0.md`)
**Session IDs:** review oracle Claude Opus 5.5 (ganda task work, headless); fact-check subagent a72301428ac5f6fb8 (Sonnet)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
