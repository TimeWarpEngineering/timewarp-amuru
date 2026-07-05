# Harden release CI pipeline

## Description

The release path (GitHub Release published → `dev workflow` release mode → nuget.org push via OIDC) has gaps that matter once we're shipping a stable 1.0 instead of betas.

## Checklist

### Major
- [x] Release mode now runs clean→build→verify-samples→test→tag-guard→push (2026-07-05)
- [x] `workflow_dispatch` now maps to Merge mode (validation pipeline, never pushes) — 2026-07-05
- [x] Tag guard added: release fails unless GITHUB_REF_NAME == v{core version}; local runs (no env var) skip with a notice. Push also uses --skip-duplicate so a core release can safely re-push an unchanged Tools version — 2026-07-05

### Minor
- [x] Path filters now include `Directory.Packages.props`, `msbuild/**`, `nuget.config`, `global.json`; CI SDK now resolved from `global.json` (was hardcoded 10.0.201, which the new global.json pin would have broken)
- [x] `ContinuousIntegrationBuild=true` under GITHUB_ACTIONS (source/Directory.Build.props)
- [x] Symbols resolved: embedded PDB only; `IncludeSymbols`/snupkg removed (was built and discarded)
- [x] Decided: GitHub Release bodies remain the release-notes channel; no PackageReleaseNotes (revisit post-1.0 if needed)

## Notes

Found by multi-agent release review (2026-07-04). Working well already: OIDC Trusted Publishing (no long-lived API key), tag-per-release discipline through v1.0.0-beta.34.
