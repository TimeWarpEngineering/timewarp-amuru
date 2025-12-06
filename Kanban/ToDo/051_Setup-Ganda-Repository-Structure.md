# Setup Ganda Repository Structure

## Description

Create the initial directory structure and configuration files in the private `timewarp-ganda` repository. This establishes the foundation for the Zana (tools library) and Ganda (CLI) projects.

## Parent

Migration Analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`

## Checklist

### Directory Structure
- [ ] Create `Source/TimeWarp.Zana/` directory
- [ ] Create `Source/TimeWarp.Ganda/` directory
- [ ] Create `exe/` directory for private runfiles
- [ ] Create `assets/` directory
- [ ] Copy logo.png to assets/

### Root Configuration Files
- [ ] Create `Directory.Build.props` (root level)
- [ ] Create `Directory.Packages.props` with package versions
- [ ] Copy `.editorconfig` from Amuru
- [ ] Create `nuget.config` with NuGet.org + GitHub Packages sources

### Source Configuration
- [ ] Create `Source/Directory.Build.props` with package metadata

## Notes

Target location: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-ganda/Cramer-2025-12-06-dev`

Package naming:
- `TimeWarp.Zana` = "Tools" (in-process utilities)
- `TimeWarp.Ganda` = "Shell" (CLI wrapper)

See analysis document for exact file contents.
