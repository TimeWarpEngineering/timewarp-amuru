# Round 2 — general
**Date:** 2026-09-23
**Scope reviewed:** Fix delta after round 1: `task.md` checklist edits (M2, M3) and the review-oracle wontfix rationale for M1 and M4. Re-verified prior IDs against the post-fix tree; product code unchanged since round 1 (no new diff to scan).

## Summary

M2 and M3 are resolved on `task.md`: the stale "props vs newest tag" wording is gone and a checklist item now records the Tools independent-cadence decision. M1 and M4 wontfix rationales were re-checked against DevCli `ci-run-promotion.cs` (workflow_dispatch runs are candidates) and the NuGet-cache compile path (no per-file suppression possible). No new findings.

## Issues

<!-- none -->
