# 033 Extract TimeWarp.Kijamii to Own Repository

## Description

After initial development in this repository, extract TimeWarp.Kijamii to its own dedicated repository (timewarp-kijamii). This social platform integration library should have independent development, allowing it to grow with new platform support without affecting the core Amuru functionality.

## Parent

031_Create-TimeWarp-Kijamii-Library (must be completed first)

## Requirements

- Complete initial development in this repo (Task 031)
- Create new repository with proper structure
- Migrate with clean history
- Update exe/post.cs to use NuGet package
- Seamless transition for users

## Checklist

### Prerequisites
- [ ] Task 031 completed (Kijamii library created and working)
- [ ] Initial testing completed
- [ ] Basic documentation in place

### Repository Creation
- [ ] Create github.com/TimeWarpEngineering/timewarp-kijamii repository
- [ ] Configure repository settings
- [ ] Set up branch protection
- [ ] Add repository secrets (NuGet API key)
- [ ] Configure security settings

### Code Migration
- [ ] Export TimeWarp.Kijamii from this repo
- [ ] Move Source/TimeWarp.Kijamii to new repo
- [ ] Set up proper project structure
- [ ] Add .gitignore
- [ ] Add LICENSE
- [ ] Create comprehensive README.md

### Project Structure (New Repo)
```
timewarp-kijamii/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── release.yml
├── Source/
│   └── TimeWarp.Kijamii/
│       ├── TimeWarp.Kijamii.csproj
│       ├── SocialClient.cs
│       ├── Platforms/
│       │   ├── Nostr/
│       │   ├── X/
│       │   └── ISocialPlatform.cs
│       ├── Configuration/
│       │   ├── SocialConfig.cs
│       │   └── ConfigManager.cs
│       └── Content/
│           └── MarkdownProcessor.cs
├── Tests/
│   └── TimeWarp.Kijamii.Tests/
├── Examples/
│   ├── PostToNostr.cs
│   └── PostToMultiple.cs
├── Directory.Build.props
├── Directory.Packages.props
├── README.md
├── LICENSE
└── CHANGELOG.md
```

### Dependencies Setup
- [ ] Configure Directory.Packages.props with:
  - Blockcore.Nostr.Client
  - System.Text.Json
  - Required HTTP libraries
- [ ] Set up test dependencies
- [ ] Configure build properties

### CI/CD Pipeline
- [ ] Create CI workflow for:
  - Build on multiple platforms
  - Run tests
  - Code coverage
  - Security scanning
- [ ] Create release workflow for:
  - NuGet package creation
  - Automated publishing
  - GitHub releases
- [ ] Test the pipelines

### Update This Repository
- [ ] Remove Source/TimeWarp.Kijamii
- [ ] Update exe/post.cs:
  ```csharp
  #:package TimeWarp.Kijamii@1.0.0
  ```
- [ ] Remove Blockcore.Nostr.Client from Directory.Packages.props
- [ ] Update solution file
- [ ] Update CI/CD workflows

### NuGet Package
- [ ] Publish initial version (1.0.0)
- [ ] Verify on NuGet.org
- [ ] Test installation
- [ ] Update package metadata

### Documentation
- [ ] Comprehensive README with:
  - Installation instructions
  - Configuration guide
  - Usage examples
  - Platform-specific features
- [ ] API documentation
- [ ] Migration guide from exe/post.cs
- [ ] Security best practices

### Testing
- [ ] Test exe/post.cs with NuGet package
- [ ] Test all platforms still work
- [ ] Test configuration management
- [ ] Test encryption/decryption
- [ ] Verify `timewarp install` includes post

## Notes

Kijamii should be extracted after initial development is stable because:
- It has external dependencies (Nostr client, etc.)
- Domain-specific (social platforms)
- Will grow with new platforms (Discord, Slack, etc.)
- Benefits from independent release cycle
- Different security considerations

The library should be designed for extensibility from the start, making it easy to add new social platforms without breaking changes.

## Future Platform Support

Once extracted, Kijamii can grow to support:
- Discord
- Slack
- Mastodon
- BlueSky
- Matrix
- Telegram
- Custom webhooks

Each platform should follow the ISocialPlatform interface for consistency.

## Security Considerations

- Configuration files contain sensitive data (API keys, private keys)
- Support encrypted configuration
- Clear documentation on secure key management
- Consider using system keychain integration
- Audit logging for posts

## Migration Path

1. Complete development in this repo (Task 031)
2. Test thoroughly
3. Create new repository
4. Migrate code
5. Publish to NuGet
6. Update references
7. Announce to users
8. Deprecate old code location