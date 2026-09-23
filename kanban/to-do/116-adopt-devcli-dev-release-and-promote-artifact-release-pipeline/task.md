# Adopt DevCli dev release and promote-artifact release pipeline

## Description

timewarp-amuru still cuts releases the old way: a human creates a GitHub
Release by hand, and `workflow.yml` rebuilds and publishes on the
`release: published` event via `dev workflow --api-key`. Every other package
repo (nuru, terminal, source-generators, state, ganda) follows the org
convention from nuru task 458: the version is typed once in
`source/Directory.Build.props`, `dev release` (from the
`TimeWarp.Nuru.DevCli` package, `release-command.cs`) runs eight guards and
creates the tag + GitHub Release, and the release run PROMOTES the CI-built
`Packages-*` artifact from the master push run at the tag SHA
(tag-gate → check-version → locate-run → download-artifact → verify → push),
never rebuilding.

Bring amuru onto that convention so `/tw-release` works here unchanged. This
surfaced on 2026-09-23 when cutting v1.1.0 (task 115): `dev release` did not
exist and the release had to be created with `gh release create` by hand.

Reference implementation: `timewarp-terminal` (consumes the DevCli package,
`tools/dev-cli/Directory.Build.props` + `Directory.Packages.props`) and
`timewarp-nuru/documentation/developer/guides/releasing.md` for the guide shape.

## Checklist

- [x] Add `TimeWarp.Nuru.DevCli` (≥ 3.0.0-beta.76, match the Nuru version already in `Directory.Packages.props`) to central package versions and reference it from `tools/dev-cli/Directory.Build.props`
- [x] Drop the local `endpoints/clean-command.cs`, `check-version-command.cs`, and `self-install-command.cs` in favour of the DevCli content copies (or exclude them from `Compile` as terminal/nuru do); add `<Using Include="DevCli" />`
- [x] `dev --help` lists `release`; `dev release --dry-run` on master evaluates all 8 guards read-only and passes (today it stops at guard 6 because tag v1.1.0 already exists on origin — the correct verdict; all 8 pass once the next bump merges, see Results)
- [x] Rewrite `endpoints/workflow-command.cs` so `--mode release` runs the promote pipeline (tag-gate → check-version → locate-run → download-artifact → verify → push) instead of rebuilding; keep `pr` and `merge` modes as `clean → build → verify-samples → test`
- [x] Update `.github/workflows/workflow.yml`: add `mode: release` + `confirm: release` break-glass inputs with the validate step; release event and break-glass both call `dev workflow --mode release --api-key …`; pass `GH_TOKEN: ${{ github.token }}` so locate-run/download-artifact can read Actions artifacts; `--file` on `dotnet run`
- [x] Upload-artifacts and keep-last-two conditions still skip PRs, release, break-glass, and probe
- [x] Add `documentation/developer/guides/releasing.md` describing this repo's instantiation (copy the nuru guide's structure; state only what amuru actually implements)
- [x] `dev workflow --mode merge` locally green; `ganda repo audit` clean
- [x] Check-version keeps working under the new pipeline: props `<Version>` vs newest tag, refuses when equal
- [ ] PR open; after merge, prove it end to end on the next real release (do not cut a test release for this task)

## Session

- Created: 2401810 (2026-09-23)
- Implemented: 2026-09-23, claude (Fable 5.1) headless via `ganda task work 116`; worktree `task-116-adopt-devcli-dev-release-and-promote-artifact-rele`, branch `task/116-adopt-devcli-dev-release-and-promote-artifact-rele`

## Notes

- Do not bump `<Version>` on this task; v1.1.0 was released from task 115. The first `dev release` under the new pipeline happens when the next version bump merges.
- Trusted publishing is already configured for this repo + `workflow.yml` (v1.1.0 published through OIDC), so no NuGet.org policy work is expected. `gh workflow run workflow.yml -f mode=probe` confirms.
- The Packages-* artifact for the tag commit must come from a green master push run; the release step never rebuilds. Keep `retention-days: 7` and the keep-last-two prune.
- Skill reference: `tw-release` in timewarp-flow (`claude/skills/tw-release/SKILL.md`) is the operator procedure this repo must satisfy.
- Lockstep gap found during the v1.1.0 release (2026-09-23): `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` carries its own `<Version>1.0.0-beta.2</Version>`, so the release run pushed `TimeWarp.Amuru.Tools.1.0.0-beta.2` (already on the feed, skipped) instead of 1.1.0. The convention derives the packable set from `IsPackable` and expects one version. Either remove the csproj override so Tools ships at the props version, or set `IsPackable=false` on it with a stated reason. Add a checklist item for whichever you choose.

## Results

### What changed

- `Directory.Packages.props`: pinned `TimeWarp.Nuru.DevCli` 3.0.0-beta.76 (same line as `TimeWarp.Nuru`).
- `tools/dev-cli/Directory.Build.props`: references the DevCli package, adds `<Using Include="DevCli" />`, extends `NoWarn` with the IDE/IL codes the package content trips (same list as timewarp-terminal).
- Deleted local `endpoints/clean-command.cs`, `check-version-command.cs`, `self-install-command.cs`; the DevCli content copies replace them. `dev.cs` registers `IRepoCleanService`, `NuGetVersionService`, `IRepoConfigService`, `IPackableProjectService`.
- `endpoints/workflow-command.cs` rewritten: `pr`/`merge` = `clean → build → verify-samples → test` (the old trailing check-version step is gone; that gate now lives in `dev release` and the release pipeline). `release` = `tag-gate → check-version → locate-run → download-artifact → verify → push`, ported from timewarp-state/nuru, never rebuilds. Amuru-specific: each packable project's `<Version>` is evaluated via `dotnet msbuild -getProperty:Version`, so verify/push expect `TimeWarp.Amuru.{props}.nupkg` and `TimeWarp.Amuru.Tools.{csproj}.nupkg` rather than lockstep; push uses `--skip-duplicate`; the best-effort timewarp-software rebuild dispatch is kept and uses `REBUILD_DISPATCH_TOKEN` so `GH_TOKEN` can stay the job token.
- `.timewarp/dev.jsonc` (new): `checkVersionConfig.packages = "TimeWarp.Amuru"` so `check-version` and `dev release` guard 7 judge the core package only (Tools has its own version, so the derived set would always read Partial and `dev release` would refuse forever).
- `.github/workflows/workflow.yml`: `mode: release` + `confirm: release` break-glass inputs with the validate step; release event and confirmed break-glass both run `dev workflow --mode release --api-key …`; `GH_TOKEN: ${{ github.token }}`; `dotnet run --file`; upload is green-master only and skips PRs, release, break-glass and probe; keep-last-two is keyed on the upload step outcome. The `push` trigger's paths filter was removed: `dev release` guard 8 and locate-run need a green run at the exact master HEAD, and a docs/kanban-only commit on master previously left HEAD with no run (observed today at 612c018). PRs keep the filter.
- `samples/Directory.Build.props` (new): `IsPackable=false`, `GeneratePackageOnBuild=false`. Found while inspecting the real `Packages-99` artifact from run 35840848324: verify-samples was packing the two sample runfiles into `artifacts/packages` (`app-context-extensions-example.1.0.0.nupkg`, `script-context-example.1.0.0.nupkg`), and the release verify step aborts on any nupkg outside the packable set. Without this the first promote would have refused.
- `documentation/developer/guides/releasing.md` (new) and an AGENTS.md `Release` bullet.

### Observations

- v1.1.0 was cut by hand today (tag at fc422dc, release-event run 35850033352 succeeded under the old rebuild pipeline). `dev release --dry-run` from the clean master worktree therefore stops at guard 6, `tag v1.1.0 already exists on origin`, which is the correct verdict; guards 1–5 print `✓`. All eight guards can only pass once the next version bump merges, exactly as the Notes say.
- `dev workflow --mode release` from the master worktree (no api key) passes the tag gate and check-version, derives `TimeWarp.Amuru 1.1.0, TimeWarp.Amuru.Tools 1.0.0-beta.2`, then aborts at locate-run because master HEAD 612c018 (kanban publish) had no CI run under the paths-filtered trigger. That is the motivation for dropping the push paths filter.
- The tag-vs-version comparison the old local `check-version` did (`props <Version>` vs newest tag) is now split: `dev release` guard 6 refuses when tag `v{Version}` exists; the DevCli `check-version` refuses when the version is already on NuGet.org.
- `ganda repo audit` needs `bin/dev` present; `dev self-install` (AOT publish with the DevCli content) succeeds and the audit is clean afterwards.

### How to validate

Smoke (from this worktree):

```bash
dotnet run --file tools/dev-cli/dev.cs -- --help            # Expect: release, check-version, clean, self-install listed
dotnet run --file tools/dev-cli/dev.cs -- check-version     # Expect: Packages checked: TimeWarp.Amuru; exit 0 until 1.1.0 indexes on NuGet, then "already released" exit 1
dotnet run --file tools/dev-cli/dev.cs -- workflow --mode merge   # Expect: Step 1/4..4/4, "Pipeline SUCCEEDED", artifacts/packages holds exactly TimeWarp.Amuru.1.1.0.nupkg and TimeWarp.Amuru.Tools.1.0.0-beta.2.nupkg
dotnet run --file tools/dev-cli/dev.cs -- self-install && ganda repo audit   # Expect: "Repository passes all audit checks."
```

Smoke (from a clean, synced master checkout, using this branch's dev.cs):

```bash
dotnet run --file <this-worktree>/tools/dev-cli/dev.cs -- release --dry-run
```

Expect: `✓ gh authenticated`, `✓ working tree clean`, `✓ on master`, `✓ in sync with origin/master`, then `Error: tag v1.1.0 already exists on origin` (exit 1) until the next version bump merges; after a bump with a green master run, all 8 guards pass and the tag/push/`gh release create` commands are printed.

Expect (CI, after merge): the master push run uploads `Packages-{n}` containing exactly the two product nupkgs; `gh workflow run workflow.yml -f mode=probe` still passes the OIDC login; the next real `dev release` produces a `release` run whose log shows `Step 1/6 … Step 6/6: Push to NuGet` with `Downloaded 'Packages-…'` and no `dotnet build`.

### Left for a later task / human

- End-to-end proof on the next real release (last checklist item); do not cut a test release.
- If Tools ever needs a release without a core bump, decide on a Tools-only path; today the tag and publish-state gate follow the core version only (documented in the guide's appendix).
