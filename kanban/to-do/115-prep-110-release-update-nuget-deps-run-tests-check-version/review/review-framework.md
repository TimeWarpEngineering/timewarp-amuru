# Review framework — task 115

**Date:** 2026-09-23
**Host task:** kanban/to-do/115-prep-110-release-update-nuget-deps-run-tests-check-version/
**Diff scope:** branch `task/115-prep-110-release-update-nuget-deps-run-tests-check` vs `origin/master` (commits fa6af4b, c69c328, 269e2f9; 33c1085 is the task spec)
**Plan / brief:** Prep 1.1.0 release — bump every NuGet dependency to latest (`Directory.Packages.props`), bring `ganda repo audit` to clean (hooks scaffold, shebangs, exec bits, props, editorconfig prune, README rename), prove build/verify-samples/test/check-version green. No tag, no `dev release`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle — Claude Fable 5.1 under `ganda task work` (2026-09-23); general reviewer — Claude Sonnet sub-agent

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
