# Round 2 — general
**Date:** 2026-11-15
**Scope reviewed:** branch `task/146-refresh-nu-get-packages-to-net-11-ga-move-self` vs `master`. Round 1 scope (pins, `global.json`, version bump, release note, TWA0038 caps, githooks, Jaribu `TestSkipped` sites) plus the fix delta `f0d74de`: `samples/script-context-example.cs`, `samples/app-context-extensions-example.cs`, `samples/json-rpc/aot-json-rpc-example.cs`.

## Summary

Round 1 raised no findings. The follow-up commit wraps each sample's existing header comment in `#region Purpose`, which is the TWA0004 contract CI failed on. The pin refresh still matches the task table, with Build.Tasks at 2.0.1-beta.1 for the recorded NU1605 floor. The recorded build, test, audit, and `./bin/dev verify-samples` cover the tree, including the sample fix. No new issues.

## Prior round

Round 1 (`review/round-1/merged.md`) raised no M IDs. Nothing to re-open.

## Record checked

Taken from `## Results` and `### How to validate`. Not re-run:

- First pass: `./bin/dev build` green; `./bin/dev test` 839 total, 838 passed, 1 skipped, 0 failed; `ganda repo audit` exit 0; `ganda nuget outdated` all up to date; NativeAOT `self-install` clean; eight dev commands; six `.githooks/*.cs` builds exit 0; nuspec file lists unchanged aside from version, commit, and dependency versions.
- After the sample fix: `./bin/dev verify-samples` 3 of 3 compile; build, test, and audit repeated green with the same counts.

`verify-samples` builds each `samples/**/*.cs` with `dotnet build`. Root `Directory.Build.props` sets `TreatWarningsAsErrors` and `CodeAnalysisTreatWarningsAsErrors`, and the samples import that file, so a remaining TWA0004 diagnostic fails that command. The green record covers the CI step that failed. The open checklist row still waits on CI itself. That is the later host node, not a missing local result.

## Goal restatement vs task

The restatement matches the description: newest outdated pins, stable preferred, self pins at 3.0.0-beta.3, version 3.0.0-beta.4, and a beta.3-shaped note that calls out NuGet.Versioning 8.0 and Hosting 11.0.0. The extra sentence that moves `global.json` to SDK 11.0.100 is the standing rule that NetAnalyzers and the SDK stay on the same build (`AGENTS.md`). It is not a misread. No `## Learnings` entry.

The Build.Tasks pin in the restatement is still 2.0.0, which is the table. The implementation's 2.0.1-beta.1 deviation is explained in Session and in the release note. It is not a restatement mismatch.

## Confirmatory reads

These are not gate re-runs.

- The three sample regions sit after the `#:` directives and before the usings. Each holds the comment that was already the file's purpose. `samples/` contains only those three `.cs` files plus `Directory.Build.props`. `rg --files-without-match --glob '*.cs' '#region Purpose'` printed nothing.
- The six `TestSkipped` call sites all pass `TimeWarpTerminal.Default`. No old one-argument call remains.
- `<Version>` appears only in `source/Directory.Build.props`, at 3.0.0-beta.4. Self pins stay at the published 3.0.0-beta.3.
- `.editorconfig` TWA0038 caps match `.timewarp/line-cap-baseline.json` exactly (that json is not in this diff). `wc -l` matches 12 of the 14 caps. `dot-net.run.cs` and `dot-net.test.cs` are one below `wc -l` because they have no trailing newline (`od` on the last bytes ends in `7d`, not `0a`). The caps match the logical line count and the pre-existing baseline. They are not a raised limit.
- `NuGet.Versioning` use in product code is still `NuGetVersion.TryParse` and comparison. The release note states the net10.0 asset change and the consumer floor.

## Issues

None.
