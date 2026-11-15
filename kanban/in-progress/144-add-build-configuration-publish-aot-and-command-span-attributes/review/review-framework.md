# Review framework — task 144

**Date:** 2026-11-15
**Host task:** kanban/in-progress/144-add-build-configuration-publish-aot-and-command-span-attributes/
**Diff scope:** branch `task/144-add-build-configuration-publish-aot-and-command` vs merge-base `b14ccb196587e1cff9b911c1ae54b24159b9c562` (local master). Product files: `source/timewarp-amuru-tools/dot-net-commands/dot-net.build-telemetry.cs`, `dot-net.build.cs`, `dot-net.pack.cs`, `dot-net.publish.cs`, `dot-net.test.cs`, `tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build-telemetry.cs`, `tools/dev-cli/endpoints/build-command.cs`, `tools/dev-cli/telemetry.md`.
**Plan / brief:** Tag `Activity.Current` with `build.command`, `build.configuration`, and `build.publish_aot` from `DotNet.Build()`, `DotNet.Publish()`, `DotNet.Pack()`, and `DotNet.Test()`.
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** review grok `01a1de7d-96a3-75c1-860e-54e41630aa9c`; implement claude `02682220-e8f1-4d06-a33c-35ceb04b6145`

## Budget (by-diff)

- Lines changed: 629
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
