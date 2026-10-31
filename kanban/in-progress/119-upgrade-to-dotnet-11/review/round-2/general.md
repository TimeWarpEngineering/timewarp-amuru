# Round 2 — general
**Date:** 2026-10-31
**Scope reviewed:** the fix delta `75b0006..0b1cccd` (commit `0b1cccd` "fix: pass StringComparison on char searches and scope CA1812"). Product files: `nuget-package-service.cs`, `file-system-walk.cs`, `glob-matcher.cs`, `text-diff.cs`, `tools/dev-cli/Directory.Build.props`, `tools/dev-cli/endpoints/workflow-command.cs`, `documentation/user/installation.md`. Also the edits to `task.md` claims.

## Summary

The fix pass closes all three round-1 findings and adds no new defects. The five char searches pass `StringComparison.Ordinal` on the char overloads. The old no-argument char overloads were already ordinal, so behavior does not change. The pragmas, the false "no overload" comments, and the locals that only held a pragma are gone. CA1812 is out of the dev-cli `NoWarn` and is suppressed only on the three handlers the mediator activates. The installation doc no longer gets the history of `dnx` and `#:package` wrong. A Release `--no-incremental` build of `timewarp-amuru.slnx` gives 0 warnings and 0 errors. A Release `--no-incremental` build of `tools/dev-cli/dev.cs` succeeds under warnings-as-errors, so the assembly-level `~T:DevCli.ReleaseCommand.Handler` and `~T:DevCli.SelfInstallCommand.Handler` targets resolve. If they didn't, CA1812 would fail the build. `./bin/dev test` reports "All tests passed!" with 1 skipped.

## Prior findings re-verified

- **M1 (bug) — fixed.** No `CA1307` text remains under `source/` or `tools/`. All five sites call `StartsWith(char, StringComparison)`, `EndsWith(char, StringComparison)`, or `LastIndexOf(char, int, StringComparison)` with `Ordinal`. `task.md` Notes "CA1307 and CA1865", the checklist line, and `## Results` now state the correct fact and record that the earlier claim was wrong.
- **M2 (nit) — fixed.** `CA1812` is gone from `tools/dev-cli/Directory.Build.props` `NoWarn`, and a comment explains why it is absent. `WorkflowCommand.Handler` carries a type-level `[SuppressMessage]`. The two TimeWarp.Nuru.DevCli content handlers get assembly-level, type-scoped suppressions, because those files live in the NuGet cache. Other dead internal classes in the dev CLI are reported again. `tests/timewarp-amuru/Directory.Build.props` still has `CA1812` in `NoWarn`. That entry predates this task (it is the same at `75b0006`) and is out of scope.
- **M3 (nit) — fixed.** `installation.md:81` and `:139` say ".NET SDK (.NET 10 and later)". The requirement lines still say .NET 11.

## Issues

None.

## Checked, no finding

- The assembly-level suppressions sit at the top of `workflow-command.cs` and not in a `GlobalSuppressions.cs`. The `Directory.Build.props` comment points readers to them, and they are next to the one type-level suppression. That is acceptable, so no finding.
- The goal restatement adds a paragraph for the fix pass that matches the round-1 findings. It is not a misread.
