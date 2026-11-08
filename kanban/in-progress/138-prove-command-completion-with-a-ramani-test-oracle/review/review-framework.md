# Review framework

## Budget (by-diff)

- Lines changed: 695
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Review framework — task 138

**Date:** 2026-11-08
**Host task:** kanban/in-progress/138-prove-command-completion-with-a-ramani-test-oracle/
**Diff scope:** branch `task/138-prove-command-completion-with-a-ramani-test-oracle` at b44b799 vs merge base c62412b (master). Product paths: `tests/.editorconfig`, `tests/timewarp-amuru/Directory.Build.props`, `tests/timewarp-amuru/command-completion/*.cs`, `tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs`.
**Plan / brief:** task.md Requirements and `## Goal (implementer restatement)`. A test-only Ramani machine that is the oracle for how one command run finishes. Production runners stay unchanged.
**Effort:** 2 (by-diff budget), roster axis general
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5, headless `ganda task work`), session id not exposed to the oracle

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
