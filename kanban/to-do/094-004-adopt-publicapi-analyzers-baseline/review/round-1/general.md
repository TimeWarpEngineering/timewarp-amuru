# Round 1 — general
**Date:** 2026-10-05
**Scope reviewed:** branch vs master: `Directory.Packages.props`, both packable csproj files, `.editorconfig`, `source/.editorconfig`, `AGENTS.md`, `documentation/developer/guides/releasing.md`, both `public-api/` file pairs (spot-checked).

## Summary

Adds PublicApiAnalyzers 5.6.0 via CPM to `timewarp-amuru` and `timewarp-amuru-tools` only (PrivateAssets=all, so not a package dependency), wires `public-api/PublicAPI.{Shipped,Unshipped}.txt` as AdditionalFiles, and prunes `public-api` from the kebab-path audit. Shipped files carry `#nullable enable`, no oblivious (`~`) or `*REMOVED*` entries; Unshipped is header-only. Docs for add/remove/release match Roslyn semantics (RS0024 forbids `*REMOVED*` in Shipped; release deletes the Shipped line). RS0026 suppression is scoped to `source/` and justified by existing overload sets. Verified: `./bin/dev build` 0 warnings / 0 errors, `ganda repo audit` passes, line counts 250 / 1491 match Results. Low risk.

## Issues

None.
