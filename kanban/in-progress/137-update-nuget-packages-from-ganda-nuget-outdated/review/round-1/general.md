# Round 1 — general
**Date:** 2026-11-07
**Scope reviewed:** branch vs master merge-base (see review-framework.md)

## Summary

The change bumps `Roslynator.Analyzers`, `Roslynator.CodeAnalysis.Analyzers` and `Roslynator.Formatting.Analyzers` from 5.0.0 to 5.0.1 in `Directory.Packages.props`. It also lands two `ganda repo audit --fix` boy-scout changes: `.vscode/settings.json` is untracked and its `.gitignore` re-include is dropped, and `.timewarp/line-cap-baseline.json` records the 14 files over 500 lines. Risk is low. The pin diff is exactly three version strings, with no BOM.

## Verification (re-run by reviewer)

- `ganda nuget outdated`: 18 packages, `All packages are up to date!`
- `ganda repo audit`: `Repository passes all audit checks.`
- `./tools/dev-cli/dev.cs build`: Build succeeded, 0 warnings, 0 errors
- `./tools/dev-cli/dev.cs test`: Total 733, Passed 732, Skipped 1, `All tests passed!`
- `source/Directory.Build.props` `<Version>` is still `3.0.0-beta.1`. `global.json` SDK and the `Microsoft.CodeAnalysis.NetAnalyzers` pin are both still `11.0.100-rc.3.26522.148`. `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` are still `2.0.0`.
- The Roslynator `PackageReference`s in `Directory.Build.props` are versionless (CPM), so no other reference had to move.
- The line-cap baseline counts match the files on disk. For `dot-net.run.cs` and `dot-net.test.cs`, `wc -l` reads one line lower because those files have no trailing newline.
- `Directory.Packages.props` starts with `<?x`, so it has no BOM, as the Notes say.

## Goal restatement check

`## Goal (implementer restatement)` matches the Description and Requirements, including the guards on the version, the SDK/NetAnalyzers pair, and the consumed Amuru pins. No misread.

## Issues

None. The audit-fix files are outside the "package-pin file" requirement, and audit fixes are repo policy.
