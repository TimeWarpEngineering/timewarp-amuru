# Round 1 — general
**Date:** 2026-09-23
**Scope reviewed:** branch task/117 vs origin/master, product files only (see review-framework.md)

## Summary

The change removes the per-csproj `<Version>` override on `TimeWarp.Amuru.Tools`, deletes the now-empty `.timewarp/dev.jsonc` core-only check-version scope, bumps `source/Directory.Build.props` to 1.1.1, adds a lockstep gate to Step 2 of the release-event pipeline in `tools/dev-cli/endpoints/workflow-command.cs` that aborts if any packable project's evaluated `<Version>` drifts from the props version, and rewrites `AGENTS.md` / `readme.md` / `documentation/developer/guides/releasing.md` to describe "one version, lockstep" instead of two independent cadences. This is a low-risk, mechanically verifiable change: I confirmed `propsVersion` is null-checked (and the pipeline aborts) before the lockstep gate ever reads it, the new abort follows the same `WriteErrorLine` + `AbortPipeline(reason)` + `return` pattern as every other gate in the file, and the docs/code abort-reason strings match exactly. All falsifiable claims in task.md Results reproduced against the live repo.

## Verification

- `grep -rn "<Version>" source/` → only hit: `source/Directory.Build.props:9:    <Version>1.1.1</Version>`
- `dotnet msbuild source/timewarp-amuru-tools/timewarp-amuru-tools.csproj -getProperty:Version -nologo` → `1.1.1`
- `grep -rn -i --exclude-dir=obj --exclude-dir=bin "cadence\|dev.jsonc\|beta.2\|checkVersionConfig" AGENTS.md readme.md documentation/ tools/ source/ .github/` → no hits
- `dotnet run --file tools/dev-cli/dev.cs -- --help` → dev-cli compiles and runs; lists `workflow`, `check-version`, `release`, etc.
- `dotnet run --file tools/dev-cli/dev.cs -- check-version` →
  ```
  Version in source: 1.1.1
  Latest NuGet version: 1.1.0
  Packages checked: TimeWarp.Amuru, TimeWarp.Amuru.Tools
  ✓ Version in source is new — safe to release.
  ```
  Both packages listed, no `checkVersionConfig` override nudge, exit 0 — matches task.md's expected output.
- `find source -name "*.csproj" | xargs grep -n "IsPackable\|<Version>"` → only two packable projects exist under `source/` (`timewarp-amuru`, `timewarp-amuru-tools`), neither has a project-level `IsPackable` or `<Version>` override — the derived packable set is exactly the pair the docs describe.
- Code reading (`tools/dev-cli/endpoints/workflow-command.cs`):
  - `propsVersion = ReadPropsVersion(repoRoot)` (line 180) is checked for `IsNullOrWhiteSpace` and aborts ("props version unreadable", line ~200-206) well before the lockstep gate (line ~300-310) ever dereferences it — no null-ref risk.
  - The new gate (`driftedPackages` check) uses the same `Terminal.WriteErrorLine(...)` → `AbortPipeline("package version differs from props version")` → `return` shape as all 18 other abort sites in `RunReleaseWorkflowAsync`; `AbortPipeline` sets `Environment.ExitCode = 1` and prints the standard "Pipeline ABORTED" banner, consistent with the rest of the method.
  - Step 5 (Verify Package Set) and Step 6 (Push) both consume `releasePackages`, which by construction only reaches those steps once every entry's `Version` equals `propsVersion` — so `FileName => $"{PackageId}.{Version}.nupkg"` is `{PackageId}.{propsVersion}.nupkg` for both packages, matching the doc's Step 5 description (`{PackageId}.{Version}.nupkg` at the props version) and the CI artifact naming.
- `Directory.Packages.props:25-26` — `TimeWarp.Amuru` is pinned at `1.0.0` and `TimeWarp.Amuru.Tools` at `1.0.0-beta.2` (both stale relative to what's actually on NuGet, consumed by `.githooks/*.cs` via unversioned `#:package TimeWarp.Amuru[.Tools]` under CPM). This file is untouched by the diff and the Tools pin's staleness is symmetric with the pre-existing core pin — confirms task.md's claim that bumping this pin is correctly deferred as a post-publish follow-up, not a gap introduced by this PR.
- Cross-checked the pre-flight `dev release` guard 7 (from `~/.nuget/packages/timewarp.nuru.devcli/3.0.0-beta.76/content/any/endpoints/release-command.cs`, the version pinned in `Directory.Packages.props`) against `documentation/developer/guides/releasing.md`'s guard 7 text: the reference package's own top-of-file comment says `check-version 3-state (None proceed; All/Partial refuse)`, and its "Step 7: check-version 3-state gate" falls back to `PackableProjectService.GetPackableProjectsAsync` when `config.CheckVersionConfig?.Packages` is null (which it now is, since `.timewarp/dev.jsonc` is deleted) — so guard 7 auto-covers both packages and legitimately treats `Partial` as an abort, which is a different (and correctly different) gate from the release-event Step 2/6 in `workflow-command.cs`, where `check-version`'s `None`/`Partial` proceed and only `All` aborts. The doc's Step 2/6 vs guard 7 wording is not a contradiction — it accurately describes two distinct gates with different semantics.
- Diffed every abort-reason string quoted in `releasing.md` (`"release tag does not match source version"`, `"tag pin mismatch"`, `"commit not on master"`, `"master ref unresolvable"`, `"version already released"`, `"no packable projects found"`, `"package version unresolvable"`, `"package version differs from props version"`, `"gh CLI unavailable"`, `"gh run list failed"`, `"no successful CI run found"`, `"downloaded package set does not match derived packable set"`) against the literal `AbortPipeline("...")` call-site strings in `workflow-command.cs` — all match exactly.
- Not run (per instructions): `dev workflow --mode merge`, `ganda repo audit`, `dev release --dry-run` — task.md Results already documents these with concrete output (Pipeline SUCCEEDED, 494 passed/1 skipped, audit clean, dry-run refuses only at "release must be cut from master"), and nothing in the diff or code reading contradicts those claims.

## Issues

No issues found. The lockstep gate is correctly guarded (propsVersion non-null before use), follows the existing abort convention exactly, doesn't disturb Step 5/6's consumption of `releasePackages`, the deleted `.timewarp/dev.jsonc` config falls back cleanly to MSBuild-derived package discovery in both the release-event pipeline and the `dev release` pre-flight guard, the `Directory.Packages.props` Tools pin is correctly out of scope for this PR, and every abort-reason string quoted in the rewritten docs matches the code verbatim.
