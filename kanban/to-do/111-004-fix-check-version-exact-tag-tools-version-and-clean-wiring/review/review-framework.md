# Review framework — task 111-004

**Date:** 2026-09-30
**Host task:** kanban/to-do/111-004-fix-check-version-exact-tag-tools-version-and-clean-wiring/
**Diff scope:** branch `task/111-004-fix-check-version-exact-tag-tools-version-and-clea` vs `origin/feature/overnight-amuru` (commit `452dd14`)
**Plan / brief:** Parent 111 findings M16–M20. Exact `v{version}` tag for `IsNewVersion`, unlisted NuGet versions count as already published, per-package csproj `<Version>` override, `dev clean` delegates to `RepoCleanService` and still clears the local feed, root-bin children get tracked-file and reparse guards.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Grok `01a0efe0-95ac-7b52-8bd8-1212d18c3d0c` (2026-09-30)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
