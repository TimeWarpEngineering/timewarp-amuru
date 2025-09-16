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
- [ ] Rename Source/TimeWarp.Amuru.Tool to Source/TimeWarp.Ganda
- [ ] Update TimeWarp.Ganda.csproj with new PackageId
- [ ] Update all project references
- [ ] Update Directory.Build.props if needed
- [ ] Update solution file

### CI/CD Updates
- [ ] Update .github/workflows/ci-cd.yml references
- [ ] Update build scripts
- [ ] Update release scripts
- [ ] Test GitHub Actions workflow

### Documentation
- [ ] Update main README.md
- [ ] Update Source/TimeWarp.Ganda/README.md
- [ ] Update Architecture/TimeWarp-Ecosystem-Architecture.md
- [ ] Update installation instructions
- [ ] Create migration notice for users

### Package Management
- [ ] Publish final version of TimeWarp.Amuru.Tool with deprecation notice
- [ ] Publish first version of TimeWarp.Ganda
- [ ] Update NuGet package description
- [ ] Add package tags for discoverability

### Testing
- [ ] Test local installation: `dotnet tool install --global TimeWarp.Ganda`
- [ ] Test `timewarp` command still works
- [ ] Test `timewarp install` functionality
- [ ] Test all subcommands

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