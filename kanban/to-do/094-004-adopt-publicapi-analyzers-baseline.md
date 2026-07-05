# Adopt PublicAPI analyzers baseline

## Description

Once the 1.0 surface ships, guard it: adopt `Microsoft.CodeAnalysis.PublicApiAnalyzers` with checked-in `PublicAPI.Shipped.txt`/`PublicAPI.Unshipped.txt` per packable project, so every surface change is deliberate and reviewable. Fits the repo's existing analyzer posture (BannedApiAnalyzers already in `Directory.Build.props`).

Timing per owner decision (2026-07-05): at/after the 1.0 release — not a release gate.

## Checklist

- [ ] Add `Microsoft.CodeAnalysis.PublicApiAnalyzers` to `Directory.Packages.props` and the packable projects (core + Tools)
- [ ] Generate `PublicAPI.Shipped.txt` from the released 1.0 core surface; Tools tracks in `Unshipped` while beta
- [ ] Ensure RS0016/RS0017 run as errors in CI (already `TreatWarningsAsErrors`)
- [ ] Document the workflow (how to intentionally add API) in Agents.md or contributing docs

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)
