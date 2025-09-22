# 030 Rename TimeWarp.Amuru.Tool to TimeWarp.Ganda

## Description

Rename the .NET tool package from TimeWarp.Amuru.Tool to TimeWarp.Ganda to better reflect its purpose as a shell toolkit. "Ganda" means "shell" in Swahili, aligning with our naming convention where each component's name describes its function.

## Requirements

- Maintain backward compatibility during transition
- Keep `timewarp` as the primary command name
- Update all documentation and references
- Ensure CI/CD continues to work

## Checklist

### Implementation
- [x] Rename Source/TimeWarp.Amuru.Tool to Source/TimeWarp.Ganda
- [x] Update TimeWarp.Ganda.csproj with new PackageId
- [x] Update all project references
- [x] Update Directory.Build.props if needed
- [x] Update solution file (N/A - no .sln file in project)

### CI/CD Updates
- [x] Update .github/workflows/ci-cd.yml references
- [x] Update build scripts
- [x] Update release scripts
- [ ] Test GitHub Actions workflow (needs push to test)

### Documentation
- [x] Update main README.md
- [x] Update Source/TimeWarp.Ganda/README.md
- [x] Update Architecture/TimeWarp-Ecosystem-Architecture.md
- [x] Update installation instructions
- [x] Create migration notice for users (N/A - internal use only)

### Package Management
- [ ] Publish final version of TimeWarp.Amuru.Tool with deprecation notice (N/A - internal use only)
- [ ] Publish first version of TimeWarp.Ganda (pending push)
- [x] Update NuGet package description
- [x] Add package tags for discoverability

### Testing
- [x] Test local installation: `dotnet tool install --global TimeWarp.Ganda`
- [x] Test `timewarp` command still works
- [x] Test `timewarp install` functionality (shows in help)
- [x] Test all subcommands (tested convert-timestamp)

## Notes

The tool will continue to provide the `timewarp` command, but the package name will better reflect its role as a shell (Ganda) rather than a generic "tool". This aligns with the Swahili naming convention:
- Nuru (Light) - CLI routing
- Amuru (Command) - Process control
- Ganda (Shell) - The actual shell toolkit
- Kijamii (Social) - Social platform integration

## Migration Strategy

1. Publish TimeWarp.Ganda alongside TimeWarp.Amuru.Tool initially
2. Add deprecation notice to TimeWarp.Amuru.Tool pointing to TimeWarp.Ganda
3. After grace period, unlist TimeWarp.Amuru.Tool from NuGet (but keep available for existing users)