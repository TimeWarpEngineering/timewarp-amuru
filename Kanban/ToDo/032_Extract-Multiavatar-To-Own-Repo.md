# 032 Extract TimeWarp.Multiavatar to Own Repository

## Description

Move TimeWarp.Multiavatar from this repository to its own dedicated repository (timewarp-multiavatar). This library is a specialized avatar generation tool that doesn't depend on Amuru and should have its own development lifecycle, issue tracking, and release schedule.

## Requirements

- Create new repository with proper structure
- Preserve git history if possible
- Update all references to use NuGet package
- Maintain backward compatibility
- No disruption to existing users

## Checklist

### Repository Creation
- [ ] Create github.com/TimeWarpEngineering/timewarp-multiavatar repository
- [ ] Set up repository settings (description, topics, etc.)
- [ ] Configure branch protection rules
- [ ] Set up repository secrets for CI/CD

### Code Migration
- [ ] Export TimeWarp.Multiavatar with git history (using git filter-branch or similar)
- [ ] Move Source/TimeWarp.Multiavatar to new repo
- [ ] Move relevant documentation
- [ ] Set up .gitignore
- [ ] Add LICENSE file
- [ ] Create comprehensive README.md

### Project Structure (New Repo)
```
timewarp-multiavatar/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── release.yml
├── Source/
│   └── TimeWarp.Multiavatar/
│       ├── TimeWarp.Multiavatar.csproj
│       ├── Data/
│       ├── MultiavatarGenerator.cs
│       └── README.md
├── Tests/
│   └── TimeWarp.Multiavatar.Tests/
├── Directory.Build.props
├── Directory.Packages.props
├── README.md
├── LICENSE
└── CHANGELOG.md
```

### CI/CD Setup
- [ ] Create CI workflow for build and test
- [ ] Create release workflow for NuGet publishing
- [ ] Configure NuGet API key secret
- [ ] Test automated publishing

### Update This Repository
- [ ] Remove Source/TimeWarp.Multiavatar
- [ ] Update exe/multiavatar.cs to use NuGet package:
  ```csharp
  #:package TimeWarp.Multiavatar@1.0.0
  ```
- [ ] Update exe/generate-avatar.cs similarly
- [ ] Update solution file
- [ ] Update CI/CD workflows
- [ ] Update documentation

### NuGet Package
- [ ] Publish initial version from new repo
- [ ] Verify package is available on NuGet.org
- [ ] Test installation in a clean project
- [ ] Update package metadata with new repo URL

### Documentation Updates
- [ ] Update Architecture/TimeWarp-Ecosystem-Architecture.md
- [ ] Update main README.md in this repo
- [ ] Create migration guide
- [ ] Update any tutorials or examples

### Testing
- [ ] Test exe/multiavatar.cs with NuGet package
- [ ] Test exe/generate-avatar.cs with NuGet package
- [ ] Test `timewarp install` still includes multiavatar
- [ ] Verify no regression in functionality

## Notes

TimeWarp.Multiavatar is a good candidate for extraction because:
- No dependency on Amuru
- Self-contained functionality
- Significant embedded data (JSON files)
- Could benefit from independent versioning
- Specific domain (avatar generation)

The extraction should be done carefully to preserve functionality and not disrupt current users. The exe wrappers will continue to work, just referencing the NuGet package instead of the local project.

## Migration Announcement

After extraction, announce to users:
- TimeWarp.Multiavatar now has its own repository
- NuGet package remains the same
- exe commands continue to work unchanged
- Benefits: faster releases, focused development

## Related Tasks

- This task should be completed before task 033 (Extract Kijamii)
- Coordinate with any active PRs affecting Multiavatar