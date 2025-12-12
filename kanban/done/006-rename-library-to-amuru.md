# Rename Library from TimeWarp.Cli to TimeWarp.Amuru

## Description

Rename the entire library from TimeWarp.Cli to TimeWarp.Amuru to align with the new repository name (timewarp-amuru). This involves updating all namespaces, package identifiers, project files, and documentation throughout the codebase. Additionally, the old TimeWarp.Cli package should be unlisted from NuGet to prevent confusion.

## Requirements

- Update all namespaces from TimeWarp.Cli to TimeWarp.Amuru
- Update Package ID from TimeWarp.Cli to TimeWarp.Amuru
- Update all project file names and references
- Update all documentation references
- Unlist TimeWarp.Cli package from NuGet.org
- Ensure all scripts and tests continue to work with new naming
- Maintain backward compatibility notice for users migrating from TimeWarp.Cli

## Checklist

### Project Structure
- [ ] Rename Source/TimeWarp.Cli/ to Source/TimeWarp.Amuru/
- [ ] Update TimeWarp.Cli.csproj to TimeWarp.Amuru.csproj
- [ ] Update PackageId in project file to TimeWarp.Amuru
- [ ] Update all namespace declarations from TimeWarp.Cli to TimeWarp.Amuru
- [ ] Update GlobalUsings.cs namespace references

### Build System
- [ ] Update Scripts/Build.cs project references
- [ ] Update Scripts/Pack.cs project references
- [ ] Update Scripts/Clean.cs cleanup paths
- [ ] Update LocalNuGetFeed references

### Tests and Examples
- [ ] Update all test files to use #:package TimeWarp.Amuru
- [ ] Update all example scripts in Spikes/CsScripts/
- [ ] Update test runner references
- [ ] Verify all tests pass with new naming

### Documentation
- [ ] Update README.md with new package name
- [ ] Update CLAUDE.md with new naming throughout
- [ ] Update all code examples in documentation
- [ ] Add migration guide from TimeWarp.Cli to TimeWarp.Amuru
- [ ] Update GitHub repository description

### NuGet Management
- [ ] Unlist all versions of TimeWarp.Cli from NuGet.org
- [ ] Add deprecation notice to TimeWarp.Cli package pointing to TimeWarp.Amuru
- [ ] Publish initial TimeWarp.Amuru package version
- [ ] Update GitHub Actions workflows for new package name

### Git Repository
- [ ] Update .gitignore paths if needed
- [ ] Update GitHub Actions workflow files
- [ ] Update any repository-specific configurations

## Notes

Migration message for TimeWarp.Cli package:
```
This package has been renamed to TimeWarp.Amuru. 
Please update your references:
- Old: #:package TimeWarp.Cli
- New: #:package TimeWarp.Amuru

The API remains unchanged, only the package name has changed.
```

Important considerations:
- The unlisting process on NuGet.org should be done after the new package is successfully published
- Consider keeping TimeWarp.Cli listed but deprecated for a transition period
- Ensure all internal references are updated before publishing

## Implementation Notes

Completed successfully on 2025-07-31:
- Used `git mv` to rename directories and preserve history
- Updated all namespaces from TimeWarp.Cli to TimeWarp.Amuru
- Updated project references in all build scripts and tests
- Updated documentation (README.md, CLAUDE.md)
- Updated GitHub Actions workflow
- All 332 tests passing successfully
- Ready for NuGet publication