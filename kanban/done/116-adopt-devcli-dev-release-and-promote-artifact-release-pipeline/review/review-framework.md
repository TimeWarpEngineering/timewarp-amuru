# Review framework — task 116

**Date:** 2026-09-23
**Host task:** kanban/to-do/116-adopt-devcli-dev-release-and-promote-artifact-release-pipeline/
**Diff scope:** branch task/116-adopt-devcli-dev-release-and-promote-artifact-rele (HEAD cb0f23a) vs merge-base with origin/master (612c018); commits 9c06bcb, cb0f23a
**Plan / brief:** Adopt TimeWarp.Nuru.DevCli (clean/check-version/release/self-install), rewrite `dev workflow --mode release` as the promote-artifact pipeline (tag-gate → check-version → locate-run → download-artifact → verify → push), update workflow.yml (break-glass inputs, GH_TOKEN, --file), add releasing guide, stop packing samples, add .timewarp/dev.jsonc.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle: claude (Fable 5.1) headless via `ganda task work 116`; reviewer sub-agent: claude sonnet (general)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
