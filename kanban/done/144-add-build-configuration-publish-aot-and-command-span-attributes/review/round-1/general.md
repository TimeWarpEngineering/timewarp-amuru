# Round 1 — general
**Date:** 2026-11-15
**Scope reviewed:** branch `task/144-add-build-configuration-publish-aot-and-command` vs merge-base `b14ccb196587e1cff9b911c1ae54b24159b9c562`. Product diff plus `## Goal (implementer restatement)`, the original task, and `## Results` / `### How to validate`.

## Summary

`DotNetBuildTelemetry.Tag` writes `build.command`, `build.configuration` (only when `WithConfiguration` or a `Configuration` property has a value), and `build.publish_aot` onto `Activity.Current` from the `Build()` method of the build, publish, pack, and test builders. Publish is AOT when an explicit `PublishAot` property parses as true, when a `.cs` runfile has no `#:property PublishAot=false` directive (the SDK default), or when the project file text contains `<PublishAot>true</PublishAot>`. No new ActivitySource is created, and the tag values are only the command word, the configuration name, and a bool.

The implementer restatement matches the original task: same three tags, same four entry points, tags on the ambient activity, and `dev build` versus `dev self-install` (`DotNet.Publish` of `tools/dev-cli/dev.cs`) told apart by `build.command` and `build.publish_aot`. Self-install in TimeWarp.Nuru.DevCli 4.0.0-beta.14 publishes that runfile with no configuration and no `PublishAot` property, which is the path the heuristic marks as AOT. `dev build` now goes through `DotNet.Build(...).WithConfiguration("Release")`, and its capture-then-print output matches the previous `RunProcessAsync` helper. Recorded gates (`./bin/dev build`, `./bin/dev test` 853 total / 852 passed / 1 skipped, `ganda repo audit`) cover this change: `./bin/dev test` runs `tests/timewarp-amuru/multi-file-runners/run-tests.cs`, which compiles `single-file-tests/**/*.cs`. No misread, so `## Learnings` is unchanged.

## Issues

None.
