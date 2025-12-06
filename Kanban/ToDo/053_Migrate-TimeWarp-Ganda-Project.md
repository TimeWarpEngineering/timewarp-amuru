# Migrate TimeWarp.Ganda Project

## Description

Migrate the TimeWarp.Ganda CLI tool from the Amuru repository to the Ganda repository. Update references to use PackageReference for Amuru and ProjectReference for Zana.

## Parent

Migration Analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`

## Requirements

- CLI must work identically after migration
- Must reference Zana via ProjectReference (same repo)
- Must reference Amuru via PackageReference (from NuGet.org)
- Must remain a dotnet tool with `timewarp` command name

## Checklist

### Copy Project
- [ ] Copy `Source/TimeWarp.Ganda/` from Amuru to Ganda repo
- [ ] Copy `readme.md` to repo root

### Update Project File
- [ ] Change Amuru ProjectReference to PackageReference
- [ ] Add ProjectReference to Zana
- [ ] Update asset paths (logo, readme)

### Update Code
- [ ] Update `GlobalUsings.cs` to include `TimeWarp.Zana`
- [ ] Verify all existing commands still work

### Validation
- [ ] Build project successfully
- [ ] Run `timewarp --help` and verify output
- [ ] Test at least one command (e.g., `timewarp multiavatar test`)

## Notes

Current Ganda location in Amuru: `Source/TimeWarp.Ganda/`

Commands to preserve:
- multiavatar
- generate-avatar
- convert-timestamp
- generate-color
- ssh-key-helper
- install
- list
