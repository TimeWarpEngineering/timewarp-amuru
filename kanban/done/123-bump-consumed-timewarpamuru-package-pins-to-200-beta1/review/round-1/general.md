# Round 1 — general
**Date:** 2026-10-06
**Scope reviewed:** `Directory.Packages.props` diff plus the five `.githooks/*.cs` consumers and `task.md` Results

## Summary

The change moves the two self-pins in `Directory.Packages.props` from `1.0.0` / `1.0.0-beta.2` to `2.0.0-beta.1`. The "Self pins" comment stays and no other versions change. `source/Directory.Build.props` is still `2.0.0-beta.1`. The only consumers are the five hook runfiles (`#:package TimeWarp.Amuru[.Tools]`). None of them call a member that broke in 2.0 (a grep for `BranchExists`, `UpdateBranch`, `GetWorktreePath`, `MasterWorktree`, `WithConfig`, `WithTargetFramework`, and `WithStandardInput` found nothing). I rebuilt `dotnet build .githooks/pre-push.cs` and it succeeded. Results record per-hook compiles, the post-commit attest exercise, the template-ownership check, and the advisory audit warning with its rationale. Risk is low.

## Issues

<!-- None. -->
