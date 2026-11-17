# Round 1 — general
**Date:** 2026-11-17
**Scope reviewed:** branch `task/148-upgrade-cs-check-to-5-0-1` vs merge-base `938fc26`. Product file: `Directory.Packages.props`. Call sites: the round-trip runfile, `.github/workflows/workflow.yml` `CsCheck_Seed`, and the CsCheck 5.0.1 public surface the test uses.

## Summary

The change pins the dev-only CsCheck package at 5.0.1. The property test, its comments, and the CI seed are unchanged. That matches the task and the implementer restatement: same generators, default 100 iterations, no cases removed, no package version bump.

The test's call sites still exist on 5.0.1. `Gen.Frequency` takes `(int, IGen<T>)` tuples. `Gen.Char` still has char-range and string indexers, and `Gen.Int` still has an int range indexer. `Array` is an extension property whose `Item(int, int)` indexer keeps `.Array[0, 12]` and `.Array[0, 6]`. `SampleAsync` still takes `writeLine`, `threads`, and `print`, and its default iteration count is 100. `CsCheckException` is still the failure type. The assembly still formats `Set seed: "…" or -e CsCheck_Seed=… to reproduce`. The workflow still sets `CsCheck_Seed: 012XdpNskFd2`. `#:package CsCheck` has no version, and this repo's central package management is what runfiles consume, so the pin is the version the test restores. The repo targets `net11.0`, which 5.0.1 supports. `TimeWarp.Amuru` stays `3.0.0-beta.5`.

Recorded gates cover the delta: `./bin/dev build` 0 warnings, `./bin/dev test` 878 passed / 1 skipped with `WithArgumentsRoundTrip_Given_` 4/4, `ganda repo audit` clean after the gitignored icon fix, and `ganda nuget outdated` up to date. `### How to validate` names the smoke command and the expected 5.0.1 assets resolution. This review did not re-run those gates. No misread between the restatement and the task.
