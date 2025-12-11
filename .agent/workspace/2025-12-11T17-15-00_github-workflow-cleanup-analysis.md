# GitHub Workflow Cleanup Analysis: Post-Ganda Migration

## Executive Summary

The CI/CD workflow has **significant dead code and orphaned references** after the Ganda/exe migration. The workflow still builds standalone executables (`exe/`) on 3 platforms with full attestation, but these utilities are no longer the focus of this repository. The sync workflow references a non-existent `Tools/FileSync/` directory. `Scripts/Build.cs` still references the removed `TimeWarp.Ganda` project.

## Scope

**Analyzed Files:**
- `.github/workflows/ci-cd.yml` (334 lines)
- `.github/workflows/sync-configurable-files.yml` (99 lines)
- `.github/sync-config.yml` (45 lines)
- `Scripts/Build.cs` (55 lines)
- `exe/` directory contents

## Findings

### 1. CI/CD Workflow Issues (`ci-cd.yml`)

#### Issue A: Executable Build Jobs Should Be Removed or Simplified

The workflow has **3 major jobs** for building executables that are now secondary to the main library:

| Job | Lines | Purpose | Status |
|-----|-------|---------|--------|
| `build-executables` | 53-122 | Builds 12+ exe/*.cs files across 3 platforms | **REMOVE or simplify** |
| `build-installer` | 124-191 | Builds installer.cs (depends on executables) | **REMOVE or simplify** |
| `build-packages` | 193-232 | Builds NuGet + runs tests | **KEEP** |
| `publish-release` | 234-334 | Publishes NuGet + release assets | **MODIFY** |

**Problem**: 70% of the workflow (230+ lines) is dedicated to building standalone executables across 3 platforms. These `exe/` scripts are utilities that were meant to be part of Ganda, but they're still being built here with:
- Full matrix builds (Linux, Windows, macOS)
- Artifact attestation
- Archive creation (tar.gz/zip)
- 90-day retention

**Recommendation**: Since Ganda moved to a separate repo, decide:
1. **Option A**: Remove `exe/` directory entirely and all executable build jobs (cleanest)
2. **Option B**: Keep `exe/` but only build on Linux, drop attestation/archives (minimal)

#### Issue B: Paths Trigger Still Includes `exe/`

```yaml
paths:
  - 'Source/**'
  - 'Tests/**'
  - 'Scripts/**'
  - '.github/workflows/**'
  - 'exe/**'           # <-- Still triggers builds for exe/ changes
  - 'Directory.Build.props'
```

**Action**: Remove `'exe/**'` if executables are removed, or keep if retaining.

#### Issue C: Release Assets Include Executables

Lines 327-333:
```yaml
- name: Upload Release Assets
  uses: softprops/action-gh-release@v1
  with:
    files: |
      artifacts/executables/*    # <-- Executables
      artifacts/installer/*      # <-- Installer
      artifacts/packages/*       # <-- NuGet packages
```

**Action**: If keeping only NuGet, simplify to:
```yaml
files: artifacts/packages/*
```

---

### 2. Sync Workflow Issues (`sync-configurable-files.yml`)

#### Issue: References Non-Existent Script

Line 54:
```yaml
run: |
  ...
  ./Tools/FileSync/SyncConfigurableFiles.ps1
```

**Problem**: The `Tools/FileSync/` directory does not exist in this repository.

**Action**: Either:
1. Remove this entire workflow file
2. Create the missing `Tools/FileSync/SyncConfigurableFiles.ps1` script
3. Disable the workflow temporarily

---

### 3. Build Script Issue (`Scripts/Build.cs`)

#### Issue: References Removed Ganda Project

Line 15:
```csharp
string[] projectsToBuild = [
  "../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj",
  "../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj",
  "../Source/TimeWarp.Ganda/TimeWarp.Ganda.csproj"  // <-- DOES NOT EXIST
];
```

**Status**: `Source/TimeWarp.Ganda/` directory has been removed, but the build script still references it.

**Action**: Remove the Ganda project reference from the array.

---

### 4. Solution File Status

**Good News**: `TimeWarp.Amuru.slnx` has already been cleaned up and only references:
- `Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj`
- `Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj`

---

### 5. Exe Directory Decision

Current `exe/` contents (13 files):
```
check-assembly-metadata.cs    generate-color.cs    multiavatar.cs
clear-runfile-cache.cs        github-report.cs     post.cs
convert-timestamp.cs          installer.cs         ssh-key-helper.cs
Directory.Build.props         update-master.cs
display-avatar.cs             generate-avatar.cs
```

**Question**: Should these stay in this repo or move to timewarp-ganda?

Per the migration analysis, these were meant to become part of the Ganda ecosystem as either:
- `exe/` private runfiles in timewarp-ganda
- Commands within the Ganda CLI tool

---

## Recommendations

### Priority 1: Fix Broken References (CRITICAL)

| File | Line | Issue | Fix |
|------|------|-------|-----|
| `Scripts/Build.cs` | 15 | References non-existent Ganda project | Remove line |
| `sync-configurable-files.yml` | 54 | References non-existent `Tools/FileSync/` | Delete workflow or create script |

### Priority 2: Simplify CI/CD Workflow (HIGH)

**If removing exe/ directory:**

1. Delete `exe/` directory entirely
2. Remove from `ci-cd.yml`:
   - `build-executables` job (lines 37-122)
   - `build-installer` job (lines 124-191)
   - `'exe/**'` from paths trigger (lines 12, 22)
   - Executable download/upload in `publish-release` (lines 276-292, 330-332)
3. Update `build-packages` to not depend on removed jobs (remove `needs: [build-executables, build-installer]`)

**If keeping exe/ directory:**

1. Simplify to single-platform build (Linux only)
2. Remove attestation overhead
3. Consider moving to separate workflow that only runs on tags/releases

### Priority 3: Documentation Cleanup (MEDIUM)

Multiple files still reference Ganda heavily:
- `readme.md` - References Ganda CLI tool installation
- `Documentation/User/Installation.md` - Full Ganda installation guide
- `Documentation/Developer/ProgressiveEnhancementPattern.md` - Layer 5 Ganda references

These should be updated to point to the timewarp-ganda repo exclusively.

---

## Proposed Simplified ci-cd.yml

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [master]
    paths:
      - 'Source/**'
      - 'Tests/**'
      - 'Scripts/**'
      - '.github/workflows/**'
      - 'Directory.Build.props'
  pull_request:
    branches: [master]
    paths:
      - 'Source/**'
      - 'Tests/**'
      - 'Scripts/**'
      - '.github/workflows/**'
      - 'Directory.Build.props'
  release:
    types: [published]
  workflow_dispatch:

permissions:
  contents: write
  packages: write
  id-token: write
  attestations: write

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
          dotnet-quality: 'preview'

      - name: Make scripts executable
        run: |
          chmod +x Scripts/Build.cs
          chmod +x Tests/RunTests.cs
          find Tests/Integration -name "*.cs" -type f -exec chmod +x {} \;

      - name: Build and Test
        run: |
          dotnet build Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj -c Release
          ./Tests/RunTests.cs

      - name: Upload NuGet Packages
        uses: actions/upload-artifact@v4
        with:
          name: packages-${{ github.sha }}
          if-no-files-found: error
          retention-days: 90
          path: artifacts/packages/TimeWarp.Amuru.*.nupkg

  publish-release:
    if: github.event_name == 'release'
    needs: build-and-test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          ref: ${{ github.event.release.tag_name }}

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
          dotnet-quality: 'preview'

      - name: Build Release Package
        run: dotnet build Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj -c Release

      - name: Check if version already published
        run: |
          VERSION=$(grep '<Version>' Source/Directory.Build.props | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/')
          echo "Checking if TimeWarp.Amuru $VERSION is already published..."
          if dotnet package search TimeWarp.Amuru --exact-match --prerelease --source https://api.nuget.org/v3/index.json | grep -q "$VERSION"; then
            echo "Version $VERSION already published!"
            exit 1
          fi

      - name: Publish to NuGet.org
        run: |
          VERSION=$(grep '<Version>' Source/Directory.Build.props | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/')
          dotnet nuget push "artifacts/packages/TimeWarp.Amuru.$VERSION.nupkg" \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate
```

This reduces the workflow from **334 lines to ~85 lines** (75% reduction).

---

## Summary

| Category | Items Found | Action Required |
|----------|-------------|-----------------|
| Broken references | 2 | **CRITICAL** - Fix immediately |
| Dead workflow code | ~230 lines | **HIGH** - Remove/simplify |
| Orphaned sync workflow | 1 file | **HIGH** - Delete or fix |
| Documentation updates | 3+ files | **MEDIUM** - Update Ganda refs |

**Total cleanup effort**: Remove/modify ~250 lines of workflow code, fix 2 broken references.

---

## References

- Migration analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`
- Cleanup task: `Kanban/Done/059_Cleanup-Ganda-After-Migration.md`
- Ganda repository: https://github.com/TimeWarpEngineering/timewarp-ganda
