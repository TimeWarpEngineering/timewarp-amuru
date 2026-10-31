# Disposition — task 119

**Date:** 2026-10-31
**Outcome:** clean
**Rounds:** 4
**Final open count:** 0

## Summary

Round 1 (general reviewer, effort 2) raised one bug and two nits on the `net11.0` hard-cut. M1: CA1307 pragmas resting on a false overload claim. M2: CA1812 in the project-wide dev-cli `NoWarn`. M3: `installation.md` got the .NET version history wrong. Fix commit `0b1cccd` closed all three, and round 2 (effort 1) verified the fixes with a clean Release build and a passing test run.

Steve then reopened the task to move the sample monikers to `net11.0`. Round 3 (effort 3) found that delta correct: Release `-warnaserror` 0 warnings and 0 errors, multi-file runner 732 passed and 1 skipped. It raised M4 (bug): the `readme.md` badge still said `dotnet-10.0`, and both nupkgs pack that readme. Commit `ea2155d` changed the badge to `dotnet-11.0` and added `readme.md` to the validation smoke. Round 4 (effort 1) verified the fix. The smoke prints nothing, and a tree sweep leaves only the `installation.md` history lines the task says to keep. Every finding is fixed.

Final counts (`review/round-4/merged.md`): bug 0 open / 2 fixed / 0 wontfix; suggestion 0 / 0 / 0; nit 0 open / 2 fixed / 0 wontfix.

## Exception log (if accepted-exceptions)

None.

## Escalations

- None.
