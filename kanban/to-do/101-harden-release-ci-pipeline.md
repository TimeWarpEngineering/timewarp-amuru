# Harden release CI pipeline

## Description

The release path (GitHub Release published → `dev workflow` release mode → nuget.org push via OIDC) has gaps that matter once we're shipping a stable 1.0 instead of betas.

## Checklist

### Major
- [ ] **Release mode runs no tests**: `tools/dev-cli/endpoints/workflow-command.cs:11-13` — pr/merge = clean→build→verify-samples→test→check-version, but release = clean→build→push only. The stable artifact ships untested at the tagged commit. Add test (and verify-samples) to release mode
- [ ] **`workflow_dispatch` maps to Release mode**: `workflow-command.cs:79` — a manual "Run workflow" click attempts a NuGet push; OIDC login is gated to release events (`workflow.yml:47`) so today it fails confusingly, but it's one credential change away from an accidental publish. Map dispatch to the pr/merge pipeline or require explicit `--mode release`
- [ ] **No tag-vs-version guard**: the pipeline reads `source/Directory.Build.props:8` to decide what to push with no check it matches the release tag (`workflow-command.cs:218-224`). Publishing `v1.0.0` without remembering the manual props bump re-pushes the wrong version. Assert tag == props version in release mode

### Minor
- [ ] `workflow.yml:7-13,17-23` — path filters exclude `Directory.Packages.props`, `msbuild/**`, `nuget.config`: dependency bumps never run CI
- [ ] Set `ContinuousIntegrationBuild=true` when `GITHUB_ACTIONS` is true (SourceLink determinism; `EmbedUntrackedSources` is already set)
- [ ] Resolve self-contradictory symbols config: `source/Directory.Build.props:19-21` sets `IncludeSymbols`+snupkg AND `DebugType=embedded`, then push uses `--no-symbols` (`workflow-command.cs:236`) — the snupkg is built and thrown away. Drop `IncludeSymbols` (embedded PDB is fine) or actually push symbols
- [ ] Consider `PackageReleaseNotes`/generated release notes for 1.0 (currently GitHub Release bodies only)

## Notes

Found by multi-agent release review (2026-07-04). Working well already: OIDC Trusted Publishing (no long-lived API key), tag-per-release discipline through v1.0.0-beta.34.
