# Round 2 — general
**Date:** 2026-11-08
**Scope reviewed:** delta b44b799..21b7a1f (round-1 base to fix-pass head). The only product change is `tests/timewarp-amuru/Directory.Build.props` (`AddCommandCompletionRamaniGenerator`). Kitchen edits to `task.md` were also read.

## Summary

The fix pass resolves M1. The target now reads the `TimeWarp.Ramani` `PackageVersion` item from `Directory.Packages.props` and lowercases it with `ToLowerInvariant()`. It builds the NuGet cache path from that value. If the dll is missing, an `<Error>` names the version and the path. The version is no longer typed out a second time. Production code is unchanged in this delta. The `task.md` Goal addendum and Results describe the change accurately.

Verified:
- A forced rebuild (`dotnet build … --no-incremental -v:n`) of the oracle runfile resolves the analyzer to `timewarp.ramani/1.0.0-beta.23/analyzers/dotnet/cs/TimeWarp.Ramani.Generators.dll`, and the build exits 0.
- `dotnet run tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs` gives 19 total and 19 passed.

## Issues

None.

## Considered and not raised

- **`Condition="Exists(...)"` on the `Analyzer` item, after the `<Error>`.** This is redundant, because the Error already stops the target when the file is missing. It does no harm, so it is not raised.
- **No `PackageVersion` item (central package management off).** The version would be empty. The path would not exist, so the Error fires and names an empty version and the path. The failure is still visible, and this repo uses central package management.
- **Restore after a version bump.** The runfile's ProjectReference to `source/timewarp-amuru` restores the new `TimeWarp.Ramani` version as a transitive dependency. So on a clean cache, the cache folder exists for the bumped version, which is what M1 needed.
- **Goal restatement vs task.** The added Goal paragraph describes the M1 fix and does not change the original Requirements. No misread.
