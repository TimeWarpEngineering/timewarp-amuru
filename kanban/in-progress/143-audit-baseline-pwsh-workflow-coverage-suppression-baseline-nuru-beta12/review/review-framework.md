# Review framework

## Budget (by-diff)

- Lines changed: 455
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope — task 143

**Date:** 2026-11-11
**Host task:** kanban/in-progress/143-audit-baseline-pwsh-workflow-coverage-suppression-baseline-nuru-beta12/
**Diff scope:** branch `task/143-audit-baseline-pwsh-workflow-coverage-suppression` vs `master` (merge base; commits a6d89a8..86fd620)
**Plan / brief:** task.md `## Description` and `## Goal (implementer restatement)`. Clear the audit baseline, put workflow `run:` steps in pwsh with the same behavior, pin Nuru 4.0.0-beta.12, and resolve ci-code-coverage.
**Effort:** 2 (budget by diff). Roster: general only. No specialist axes were triggered.
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle session in this ganda task-work walk (Claude Code headless)

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity: bug | suggestion | nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Address the diff and surrounding call sites. Re-verify falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
- `ganda repo audit --fix` output is policy (global prefs). The seeded suppression baseline, the githook pin moves, and the package pins (including TimeWarp.Terminal 2.0.0-beta.1) are not findings unless they break the build or tests.
