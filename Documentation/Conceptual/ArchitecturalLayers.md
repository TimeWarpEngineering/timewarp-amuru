# Architectural Layers

## Overview

TimeWarp.Amuru provides a multi-layered architecture that serves different use cases and user preferences. Understanding these layers helps you choose the right abstraction for your needs—from low-level streaming APIs to high-level CLI tools.

This document explains the six distinct architectural layers and how they integrate to provide a cohesive developer experience.

## The Six Layers

```
┌─────────────────────────────────────────────────────────────┐
│  6. Standalone Executables (/exe scripts + AOT binaries)   │
│     - .NET 10 file-based apps with shebangs                 │
│     - AOT-compiled binaries via 'timewarp install'          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│  5. Ganda CLI Tool (timewarp)                               │
│     - Single multi-command dotnet global tool               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│  4. Bash Aliases Layer                                      │
│     - Familiar shell command names (Cat, Ls, Pwd)           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│  3. Strongly-Typed Fluent Builders                          │
│     - DotNet.Build(), Git.FindRoot(), Fzf.Builder()         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│  2. Commands API (Shell-Like)                               │
│     - Returns CommandOutput with exit codes                 │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│  1. Direct API (Low-Level)                                  │
│     - Returns native C# types, throws exceptions            │
└─────────────────────────────────────────────────────────────┘
```

---

## Two Distinct Implementation Patterns

**IMPORTANT**: Not all builders follow the same pattern. Understanding this distinction is crucial:

### Pattern A: Native In-Process Operations

**Starting Point**: Layer 1 (Direct API)

**Examples**: FileSystem operations (`Cat`, `Ls`, `Pwd`), `Git.FindRoot()`

**Flow**:
```
Layer 1 (Direct) → Layer 2 (Commands) → Layer 4 (Aliases) → Layer 5/6
```

**Characteristics**:
- Pure C# implementations with NO external executables
- Layer 1 is **REQUIRED** - contains the actual logic
- Layer 2 wraps Layer 1 with CommandOutput
- Layer 3 (Builder) is optional and rarely needed for simple operations

**Current Implementations**:
- ✅ FileSystem: `Direct.Cat()`, `Commands.Cat()`, Bash aliases
- ✅ Git (partial): `Git.FindRoot()` is pure C# (Layer 1/2)

### Pattern B: External Command Wrappers

**Starting Point**: Layer 3 (Builder)

**Examples**: `DotNet.Build()`, `Fzf.Builder()`, `Ghq.Builder()`, `Gwq.Builder()`

**Flow**:
```
Layer 3 (Builder) → Layer 4 (Aliases) → Layer 5/6
```

**Characteristics**:
- Wraps existing external commands (`dotnet`, `fzf`)
- Layer 1 and Layer 2 are **NOT PRESENT** - not needed
- Builder constructs arguments and calls `Shell.Builder()` to execute external command
- Provides fluent, type-safe API over existing tools

**Current Implementations**:
- ✅ DotNet: `DotNet.Build()`, `DotNet.Test()`, etc. - wraps `dotnet` CLI
- ✅ Fzf: `Fzf.Builder()` - wraps `fzf` interactive filter

### Pattern C: Hybrid Approach (Progressive Enhancement)

**Starting Point**: Pattern B, then incrementally add Pattern A

**Example**: Git (future vision)

**Strategy**:
1. Start with Layer 3 builder wrapping `git` command
2. Identify hot paths or problematic operations
3. Implement Layer 1/2 (Native) for those specific operations
4. Builder uses native where available, external where not

**Benefits**:
- Start quickly (wrap existing tool)
- Add native implementations incrementally
- Maintain 100% compatibility via passthrough
- Best of both worlds: performance + compatibility

**Visual Example**:
```
Git.Builder()
  .FindRoot()           → Native C# (Layer 1) ⚡ Fast
  .Status()             → git status (external) 🔄 Compatible
  .Commit("message")    → git commit (external) 🔄 Compatible
```

---

## Layer 1: Direct API (Low-Level)

### Purpose
The **Direct API** provides low-level, C#-idiomatic access to operations. It returns native C# types and throws exceptions on errors, making it perfect for LINQ composition and streaming scenarios.

### Location
```
Source/TimeWarp.Amuru/Native/{Domain}/Direct/
```

### File Pattern
```
Direct.{Operation}.cs
```

### Characteristics
- Returns native C# types (`string`, `IAsyncEnumerable<T>`, `void`)
- Throws exceptions on errors (`FileNotFoundException`, `IOException`, `InvalidOperationException`)
- LINQ-friendly and streaming-capable
- PowerShell-like naming conventions (GetContent, GetChildItem, SetLocation)
- Minimal overhead—direct access to underlying operations

### Example Code

**Source/TimeWarp.Amuru/Native/FileSystem/Direct/Direct.GetContent.cs**
```csharp
namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Reads file content as an async stream of lines.
  /// </summary>
  public static async IAsyncEnumerable<string> GetContent(string path)
  {
    using StreamReader reader = File.OpenText(path);
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
      yield return line;
    }
  }

  public static IAsyncEnumerable<string> Cat(string path) => GetContent(path);
}
```

### Usage

```csharp
using TimeWarp.Amuru.Native.FileSystem;

// Streaming - memory efficient for large files
await foreach (var line in Direct.Cat("huge.log"))
{
  if (line.Contains("ERROR"))
  {
    Console.WriteLine(line);
  }
}

// LINQ composition
var errorCount = await Direct.Cat("app.log")
  .Where(line => line.Contains("ERROR"))
  .CountAsync();

// Exception handling
try
{
  string gitRoot = Direct.FindRoot();
  Console.WriteLine($"Git root: {gitRoot}");
}
catch (InvalidOperationException)
{
  Console.WriteLine("Not in a git repository");
}
```

### When to Use
- Streaming large files without buffering
- LINQ operations on file content
- C#-idiomatic exception handling
- Performance-critical scenarios
- When you need strongly-typed results

---

## Layer 2: Commands API (Shell-Like)

### Purpose
The **Commands API** provides shell-like semantics with `CommandOutput` return types. It uses exit codes instead of exceptions, making it composable with external commands and perfect for shell-style scripting.

### Location
```
Source/TimeWarp.Amuru/Native/{Domain}/Commands/
```

### File Pattern
```
Commands.{Operation}.cs
```

### Characteristics
- Returns `CommandOutput` with `ExitCode`, `Stdout`, `Stderr`, `Combined`
- Shell semantics (exit codes, no exceptions by default)
- Composable/pipeable with external commands
- Same operation names as Direct but different return types
- Works consistently with `Shell.Builder()` commands

### Example Code

**Source/TimeWarp.Amuru/Native/FileSystem/Commands/Commands.GetChildItem.cs**
```csharp
namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Lists directory contents and returns them as CommandOutput.
  /// </summary>
  public static CommandOutput GetChildItem(string path = ".")
  {
    try
    {
      var entries = new List<string>();
      // ... collect directory entries ...

      return new CommandOutput(
        string.Join("\n", entries),
        string.Empty,
        0
      );
    }
    catch (DirectoryNotFoundException)
    {
      return new CommandOutput(
        string.Empty,
        $"GetChildItem: {path}: No such file or directory",
        1
      );
    }
  }

  public static CommandOutput Ls(string path = ".") => GetChildItem(path);
}
```

### Usage

```csharp
using TimeWarp.Amuru.Native.FileSystem;

// Check exit code
var result = Commands.Ls("/nonexistent");
if (!result.Success)
{
  Console.WriteLine($"Error: {result.Stderr}");
}

// Compose with external commands (all return CommandOutput)
var nativeResult = Commands.Cat("file.txt");
var externalResult = await Shell.Builder("git", "status").CaptureAsync();

// Both results have consistent interface
Console.WriteLine($"Native exit code: {nativeResult.ExitCode}");
Console.WriteLine($"External exit code: {externalResult.ExitCode}");
```

### When to Use
- Shell-style scripting with exit codes
- Composing native and external commands
- Capturing stdout/stderr separately
- Pipeline operations
- When you want consistent `CommandOutput` interface

---

## Layer 3: Strongly-Typed Fluent Builders

### Purpose
**Strongly-Typed Builders** provide domain-specific, IntelliSense-friendly configuration for complex tools. They combine type safety with fluent method chaining for excellent developer experience.

### Location
```
Source/TimeWarp.Amuru/{Tool}Commands/
```

### File Pattern
```
{Tool}.{Operation}.cs
```

### Examples
- `DotNetCommands/DotNet.Build.cs`
- `GitCommands/Git.FindRoot.cs`
- `FzfCommands/Fzf.cs`

### Characteristics
- Type-safe configuration with IntelliSense support
- Fluent API with method chaining
- Domain-specific operations (DotNet, Git, Fzf, Ghq, Gwq)
- Conditional configuration extensions (`When`, `WhenNotNull`, `Unless`)
- Functional programming patterns (`Apply`, `ForEach`, `Tap`)
- Implements `ICommandBuilder<T>` interface

### Example Code

**Source/TimeWarp.Amuru/DotNetCommands/DotNet.Build.cs**
```csharp
namespace TimeWarp.Amuru;

public static partial class DotNet
{
  public static DotNetBuildBuilder Build() => new DotNetBuildBuilder();
}

public class DotNetBuildBuilder : ICommandBuilder<DotNetBuildBuilder>
{
  private string? Configuration;
  private string? Framework;
  private bool NoRestore;

  public DotNetBuildBuilder WithConfiguration(string configuration)
  {
    Configuration = configuration;
    return this;
  }

  public DotNetBuildBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  public DotNetBuildBuilder WithNoRestore()
  {
    NoRestore = true;
    return this;
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    // Builds command and executes
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    // Builds command and captures output
  }
}
```

### Usage

```csharp
using TimeWarp.Amuru;

// Basic usage
await DotNet.Build()
  .WithConfiguration("Release")
  .WithNoRestore()
  .RunAsync();

// Conditional configuration (fluent style)
await DotNet.Build()
  .When(isRelease, b => b.WithConfiguration("Release"))
  .When(skipRestore, b => b.WithNoRestore())
  .WhenNotNull(framework, (b, f) => b.WithFramework(f))
  .RunAsync();

// Git operations
string? gitRoot = Git.FindRoot();
if (gitRoot != null)
{
  string? repoName = await Git.GetRepositoryNameAsync(gitRoot);
  Console.WriteLine($"Repository: {repoName}");
}

// Interactive selection
var selectedFile = await Fzf.Builder()
  .FromInput("file1.txt", "file2.txt", "file3.txt")
  .WithPreview("cat {}")
  .SelectAsync();
```

### Conditional Configuration Extensions

All builders support these extension methods from `ICommandBuilder<T>`:

```csharp
// Apply configuration when condition is true
.When(condition, b => b.ConfigureMethod())

// Apply when value is not null, passing the value
.WhenNotNull(value, (b, v) => b.WithValue(v))

// Apply when condition is false
.Unless(condition, b => b.ConfigureMethod())

// Extract reusable configuration
.Apply(ConfigureForProduction)

// Apply for each item in collection
.ForEach(items, (b, item) => b.AddItem(item))

// Side effects without modifying builder (logging, debugging)
.Tap(b => Console.WriteLine($"Building with: {b}"))
```

### When to Use
- Complex tool configuration (dotnet, git, fzf)
- Type-safe, IntelliSense-driven development
- Conditional configuration logic
- Reusable configuration patterns
- When readability and maintainability matter

---

## Layer 4: Bash Aliases Layer

### Purpose
The **Bash Aliases Layer** provides familiar command names for shell users. It offers a unified interface that can use either Commands or Direct APIs based on how you import it.

### Location
```
Source/TimeWarp.Amuru/Native/Aliases/Bash.cs
```

### Characteristics
- Bash-style naming (Cat, Ls, Pwd, Cd, Rm)
- Provides both Commands and Direct overloads
- Overloads can coexist due to different return types
- Enables zero-verbosity usage via `global using static`
- Familiar for bash/Unix users

### Example Code

**Source/TimeWarp.Amuru/Native/Aliases/Bash.cs**
```csharp
namespace TimeWarp.Amuru.Native.Aliases;

public static class Bash
{
  // Commands version - returns CommandOutput
  public static CommandOutput Cat(string path) =>
    FileSystem.Commands.GetContent(path);

  public static CommandOutput Ls(string path = ".") =>
    FileSystem.Commands.GetChildItem(path);

  public static CommandOutput Pwd() =>
    FileSystem.Commands.GetLocation();

  // Direct version - returns IAsyncEnumerable
  public static IAsyncEnumerable<string> CatDirect(string path) =>
    FileSystem.Direct.GetContent(path);

  public static IAsyncEnumerable<FileSystemInfo> LsDirect(string path = ".") =>
    FileSystem.Direct.GetChildItem(path);

  public static string PwdDirect() =>
    FileSystem.Direct.GetLocation();
}
```

### Usage

```csharp
// Option 1: Commands version (returns CommandOutput)
global using static TimeWarp.Amuru.Native.Aliases.Bash;

var result = Cat("file.txt");
if (result.Success)
{
  Console.WriteLine(result.Stdout);
}

// Option 2: Direct version (returns IAsyncEnumerable)
await foreach (var line in CatDirect("file.txt"))
{
  Console.WriteLine(line);
}

// Mix and match
string currentDir = PwdDirect();  // Direct - returns string
var lsResult = Ls(".");           // Commands - returns CommandOutput
```

### When to Use
- Quick scripts with minimal ceremony
- When you want bash-like command names
- Zero-verbosity with `global using static`
- Prototyping and experimentation
- Scripts for bash/Unix users transitioning to C#

---

## Layer 5: Ganda CLI Tool

### Purpose
**Ganda** (Swahili for "shell") is a single, multi-command CLI tool distributed as a dotnet global tool. It provides pre-built utilities for common tasks like avatar generation, timestamp conversion, and more.

### Location
```
Source/TimeWarp.Ganda/
```

### Binary Name
```
timewarp
```

### Characteristics
- Single executable with multiple commands
- Built on TimeWarp.Nuru for command routing
- Uses TimeWarp.Amuru APIs internally
- Distributed via NuGet as dotnet global tool
- Requires .NET SDK to be installed

### Available Commands

```bash
timewarp multiavatar <input>           # Generate SVG avatar
timewarp generate-avatar               # Generate repo avatar
timewarp convert-timestamp <ts>        # Convert Unix timestamp
timewarp generate-color <seed>         # Generate deterministic color
timewarp ssh-key-helper                # SSH key management
timewarp install [utility]             # Install AOT-compiled binaries
```

### Example Code

**Source/TimeWarp.Ganda/Program.cs**
```csharp
namespace TimeWarp.Ganda;

internal static class Program
{
  public static async Task<int> Main(string[] args)
  {
    NuruAppBuilder builder = new();
    builder.AddAutoHelp();

    builder.AddRoute(
      "multiavatar {input|Text to generate avatar from}",
      MultiavatarCommand,
      "Generate unique, deterministic SVG avatars"
    );

    builder.AddRoute(
      "generate-avatar",
      GenerateAvatarCommand,
      "Generate avatar for current git repository"
    );

    // Uses TimeWarp.Amuru internally
    private static async Task<int> GenerateAvatarCommand()
    {
      string? gitRoot = Git.FindRoot();  // Strongly-typed builder
      if (gitRoot == null)
      {
        WriteLine("Error: Not in a git repository");
        return 1;
      }

      string? repoName = await Git.GetRepositoryNameAsync(gitRoot);
      // ... generate avatar ...
    }

    NuruApp app = builder.Build();
    return await app.RunAsync(args);
  }
}
```

### Installation & Usage

```bash
# Install globally
dotnet tool install --global TimeWarp.Ganda

# Use commands
timewarp multiavatar "user@example.com"
timewarp multiavatar "John Doe" --output john.svg
timewarp convert-timestamp 1234567890
timewarp generate-avatar
timewarp install multiavatar  # Install AOT binary
```

### When to Use
- Pre-built utilities you want to use system-wide
- When .NET SDK is already installed
- Quick access to common tools (avatar, timestamp, color)
- Repository avatar generation
- SSH key management

---

## Layer 6: Standalone Executables

### Purpose
**Standalone Executables** come in two forms: .NET 10 file-based apps for development and AOT-compiled binaries for production. They provide the highest level of flexibility and deployment options.

### 6A: /exe Directory Scripts

#### Location
```
exe/*.cs
```

#### Characteristics
- .NET 10 file-based apps with shebang (`#!/usr/bin/dotnet --`)
- Single .cs files with `#:package` directives
- Can be run directly: `./exe/script.cs`
- Used for testing, prototyping, repository-specific utilities
- Requires .NET SDK

#### Example Code

**exe/display-avatar.cs**
```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru@1.0.0-beta.5
#:package TimeWarp.Nuru@2.1.0-beta.8

using TimeWarp.Amuru;
using TimeWarp.Nuru;

NuruApp app = new NuruAppBuilder()
  .AddDefaultRoute(DisplayAvatar, "Display repository avatar using chafa")
  .Build();

return await app.RunAsync(args);

static async Task<int> DisplayAvatar()
{
  string? gitRoot = Git.FindRoot();  // Uses native Git commands
  if (gitRoot == null)
  {
    WriteLine("❌ Not in a git repository");
    return 1;
  }

  string? repoName = await Git.GetRepositoryNameAsync(gitRoot);
  string avatarPath = Path.Combine(gitRoot, "assets", $"{repoName}-avatar.svg");

  if (!File.Exists(avatarPath))
  {
    WriteLine($"❌ Avatar not found: {avatarPath}");
    return 1;
  }

  int exitCode = await Shell.Builder("chafa")
    .WithArguments("--size", "50", avatarPath)
    .RunAsync();

  return exitCode;
}
```

#### Usage
```bash
# Make executable (first time)
chmod +x exe/display-avatar.cs

# Run directly
./exe/display-avatar.cs

# Or use dotnet
dotnet run exe/display-avatar.cs
```

### 6B: AOT-Compiled Binaries

#### Distribution
AOT-compiled binaries are distributed via GitHub Releases and installed using the `timewarp install` command.

#### Characteristics
- Native executables (no .NET SDK required)
- Published with GitHub Actions
- Verified with GitHub attestations
- Installs to `~/.local/bin` (Linux/macOS) or `~\.tools` (Windows)
- Supply chain security via attestation verification

#### Installation Process

**Source/TimeWarp.Amuru/Installer.cs**
```csharp
public static class Installer
{
  public static async Task<int> InstallUtilitiesAsync(string[]? specificUtilities = null)
  {
    // 1. Determine platform (win-x64, linux-x64, osx-x64)
    string platform = GetPlatform();

    // 2. Download from GitHub releases
    string archiveUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases/download/v{version}/timewarp-utilities-{platform}.tar.gz";
    await DownloadFileAsync(archiveUrl, archivePath);

    // 3. Verify attestation with gh CLI (if available)
    if (hasGhCli)
    {
      bool verified = await VerifyAttestationAsync(archivePath);
      // Prompt if verification fails
    }

    // 4. Extract and install
    await ExtractArchiveAsync(archivePath, installDir);

    // 5. Make executable (Unix)
    await Shell.Builder("chmod").WithArguments("+x", utilityPath).RunAsync();
  }
}
```

#### Usage

```bash
# Install via Ganda tool (requires .NET SDK)
timewarp install              # Install all utilities
timewarp install multiavatar  # Install specific utility

# Then use without .NET SDK
multiavatar "user@example.com"
convert-timestamp 1234567890
generate-color "my-project"

# Or install manually (without .NET SDK)
curl -LO https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/installer-linux-x64
gh attestation verify installer-linux-x64 --repo TimeWarpEngineering/timewarp-amuru
chmod +x installer-linux-x64
./installer-linux-x64
```

### When to Use /exe Scripts
- Development and prototyping
- Repository-specific utilities
- Testing new features
- Quick automation tasks
- When .NET SDK is available

### When to Use AOT Binaries
- Production environments without .NET SDK
- CI/CD pipelines with minimal dependencies
- Distribution to non-.NET developers
- Docker containers (smaller image size)
- Performance-critical scenarios (faster startup)

---

## Comparison Tables

### API Layers Comparison

| Layer | Return Type | Error Handling | Verbosity | Use Case |
|-------|-------------|----------------|-----------|----------|
| **Direct** | Native C# types | Exceptions | Low | LINQ, streaming, C# idioms |
| **Commands** | CommandOutput | Exit codes | Low | Shell composition, pipelines |
| **Strongly-Typed** | CommandResult/Output | Configurable | Medium | Complex tool configuration |
| **Bash Aliases** | Both overloads | Both patterns | Minimal | Quick scripts, prototyping |

### Distribution Layers Comparison

| Layer | Installation | Runtime Requirement | Distribution | Update Method |
|-------|-------------|---------------------|--------------|---------------|
| **Ganda** | `dotnet tool install` | .NET SDK | NuGet | `dotnet tool update` |
| **exe scripts** | Clone repository | .NET SDK | Git | `git pull` |
| **AOT binaries** | `timewarp install` | None | GitHub Releases | Re-run `timewarp install` |

---

## Decision Trees

### Which API Layer Should I Use?

```
Do you need to process large files efficiently?
├─ Yes → Use Direct API (streaming with IAsyncEnumerable)
└─ No → Continue

Do you need LINQ operations on data?
├─ Yes → Use Direct API (LINQ-friendly)
└─ No → Continue

Do you need to compose with external commands?
├─ Yes → Use Commands API (consistent CommandOutput)
└─ No → Continue

Do you need complex tool configuration?
├─ Yes → Use Strongly-Typed Builders (DotNet, Git, Fzf)
└─ No → Continue

Do you want minimal verbosity with bash-like names?
├─ Yes → Use Bash Aliases
└─ No → Use Strongly-Typed or Commands based on preference
```

### Which Distribution Should I Use?

```
Is this for end users?
├─ Yes → Continue
│   └─ Do users have .NET SDK?
│       ├─ Yes → Use Ganda (dotnet global tool)
│       └─ No → Use AOT binaries (timewarp install)
└─ No → Continue (development/internal)

Is this for development?
├─ Yes → Use /exe scripts (fast iteration)
└─ No → Continue

Is this for CI/CD?
├─ Yes → Use AOT binaries (minimal dependencies)
└─ No → Use what fits your scenario
```

---

## Integration Patterns

### Pattern 1: Composing Native and External Commands

```csharp
// Both return CommandOutput - seamlessly composable
var nativeResult = Commands.Cat("file.txt");
var externalResult = await Shell.Builder("grep", "ERROR").CaptureAsync();

// Can check both consistently
if (nativeResult.Success && externalResult.Success)
{
  Console.WriteLine("Both succeeded");
}
```

### Pattern 2: Building Ganda Commands with Strongly-Typed APIs

```csharp
// Ganda command implementation using Git strongly-typed builder
private static async Task<int> GenerateAvatarCommand()
{
  // Use strongly-typed Git API
  string? gitRoot = Git.FindRoot();
  string? repoName = await Git.GetRepositoryNameAsync(gitRoot);

  // Use strongly-typed Shell builder
  await Shell.Builder("chafa")
    .WithArguments("--size", "50", avatarPath)
    .RunAsync();
}
```

### Pattern 3: exe Scripts Using Multiple Layers

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru

using TimeWarp.Amuru;
using TimeWarp.Amuru.Native.FileSystem;

// Use Strongly-Typed Git commands
string? gitRoot = Git.FindRoot();

// Use Direct API for streaming
await foreach (var line in Direct.Cat("app.log"))
{
  if (line.Contains("ERROR"))
  {
    Console.WriteLine(line);
  }
}

// Use Shell builder for external commands
await Shell.Builder("git", "status").RunAsync();
```

### Pattern 4: Conditional Configuration Across Builders

```csharp
// All builders support the same extension methods
await DotNet.Build()
  .WhenNotNull(configuration, (b, c) => b.WithConfiguration(c))
  .Unless(skipRestore, b => b.WithNoRestore())
  .RunAsync();

await Shell.Builder("git")
  .WithArguments("push")
  .When(force, b => b.WithArguments("--force"))
  .RunAsync();

var files = await Fzf.Builder()
  .FromInput(allFiles)
  .When(showPreview, b => b.WithPreview("cat {}"))
  .SelectAsync();
```

---

## References

### Architectural Decision Records
- [ADR-0001: Use Static API Entry Point](../ArchitecturalDecisionRecords/Approved/0001-use-static-api-entry-point.md)
- [ADR-0002: Error Handling Philosophy](../ArchitecturalDecisionRecords/Approved/0002-error-handling-philosophy.md)
- [ADR-0004: Omit Interface Abstractions](../ArchitecturalDecisionRecords/Approved/0004-omit-interface-abstractions.md)
- [ADR-0005: Async-First Design](../ArchitecturalDecisionRecords/Approved/0005-async-first-design.md)
- [ADR-0006: Fluent Pipeline API](../ArchitecturalDecisionRecords/Approved/0006-fluent-pipeline-api.md)

### Design Documents
- [Native API Design](../../Analysis/Architecture/NativeApiDesign.md) - Detailed design rationale for Direct vs Commands
- [Native Namespace Design](../../Analysis/Architecture/NativeNamespaceDesign.md) - Namespace organization strategy

### API Documentation
- [README.md](../../../README.md) - Main library documentation
- [Ganda README](../../../Source/TimeWarp.Ganda/README.md) - CLI tool documentation
- [CommandExtensions.md](../../../Source/TimeWarp.Amuru/CommandExtensions.md) - Extension methods reference

### Related Concepts
- [Error Handling](../ArchitecturalDecisionRecords/Approved/0002-error-handling-philosophy.md)
- [Caching Strategy](../ArchitecturalDecisionRecords/Approved/0003-opt-in-caching-strategy.md)
- [Testing Strategy](../ArchitecturalDecisionRecords/Approved/0008-integration-testing-strategy.md)

---

## Summary

TimeWarp.Amuru's layered architecture provides:

1. **Direct API** - Low-level, C#-idiomatic, streaming-capable
2. **Commands API** - Shell-like, consistent CommandOutput interface
3. **Strongly-Typed Builders** - IntelliSense-friendly, domain-specific configuration
4. **Bash Aliases** - Familiar names, minimal verbosity
5. **Ganda CLI** - Multi-command tool for end users with .NET SDK
6. **Standalone Executables** - Development scripts and AOT-compiled binaries for production

Choose the layer that best fits your needs:
- **Performance & LINQ** → Direct API
- **Shell scripting** → Commands API or Bash Aliases
- **Complex configuration** → Strongly-Typed Builders
- **End users (with .NET)** → Ganda
- **End users (without .NET)** → AOT binaries via `timewarp install`
- **Development** → /exe scripts

The layers integrate seamlessly, allowing you to mix and match based on the task at hand.
