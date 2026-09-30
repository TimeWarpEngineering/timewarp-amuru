# Review framework — task 111-005

**Date:** 2026-09-30
**Host task:** kanban/to-do/111-005-align-amuru-skill-docs-and-ci-test-inclusion-filters/
**Diff scope:** commit `b775361` (`docs: align amuru skill and CI test inclusion with the 1.0 contract`) vs its parent. Not `origin/master` — that range includes 111-003 and 111-004.
**Plan / brief:** Parent 111 findings M15, M21–M24, M26. Align `skills/amuru/SKILL.md` and `dot-net.md` with the 1.0 contract; add CI path filters; recursive aggregate test include; `TestsDirectory` casing; drop the static `System.Console` using. M25 stays wontfix on 111.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Grok review oracle (2026-09-30). Implementer session is on `task.md`.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
