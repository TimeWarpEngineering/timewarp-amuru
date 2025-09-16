# TimeWarp.Ganda

*Ganda means "shell" in Swahili*

A shell toolkit providing CLI utilities for TimeWarp projects including avatar generation, timestamp conversion, SSH key management, and social media posting.

## Installation

### Prerequisites

For enhanced security, install GitHub CLI to verify artifact attestations:
- **macOS**: `brew install gh`
- **Windows**: `winget install GitHub.cli`
- **Linux**: See [GitHub CLI installation](https://github.com/cli/cli#installation)

### Install the Tool

```bash
dotnet tool install --global TimeWarp.Ganda
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
- `install [utility]` - Download and install standalone executables from GitHub releases

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

# Install all standalone utilities (downloads from GitHub releases)
timewarp install

# Install specific utility
timewarp install multiavatar

# Note: Installation verifies GitHub attestations if 'gh' CLI is available
```

## Library Dependencies

This tool depends on:
- **TimeWarp.Amuru** - Core library with native utilities
- **TimeWarp.Multiavatar** - Multiavatar generation
- **CliWrap** - Process execution utilities

## Standalone Installation (Without .NET SDK)

For users without .NET SDK, standalone installers are available:

```bash
# Linux/macOS
curl -LO https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/installer-linux-x64
gh attestation verify installer-linux-x64 --repo TimeWarpEngineering/timewarp-amuru
chmod +x installer-linux-x64
./installer-linux-x64

# Windows
# Download installer-win-x64.exe from releases
gh attestation verify installer-win-x64.exe --repo TimeWarpEngineering/timewarp-amuru
.\installer-win-x64.exe
```

## Security

All artifacts are built by GitHub Actions and include attestations for supply chain security.
Verify attestations using `gh attestation verify` before running executables.

## Versioning

This tool is published with the same version as the TimeWarp.Amuru library for unified versioning.

## License

Unlicense - see root LICENSE file for details.