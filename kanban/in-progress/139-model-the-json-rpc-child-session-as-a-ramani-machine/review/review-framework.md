# Review framework

## Budget (by-diff)

- Lines changed: 285
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Framework — task 139

**Date:** 2026-11-08
**Host task:** kanban/in-progress/139-model-the-json-rpc-child-session-as-a-ramani-machine/
**Diff scope:** branch `task/139-model-the-json-rpc-child-session-as-a-ramani-machi` vs `master` (merge base), HEAD 3704ce6. 14 files: `Directory.Packages.props`, `source/.editorconfig`, `source/timewarp-amuru/timewarp-amuru.csproj`, `source/timewarp-amuru/json-rpc/*.cs` (8 files), `tests/timewarp-amuru/single-file-tests/json-rpc/*.cs` (2 files), `task.md`.
**Plan / brief:** task.md `## Requirements` and `## Goal (implementer restatement)`: an internal exhaustive Ramani machine for the JSON-RPC child session, a narrow process seam with a fake, and SendAsync tests that read `MachineStep`.
**Effort:** 3 (by-diff budget). Roster axis: general only.
**Reviewer roster:** general (the review oracle, single pass, with falsifiable claims re-checked by a scratch probe outside the repo)
**Session IDs:** review oracle claude-opus-5-5 (Claude Code, headless `ganda task work`), 2026-11-08

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity: bug | suggestion | nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the call sites around it. Re-check falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
