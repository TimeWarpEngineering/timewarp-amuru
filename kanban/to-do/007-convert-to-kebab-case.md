# Convert All Files and Directories to kebab-case (XL)

## Description

Convert all file and directory names throughout the codebase from PascalCase/CamelCase to kebab-case for consistency with modern naming conventions. This is an extra-large task that will touch every file and directory in the project, requiring careful coordination to ensure all references are updated correctly.

## Requirements

- Convert all directory names to kebab-case (e.g., Source/ → source/, TimeWarp.Cli/ → timewarp-cli/)
- Convert all file names to kebab-case (e.g., CommandExtensions.cs → command-extensions.cs)
- Update all file references in code, scripts, and documentation
- Ensure git history is preserved appropriately
- Update all build scripts and project references
- Maintain functionality of all tests and scripts
- Create migration script to automate the conversion process

## Checklist

### Planning
- [ ] Create comprehensive list of all files and directories to rename
- [ ] Map out all cross-references that need updating
- [ ] Design migration script to handle renaming systematically
- [ ] Plan git strategy to preserve history (git mv commands)

### Directory Structure
- [ ] Rename Source/ → source/
- [ ] Rename Tests/ → tests/
- [ ] Rename Scripts/ → scripts/
- [ ] Rename Spikes/CsScripts/ → spikes/cs-scripts/
- [ ] Rename LocalNuGetFeed/ → local-nuget-feed/
- [ ] Rename Scratch/ → scratch/
- [ ] Rename Kanban/ subdirectories (Backlog/, ToDo/, InProgress/, Done/)

### Source Files
- [ ] CommandExtensions.cs → command-extensions.cs
- [ ] CommandResult.cs → command-result.cs
- [ ] GlobalUsings.cs → global-usings.cs
- [ ] All other .cs files in source directory
- [ ] Update namespace declarations if needed

### Build and Script Files
- [ ] Build.cs → build.cs
- [ ] Pack.cs → pack.cs
- [ ] Clean.cs → clean.cs
- [ ] RunTests.cs → run-tests.cs
- [ ] Update all script shebangs and references

### Project Files
- [ ] TimeWarp.Cli.csproj → timewarp-cli.csproj (or timewarp-amuru.csproj)
- [ ] Directory.Build.props → directory.build.props
- [ ] Update all project references in .csproj files

### Configuration Files
- [ ] nuget.config (already kebab-case)
- [ ] .gitignore references
- [ ] .editorconfig (if exists)
- [ ] GitHub Actions workflow files

### Documentation
- [ ] CLAUDE.md → claude.md
- [ ] README.md → readme.md
- [ ] Update all file path references in documentation
- [ ] Update code examples with new file names
- [ ] Task-Template.md → task-template.md

### Tests
- [ ] Update all test file names
- [ ] Update test references and imports
- [ ] Update #:package references in test scripts
- [ ] Verify all tests pass after renaming

### Git Operations
- [ ] Use git mv for all renames to preserve history
- [ ] Create single commit for all renames
- [ ] Update .gitignore with new paths
- [ ] Test checkout on different platforms

## Notes

Example transformations:
```
CommandExtensions.cs → command-extensions.cs
TimeWarp.Cli/ → timewarp-cli/
GlobalUsings.cs → global-usings.cs
LocalNuGetFeed/ → local-nuget-feed/
```

Migration script outline:
```bash
#!/bin/bash
# Rename directories first (deepest first)
git mv Source/TimeWarp.Cli source/timewarp-cli
git mv Scripts scripts
git mv Tests tests
# Then rename files
git mv source/timewarp-cli/CommandExtensions.cs source/timewarp-cli/command-extensions.cs
# Update references in files
find . -name "*.cs" -o -name "*.csproj" -o -name "*.md" | xargs sed -i 's/CommandExtensions\.cs/command-extensions.cs/g'
```

Critical considerations:
- This change will break all existing PRs and branches
- Coordinate with team before executing
- Consider doing this as part of the TimeWarp.Amuru rename
- Test thoroughly on Windows, Linux, and macOS
- Some tools may be case-insensitive (Windows) - test carefully

## Implementation Notes

[To be filled during implementation]