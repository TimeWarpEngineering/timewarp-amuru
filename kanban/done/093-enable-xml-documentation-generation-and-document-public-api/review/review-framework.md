# Review framework

## Budget (by-diff)

- Lines changed: 1953
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Framework

**Date:** 2026-10-06
**Host task:** kanban/to-do/093-enable-xml-documentation-generation-and-document-public-api/
**Diff scope:** branch task/093-enable-xml-documentation-generation-and-document-p vs origin/master (source/timewarp-amuru-tools/)
**Plan / brief:** enable GenerateDocumentationFile on Tools; burn down 371 CS1591 with XML docs; no public-api change
**Effort:** 3 (by-diff budget); roster axes: general
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5) + general reviewer subagent a641b0de6267ecda2 (Sonnet)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
