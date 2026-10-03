# 031 Create TimeWarp.Kijamii Social Library

## Description

Extract the social media posting functionality from exe/post.cs into a proper library called TimeWarp.Kijamii. "Kijamii" means "social" in Swahili, reflecting the library's purpose of integrating with social platforms like Nostr and X (Twitter), with potential for Discord, Slack, and other platforms in the future.

## Requirements

- Move core posting logic from exe/post.cs to library
- Support multiple social platforms with clean abstractions
- Handle configuration and secrets securely
- Make exe/post.cs a thin wrapper around the library
- Maintain all current functionality

## Checklist

### Library Structure
- [ ] Create Source/TimeWarp.Kijamii directory
- [ ] Create TimeWarp.Kijamii.csproj
- [ ] Add to solution file
- [ ] Configure NuGet package metadata

### Core Implementation
- [ ] Create SocialClient.cs as main entry point
- [ ] Create ISocialPlatform interface
- [ ] Create Platforms/ directory structure:
  - [ ] Platforms/Nostr/NostrClient.cs
  - [ ] Platforms/X/XClient.cs
  - [ ] Platforms/PlatformBase.cs (shared functionality)

### Configuration Management
- [ ] Create Configuration/SocialConfig.cs
- [ ] Create Configuration/ConfigManager.cs
- [ ] Create Configuration/SecretManager.cs
- [ ] Support both plaintext and encrypted configs
- [ ] Implement SSH key encryption/decryption

### Content Processing
- [ ] Create Content/MarkdownProcessor.cs
- [ ] Create Content/ContentFormatter.cs
- [ ] Support different content types (blips, blogs)
- [ ] Handle content length limits per platform

### Platform Implementations
- [ ] Port Nostr posting logic from post.cs
- [ ] Port X/Twitter posting logic from post.cs
- [ ] Implement proper error handling
- [ ] Add retry logic for failed posts
- [ ] Support platform-specific features

### Update exe/post.cs
- [ ] Remove implementation code
- [ ] Add #:project reference to TimeWarp.Kijamii
- [ ] Keep CLI routing logic only
- [ ] Delegate all work to library

### Testing
- [ ] Unit tests for each platform
- [ ] Integration tests with mock servers
- [ ] Configuration tests
- [ ] Encryption/decryption tests

### Documentation
- [ ] Create README.md for the library
- [ ] Document configuration format
- [ ] Provide usage examples
- [ ] Document platform-specific features
- [ ] Add XML documentation comments

## Notes

This library will handle all social platform integrations, allowing users to post content from any .NET application or script. The design should be extensible to easily add new platforms.

Key design principles:
- Platform abstraction through interfaces
- Secure configuration management
- Async throughout
- Proper error handling and logging
- Extensible for future platforms

## Implementation Example

```csharp
// Library usage
using TimeWarp.Kijamii;

var client = new SocialClient();
await client.LoadConfigAsync("~/.timewarp/social.json");

// Post to all configured platforms
await client.PostAsync("My content", platforms: SocialPlatforms.All);

// Post to specific platform
await client.PostToNostrAsync("Nostr-specific content");
await client.PostToXAsync("X-specific content");

// Future: Post to Discord, Slack, etc.
await client.PostToDiscordAsync(channelId, "Discord message");
```

## Dependencies

- Blockcore.Nostr.Client (for Nostr)
- HTTP client libraries for X API
- System.Text.Json for configuration
- System.Security.Cryptography for encryption

## Future Enhancements

- Discord integration
- Slack integration
- Mastodon support
- BlueSky support
- Webhook support for custom integrations
- Scheduling posts
- Multi-account support per platform