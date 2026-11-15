# Round 1 — general
**Date:** 2026-11-15
**Scope reviewed:** branch `task/146-refresh-nu-get-packages-to-net-11-ga-move-self` vs `master`: `Directory.Packages.props`, `source/Directory.Build.props`, `global.json`, `AGENTS.md`, `readme.md`, `documentation/release-notes/3.0.0-beta.4.md`, `.editorconfig` TWA0038 caps, `.githooks/*`, `tests/timewarp-amuru/multi-file-runners/run-tests.cs`, and the six `TestSkipped` call sites.

## Summary

The change refreshes every pin the task table names, moves the githook self pins to the published 3.0.0-beta.3 packages, and bumps `<Version>` to 3.0.0-beta.4 with a release note in the beta.3 shape. `TimeWarp.Build.Tasks` is 2.0.1-beta.1 rather than stable 2.0.0 because Nuru 4.0.0-beta.14 requires that floor. Risk is low. The recorded build, test, and audit cover the diff. No issues.

## Record checked

Taken from `## Results` and `### How to validate`. Not re-run:

- `./bin/dev build` green, 0 warnings, 0 errors.
- `./bin/dev test` 839 total, 838 passed, 1 skipped, 0 failed. The same 1-skip total is the pre-existing skip recorded on task 142.
- `ganda repo audit` exit 0 after `ganda repo audit --fix` (Purpose regions, `prepare-commit-msg`, vscode window icon).
- `ganda nuget outdated`: all packages up to date.
- NativeAOT `self-install` clean; `./bin/dev --capabilities` lists the same 8 commands.
- `dotnet build` of the six `.githooks/*.cs` runfiles exit 0 against the beta.3 self pins.
- Both `PublicAPI.Unshipped.txt` files are header-only. Readme version line points at the beta.4 note.

The open checklist row bundles "audit zero Errors" with "CI green on the PR". Audit is recorded. CI is the later host node. That open box is not a missing result.

## Goal restatement vs task

The restatement matches the description: newest outdated pins, stable preferred, self pins at 3.0.0-beta.3, version 3.0.0-beta.4, and a beta.3-shaped note that calls out NuGet.Versioning 8.0 and Hosting 11.0.0. The extra sentence that moves `global.json` to SDK 11.0.100 is the standing rule that NetAnalyzers and the SDK stay on the same build (`AGENTS.md`). It is not a misread. No `## Learnings` entry.

The Build.Tasks pin in the restatement is still 2.0.0, which is the table. The implementation's 2.0.1-beta.1 deviation is explained in Session and in the release note. It is not a restatement mismatch.

## Confirmatory reads

These are not gate re-runs.

- `TimeWarp.Nuru` 4.0.0-beta.14 nuspec depends on `TimeWarp.Build.Tasks` version `2.0.1-beta.1` (NuGet minimum). A 2.0.0 pin is a downgrade. The prerelease pin is the restore constraint the task's "prefer stable" rule cannot override. The same nuspec depends on `TimeWarp.Terminal` `2.0.0-beta.2`, which this change takes.
- `Microsoft.Extensions.Hosting` is referenced only by `tests/timewarp-amuru/single-file-tests/json-rpc/mcp-stdio-server/mcp-stdio-server.cs`. It stays out of the shipped graph.
- `NuGet.Versioning` 8.0.0 ships `lib/net10.0` and `lib/net472`. `TimeWarp.Amuru.Tools` targets `net11.0` and is the direct reference. The release note states the consumer floor.
- Jaribu 1.0.0-beta.15 `TestSkipped(string)` wrote the skip line. Beta.16 `TestSkipped(ITerminal, string)` does the same write on the passed terminal. All six call sites pass `TimeWarpTerminal.Default`. No skip-registration contract was dropped.
- `FileLengthAnalyzer` reads `dotnet_diagnostic.TWA0038.max_lines` (default 500). The 14 `.editorconfig` caps equal `.timewarp/line-cap-baseline.json`. Those files are not in this diff, so the caps are the existing shrink-only ratchet, not a raised limit. `run-tests.cs` gained a Purpose region and is not one of the 14.
- `prepare-commit-msg` is the audit `--fix` hook (symlink plus runfile, same shape as the other hooks). It exits 0 on every path and stamps `Task:` only when the message already has text. Policy is to keep that baseline. The release note's "five" matches the task's five existing hooks.

## Issues

None.
