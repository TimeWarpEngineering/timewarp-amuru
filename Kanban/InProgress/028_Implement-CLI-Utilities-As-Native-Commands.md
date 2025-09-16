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
- [x] **ConvertTimestamp** - Convert Unix timestamps to human-readable dates
  - [x] FromUnix(long timestamp) method (CLI implementation)
  - [ ] ToUnix(DateTime date) method
  - [x] Support various output formats (ISO8601, RFC3339, custom)
  - [x] Handle timezones properly
  - [ ] CLI: `--GitCommitTimestamp` parameter support

- [x] **GenerateAvatar** - Generate avatar images from input
  - [ ] FromEmail(string email) method
  - [x] FromSeed(string seed) method (via git repo name)
  - [x] Support multiple output formats (PNG, SVG)
  - [ ] Configurable sizes
  - [ ] CLI: `--email` and `--seed` parameters

- [x] **GenerateColor** - Generate color values and schemes
  - [x] FromSeed(string seed) method
  - [ ] GeneratePalette(int count) method
  - [x] Support various color formats (HEX, RGB, HSL)
  - [ ] Complementary/analogous color generation
  - [x] CLI: Color scheme output options

- [x] **Multiavatar** - Multi-style avatar generation
  - [x] Generate(string identifier) method
  - [x] Support multiple avatar styles
  - [x] SVG output with customization options
  - [x] Deterministic generation from seed

- [x] **Post** - Social platform posting tool (TimeWarp.Kijamii planned)
  - [x] Working exe/post.cs implementation
  - [x] Support markdown content
  - [x] Nostr and X (Twitter) platform support
  - [ ] Move to TimeWarp.Kijamii library (see Task 031)

- [ ] **SshKeyHelper** - SSH key management utility
  - [ ] GenerateKeyPair() method
  - [ ] GetPublicKey(string privateKeyPath) method
  - [ ] ValidateKey(string keyPath) method
  - [ ] CLI: `--generate`, `--validate` parameters

### Package Structure
- [x] Create `TimeWarp.Amuru.Native.Utilities` namespace (using TimeWarp.Multiavatar)
- [x] Implement each utility as a static class with fluent builder pattern where appropriate
- [x] Follow existing native command patterns (Commands vs Direct APIs)

### .NET Tool Package (TimeWarp.Ganda)
- [x] Create separate tool project (currently TimeWarp.Amuru.Tool, renaming to TimeWarp.Ganda - see Task 030)
- [x] Configure as .NET tool in .csproj
- [x] Implement command-line interface using TimeWarp.Nuru
- [x] Support command routing: `timewarp <command> [options]`
- [x] Add global tool installation support

### Standalone Binaries
- [ ] Configure PublishAot for minimal size
- [x] Setup GitHub Actions to build platform-specific binaries
- [x] Publish as release assets:
  - [x] convert-timestamp-win-x64.exe
  - [x] convert-timestamp-linux-x64
  - [x] convert-timestamp-osx-x64
  - [x] (repeat for each utility)
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
- [x] Update README with utilities section
- [x] Document each utility's API (in Tool README)
- [x] Provide CLI usage examples
- [ ] Add MSBuild integration examples
- [ ] Create migration guide from private repo versions

## Implementation Status

### Completed (5/6 utilities)
✅ **Multiavatar** - Fully implemented with TimeWarp.Multiavatar library and CLI
✅ **ConvertTimestamp** - Basic Unix to ISO8601 conversion implemented
✅ **GenerateColor** - Deterministic color generation with HEX/RGB/HSL output
✅ **GenerateAvatar** - Repository avatar generation using Multiavatar
✅ **Post** - Social platform posting tool (exe/post.cs working, TimeWarp.Kijamii library planned)

### Remaining (1/6 utilities)
⏳ **SshKeyHelper** - SSH key management (not started)

### Distribution Channels
✅ **NuGet Packages** - TimeWarp.Multiavatar published, TimeWarp.Kijamii planned
✅ **.NET Tool** - TimeWarp.Ganda (formerly Amuru.Tool) configured and ready
✅ **CI/CD Pipeline** - Automated builds for all platforms
✅ **Standalone Executables** - Working for multiavatar, convert-timestamp, generate-color, generate-avatar

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

1. **NuGet Packages**
   - `TimeWarp.Amuru` - Core process control library with Native utilities
   - `TimeWarp.Multiavatar` - Avatar generation (separate package)
   - `TimeWarp.Kijamii` - Social platform posting (planned, see Task 031)
   - Available for C# script usage

2. **.NET Tool** (`TimeWarp.Ganda`, formerly `TimeWarp.Amuru.Tool`)
   - Shell toolkit with `timewarp` command
   - Installable via `dotnet tool install --global TimeWarp.Ganda`
   - Provides access to all utilities
   - See Task 030 for rename

3. **GitHub Releases**
   - Attach standalone binaries to each release
   - Platform-specific builds (win-x64, linux-x64, osx-x64)
   - Install via `timewarp install` to user bin directory

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
- TimeWarp.Nuru for CLI routing (already integrated)
- TimeWarp.Multiavatar for avatar generation (complete)
- Blockcore.Nostr.Client for Nostr posting (integrated)
- Consider SSH.NET for SSH key operations

## Related Tasks

- Task 030: Rename TimeWarp.Amuru.Tool to TimeWarp.Ganda
- Task 031: Create TimeWarp.Kijamii Library (for Post utility)
- Task 032: Extract TimeWarp.Multiavatar to Own Repository
- Task 033: Extract TimeWarp.Kijamii to Own Repository

## References

- GitHub Issue #14
- Existing private repo implementations
- .NET Tool documentation: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
- PublishAot documentation: https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot