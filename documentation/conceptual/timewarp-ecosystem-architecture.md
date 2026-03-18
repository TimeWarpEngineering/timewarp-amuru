# TimeWarp Ecosystem Architecture

## Overview

The TimeWarp ecosystem consists of specialized libraries and tools designed for building powerful command-line applications and shell automation in C#. Each component has a specific responsibility and uses Swahili naming to convey its purpose.

## Core Components

### TimeWarp.Nuru - "Light" (CLI Routing Framework)
**Repository:** Separate (timewarp-nuru)
**Purpose:** Route-based CLI application framework
**Responsibility:** Illuminate the path through your CLI with elegant routing

A lightweight, route-based CLI framework that makes building console applications intuitive. Like light showing the way, Nuru guides users through your command-line interface with clear, discoverable routes.

### TimeWarp.Amuru - "Command" (Process Control Library)
**Repository:** This repository (timewarp-amuru)
**Purpose:** Fluent API for process execution and shell scripting in C#
**Responsibility:** Execute and control external processes

Transforms shell scripting into type-safe C# with a fluent API. Amuru provides elegant command execution, process control, and shell scripting capabilities for .NET applications.

### TimeWarp.Ganda - "Shell" (Command Toolkit)
**Repository:** This repository (Source/TimeWarp.Ganda, formerly TimeWarp.Amuru.Tool)
**Purpose:** Curated collection of command-line utilities
**Responsibility:** The actual shell - executable commands for daily use

Your personal shell toolkit. Ganda provides the `timewarp` command and can install standalone executables to your system. It's the bridge between libraries and practical command-line tools.

### TimeWarp.Kijamii - "Social" (Social Platform Integration)
**Repository:** Currently here, planned extraction to timewarp-kijamii
**Purpose:** Post content to social platforms (Nostr, X, etc.)
**Responsibility:** Social media and communication platform integration

Broadcast your content across social platforms. Kijamii handles authentication, formatting, and posting to various social networks and communication channels.

### TimeWarp.Multiavatar (Avatar Generation)
**Repository:** Currently here, planned extraction to timewarp-multiavatar
**Purpose:** Generate unique, deterministic avatars
**Responsibility:** Visual identity generation

Creates beautiful, unique avatars from any text input. Based on the Multiavatar project with 12 billion possible combinations.

## Architecture Principles

### 1. Single Responsibility
Each library has one clear purpose:
- Nuru: CLI routing
- Amuru: Process control
- Ganda: Shell commands
- Kijamii: Social posting
- Multiavatar: Avatar generation

### 2. Dependency Direction
```
exe/ commands (in Ganda)
    ↓ references
Libraries (Multiavatar, Kijamii, etc.)
    ↓ may use
Amuru (for process control)
    ↓ may use
Nuru (for CLI routing)
```

### 3. Distribution Strategy

#### Libraries (NuGet Packages)
- Published individually to NuGet
- Can be referenced from any .NET project
- Versioned independently

#### Executables (exe/ folder)
- Thin wrappers around libraries
- Built as self-contained executables
- Installed via `timewarp install` to user's bin directory
- Act as custom shell commands

## Repository Structure

### Current State (timewarp-amuru repository)
```
Source/
  TimeWarp.Amuru/           # Process control library
    Native/
      Utilities/            # Built-in utilities (no external deps)
  TimeWarp.Ganda/           # Shell toolkit (formerly Tool)
  TimeWarp.Multiavatar/     # Avatar library (to be extracted)
  TimeWarp.Kijamii/         # Social library (to be created, then extracted)

exe/                        # Executable wrappers
  multiavatar.cs            # References TimeWarp.Multiavatar
  post.cs                   # Will reference TimeWarp.Kijamii
  convert-timestamp.cs      # Uses Amuru Native utilities
  generate-color.cs         # Uses Amuru Native utilities
  generate-avatar.cs        # References TimeWarp.Multiavatar
  installer.cs              # Installation utility
```

### Future State
```
timewarp-amuru/             # This repo - Process control + Shell
  Source/
    TimeWarp.Amuru/
    TimeWarp.Ganda/
  exe/                      # All shell commands

timewarp-multiavatar/       # Separate repo
  Source/
    TimeWarp.Multiavatar/

timewarp-kijamii/           # Separate repo
  Source/
    TimeWarp.Kijamii/

timewarp-nuru/              # Already separate
  Source/
    TimeWarp.Nuru/
```

## Decision Rationale

### Why Swahili Names?
- Meaningful names that describe function
- Consistent theme across ecosystem
- Unique and memorable
- Avoids generic terms like "Tool" or "Utilities"

### Why Extract Libraries?
- **Single Responsibility:** Each repo has one purpose
- **Independent Versioning:** Libraries can evolve at their own pace
- **Cleaner Dependencies:** Users only get what they need
- **Focused Issues/PRs:** Repository activity stays relevant

### Why Keep exe/ Together?
- **Unified Installation:** One `timewarp install` for all commands
- **Curated Toolkit:** Personal selection of useful commands
- **Consistent Experience:** All commands work the same way
- **Dogfooding:** Testing the libraries through real usage

### What Belongs in Amuru Native?
Utilities that are:
1. Core to shell/process operations
2. Have no external dependencies
3. Small and focused
4. Generally useful across many scenarios

Examples: ConvertTimestamp, GenerateColor, File operations

### What Gets Its Own Library?
Features that:
1. Have external dependencies (Nostr client, ImageSharp, etc.)
2. Are domain-specific (social media, avatars)
3. Could be useful standalone
4. Might need independent versioning

Examples: Multiavatar, Kijamii (Social)

## Usage Patterns

### As a Developer (Using Libraries)
```csharp
// In your .csproj or C# script
#:package TimeWarp.Amuru
#:package TimeWarp.Kijamii

using TimeWarp.Amuru;
using TimeWarp.Kijamii;

// Use process control
await Shell.Builder("git", "status").RunAsync();

// Use social posting
await SocialClient.PostToNostr(content);
```

### As a User (Using Shell Commands)
```bash
# Install the shell toolkit
dotnet tool install -g TimeWarp.Ganda

# Install exe commands to system
timewarp install

# Use the commands
multiavatar "user@example.com" --output avatar.svg
post my-blog.md --platform nostr
convert-timestamp 1234567890
```

### Hybrid Usage (From Amuru Scripts)
```csharp
// Call your own shell commands from C# scripts
await Shell.Builder("multiavatar", "test@example.com")
    .WithArguments("--output", "avatar.svg")
    .RunAsync();
```

## Migration Path

### Phase 1: Reorganize (In Progress)
1. ✅ Fix exe/post.cs to compile
2. ⏳ Rename TimeWarp.Amuru.Tool → TimeWarp.Ganda
3. ⏳ Create TimeWarp.Kijamii library from post.cs
4. ⏳ Update exe/post.cs to reference library

### Phase 2: Extract Libraries
1. Create timewarp-multiavatar repository
2. Move TimeWarp.Multiavatar project
3. Publish to NuGet
4. Update exe references to NuGet package

5. Create timewarp-kijamii repository
6. Move TimeWarp.Kijamii project
7. Publish to NuGet
8. Update exe references to NuGet package

### Phase 3: Documentation
1. Update all READMEs
2. Create migration guides
3. Update CI/CD pipelines
4. Announce changes

## Benefits of This Architecture

### For Users
- **Clean Installation:** Only get what you need
- **Clear Purpose:** Each tool does one thing well
- **Consistent Experience:** All commands work similarly
- **Growing Ecosystem:** New tools can be added easily

### For Developers
- **Type Safety:** Full IntelliSense and compile-time checking
- **Modularity:** Use only the libraries you need
- **Flexibility:** Use as library or shell command
- **Testability:** Each component can be tested independently

### For Maintainers
- **Focused Repositories:** Each repo has clear scope
- **Independent Releases:** Version and release separately
- **Clean Dependencies:** No unnecessary coupling
- **Easier Contributions:** Contributors know where to make changes

## Summary

The TimeWarp ecosystem provides a comprehensive toolkit for command-line development in C#:
- **Write** shell scripts in type-safe C# (Amuru)
- **Build** elegant CLI apps with routing (Nuru)
- **Use** powerful commands from your shell (Ganda)
- **Extend** with specialized libraries (Multiavatar, Kijamii)

Each component has a clear responsibility, meaningful name, and specific purpose. Together, they form a powerful ecosystem for modern command-line development.