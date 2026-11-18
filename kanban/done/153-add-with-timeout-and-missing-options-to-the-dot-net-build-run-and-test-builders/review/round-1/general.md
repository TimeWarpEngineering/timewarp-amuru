# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch vs merge-base with master (source, tests, PublicAPI, dot-net.md, .editorconfig, line-cap baseline, Directory.Packages.props)

## Summary

The change adds `WithTimeout` / `WithTimeoutGracePeriod` to the Build, Run, and Test builders (they set `CommandOptions`, same as `DotNetBuilder`), `WithArtifactsPath` to Build/Run/Test/Publish/Pack, a verbatim `WithArguments` pass-through on Test, and `WithNoCache` / `WithNoLogo` on Run. To stay under TWA0038 the option setters of Build/Run/Test/Publish moved to `*.options.cs` partials; a line-multiset comparison of each old file against the new pair shows nothing was dropped (only the `class` → `partial class` line differs), and the public method sets match old + new members exactly. PublicAPI.Unshipped lists all 14 new members. Tests cover command strings, blank-argument guards, and a mock-backed timeout per builder. The restatement matches the task; no misread.

Risk is low. One gap: the Jaribu call sites this task exists to unblock (task Notes) include `dotnet build … --artifacts-path … --nologo`, and `DotNetBuildBuilder` still has no `WithNoLogo`, so the headline call site stays on the raw argument list.

Verification commands (read-only, not gates): `git diff` per file; `comm` of sorted old vs new lines for the four split builders; `rg -- '--nologo'` over the local timewarp-jaribu master checkout to confirm the call-site shapes.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.build.options.cs:1 (DotNetBuildBuilder has no `WithNoLogo`)
- Description: The task Notes name `source/timewarp-jaribu/runfile-coverage.static.cs` as a call site that moves to the typed builder once this lands. That call is `dotnet build <entry> --artifacts-path <dir> --nologo --verbosity minimal` (jaribu master, runfile-coverage.static.cs:256-263). `tools/dev-cli/endpoints/benchmark-measure.cs:31-37` (`build … -c Release -p:… --nologo`) and the runfile-coverage launch tests (`build … -c Release --nologo`) have the same shape. `dotnet build` accepts `--nologo`, and `DotNetPackBuilder` and `DotNetTestBuilder` already expose `WithNoLogo()`, but `DotNetBuildBuilder` does not, and it has no extra-argument pass-through either. So the motivating `--artifacts-path` call still cannot move off `DotNet.Builder().WithArguments(…)`, and Jaribu's follow-up would bounce back for another beta. The task's Requirements list did not name build `--nologo` — this is a spec gap, not an implementer misread — but the Goal ("Jaribu's typed dotnet calls need options the Build, Run, and Test builders do not expose") covers it.
- Suggestion: Add `DotNetBuildBuilder.WithNoLogo()` (emit `--nologo`, same doc and placement style as `DotNetPackBuilder.WithNoLogo`), a `PublicAPI.Unshipped.txt` entry, a command-string test in `tests/.../dot-net.build.cs`, and a line in `dot-net.md`. Put it in `dot-net.build.options.cs` or `dot-net.build.cs` as the TWA0038 cap allows (build.cs is 443 lines).
- Status: open

<!-- No other issues. Checked and not raised: Run emits --nologo before `--` (matches Jaribu benchmark-suite.cs:227 `run <gen> --nologo -- --repo …`, and documented as forwarded to the program); Test WithArguments appends after --property tokens without `--` (tested); timeout validation is delegated to CommandOptions and tested for invalid values; Directory.Packages.props self-pin bump to 3.0.0-beta.9 is the githook runfile pin, recorded in Results; .editorconfig / line-cap-baseline removals match the split files now under 500 lines. -->
