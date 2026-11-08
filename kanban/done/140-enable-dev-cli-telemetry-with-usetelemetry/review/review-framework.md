# Review framework

## Budget (by-diff)

- Lines changed: 209
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 140

**Date:** 2026-11-08
**Host task:** kanban/in-progress/140-enable-dev-cli-telemetry-with-usetelemetry/
**Diff scope:** branch `task/140-enable-dev-cli-telemetry-with-usetelemetry` vs `master` (merge base); product files `tools/dev-cli/dev.cs`, `tools/dev-cli/endpoints/incoming-trace-context.cs`, `tools/dev-cli/telemetry.md`
**Plan / brief:** opt the dev CLI into Nuru `UseTelemetry` (service `timewarp-amuru-dev`), restore caller `TRACEPARENT` before `RunAsync`, document env vars; reference timewarp-nuru kanban 556
**Effort:** 2 (by-diff budget; roster axes general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude-opus-5-5, ganda task work review node), 2026-11-08

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
