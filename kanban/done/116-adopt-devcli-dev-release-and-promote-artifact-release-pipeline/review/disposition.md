# Disposition — task 116

**Date:** 2026-09-23
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 (`general`, effort 1) found no correctness bugs in the DevCli adoption, the promote-artifact release pipeline, the workflow.yml changes, or the releasing guide; every falsifiable claim (8 guards, check-version None/Partial/All exit codes, `.timewarp/dev.jsonc` path and schema, `CiRunPromotion` usage, samples no-pack fix) was verified against the DevCli 3.0.0-beta.76 package content. Two nits on `task.md` bookkeeping were fixed on this task id; two suggestions were declined with rationale. Round 2 re-verified the fixes and found nothing new.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M1 | suggestion | DevCli `OrderCandidateRuns` accepts `workflow_dispatch` runs at the same SHA, so a dispatch-merge artifact in the keep-last-two rotation is still a tested, promotable artifact; the predicted Step 4/6 abort does not occur. | review oracle |
| M4 | suggestion | DevCli content compiles from the NuGet cache where `.editorconfig` and pragmas cannot reach; project-wide `NoWarn` is the only consumer-side mechanism, matching the timewarp-terminal adoption. | review oracle |

## Escalations

- None.
