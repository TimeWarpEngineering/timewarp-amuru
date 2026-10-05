# Round 1 — general
**Date:** 2026-10-05
**Scope reviewed:** `git diff master...HEAD` test files: dot-net.tool.cs (new, 28 tests), dot-net.cli-smoke.cs (7 tool smokes + helper), direct.get-location.cs, direct.set-location.cs; compared with DotNet.Tool.*.cs builders and Direct.GetLocation/SetLocation. Ran all four files with `dotnet run`.
## Summary
Builder-string assertions match actual argument ordering in every Tool builder (verified against each Build()), all 28 + 1 + 3 tests pass, and no state leaks were found (temp dirs deleted in finally, cwd restored, install/update confined to a cleared local feed and tool-path). The only failure in dot-net.cli-smoke.cs is `NuGet Why ... Accept Posit...` ("No assets file was found"), which is pre-existing code not touched by this diff. Remaining points are minor robustness nits. New files are picked up by the multi-file runner through [ModuleInitializer] Register, consistent with siblings.
## Issues
### Issue 1 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs (ToolSearch_Should_AcceptDetailSkipAndTake)
- Description: This smoke queries the public NuGet feed (network) and asserts the English text "Could not find any results.". It fails offline or on feed errors, unlike the other tool smokes which are hermetic.
- Suggestion: Point it at the cleared local feed via a nuget config (Search has no --configfile, so use `--source` through WithSource if available, otherwise accept/skip when offline), or drop the output-text assertion and only assert the flags are accepted (no "Unrecognized").
- Status: open
### Issue 2 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs (Tool* smokes, e.g. "Cannot find a manifest file", "could not be found", "is not found in NuGet feeds")
- Description: Assertions match localized/English SDK message text; they would fail under a non-English DOTNET_CLI_UI_LANGUAGE or if wording changes in a future SDK (global.json pins the feature band, which limits the risk).
- Suggestion: Set `DOTNET_CLI_UI_LANGUAGE=en` in the smoke environment variables.
- Status: open
### Issue 3 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs:469-480 (WriteClearedNuGetConfig)
- Description: sourceDirectory is concatenated into XML without escaping; temp paths with `&` or `<` would break the config. Practically unlikely.
- Suggestion: Use System.Security.SecurityElement.Escape or XElement.
- Status: open
### Issue 4 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/native/file-system/direct.get-location.cs:29-31 and direct.set-location.cs
- Description: GetLocation compares against Environment.CurrentDirectory, and SetLocation tests mutate it process-wide. This is only safe if the runner executes tests sequentially (Jaribu does, and commands.set-location.cs / repo-clean-service.cs already mutate cwd the same way with restore in finally), so no current bug. SetLocation test also asserts `ShouldBe(directory)` on the raw temp path, which could differ on platforms where /tmp is a symlink (e.g. macOS); fine on Linux.
- Suggestion: None required; optionally compare via Path.GetFullPath/real path for macOS.
- Status: open
