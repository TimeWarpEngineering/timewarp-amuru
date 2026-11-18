# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/155-bump-version-to-3-0-0-beta-10` vs `master`. Product diff is `source/Directory.Build.props` line 9. Kitchen edits are the task move, the goal restatement, and `## Results`.

## Summary

`<Version>` changes from `3.0.0-beta.9` to `3.0.0-beta.10`. That is the only product edit. It is the only `<Version>` under `*.csproj` and `*.props`. Samples, tests, and the analyzers project set `IsPackable` false, so the two packable projects inherit this props version. `Directory.Packages.props` still consumes `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` at `3.0.0-beta.9`, which is the published package. The task forbids any other product edit, and those pins are not the version being built.

The recorded gates cover the change: `./bin/dev build` succeeded with 0 warnings and 0 errors and produced `3.0.0-beta.10` packages; `./bin/dev check-version` reports source `3.0.0-beta.10`, NuGet `3.0.0-beta.9`, and "Version in source is new — safe to release."; `ganda repo audit` exited 0 with the pre-existing `vscode-window-icon` advisory. No test file applies to an MSBuild property change. This review did not re-run build, test, or audit.

`source/timewarp-amuru-tools/public-api/PublicAPI.Unshipped.txt` still lists the builder surface. That file is unchanged on this branch, and the task forbids promoting it here. The release guide's promotion step is not a pipeline guard.

The description says a hand commit after the bump leaves an unattested tip and fails a release attestation gate. The bump commit is `22ef152` (version line plus kitchen results). Host commit `2ff481b` adds only the implementer session line. `git notes --ref=refs/notes/ganda-audit show` on `22ef152`, `2ff481b`, and merge-base `15ab788` found no note, so the missing note is not unique to this tip. `documentation/developer/guides/releasing.md` says this repo does not run an attestation gate at release time. No finding.

No issues.
