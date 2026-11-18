# Round 2 — general
**Date:** 2026-11-18
**Scope reviewed:** delta since round-1 base `7431589` — `fff135d` (docs + test for Build `WithNoLogo`), `2bfa8b0` (Nuru pin bump), and the task.md Results/Session updates.

## Summary

The fix pass answers M1. `DotNetBuildBuilder.WithNoLogo()` was already on the builder (`dot-net.build.cs:208`) and in `public-api/PublicAPI.Shipped.txt:222`, so the round-1 finding rested on a false premise; no new member or Unshipped entry was needed. The pass documents `WithNoLogo` in `dot-net.md` and adds `RunfileCoverageShape_Should_IncludeArtifactsPathNoLogoAndVerbosity`, whose expected string (`--artifacts-path … --verbosity minimal --nologo`) matches the builder's argument order (`dot-net.build.cs` artifacts-path, then verbosity, then boolean flags including `--nologo`). The test file now has 14 test methods, matching the recorded "14 passed". The Nuru `4.0.0-beta.15` → `4.0.0-beta.16` pin bump is an audit `package-pin-currency` fix with a recorded audit rerun (Passed 44, Failed 0), which is baseline policy. The updated Goal restatement still matches the task Requirements.

## Prior findings

- M1 (suggestion) — fixed. The member already shipped; the documentation and command-string test give the runfile-coverage shape a typed call.

## Issues

None.
