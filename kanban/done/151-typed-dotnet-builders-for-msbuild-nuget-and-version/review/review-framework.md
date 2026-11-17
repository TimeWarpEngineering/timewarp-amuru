# Review framework — task 151

**Date:** 2026-11-18
**Host task:** kanban/in-progress/151-typed-dotnet-builders-for-msbuild-nuget-and-version/
**Diff scope:** branch `task/151-typed-dotnet-builders-for-msbuild-nuget-and` vs merge-base `bfee4147` (`master`). Product diff is the `247e895` builder commit (msbuild, nuget verify, public API, docs, tests, version). Kanban move commits are not product behavior.
**Plan / brief:** `task.md` original requirements and `## Goal (implementer restatement)`: `DotNet.MsBuild()` and `DotNet.NuGet().Verify()`, push api keys stay out of telemetry tags, no second version/info spelling, public API, `dot-net.md`, Jaribu argument tests, bump to `3.0.0-beta.7`.
**Effort:** 3 (general only)
**Reviewer roster:** general
**Session IDs:** implementer-claude session `403bd4f0-1397-412c-b3ef-37e513e541ea`; reviewer Grok session `01a1eb37-7c5c-7982-9c80-73c40fc854a1`

## Budget (by-diff)

- Lines changed: 1163
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
