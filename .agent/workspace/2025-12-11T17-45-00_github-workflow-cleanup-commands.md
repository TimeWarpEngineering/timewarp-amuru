# GitHub Workflow Cleanup: Execution Commands

## Executive Summary

Based on your decisions, here are the exact commands and file changes needed to clean up the repository after the Ganda/exe migration. All 13 files in Amuru's `exe/` directory already exist in the Ganda repo (plus Ganda has additional files).

## Exe Directory Comparison

| File | In Amuru | In Ganda | Action |
|------|----------|----------|--------|
| `Directory.Build.props` | ✅ | ✅ | DELETE |
| `check-assembly-metadata.cs` | ✅ | ✅ | DELETE |
| `clear-runfile-cache.cs` | ✅ | ✅ | DELETE |
| `convert-timestamp.cs` | ✅ | ✅ | DELETE |
| `display-avatar.cs` | ✅ | ✅ | DELETE |
| `generate-avatar.cs` | ✅ | ✅ | DELETE |
| `generate-color.cs` | ✅ | ✅ | DELETE |
| `github-report.cs` | ✅ | ✅ | DELETE |
| `installer.cs` | ✅ | ✅ | DELETE |
| `multiavatar.cs` | ✅ | ✅ | DELETE |
| `post.cs` | ✅ | ✅ | DELETE |
| `ssh-key-helper.cs` | ✅ | ✅ | DELETE |
| `update-master.cs` | ✅ | ✅ | DELETE |

**Ganda has additional files**: `dev-setup.cs`, `github-restore.cs`, `kanban.cs`, `repo-setup.cs`

---

## Commands to Execute

### 1. Remove Ganda Reference from Scripts/Build.cs

Edit `Scripts/Build.cs` line 12-16, change from:
```csharp
string[] projectsToBuild = [
  "../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj",
  "../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj",
  "../Source/TimeWarp.Ganda/TimeWarp.Ganda.csproj"
];
```

To:
```csharp
string[] projectsToBuild = [
  "../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj",
  "../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj"
];
```

### 2. Delete Sync Workflow

```bash
rm .github/workflows/sync-configurable-files.yml
```

### 3. Delete exe/ Directory

```bash
rm -rf exe/
```

### 4. Replace ci-cd.yml

Replace `.github/workflows/ci-cd.yml` with:

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
    if: github.event_name != 'release'
    runs-on: ubuntu-latest
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
          dotnet-quality: 'preview'

      - name: Make scripts executable
        run: |
          chmod +x Scripts/Build.cs
          chmod +x Scripts/Clean.cs
          chmod +x Tests/RunTests.cs
          find Tests/Integration -name "*.cs" -type f -exec chmod +x {} \;

      - name: Build and Test
        run: |
          echo "Building TimeWarp.Amuru..."
          dotnet build Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj --configuration Release

          echo "Running tests..."
          ./Tests/RunTests.cs

      - name: Upload NuGet Packages
        uses: actions/upload-artifact@v4
        with:
          name: packages-${{ github.sha }}
          if-no-files-found: error
          retention-days: 90
          path: |
            artifacts/packages/TimeWarp.Amuru.*.nupkg

  publish-release:
    if: github.event_name == 'release'
    runs-on: ubuntu-latest
    permissions:
      contents: write
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          ref: ${{ github.event.release.tag_name }}

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
          dotnet-quality: 'preview'

      - name: Get commit SHA from release tag
        id: get-sha
        run: |
          SHA=$(git rev-parse HEAD)
          echo "sha=$SHA" >> $GITHUB_OUTPUT
          echo "Release tag ${{ github.event.release.tag_name }} points to commit $SHA"

      - name: Find workflow run for commit
        id: find-run
        run: |
          RUN_ID=$(gh api "/repos/${{ github.repository }}/actions/runs?event=push&status=completed&branch=master" \
            --jq ".workflow_runs[] | select(.head_sha == \"${{ steps.get-sha.outputs.sha }}\" and .event == \"push\") | .id" \
            | head -1)

          if [ -z "$RUN_ID" ]; then
            echo "ERROR: Could not find workflow run for commit ${{ steps.get-sha.outputs.sha }}"
            exit 1
          fi

          echo "run-id=$RUN_ID" >> $GITHUB_OUTPUT
          echo "Found workflow run ID: $RUN_ID for commit ${{ steps.get-sha.outputs.sha }}"
        env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}

      - name: Download package artifacts
        uses: actions/download-artifact@v4
        with:
          name: packages-${{ steps.get-sha.outputs.sha }}
          path: artifacts/packages
          github-token: ${{ secrets.GITHUB_TOKEN }}
          run-id: ${{ steps.find-run.outputs.run-id }}

      - name: Check if version already published
        run: |
          VERSION=$(grep '<Version>' Source/Directory.Build.props | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/')

          echo "Checking if TimeWarp.Amuru $VERSION is already published on NuGet.org..."
          if dotnet package search TimeWarp.Amuru --exact-match --prerelease --source https://api.nuget.org/v3/index.json | grep -q "$VERSION"; then
            echo "⚠️ WARNING: TimeWarp.Amuru $VERSION is already published to NuGet.org"
            echo "❌ This version cannot be republished. Please increment the version in Directory.Build.props"
            exit 1
          else
            echo "✅ TimeWarp.Amuru $VERSION is not yet published on NuGet.org"
          fi

      - name: Publish to NuGet.org
        run: |
          VERSION=$(grep '<Version>' Source/Directory.Build.props | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/')

          echo "Publishing TimeWarp.Amuru $VERSION..."
          dotnet nuget push "artifacts/packages/TimeWarp.Amuru.$VERSION.nupkg" \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate
        env:
          DOTNET_NUGET_SIGNATURE_VERIFICATION: false

      - name: Upload Release Assets
        uses: softprops/action-gh-release@v1
        with:
          files: |
            artifacts/packages/*
```

---

## Summary of Changes

| Action | Before | After | Reduction |
|--------|--------|-------|-----------|
| `ci-cd.yml` | 334 lines | ~130 lines | 61% |
| Jobs | 4 (build-exe, build-installer, build-packages, publish) | 2 (build-and-test, publish-release) | 50% |
| Matrix builds | 3 platforms × 2 jobs = 6 builds | 1 build | 83% |
| `exe/` directory | 13 files | 0 files | 100% |
| Workflow files | 2 | 1 | 50% |

---

## Files to Delete

```
.github/workflows/sync-configurable-files.yml
exe/Directory.Build.props
exe/check-assembly-metadata.cs
exe/clear-runfile-cache.cs
exe/convert-timestamp.cs
exe/display-avatar.cs
exe/generate-avatar.cs
exe/generate-color.cs
exe/github-report.cs
exe/installer.cs
exe/multiavatar.cs
exe/post.cs
exe/ssh-key-helper.cs
exe/update-master.cs
```

## Files to Modify

```
Scripts/Build.cs           # Remove Ganda project reference
.github/workflows/ci-cd.yml # Replace with simplified version
```
