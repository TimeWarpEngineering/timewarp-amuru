# Review framework

## Budget (by-diff)

- Lines changed: 574
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 082

**Date:** 2026-10-06
**Host task:** kanban/to-do/082-clean-up-repository-to-follow-kebab-case-naming-and-c-conventions/
**Diff scope:** branch `task/082-clean-up-repository-to-follow-kebab-case-naming-an` vs `master` (`b57bc72..4961ee9`, 106 files, mostly `git mv` renames)
**Plan / brief:** rename PascalCase `.cs` to kebab-case, replace 37 purpose stubs with real Purpose/Design regions, fold 106 items (RunBuilder doc text, readme casing, PackageProjectUrl, nullability left as shipped)
**Effort:** 2 (by-diff budget)
**Reviewer roster:** general (review oracle, plus one sonnet sub-agent that verified every new Design claim against its file)
**Session IDs:** review oracle session 76a8b384-43a0-48ac-9fbf-4dd5351e8d62; verifier sub-agent a0ecfbbe0bc1f9b20

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
