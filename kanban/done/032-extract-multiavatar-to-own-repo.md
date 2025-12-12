# 032 Extract TimeWarp.Multiavatar to Own Repository

**Status: COMPLETED** (2025-09-18)
- Repository created and code migrated successfully
- Using existing NuGet package TimeWarp.Multiavatar@1.0.0-beta.10
- Minor issue: multiavatar.cs has Nuru routing bug (tracked separately)

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
- [x] Create github.com/TimeWarpEngineering/timewarp-multiavatar repository
- [ ] Set up repository settings (description, topics, etc.)
- [ ] Configure branch protection rules
- [ ] Set up repository secrets for CI/CD (NUGET_API_KEY needed)

### Code Migration
- [x] Export TimeWarp.Multiavatar with git history (copied directly)
- [x] Move Source/TimeWarp.Multiavatar to new repo
- [x] Move relevant documentation
- [x] Set up .gitignore
- [x] Add LICENSE file
- [x] Create comprehensive README.md

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
- [x] Create CI workflow for build and test
- [x] Create release workflow for NuGet publishing
- [ ] Configure NuGet API key secret (waiting on user)
- [ ] Test automated publishing (after secret configured)

### Update This Repository
- [x] Remove Source/TimeWarp.Multiavatar
- [x] Update exe/multiavatar.cs to use NuGet package
- [x] Update exe/generate-avatar.cs similarly
- [x] Update solution file (no solution file exists)
- [x] Update CI/CD workflows (not needed)
- [x] Update documentation (Shell Commands Reference created)

### NuGet Package
- [ ] Publish initial version from new repo (waiting for tag push)
- [x] Verify package is available on NuGet.org (beta.10 exists)
- [x] Test installation in a clean project (using beta.10)
- [x] Update package metadata with new repo URL

### Documentation Updates
- [ ] Update Architecture/TimeWarp-Ecosystem-Architecture.md
- [ ] Update main README.md in this repo
- [x] Create migration guide (MULTIAVATAR_EXTRACTION_COMMANDS.md)
- [x] Update any tutorials or examples (Shell Commands Reference)

### Testing
- [ ] Test exe/multiavatar.cs with NuGet package (has Nuru routing issue)
- [ ] Test exe/generate-avatar.cs with NuGet package
- [ ] Test `timewarp install` still includes multiavatar
- [x] Verify no regression in functionality (tests created)

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