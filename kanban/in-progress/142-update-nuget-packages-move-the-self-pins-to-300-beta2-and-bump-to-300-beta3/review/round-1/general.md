# Round 1 — general
**Date:** 2026-11-09
**Scope reviewed:** branch vs `master`: `Directory.Packages.props`, `source/Directory.Build.props`, `readme.md`, `documentation/release-notes/3.0.0-beta.3.md`

## Summary

The change moves five pins in `Directory.Packages.props`: Ramani beta.24, SourceGenerators beta.12, Hosting 10.0.14, and the Amuru and Amuru.Tools self pins to 3.0.0-beta.2. A separate commit bumps `<Version>` to 3.0.0-beta.3, adds the release note, and repoints the readme. Risk is low. No source, test, or public API file changes.

## Verification (re-run by the reviewer, not taken from Results)

- `ganda nuget outdated` on the branch: all 21 packages are up to date. The pins match the Results table, and nothing else in `Directory.Packages.props` moved.
- `Microsoft.Extensions.Hosting` is referenced only by `tests/timewarp-amuru/single-file-tests/json-rpc/mcp-stdio-server/mcp-stdio-server.cs` (`#:package`), so it stays out of the shipped graph. There is no stable 11.0 yet, so taking 10.0.14 follows the task's fallback rule.
- `dotnet build` of all five `.githooks/*.cs` runfiles succeeded. `project.assets.json` for pre-commit resolves `TimeWarp.Amuru/3.0.0-beta.2` and `TimeWarp.Amuru.Tools/3.0.0-beta.2` on SDK `11.0.100-rc.3.26522.148` (`global.json`).
- `./tools/dev-cli/dev.cs check-version`: source 3.0.0-beta.3, latest NuGet 3.0.0-beta.2, new.
- `./tools/dev-cli/dev.cs build`: 0 warnings, 0 errors. It packed both 3.0.0-beta.3 nupkgs.
- Both `PublicAPI.Unshipped.txt` files are header-only (`#nullable enable`).
- The version bump is its own commit (`0bdfcad`), after the pin commit (`8da7577`).
- The release note has the same shape as beta.2, including the "It is not a NuGet publish" line, which follows existing convention. It has dependency updates, githook self pins, and Unchanged sections.
- `dev test` was not re-run by the reviewer. The implementer reports 839 total, 838 passed, 1 pre-existing skip, on both commits. No test or source file changed.

## Goal restatement vs task

The `## Goal (implementer restatement)` matches the original Description and Requirements, including the Hosting 11.0-or-10.0.14 rule, the Ramani stop-and-note rule, the separate bump commit, and no publish. No misread.

## Issues

None.
