# Review framework — task 156

**Date:** 2026-11-19
**Host task:** kanban/in-progress/156-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-amuru/
**Diff scope:** branch `task/156-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `1f0e89bbde9b9f47760eadc88c925fc4d192083a` (master). Product diff is `Directory.Packages.props` (three version lines). Kitchen `task.md` is also on the branch. Master is two kanban-land commits ahead; those commits do not touch product files.
**Plan / brief:** Bump three dev-only central pins in `Directory.Packages.props`: TimeWarp.Jaribu to 1.0.0-beta.18, and TimeWarp.Amuru and TimeWarp.Amuru.Tools to 3.0.0-beta.10. Leave `<Version>` in `source/Directory.Build.props` unchanged.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok session `01a1f1d0-38f0-7370-b8c7-2b160f9c240f` (2026-11-19). Implementer session not reported.

## Budget (by-diff)

- Lines changed: 51
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
