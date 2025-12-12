# Setup Ganda Repository Structure

## Description

Create the initial directory structure and configuration files in the private `timewarp-ganda` repository. This establishes the foundation for the Zana (tools library) and Ganda (CLI) projects. Follow kebab-case naming conventions from timewarp-flexbox.

## Parent

Migration Analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`

## Checklist

### Directory Structure (kebab-case)
- [x] Create `source/timewarp-zana/` directory
- [x] Create `source/timewarp-ganda/` directory
- [x] Create `exe/` directory for private runfiles
- [x] Create `assets/` directory
- [x] Copy logo.png to assets/

### Documentation (copy from timewarp-flexbox)
- [x] Create `documentation/developer/standards/` directory structure
- [x] Copy `documentation/developer/standards/overview.md`
- [x] Copy `documentation/developer/standards/architectural-standards.md`
- [x] Copy `documentation/developer/standards/code-organization.md`
- [x] Copy `documentation/developer/standards/coding-practices.md`
- [x] Copy `documentation/developer/standards/csharp-coding.md`
- [x] Copy `documentation/developer/standards/documentation-standards.md`
- [x] Copy `documentation/developer/standards/enforcement.md`
- [x] Copy `documentation/developer/standards/file-naming.md`
- [x] Copy `documentation/developer/standards/git-commit-message-format.md`
- [x] Copy `documentation/developer/standards/git-workflow.md`
- [x] Copy `documentation/developer/standards/naming-conventions.md`
- [x] Copy `documentation/developer/standards/testing.md`
- [x] Copy `documentation/developer/standards/xml-documentation.md`
- [x] Update overview.md references from "TimeWarp.Flexbox" to "TimeWarp.Ganda"

### Root Configuration Files
- [x] Create `Directory.Build.props` (root level)
- [x] Create `Directory.Packages.props` with package versions
- [x] Copy `.editorconfig` from flexbox or Amuru
- [x] Create `nuget.config` with NuGet.org + GitHub Packages sources

### Source Configuration
- [x] Create `source/Directory.Build.props` with package metadata

## Notes

Target location: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-ganda/Cramer-2025-12-06-dev`

Reference structure: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-flexbox/Cramer-2025-11-21-dev`

Package naming:
- `TimeWarp.Zana` = "Tools" (in-process utilities)
- `TimeWarp.Ganda` = "Shell" (CLI wrapper)

Directory naming convention: **kebab-case** for all folders and files (except MSBuild files like `Directory.Build.props`)

See analysis document for exact file contents.

## Results

Repository structure successfully created at `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-ganda/Cramer-2025-12-06-dev`:

- **Directory structure**: source/timewarp-zana/, source/timewarp-ganda/, exe/, assets/
- **Documentation**: 13 developer standards files copied and updated
- **Configuration**: Directory.Build.props (root + source), Directory.Packages.props, .editorconfig, nuget.config

Remaining migration phases (2-5) to be tracked in their respective repositories.
