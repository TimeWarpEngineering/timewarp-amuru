# TimeWarp.Amuru.Tool

A collection of CLI utilities for TimeWarp projects including avatar generation, timestamp conversion, SSH key management, and social media posting.

## Installation

```bash
dotnet tool install --global TimeWarp.Amuru.Tool
```

## Usage

```bash
timewarp --help
```

## Commands

- `convert-timestamp <timestamp>` - Convert Unix timestamps to human-readable dates
- `generate-avatar --email <email>` - Generate avatar from email address
- `generate-color <seed>` - Generate color values from seeds
- `multiavatar <identifier>` - Generate multi-style avatar
- `post --content <content>` - Create and share content
- `ssh --generate` - SSH key management operations

## Examples

```bash
# Convert timestamp
timewarp convert-timestamp 1234567890

# Generate color
timewarp generate-color "my project"

# Generate avatar
timewarp generate-avatar --email user@example.com

# Generate SSH key
timewarp ssh --generate
```

## Library Dependencies

This tool depends on:
- **TimeWarp.Amuru** - Core library with native utilities
- **TimeWarp.Multiavatar** - Multiavatar generation
- **CliWrap** - Process execution utilities

## Versioning

This tool is published with the same version as the TimeWarp.Amuru library for unified versioning.

## License

Unlicense - see root LICENSE file for details.