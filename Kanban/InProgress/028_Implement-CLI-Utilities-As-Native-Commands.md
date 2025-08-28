# 028 Implement CLI Utilities As Native Commands

## Description

Add common CLI utilities as native commands in TimeWarp.Amuru with multiple distribution methods to support different use cases across TimeWarp projects. These utilities (convert-timestamp, generate-avatar, generate-color, multiavatar, post, ssh-key-helper) are currently duplicated across private repos and need to be consolidated into a public, accessible solution.

GitHub Issue: #14

## Business Value

**ROI Score: 9/10** - These utilities are used across multiple TimeWarp projects and CI/CD pipelines. Native implementation provides:
- **Public Accessibility**: CI/CD pipelines can access without private repo dependencies
- **Single Maintenance Point**: No more duplication across repositories  
- **Multiple Consumption Patterns**: Library, CLI tool, or standalone binaries
- **Cross-Platform Support**: Consistent behavior across Windows, Linux, macOS
- **Version Management**: Proper versioning through NuGet and GitHub releases

## Requirements

- Implement utilities as native commands following existing Amuru patterns
- Create separate .NET tool package (TimeWarp.Amuru.Tool) to avoid library/tool conflicts
- Publish standalone executables for zero-dependency usage
- Support library usage via static methods
- Ensure all tools work cross-platform
- Maintain backward compatibility with existing usage patterns

## Checklist

### Core Utilities Implementation
- [ ] **ConvertTimestamp** - Convert Unix timestamps to human-readable dates
  - [ ] FromUnix(long timestamp) method
  - [ ] ToUnix(DateTime date) method
  - [ ] Support various output formats (ISO8601, RFC3339, custom)
  - [ ] Handle timezones properly
  - [ ] CLI: `--GitCommitTimestamp` parameter support

- [ ] **GenerateAvatar** - Generate avatar images from input
  - [ ] FromEmail(string email) method
  - [ ] FromSeed(string seed) method
  - [ ] Support multiple output formats (PNG, SVG)
  - [ ] Configurable sizes
  - [ ] CLI: `--email` and `--seed` parameters

- [ ] **GenerateColor** - Generate color values and schemes
  - [ ] FromSeed(string seed) method
  - [ ] GeneratePalette(int count) method
  - [ ] Support various color formats (HEX, RGB, HSL)
  - [ ] Complementary/analogous color generation
  - [ ] CLI: Color scheme output options

- [ ] **Multiavatar** - Multi-style avatar generation
  - [ ] Generate(string identifier) method
  - [ ] Support multiple avatar styles
  - [ ] SVG output with customization options
  - [ ] Deterministic generation from seed

- [ ] **Post** - Blog/blip posting tool
  - [ ] Create(string title, string content) method
  - [ ] Support markdown content
  - [ ] Template support
  - [ ] CLI: `--title` and `--content` parameters

- [ ] **SshKeyHelper** - SSH key management utility
  - [ ] GenerateKeyPair() method
  - [ ] GetPublicKey(string privateKeyPath) method
  - [ ] ValidateKey(string keyPath) method
  - [ ] CLI: `--generate`, `--validate` parameters

### Package Structure
- [ ] Create `TimeWarp.Amuru.Native.Utilities` namespace
- [ ] Implement each utility as a static class with fluent builder pattern where appropriate
- [ ] Follow existing native command patterns (Commands vs Direct APIs)

### .NET Tool Package
- [ ] Create separate `TimeWarp.Amuru.Tool` project
- [ ] Configure as .NET tool in .csproj
- [ ] Implement command-line interface using System.CommandLine or similar
- [ ] Support command routing: `timewarp <command> [options]`
- [ ] Add global tool installation support

### Standalone Binaries
- [ ] Configure PublishAot for minimal size
- [ ] Setup GitHub Actions to build platform-specific binaries
- [ ] Publish as release assets:
  - [ ] convert-timestamp-win-x64.exe
  - [ ] convert-timestamp-linux-x64
  - [ ] convert-timestamp-osx-x64
  - [ ] (repeat for each utility)
- [ ] Include SHA256 checksums

### MSBuild Integration
- [ ] Document usage in MSBuild tasks
- [ ] Ensure console output is MSBuild-friendly
- [ ] Support exit codes for build success/failure
- [ ] Example: `<Exec Command="timewarp convert-timestamp --GitCommitTimestamp $(Timestamp)" />`

### Testing
- [ ] Unit tests for each utility's core functionality
- [ ] Integration tests for CLI interface
- [ ] Cross-platform testing (Windows, Linux, macOS)
- [ ] Test standalone binary execution
- [ ] Test .NET tool installation and usage
- [ ] Performance benchmarks

### Documentation
- [ ] Update README with utilities section
- [ ] Document each utility's API
- [ ] Provide CLI usage examples
- [ ] Add MSBuild integration examples
- [ ] Create migration guide from private repo versions

## Implementation Notes

### Example Usage Patterns

```csharp
// Library usage in C# scripts
#:package TimeWarp.Amuru@1.0.0

using TimeWarp.Amuru.Native.Utilities;

// Convert timestamp
var timestamp = 1234567890;
string readableDate = ConvertTimestamp.FromUnix(timestamp);
Console.WriteLine($"Commit date: {readableDate}");

// Generate avatar
string avatarSvg = GenerateAvatar.FromEmail("user@example.com")
    .WithSize(256)
    .AsSvg();

// SSH key management
var keyPair = SshKeyHelper.GenerateKeyPair()
    .WithAlgorithm("ed25519")
    .WithComment("CI/CD Key");
```

```bash
# CLI tool usage (after dotnet tool install)
timewarp convert-timestamp --GitCommitTimestamp 1234567890
timewarp generate-avatar --email user@example.com --size 256 --output avatar.png
timewarp ssh-key-helper --generate --algorithm ed25519 --output ~/.ssh/id_ed25519
timewarp post --title "Release Notes" --content release.md --template blog

# Standalone binary usage (no .NET required)
./convert-timestamp-linux-x64 --GitCommitTimestamp 1234567890
```

```xml
<!-- MSBuild integration -->
<Target Name="GenerateBuildMetadata">
  <Exec Command="timewarp convert-timestamp --GitCommitTimestamp $(GitTimestamp)" 
        ConsoleToMSBuild="true">
    <Output TaskParameter="ConsoleOutput" PropertyName="BuildDate"/>
  </Exec>
  
  <Message Text="Build date: $(BuildDate)" Importance="high"/>
</Target>
```

### Distribution Strategy

1. **NuGet Package** (`TimeWarp.Amuru`)
   - Include utilities in main library
   - Available for C# script usage
   - Version aligned with main package

2. **.NET Tool** (`TimeWarp.Amuru.Tool`)
   - Separate package to avoid conflicts
   - Installable via `dotnet tool install`
   - Global or local tool support

3. **GitHub Releases**
   - Attach standalone binaries to each release
   - Platform-specific builds
   - Include version in filename

### Technical Considerations

- **Size Optimization**: Use PublishAot and trimming for standalone binaries
- **Dependency Management**: Minimize external dependencies for utilities
- **Error Handling**: Consistent error codes and messages across all distribution methods
- **Performance**: Native implementations should be fast enough for build scripts
- **Deterministic Output**: Same input should produce same output (important for avatars/colors)

## Source Code Location

The existing implementations for these utilities can be found at:
- **Source Path**: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-flow/Cramer-2025-08-01-cron/exe/`

To begin implementation:
1. Copy the source files from the above location to an `/exe` directory in this project (at least temporarily)
2. Review and refactor the existing code to follow TimeWarp.Amuru patterns
3. Migrate functionality into the native commands structure

Available source files:
- `convert-timestamp.cs` - Unix timestamp conversion utility
- `generate-avatar.cs` - Avatar generation from email/seed
- `generate-color.cs` - Color scheme generation
- `multiavatar.cs` - Multi-style avatar generator
- `post.cs` - Blog/blip posting tool
- `ssh-key-helper` - SSH key management utility

## Dependencies

- Task 019 (Native namespace structure) must be complete
- System.CommandLine for CLI parsing (or similar)
- Consider ImageSharp for avatar generation
- Consider SSH.NET for SSH key operations

## References

- GitHub Issue #14
- Existing private repo implementations
- .NET Tool documentation: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
- PublishAot documentation: https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot