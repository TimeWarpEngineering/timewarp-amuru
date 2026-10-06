# Round 1 — general
**Date:** 2026-10-07
**Scope reviewed:** branch vs `master`: `Directory.Packages.props` pin change and task.md Results

## Summary

The change bumps the two CPM self pins consumed by the `.githooks` runfiles (`#:package TimeWarp.Amuru` / `TimeWarp.Amuru.Tools`, versionless under CPM) from 2.0.0-beta.1 to 2.0.0-beta.2 and keeps the "Self pins" comment. Verified: no remaining `2.0.0-beta.1` references in props/csproj/cs/json outside kanban; `source/Directory.Build.props` is still `2.0.0-beta.2`; hook bodies are untouched (no `.githooks` diff). Results record per-hook compile, attest line and audit pass. Risk is minimal.

## Issues

<!-- None. -->
