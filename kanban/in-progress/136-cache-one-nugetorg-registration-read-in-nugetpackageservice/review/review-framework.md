# Review framework

## Budget (by-diff)

- Lines changed: 747
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope

- **Date:** 2026-11-07
- **Host task:** kanban/in-progress/136-cache-one-nugetorg-registration-read-in-nugetpackageservice/
- **Diff scope:** branch `task/136-cache-one-nugetorg-registration-read-in-nugetpacka` vs merge base `7b5cba1` (master). Product files: `source/timewarp-amuru-tools/nu-get/nuget-package-service.cs`, `nu-get/inuget-package-service.cs`, `global-usings.cs`, `public-api/PublicAPI.Unshipped.txt`, `tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.registration-cache.cs`.
- **Plan / brief:** task.md `## Requirements` and `## Goal (implementer restatement)`: one nuget.org registration walk per package id per process, shared by `SearchAsync` and `GetLatestVersionsAsync`, in-flight sharing, no caching of 404 or failure.
- **Effort:** 2 (budget by diff); roster axes: general
- **Reviewer roster:** general (review oracle, single pass)
- **Session IDs:** review oracle claude-opus-5-5 under ganda task work (session id not exposed to the oracle)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
