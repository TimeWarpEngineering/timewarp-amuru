# Cleanup GitHub Workflows Post-Ganda Migration

## Description

Remove dead code and orphaned references from the CI/CD pipeline after the Ganda/exe migration to timewarp-ganda repository. The workflow currently builds executables across 3 platforms that no longer belong in this repo, references a non-existent sync script, and `Scripts/Build.cs` still references the removed Ganda project.

## Requirements

- All 13 `exe/` files confirmed to exist in timewarp-ganda repo
- CI/CD workflow reduced from 334 lines to ~130 lines (61% reduction)
- No broken references in build scripts

## Checklist

### Cleanup
- [ ] Remove Ganda project reference from `Scripts/Build.cs` (line 15)
- [ ] Delete `.github/workflows/sync-configurable-files.yml` (references non-existent `Tools/FileSync/`)
- [ ] Delete `exe/` directory (all 13 files exist in Ganda repo)
- [ ] Replace `.github/workflows/ci-cd.yml` with simplified version

### Verification
- [ ] Build succeeds: `dotnet build Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj`
- [ ] Tests pass: `./Tests/RunTests.cs`
- [ ] No references to `exe/` in workflow paths

## Notes

**Analysis Reports:**
- `.agent/workspace/2025-12-11T17-15-00_github-workflow-cleanup-analysis.md` - Full findings
- `.agent/workspace/2025-12-11T17-45-00_github-workflow-cleanup-commands.md` - Exact commands and replacement workflow

**Exe Directory Comparison (Amuru vs Ganda):**

| File | In Amuru | In Ganda |
|------|----------|----------|
| `Directory.Build.props` | ✅ | ✅ |
| `check-assembly-metadata.cs` | ✅ | ✅ |
| `clear-runfile-cache.cs` | ✅ | ✅ |
| `convert-timestamp.cs` | ✅ | ✅ |
| `display-avatar.cs` | ✅ | ✅ |
| `generate-avatar.cs` | ✅ | ✅ |
| `generate-color.cs` | ✅ | ✅ |
| `github-report.cs` | ✅ | ✅ |
| `installer.cs` | ✅ | ✅ |
| `multiavatar.cs` | ✅ | ✅ |
| `post.cs` | ✅ | ✅ |
| `ssh-key-helper.cs` | ✅ | ✅ |
| `update-master.cs` | ✅ | ✅ |

**Impact Summary:**

| Metric | Before | After |
|--------|--------|-------|
| `ci-cd.yml` lines | 334 | ~130 |
| CI jobs | 4 | 2 |
| Matrix builds | 6 (3 platforms × 2 jobs) | 1 |
| `exe/` files | 13 | 0 |
| Workflow files | 2 | 1 |

**Related:**
- Migration analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`
- Completed cleanup: `Kanban/Done/059_Cleanup-Ganda-After-Migration.md`
- Ganda repository: https://github.com/TimeWarpEngineering/timewarp-ganda
