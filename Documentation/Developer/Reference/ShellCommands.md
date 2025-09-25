# Shell Commands Reference

Complete reference for all TimeWarp.Ganda shell commands and standalone executables.

## Command Categories

- [Avatar Generation](#avatar-generation)
- [Utilities](#utilities)
- [Installation Management](#installation-management)

## Avatar Generation

### multiavatar

Generates unique, deterministic SVG avatars from any text input using SHA256 hashing.

**Syntax:**
```bash
multiavatar <input> [options]
```

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `input` | Yes | Text to generate avatar from (email, username, etc.) |

**Options:**
| Option | Alias | Description |
|--------|-------|-------------|
| `--output <file>` | `-o` | Save SVG to file instead of stdout |
| `--no-env` | | Generate without environment circle |
| `--output-hash` | | Display hash calculation details instead of SVG |

**Examples:**
```bash
# Output SVG to console
multiavatar "user@example.com"

# Save to file
multiavatar "john.doe" -o avatar.svg

# Generate without background
multiavatar "alice" --no-env

# Show hash details
multiavatar "test" --output-hash
```

**Output Hash Format:**
```
<input>:
  SHA256: <64-char hex string>
  Numbers: <numeric representation>
  Hash-12: <12-char segment>
  Parts:
    env: <2 chars> -> <selection>
    clo: <2 chars> -> <selection>
    head: <2 chars> -> <selection>
    mouth: <2 chars> -> <selection>
    eyes: <2 chars> -> <selection>
    top: <2 chars> -> <selection>
```

**Exit Codes:**
- `0`: Success
- `1`: Invalid arguments
- `2`: File write error

### generate-avatar

Generates an SVG avatar for the current git repository.

**Syntax:**
```bash
generate-avatar
```

**Behavior:**
1. Detects git repository root
2. Extracts repository name from git remote origin
3. Creates `assets/` directory if needed
4. Generates `assets/{repo-name}-avatar.svg`

**Requirements:**
- Must be run within a git repository
- Requires write permissions to create/modify assets directory

**Exit Codes:**
- `0`: Success
- `1`: Not in a git repository
- `2`: File write error

## Utilities

### convert-timestamp

Converts Unix timestamps to ISO 8601 format and vice versa.

**Syntax:**
```bash
convert-timestamp <value>
```

**Parameters:**
| Parameter | Description |
|-----------|-------------|
| `value` | Unix timestamp (seconds) or ISO date string |

**Examples:**
```bash
# Unix to ISO
convert-timestamp 1234567890
# Output: 2009-02-13T23:31:30+00:00

# ISO to Unix
convert-timestamp "2024-01-01T00:00:00Z"
# Output: 1704067200

# Current time
convert-timestamp now
# Output: 2024-11-15T10:30:45+00:00
```

### generate-color

Generates deterministic colors from seed text.

**Syntax:**
```bash
generate-color <seed> [options]
```

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `seed` | Yes | Text seed for color generation |

**Options:**
| Option | Description |
|--------|-------------|
| `--format <type>` | Output format: hex, rgb, hsl (default: all) |
| `--count <n>` | Number of colors to generate (default: 1) |

**Examples:**
```bash
# Generate single color
generate-color "my-project"
# Output:
# Hex: #3A7F9B
# RGB: rgb(58, 127, 155)
# HSL: hsl(197, 45%, 42%)

# Generate multiple colors
generate-color "theme" --count 5
```

### post

Creates timestamped posts with automatic directory organization.

**Syntax:**
```bash
post <content> [options]
```

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `content` | Yes | Post content or title |

**Options:**
| Option | Alias | Description |
|--------|-------|-------------|
| `--dir <path>` | `-d` | Base directory for posts (default: ./posts) |
| `--ext <extension>` | `-e` | File extension (default: md) |
| `--template <file>` | `-t` | Template file to use |

**Directory Structure:**
```
posts/
└── 2024/
    └── 11/
        └── 15/
            └── 2024-11-15-1030-my-post.md
```

## Installation Management

### timewarp install

Downloads and installs standalone executables from GitHub releases.

**Syntax:**
```bash
timewarp install [utility]
```

**Parameters:**
| Parameter | Description |
|-----------|-------------|
| `utility` | Specific utility to install (optional) |

**Without parameters:**
Installs all available utilities:
- multiavatar
- generate-avatar
- convert-timestamp
- generate-color
- post

**Security:**
- Verifies GitHub attestations if `gh` CLI is available
- Downloads from official TimeWarpEngineering/timewarp-amuru releases
- Installs to `~/.local/bin` (Linux/macOS) or `%USERPROFILE%\.local\bin` (Windows)

**Examples:**
```bash
# Install all utilities
timewarp install

# Install specific utility
timewarp install multiavatar

# Force reinstall
timewarp install --force
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `TIMEWARP_BIN_DIR` | Installation directory for commands | `~/.local/bin` |
| `TIMEWARP_NO_VERIFY` | Skip attestation verification | `false` |
| `TIMEWARP_GITHUB_TOKEN` | GitHub token for API rate limits | None |

## File Formats

### SVG Avatar Output

The multiavatar command generates standard SVG 1.1 with:
- Viewbox: 0 0 231 231
- No external dependencies
- Inline styles
- Compatible with all modern browsers

### Hash Calculation

Avatar generation uses:
- SHA256 hashing of input text
- First 48 characters for part selection
- Deterministic color generation
- 48 billion unique combinations

## Error Handling

All commands follow consistent error handling:

1. **Invalid Arguments**: Exit code 1 with usage message
2. **File System Errors**: Exit code 2 with error details
3. **Network Errors**: Exit code 3 with connection details
4. **Permission Errors**: Exit code 4 with permission requirements

## See Also

- [TimeWarp.Ganda README](../../../Source/TimeWarp.Ganda/README.md) - Tool overview
- [How to Use Shell Commands](../HowToGuides/HowToUseShellCommands.md) - Usage guide
- [Architecture Documentation](../../../Architecture/TimeWarp-Ecosystem-Architecture.md) - System design