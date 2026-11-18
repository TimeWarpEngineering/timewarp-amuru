# Review framework

## Budget (by-diff)

- Lines changed: 732
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-11-18
**Host task:** kanban/in-progress/154-add-the-dot-net-builder-options-timewarp-nuru-598-needed-ms-build-node-reuse-a-format-builder-and-standard-input/
**Diff scope:** branch `task/154-add-the-dot-net-builder-options-timewarp-nuru-598`, commits after the task 153 merge `e6ad360` (`43c74b6`..`fb34a5e`). The `git merge-base` with `origin/master` is older (`b3dd325`), so that diff also shows task 153. Task 153 is out of scope.
**Plan / brief:** task.md `## Goal (implementer restatement)` and `## Requirements`: `WithNodeReuse(bool)` on `DotNetMsBuildBuilder`, a `DotNet.Format()` builder, `WithStandardInput(string)` on `DotNetRunBuilder` and `DotNetBuilder`, `format` moved off the analyzer gap list.
**Effort:** 2 (by-diff budget), roster axes: general
**Reviewer roster:** general (review oracle, Claude Opus 5.5)
**Session IDs:** review oracle, ganda task work review node, 2026-11-18

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading
  the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to
  the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the
  command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
