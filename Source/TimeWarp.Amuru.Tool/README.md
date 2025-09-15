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

### Avatar Generation
- `multiavatar <input>` - Generate unique SVG avatar from any text
- `multiavatar <input> --output <file>` - Generate and save to file
- `multiavatar <input> --no-env` - Generate without environment circle
- `multiavatar <input> --output-hash` - Display hash information
- `generate-avatar` - Generate avatar for current git repository

### Utilities
- `convert-timestamp <timestamp>` - Convert Unix timestamps to ISO 8601 format
- `generate-color <seed>` - Generate deterministic colors from seed text
- `install [utility]` - Install standalone executables to system PATH

### Future Commands
- `post --content <content>` - Social media posting (coming soon)
- `ssh --generate` - SSH key management (coming soon)

## Examples

```bash
# Generate avatar to stdout
timewarp multiavatar "user@example.com"

# Save avatar to file
timewarp multiavatar "John Doe" --output john.svg

# Generate without environment circle
timewarp multiavatar "test" --no-env

# Generate repository avatar
cd /my/git/repo
timewarp generate-avatar

# Convert Unix timestamp
timewarp convert-timestamp 1234567890
# Output: 2009-02-13T23:31:30+00:00

# Generate color from seed
timewarp generate-color "my-project"
# Output: Hex, RGB, and HSL values

# Install all standalone utilities
timewarp install

# Install specific utility
timewarp install multiavatar
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