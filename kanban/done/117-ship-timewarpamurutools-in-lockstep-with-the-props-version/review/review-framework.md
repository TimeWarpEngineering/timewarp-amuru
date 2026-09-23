# Review framework — task 117

**Date:** 2026-09-23
**Host task:** kanban/to-do/117-ship-timewarpamurutools-in-lockstep-with-the-props-version/
**Diff scope:** branch `task/117-ship-timewarpamurutools-in-lockstep-with-the-props` vs `origin/master` (commits d09418d..44d3167), product files only: `.timewarp/dev.jsonc` (deleted), `AGENTS.md`, `documentation/developer/guides/releasing.md`, `readme.md`, `source/Directory.Build.props`, `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj`, `tools/dev-cli/endpoints/workflow-command.cs`
**Plan / brief:** task.md Description + Checklist — remove the Tools per-csproj `<Version>` and the core-only check-version scope, add a lockstep gate to the release pipeline so any packable project whose evaluated version differs from the props version aborts, rewrite docs to "one version, lockstep", bump props to 1.1.1.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle = Claude Fable 5.1 (ganda task work, headless); general reviewer = Claude Sonnet subagent

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
