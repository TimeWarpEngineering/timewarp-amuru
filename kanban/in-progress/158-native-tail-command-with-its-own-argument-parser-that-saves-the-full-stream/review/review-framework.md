# Review framework

## Budget (by-diff)

- Lines changed: 788
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Framework — task 158

**Date:** 2026-11-27
**Host task:** kanban/in-progress/158-native-tail-command-with-its-own-argument-parser-that-saves-the-full-stream/
**Diff scope:** branch `task/158-native-tail-command-with-its-own-argument-parser` vs `master` (commits 936ef62, acba1f3, 5a9af3f)
**Plan / brief:** task.md `## Requirements` and `## Goal (implementer restatement)`: native `Tail` with its own parser, byte slice, headers, saved stdin stream with note/cap/cleanup, polling `-f`, comparison tests, bash alias, docs.
**Effort:** 3 (by-diff budget), roster axis general only
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle under `ganda task work` (headless print mode; no session id exposed)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading
  the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to
  the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the
  command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
