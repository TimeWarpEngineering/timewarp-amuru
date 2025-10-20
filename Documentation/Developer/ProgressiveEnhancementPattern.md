# Progressive Enhancement Pattern

## Overview

The **Progressive Enhancement Pattern** is a strategy for incrementally replacing external command functionality with native C# implementations while maintaining full backward compatibility. This pattern allows you to take control of external tools, improve performance, fix bugs, and add features without breaking existing functionality.

This document explains how to apply this pattern to enhance any external command (git, grep, find, sed, curl, etc.) by progressively implementing up to six architectural layers.

## The Six Layers

TimeWarp.Amuru provides six architectural layers that work together to enable progressive enhancement:

```
┌─────────────────────────────────────────────────────────┐
│ Layer 6: AOT Executable                                 │
│   Enhanced binary with catchall passthrough            │
│   Example: Enhanced 'git' with our features + standard │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Layer 5: Ganda CLI Tool                                 │
│   Exposed as timewarp subcommands                       │
│   Example: timewarp git-root                            │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Layer 4: Bash Aliases                                   │
│   Familiar shell-style command names                    │
│   Example: GitRoot(), GitRepoName()                     │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Layer 3: Strongly-Typed Builder (Optional)             │
│   Fluent API for complex configuration                  │
│   Example: Git.Builder().FindRoot()                     │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Layer 2: Commands API                                   │
│   Returns CommandOutput (shell semantics)              │
│   Example: Commands.FindRoot()                          │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Layer 1: Direct API                                     │
│   Pure C# in-process (throws exceptions)               │
│   Example: Direct.FindRoot()                            │
└─────────────────────────────────────────────────────────┘
```

**You don't need all six layers!** Implement only what makes sense for your use case. Layers 1 & 2 are sufficient for most scenarios.

## When to Use This Pattern

Consider progressive enhancement when:

### Performance Matters
- External command has significant process spawning overhead
- Called frequently in tight loops
- Startup time is critical (especially on Windows)

### Control Needed
- External tool has bugs you want to fix
- Missing features you need to add
- Want better error messages or diagnostics
- Need to customize behavior for specific scenarios

### Cross-Platform Issues
- External tool behaves differently across platforms
- Installation/availability varies by OS
- Want consistent behavior everywhere

### Integration Required
- Need tight integration with C# code
- Want LINQ composition
- Need async/await support
- Prefer exceptions over exit codes

### Distribution Concerns
- Want to reduce external dependencies
- Need standalone executables
- Targeting environments without the external tool

## Pattern Benefits

### 1. Incremental Migration
- Start with critical functionality
- Add more features over time
- Don't need full replacement upfront
- Can stop at any layer

### 2. Zero Breaking Changes
- Existing code continues to work
- Users opt-in to new implementations
- Catchall passthrough for unimplemented features
- Gradual migration path

### 3. Multiple Consumption Patterns
- **C# developers**: Direct API (exceptions) or Commands API (exit codes)
- **Shell users**: Bash aliases with familiar names
- **CLI users**: Ganda tool commands
- **Production**: AOT executables without .NET SDK

### 4. Performance Improvement
- No process spawning overhead
- Better memory management
- Faster startup with AOT compilation
- Reduced I/O and context switching

### 5. Full Control
- Fix bugs without waiting for upstream
- Add features the original tool lacks
- Better error messages and diagnostics
- Customize behavior for your needs

## Two Distinct Patterns

**IMPORTANT**: There are two distinct approaches depending on what you're building. Don't confuse them!

### Pattern A: Native In-Process Operations

**When to use**: Implementing NEW functionality that doesn't exist as an external command, OR replacing external command with pure C# for performance/control.

**Example**: FileSystem operations (Cat, Ls, Pwd), Git.FindRoot

**Layer Flow**:
```
Layer 1 (Direct) → Layer 2 (Commands) → Layer 4 (Aliases) → Layer 5 (Ganda) → Layer 6 (AOT)
```

**Characteristics**:
- Pure C# in-process (no external executables)
- MUST start with Layer 1 (Direct API)
- Layer 1 is the foundation - everything else wraps it
- Examples: `Direct.Cat()`, `Direct.FindRoot()`

**Why Layer 1 required**: You're implementing the actual functionality in C#, so Direct API is where the logic lives.

### Pattern B: External Command Wrappers

**When to use**: Wrapping EXISTING external executables with fluent, type-safe API.

**Example**: DotNet, Fzf, Ghq, Gwq

**Layer Flow**:
```
Layer 3 (Builder) → Layer 4 (Aliases) → Layer 5 (Ganda) → Layer 6 (AOT)
```

**Characteristics**:
- Wraps external commands (`dotnet`, `fzf`, `ghq`)
- Start DIRECTLY at Layer 3 (Builder)
- NO Layer 1 or Layer 2 needed
- Builder constructs command-line arguments and executes via Shell
- Examples: `DotNet.Build()`, `Fzf.Builder()`, `Ghq.Builder()`

**Why NO Layer 1/2**: The external command already exists - you're just making it fluent and type-safe, not reimplementing it.

### Pattern C: Hybrid Approach (Progressive Enhancement)

**When to use**: Want to incrementally replace external command features with native implementations.

**Example**: Git with native FindRoot but external for other operations

**Layer Flow**:
```
Start: Layer 3 (Builder wrapping external git)
      ↓
Add:  Layers 1-2 for specific operations (native FindRoot)
      ↓
Result: Layer 6 (Enhanced git with catchall passthrough)
```

**Strategy**:
1. Begin with Pattern B (Layer 3 builder wrapping `git`)
2. Identify hot paths or problematic operations
3. Add Pattern A (Layers 1-2) for those specific operations
4. Builder uses native where available, external where not
5. Eventually create Layer 6 (enhanced `git` binary with catchall)

**Example**: Enhanced git that uses native C# for `FindRoot` (fast) but passes through to real git for `commit`, `push`, etc. (compatible).

## Decision Tree

```
┌─────────────────────────────────────────────────────────────┐
│ What are you building?                                      │
└─────────────────────────────────────────────────────────────┘
                          │
                          ↓
        ┌─────────────────┴──────────────────┐
        │                                     │
        ↓                                     ↓
  New Functionality                  Wrapping Existing Tool
  (Not available elsewhere)          (External command exists)
        │                                     │
        ↓                                     ↓
    Pattern A                             Pattern B
  (Native In-Process)                 (External Wrapper)
        │                                     │
        ↓                                     ↓
  Start Layer 1                         Start Layer 3
  (Direct API)                          (Builder)
  Pure C# implementation                Wraps external command
        │                                     │
  Example:                              Example:
  - FileSystem.Cat()                    - DotNet.Build()
  - Git.FindRoot()                      - Fzf.Builder()
  - Grep with regex                     - Ghq.Builder()


┌─────────────────────────────────────────────────────────────┐
│ Want to progressively replace external tool?                │
└─────────────────────────────────────────────────────────────┘
                          │
                          ↓
                     Pattern C
                  (Hybrid Approach)
                          │
                          ↓
        Start with Pattern B (Layer 3 wrapper)
                          ↓
        Add Pattern A (Layers 1-2) for hot paths
                          ↓
        Create Layer 6 (Enhanced AOT with catchall)
                          │
                     Example:
        Enhanced git with native FindRoot (fast)
        + catchall to real git (compatible)
```

## Visual Flow Diagrams

### Pattern A: Native In-Process Flow

```
┌─────────────────────────────────────────────────────┐
│ Pattern A: Native In-Process Operations            │
│ Example: FileSystem.Cat(), Git.FindRoot()          │
└─────────────────────────────────────────────────────┘

Implementation Flow:
═══════════════════

1. Layer 1: Direct API (Pure C#) ✅ REQUIRED
   ┌─────────────────────────────────────┐
   │ Direct.FindRoot()                   │
   │                                     │
   │ • Pure C# logic                     │
   │ • Throws exceptions                 │
   │ • Returns string                    │
   │ • In-process execution              │
   └─────────────────────────────────────┘
              ↓ wraps with try/catch

2. Layer 2: Commands API ✅ RECOMMENDED
   ┌─────────────────────────────────────┐
   │ Commands.FindRoot()                 │
   │                                     │
   │ • Calls Direct.FindRoot()           │
   │ • Returns CommandOutput             │
   │ • Exit codes instead of exceptions  │
   │ • Shell-compatible                  │
   └─────────────────────────────────────┘
              ↓ expose with familiar names

3. Layer 4: Bash Aliases (Optional)
   ┌─────────────────────────────────────┐
   │ Bash.GitRoot()                      │
   │                                     │
   │ • Friendly bash-style name          │
   │ • Calls Direct or Commands          │
   │ • Minimal verbosity                 │
   └─────────────────────────────────────┘
              ↓ expose as CLI command

4. Layer 5: Ganda CLI (Optional)
   ┌─────────────────────────────────────┐
   │ timewarp git-root                   │
   │                                     │
   │ • CLI subcommand                    │
   │ • Calls Direct.FindRoot()           │
   │ • Global tool via NuGet             │
   └─────────────────────────────────────┘
              ↓ compile standalone

5. Layer 6: AOT Executable (Optional)
   ┌─────────────────────────────────────┐
   │ Enhanced executable with native     │
   │                                     │
   │ • No .NET SDK required              │
   │ • Native code via AOT               │
   │ • Distributed via GitHub releases   │
   └─────────────────────────────────────┘

Usage Example:
═════════════
// C# - Direct API
string root = Direct.FindRoot();  // Throws on error

// C# - Commands API
CommandOutput result = Commands.FindRoot();  // Exit code 0 or 1

// C# - Bash Alias
string root = GitRoot();  // Familiar name

// CLI
$ timewarp git-root
/home/user/project

// Enhanced standalone
$ git find-root
/home/user/project
```

### Pattern B: External Command Wrapper Flow

```
┌─────────────────────────────────────────────────────┐
│ Pattern B: External Command Wrappers               │
│ Example: DotNet.Build(), Fzf.Builder()             │
└─────────────────────────────────────────────────────┘

Implementation Flow:
═══════════════════

❌ NO Layer 1: Direct API (Not Needed)
   ┌─────────────────────────────────────┐
   │ External command already exists     │
   │ • dotnet, fzf, ghq, gwq, etc.       │
   │ • We're just wrapping it            │
   │ • Not reimplementing functionality  │
   └─────────────────────────────────────┘
              ↓ start directly at Layer 3

1. Layer 3: Strongly-Typed Builder ✅ START HERE
   ┌─────────────────────────────────────┐
   │ DotNet.Builder()                    │
   │   .WithConfiguration("Release")     │
   │   .Build()                          │
   │                                     │
   │ • Fluent API                        │
   │ • Type-safe configuration           │
   │ • Builds command-line arguments     │
   │ • Calls Shell.Builder("dotnet", ...)│
   └─────────────────────────────────────┘
              ↓ expose with familiar names

2. Layer 4: Bash Aliases (Optional)
   ┌─────────────────────────────────────┐
   │ Bash.DotnetBuild()                  │
   │                                     │
   │ • Calls DotNet.Builder().Build()    │
   │ • Minimal verbosity                 │
   └─────────────────────────────────────┘
              ↓ expose as CLI command

3. Layer 5: Ganda CLI (Optional)
   ┌─────────────────────────────────────┐
   │ timewarp dotnet-build               │
   │                                     │
   │ • CLI subcommand                    │
   │ • Calls DotNet.Builder()            │
   └─────────────────────────────────────┘
              ↓ compile standalone

4. Layer 6: AOT Executable (Optional)
   ┌─────────────────────────────────────┐
   │ Enhanced dotnet executable          │
   │                                     │
   │ • Adds fluent API wrapper           │
   │ • Passes through to real dotnet     │
   └─────────────────────────────────────┘

Usage Example:
═════════════
// C# - Builder
await DotNet.Builder()
  .WithConfiguration("Release")
  .Build()
  .RunAsync();

// C# - Bash Alias
await DotnetBuild("Release");

// CLI
$ timewarp dotnet-build --configuration Release

Key Insight:
═══════════
✅ Layer 3 is the ENTRY POINT for Pattern B
❌ NO Layer 1 or Layer 2 needed
⚡ External command does the work
🎯 Builder just makes it type-safe and fluent
```

### Pattern C: Hybrid (Progressive Enhancement) Flow

```
┌─────────────────────────────────────────────────────┐
│ Pattern C: Hybrid Approach                         │
│ Example: Git with native + external                │
└─────────────────────────────────────────────────────┘

Progressive Implementation:
═══════════════════════════

Phase 1: Start with Pattern B
   ┌─────────────────────────────────────┐
   │ Git.Builder()                       │
   │   .WithArguments("status")          │
   │   .RunAsync()                       │
   │                                     │
   │ • Wraps external git command        │
   │ • All operations call git           │
   └─────────────────────────────────────┘
              ↓ identify hot paths

Phase 2: Add Pattern A for specific operations
   ┌─────────────────────────────────────┐
   │ Layer 1 & 2: Native FindRoot        │
   │                                     │
   │ Direct.FindRoot() ✅ (pure C#)      │
   │ Commands.FindRoot() ✅ (exit codes) │
   │                                     │
   │ • 100x faster than git rev-parse    │
   │ • No process spawning               │
   └─────────────────────────────────────┘
              ↓ builder chooses implementation

Phase 3: Builder uses hybrid approach
   ┌─────────────────────────────────────┐
   │ Git.Builder()                       │
   │                                     │
   │ FindRoot()     → Direct.FindRoot()  │
   │   ⚡ Native C#                      │
   │                                     │
   │ Status()       → git status         │
   │   🔄 External git                   │
   │                                     │
   │ Commit()       → git commit         │
   │   🔄 External git                   │
   └─────────────────────────────────────┘
              ↓ create enhanced executable

Phase 4: Enhanced git with catchall
   ┌─────────────────────────────────────┐
   │ Enhanced git executable             │
   │                                     │
   │ git find-root  → Direct.FindRoot()  │
   │   ⚡ Native (fast)                  │
   │                                     │
   │ git status     → real git status    │
   │   🔄 Passthrough (compatible)       │
   │                                     │
   │ git commit     → real git commit    │
   │   🔄 Passthrough (compatible)       │
   └─────────────────────────────────────┘

Benefits:
═════════
✅ Start fast (Pattern B wrapper)
✅ Add native when beneficial (Pattern A)
✅ Maintain 100% compatibility (catchall)
✅ Incremental migration (one operation at a time)
✅ Best of both worlds (performance + compatibility)

Migration Path:
══════════════
Time:  Week 1        Week 2         Week 3         Month 2
       │             │              │              │
       ↓             ↓              ↓              ↓
Step:  Builder       Native         Hybrid         Enhanced AOT
       (Pattern B)   (Pattern A)    Builder        (Layer 6)

       Wrap git  →  Add FindRoot → Builder uses → Catchall
       commands     in pure C#     both native     passthrough
                                   and external    for rest
```

## When Layer 1 is Required vs Optional

### Layer 1 REQUIRED (Pattern A)
- ✅ Implementing new functionality in pure C#
- ✅ Examples: FileSystem operations, Git.FindRoot, Text processing
- ✅ Layer 1 (Direct) contains the actual implementation logic
- ✅ Layer 2 (Commands) wraps Layer 1 with CommandOutput

### Layer 1 NOT NEEDED (Pattern B)
- ❌ Wrapping existing external commands
- ❌ Examples: DotNet.Build(), Fzf.Builder(), Ghq.Builder()
- ❌ External tool already implements the functionality
- ❌ Builder just constructs arguments and calls `Shell.Builder()`

### Layer 1 OPTIONAL (Pattern C)
- 🔄 Start without Layer 1 (wrap external command)
- 🔄 Add Layer 1 later for specific operations
- 🔄 Mix native (fast) and external (compatible)
- 🔄 Example: Git builder with native FindRoot

## Layer-by-Layer Guide

### Layer 1: Direct API (Pure C#) - Required for Pattern A Only

**Purpose**: In-process C# implementations with native types and exceptions

**Location**: `Source/TimeWarp.Amuru/Native/{Domain}/Direct/`

**File Pattern**: `Direct.{Operation}.cs`

**Characteristics**:
- Pure C# (no external processes)
- Returns native types (`string`, `IAsyncEnumerable<T>`, `int`, etc.)
- Throws exceptions on errors (`InvalidOperationException`, `FileNotFoundException`)
- LINQ-friendly
- PowerShell-like naming conventions (`GetContent`, `FindRoot`, `GetChildItem`)

**When to Implement**:
- **Always start here** - this is the foundation
- Required for all other layers

**Example - Git.FindRoot**:

```csharp
// Source/TimeWarp.Amuru/Native/Git/Direct/Direct.FindRoot.cs
namespace TimeWarp.Amuru.Native.Git;

/// <summary>
/// Direct C#-style API for Git operations.
/// </summary>
public static partial class Direct
{
  /// <summary>
  /// Finds the root directory of the git repository by walking up the directory tree.
  /// Pure C# implementation - no external git process.
  /// </summary>
  /// <param name="startPath">The directory to start searching from. Defaults to current directory.</param>
  /// <returns>The absolute path to the git repository root.</returns>
  /// <exception cref="InvalidOperationException">When not in a git repository.</exception>
  /// <example>
  /// <code>
  /// try
  /// {
  ///   string root = Direct.FindRoot();
  ///   Console.WriteLine($"Git root: {root}");
  /// }
  /// catch (InvalidOperationException)
  /// {
  ///   Console.WriteLine("Not in a git repository");
  /// }
  /// </code>
  /// </example>
  public static string FindRoot(string? startPath = null)
  {
    string currentPath = startPath ?? Directory.GetCurrentDirectory();

    while (!string.IsNullOrEmpty(currentPath))
    {
      string gitPath = Path.Combine(currentPath, ".git");

      // Check if .git exists (either as directory or file for worktrees)
      if (Directory.Exists(gitPath) || File.Exists(gitPath))
      {
        return currentPath;
      }

      DirectoryInfo? parent = Directory.GetParent(currentPath);
      if (parent == null)
      {
        break;
      }

      currentPath = parent.FullName;
    }

    throw new InvalidOperationException(
      $"Not in a git repository (or any parent up to mount point). " +
      $"Started search from: {startPath ?? Directory.GetCurrentDirectory()}"
    );
  }
}
```

**Usage**:

```csharp
using TimeWarp.Amuru.Native.Git;

try
{
  string root = Direct.FindRoot();
  Console.WriteLine($"Git root: {root}");

  // Can chain with other operations
  string readme = Path.Combine(root, "README.md");
  if (File.Exists(readme))
  {
    Console.WriteLine("README found");
  }
}
catch (InvalidOperationException ex)
{
  Console.WriteLine($"Error: {ex.Message}");
  return 1;
}
```

### Layer 2: Commands API (Shell-Like) - RECOMMENDED

**Purpose**: Shell-compatible interface with `CommandOutput` return type

**Location**: `Source/TimeWarp.Amuru/Native/{Domain}/Commands/`

**File Pattern**: `Commands.{Operation}.cs`

**Characteristics**:
- Wraps Direct API with try/catch
- Returns `CommandOutput` (ExitCode, Stdout, Stderr, Combined)
- No exceptions (shell semantics with exit codes)
- Composable with external commands
- Same operation names as Direct but different return type

**When to Implement**:
- **Highly recommended** - provides shell-like behavior
- For pipeline composition
- When exit codes matter more than exceptions
- For consistency with external commands

**Example - Git.FindRoot**:

```csharp
// Source/TimeWarp.Amuru/Native/Git/Commands/Commands.FindRoot.cs
namespace TimeWarp.Amuru.Native.Git;

/// <summary>
/// Shell-compatible API for Git operations.
/// Returns CommandOutput with exit codes instead of throwing exceptions.
/// </summary>
public static partial class Commands
{
  /// <summary>
  /// Finds git repository root and returns result as CommandOutput.
  /// </summary>
  /// <param name="startPath">The directory to start searching from. Defaults to current directory.</param>
  /// <returns>CommandOutput with root path in stdout on success (exit code 0), or error in stderr (exit code 1).</returns>
  /// <example>
  /// <code>
  /// CommandOutput result = Commands.FindRoot();
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Git root: {result.Stdout}");
  /// }
  /// else
  /// {
  ///   Console.Error.WriteLine($"Error: {result.Stderr}");
  /// }
  /// </code>
  /// </example>
  public static CommandOutput FindRoot(string? startPath = null)
  {
    try
    {
      string root = Direct.FindRoot(startPath);
      return new CommandOutput(
        stdout: root,
        stderr: string.Empty,
        exitCode: 0
      );
    }
    catch (InvalidOperationException ex)
    {
      return new CommandOutput(
        stdout: string.Empty,
        stderr: ex.Message,
        exitCode: 1
      );
    }
    catch (Exception ex)
    {
      return new CommandOutput(
        stdout: string.Empty,
        stderr: $"Unexpected error: {ex.Message}",
        exitCode: 2
      );
    }
  }
}
```

**Usage**:

```csharp
using TimeWarp.Amuru.Native.Git;

CommandOutput result = Commands.FindRoot();

if (result.Success)
{
  Console.WriteLine($"Git root: {result.Stdout}");

  // Can compose with other CommandOutput operations
  CommandOutput statusResult = await Shell.Builder("git", "status")
    .WithWorkingDirectory(result.Stdout)
    .CaptureAsync();
}
else
{
  Console.Error.WriteLine($"Error: {result.Stderr}");
  return result.ExitCode;
}
```

### Layer 3: Strongly-Typed Builder (Optional)

**Purpose**: Fluent API for operations requiring complex configuration

**Location**: `Source/TimeWarp.Amuru/{Tool}Commands/`

**File Pattern**: `{Tool}.{Operation}.cs` or `{Tool}Builder.cs`

**Characteristics**:
- Fluent builder with `.WithX()` methods
- Type-safe configuration
- IntelliSense-friendly
- Similar to `DotNet.Build()`, `Fzf.Builder()` patterns
- Wraps external executables OR native implementations

**When to Implement**:
- Operations need complex configuration
- Want fluent method chaining
- Building comprehensive tool wrapper
- Following existing builder patterns (DotNet, Fzf, Ghq)

**When to Skip**:
- Simple operations (like FindRoot) don't need builders
- Layers 1 & 2 are sufficient
- No complex configuration required

**Example** (illustrative - may not be needed for simple Git operations):

```csharp
// Source/TimeWarp.Amuru/GitCommands/Git.cs
namespace TimeWarp.Amuru;

public static class Git
{
  public static GitBuilder Builder() => new GitBuilder();
}

public class GitBuilder : ICommandBuilder<GitBuilder>
{
  private string? StartPath;
  private bool ThrowOnNotFound = true;

  public GitBuilder WithStartPath(string path)
  {
    StartPath = path;
    return this;
  }

  public GitBuilder WithNoValidation()
  {
    ThrowOnNotFound = false;
    return this;
  }

  public string? FindRoot()
  {
    try
    {
      return Native.Git.Direct.FindRoot(StartPath);
    }
    catch (InvalidOperationException) when (!ThrowOnNotFound)
    {
      return null;
    }
  }

  public CommandOutput FindRootAsOutput()
  {
    return Native.Git.Commands.FindRoot(StartPath);
  }
}
```

**Usage**:

```csharp
using TimeWarp.Amuru;

// Fluent configuration
string? root = Git.Builder()
  .WithStartPath("/some/path")
  .WithNoValidation()
  .FindRoot();

// Or as CommandOutput
CommandOutput result = Git.Builder()
  .WithStartPath("/some/path")
  .FindRootAsOutput();
```

### Layer 4: Bash Aliases (Optional but Recommended)

**Purpose**: Familiar command names for shell users

**Location**: `Source/TimeWarp.Amuru/Native/Aliases/Bash.cs`

**Characteristics**:
- Bash-style naming (`Cat`, `Ls`, `GitRoot`, `Grep`)
- Minimal verbosity with `global using static`
- Can provide both Direct and Commands overloads
- Coexist due to different return types

**When to Implement**:
- Want bash-like command names
- Targeting shell users transitioning to C#
- For quick scripting scenarios
- To reduce verbosity

**Example**:

```csharp
// Source/TimeWarp.Amuru/Native/Aliases/Bash.cs
namespace TimeWarp.Amuru.Native.Aliases;

/// <summary>
/// Unified bash-style aliases for native commands.
/// </summary>
public static class Bash
{
  // ... existing file system aliases (Cat, Ls, Pwd, Cd, Rm) ...

  // ===== Git Operations =====

  /// <summary>
  /// Finds git repository root (Direct version - throws exceptions).
  /// </summary>
  public static string GitRoot(string? startPath = null) =>
    Git.Direct.FindRoot(startPath);

  /// <summary>
  /// Finds git repository root (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput GitRootCmd(string? startPath = null) =>
    Git.Commands.FindRoot(startPath);

  /// <summary>
  /// Gets repository name (Direct version - throws exceptions).
  /// </summary>
  public static async Task<string> GitRepoName(string? gitRoot = null, CancellationToken ct = default) =>
    await Git.Direct.GetRepositoryNameAsync(gitRoot, ct);
}
```

**Usage**:

```csharp
global using static TimeWarp.Amuru.Native.Aliases.Bash;

// Zero verbosity!
try
{
  string root = GitRoot();
  string name = await GitRepoName(root);
  Console.WriteLine($"Repository: {name} at {root}");
}
catch (InvalidOperationException ex)
{
  Console.WriteLine($"Error: {ex.Message}");
}

// Or use Commands version
CommandOutput result = GitRootCmd();
if (result.Success)
{
  Console.WriteLine($"Git root: {result.Stdout}");
}
```

### Layer 5: Ganda CLI (Optional)

**Purpose**: Expose as `timewarp` subcommands for CLI users

**Location**: `Source/TimeWarp.Ganda/Program.cs`

**Characteristics**:
- Part of the `timewarp` multi-command CLI tool
- Uses TimeWarp.Nuru for routing
- Distributed via NuGet as dotnet global tool
- Requires .NET SDK

**When to Implement**:
- Operation is useful as standalone CLI command
- Targeting users with .NET SDK installed
- Want system-wide availability via `dotnet tool install`

**Example**:

```csharp
// Source/TimeWarp.Ganda/Program.cs
namespace TimeWarp.Ganda;

internal static class Program
{
  public static async Task<int> Main(string[] args)
  {
    NuruAppBuilder builder = new();
    builder.AddAutoHelp();

    // ... existing commands ...

    builder.AddRoute(
      "git-root --start-path? {path?|Starting directory path}",
      GitRootCommand,
      "Find git repository root directory"
    );

    builder.AddRoute(
      "git-repo-name --git-root? {root?|Git repository root path}",
      GitRepoNameCommand,
      "Get repository name from git remote URL"
    );

    NuruApp app = builder.Build();
    return await app.RunAsync(args);
  }

  private static int GitRootCommand(string? path)
  {
    try
    {
      string root = Native.Git.Direct.FindRoot(path);
      WriteLine(root);
      return 0;
    }
    catch (InvalidOperationException ex)
    {
      WriteLine($"Error: {ex.Message}");
      return 1;
    }
  }

  private static async Task<int> GitRepoNameCommand(string? root)
  {
    try
    {
      string name = await Native.Git.Direct.GetRepositoryNameAsync(root);
      WriteLine(name);
      return 0;
    }
    catch (InvalidOperationException ex)
    {
      WriteLine($"Error: {ex.Message}");
      return 1;
    }
  }
}
```

**Usage**:

```bash
# Install globally
dotnet tool install --global TimeWarp.Ganda

# Use commands
timewarp git-root
timewarp git-root --start-path /some/path
timewarp git-repo-name
timewarp git-repo-name --git-root /path/to/repo
```

### Layer 6: Enhanced AOT Executable (Advanced)

**Purpose**: Replace system command with enhanced version that adds features while maintaining backward compatibility

**Location**: New project or exe script (e.g., `exe/git.cs` or `Source/TimeWarp.Git/`)

**Strategy**: Use Nuru catchall pattern to passthrough unimplemented commands

**Characteristics**:
- Nuru-based wrapper with route definitions
- Enhanced commands use native implementations
- Catchall route passes through to original command
- Published as AOT binary via GitHub releases
- No .NET SDK required at runtime
- Can replace system command

**When to Implement**:
- Want to distribute to users without .NET SDK
- Want to enhance existing command transparently
- Production environments with minimal dependencies
- Performance-critical scenarios (AOT startup)

**Example**:

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Nuru

using TimeWarp.Amuru;
using TimeWarp.Amuru.Native.Git;
using TimeWarp.Nuru;
using static System.Console;

NuruApp app = new NuruAppBuilder()
  .AddAutoHelp()
  // Our enhanced commands using native implementations
  .AddRoute("find-root --start-path? {path?}", FindRootCommand, "Find repository root (enhanced - pure C#)")
  .AddRoute("repo-name --git-root? {root?}", RepoNameCommand, "Get repository name (enhanced)")
  // Catchall passthrough for everything else
  .AddRoute("{...args}", PassthroughCommand, "Standard git command (passthrough)")
  .Build();

return await app.RunAsync(args);

static int FindRootCommand(string? path)
{
  try
  {
    string root = Direct.FindRoot(path);
    WriteLine(root);
    return 0;
  }
  catch (InvalidOperationException ex)
  {
    Error.WriteLine($"Error: {ex.Message}");
    return 1;
  }
}

static async Task<int> RepoNameCommand(string? root)
{
  try
  {
    string name = await Direct.GetRepositoryNameAsync(root);
    WriteLine(name);
    return 0;
  }
  catch (InvalidOperationException ex)
  {
    Error.WriteLine($"Error: {ex.Message}");
    return 1;
  }
}

static async Task<int> PassthroughCommand(string[] args)
{
  // Pass through to real git binary
  return await Shell.Builder("git")
    .WithArguments(args)
    .RunAsync();
}
```

**Publish as AOT Binary**:

```bash
dotnet publish git.cs \
  -c Release \
  -r linux-x64 \
  -p:PublishAot=true \
  -p:StripSymbols=true \
  -p:InvariantGlobalization=true \
  -o ./bin

# Result: Single native executable ~/bin/git
```

**Installation**:

```bash
# Via timewarp install (downloads from GitHub releases)
timewarp install git

# Or manually
curl -LO https://github.com/.../timewarp-git-linux-x64
gh attestation verify timewarp-git-linux-x64 --repo TimeWarpEngineering/timewarp-amuru
chmod +x timewarp-git-linux-x64
mv timewarp-git-linux-x64 ~/.local/bin/git
```

**Usage**:

```bash
# Our enhanced commands (native C# - no process spawning)
git find-root
git repo-name

# Standard git commands (passthrough to real git)
git status
git add .
git commit -m "message"
git push

# Users get both enhancements AND full compatibility!
```

## Complete Implementation Example: Git

This section shows a complete implementation of Git operations through all relevant layers.

### Step 1: Implement Direct API (Layer 1)

```csharp
// File: Source/TimeWarp.Amuru/Native/Git/Direct/Direct.FindRoot.cs
namespace TimeWarp.Amuru.Native.Git;

public static partial class Direct
{
  public static string FindRoot(string? startPath = null)
  {
    string currentPath = startPath ?? Directory.GetCurrentDirectory();

    while (!string.IsNullOrEmpty(currentPath))
    {
      string gitPath = Path.Combine(currentPath, ".git");
      if (Directory.Exists(gitPath) || File.Exists(gitPath))
      {
        return currentPath;
      }

      DirectoryInfo? parent = Directory.GetParent(currentPath);
      if (parent == null) break;
      currentPath = parent.FullName;
    }

    throw new InvalidOperationException(
      $"Not in a git repository. Started from: {startPath ?? Directory.GetCurrentDirectory()}"
    );
  }
}

// File: Source/TimeWarp.Amuru/Native/Git/Direct/Direct.GetRepositoryName.cs
namespace TimeWarp.Amuru.Native.Git;

public static partial class Direct
{
  public static async Task<string> GetRepositoryNameAsync(
    string? gitRoot = null,
    CancellationToken cancellationToken = default)
  {
    gitRoot ??= FindRoot();

    CommandOutput result = await Shell.Builder("git")
      .WithArguments("remote", "get-url", "origin")
      .WithWorkingDirectory(gitRoot)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success && !string.IsNullOrWhiteSpace(result.Stdout))
    {
      string output = result.Stdout.Trim();

      if (output.Contains("github.com", StringComparison.OrdinalIgnoreCase) ||
          output.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase) ||
          output.Contains("bitbucket.org", StringComparison.OrdinalIgnoreCase))
      {
        string repoName = output.Split('/').Last();
        if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
          repoName = repoName[..^4];
        }
        return repoName;
      }
    }

    // Fallback to directory name
    return new DirectoryInfo(gitRoot).Name;
  }
}
```

### Step 2: Implement Commands API (Layer 2)

```csharp
// File: Source/TimeWarp.Amuru/Native/Git/Commands/Commands.FindRoot.cs
namespace TimeWarp.Amuru.Native.Git;

public static partial class Commands
{
  public static CommandOutput FindRoot(string? startPath = null)
  {
    try
    {
      string root = Direct.FindRoot(startPath);
      return new CommandOutput(root, string.Empty, 0);
    }
    catch (InvalidOperationException ex)
    {
      return new CommandOutput(string.Empty, ex.Message, 1);
    }
  }
}

// File: Source/TimeWarp.Amuru/Native/Git/Commands/Commands.GetRepositoryName.cs
namespace TimeWarp.Amuru.Native.Git;

public static partial class Commands
{
  public static async Task<CommandOutput> GetRepositoryNameAsync(
    string? gitRoot = null,
    CancellationToken cancellationToken = default)
  {
    try
    {
      string name = await Direct.GetRepositoryNameAsync(gitRoot, cancellationToken);
      return new CommandOutput(name, string.Empty, 0);
    }
    catch (InvalidOperationException ex)
    {
      return new CommandOutput(string.Empty, ex.Message, 1);
    }
  }
}
```

### Step 3: Add Bash Aliases (Layer 4)

```csharp
// File: Source/TimeWarp.Amuru/Native/Aliases/Bash.cs (add to existing file)
namespace TimeWarp.Amuru.Native.Aliases;

public static class Bash
{
  // ... existing aliases ...

  // ===== Git Operations =====

  public static string GitRoot(string? startPath = null) =>
    Git.Direct.FindRoot(startPath);

  public static CommandOutput GitRootCmd(string? startPath = null) =>
    Git.Commands.FindRoot(startPath);

  public static async Task<string> GitRepoName(
    string? gitRoot = null,
    CancellationToken ct = default) =>
    await Git.Direct.GetRepositoryNameAsync(gitRoot, ct);
}
```

### Step 4: Add to Ganda (Layer 5)

```csharp
// File: Source/TimeWarp.Ganda/Program.cs (add to existing routes)
builder.AddRoute(
  "git-root --start-path? {path?}",
  GitRootCommand,
  "Find git repository root"
);

builder.AddRoute(
  "git-repo-name --git-root? {root?}",
  GitRepoNameCommand,
  "Get repository name"
);

private static int GitRootCommand(string? path)
{
  try
  {
    string root = Native.Git.Direct.FindRoot(path);
    WriteLine(root);
    return 0;
  }
  catch (InvalidOperationException ex)
  {
    Error.WriteLine($"Error: {ex.Message}");
    return 1;
  }
}

private static async Task<int> GitRepoNameCommand(string? root)
{
  try
  {
    string name = await Native.Git.Direct.GetRepositoryNameAsync(root);
    WriteLine(name);
    return 0;
  }
  catch (InvalidOperationException ex)
  {
    Error.WriteLine($"Error: {ex.Message}");
    return 1;
  }
}
```

### Step 5: Create Enhanced Executable (Layer 6)

See the Layer 6 example above for the complete enhanced git executable.

## Applying to Other Commands

### Template Checklist

Use this checklist when progressively enhancing any external command:

#### Planning
- [ ] **Identify** - What functionality should be enhanced?
- [ ] **Research** - How does the original command work?
- [ ] **Analyze** - What are the performance/control benefits?
- [ ] **Decide** - Which layers are needed? (Start with 1 & 2)

#### Implementation
- [ ] **Layer 1 (Direct)** - Implement pure C# version
- [ ] **Test Layer 1** - Unit and integration tests
- [ ] **Layer 2 (Commands)** - Wrap with CommandOutput
- [ ] **Test Layer 2** - Integration tests
- [ ] **Layer 4 (Aliases)** - Add bash-style names (optional)
- [ ] **Layer 5 (Ganda)** - Add CLI commands (optional)
- [ ] **Layer 6 (AOT)** - Create enhanced executable (optional)

#### Documentation
- [ ] **API Docs** - Document all methods with XML comments
- [ ] **Usage Examples** - Show all consumption patterns
- [ ] **Migration Guide** - Help users transition from external command
- [ ] **README** - Add section for new functionality

#### Verification
- [ ] **Cross-Platform** - Test on Windows, Linux, macOS
- [ ] **Edge Cases** - Handle errors, missing files, etc.
- [ ] **Performance** - Benchmark vs external command
- [ ] **Integration** - Verify existing code still works

### Candidate Commands for Enhancement

#### High Priority
Commands where C# implementation provides significant benefits:

- **grep** - Better Unicode support, C# regex, streaming
- **find** - Modern filtering, LINQ composition, cross-platform consistency
- **sed** - C# regex instead of cryptic syntax, better error messages

#### Medium Priority
Commands with useful enhancements:

- **curl** - Better progress reporting, cleaner API, built-in retry logic
- **tar** - Simpler extraction/compression, streaming support
- **ps** - Cross-platform process info, rich object model
- **wc** - More statistics, streaming, custom delimiters

#### Low Priority
Nice-to-have enhancements:

- **diff** - Smarter algorithms, semantic diff, custom comparers
- **sort** - Configurable collation, stable sort guarantees
- **head/tail** - Streaming, follow mode, custom line endings

## Best Practices

### DO ✅

- **Start with Layer 1 (Direct API)** - It's the foundation for everything else
- **Add Layer 2 (Commands)** - Provides shell compatibility
- **Write integration tests** - Verify behavior matches external command
- **Document migration path** - Help users transition smoothly
- **Use PowerShell-style naming** - GetContent not ReadFile, FindRoot not GetGitRoot
- **Handle edge cases** - Empty directories, permission errors, missing dependencies
- **Test cross-platform** - Windows, Linux, macOS, ARM64
- **Benchmark performance** - Verify improvement over external command

### DON'T ❌

- **Skip Direct API** - Don't jump straight to builders
- **Break backward compatibility** - Always provide migration path
- **Implement full replacement** - Partial enhancement is often sufficient
- **Forget error handling** - Provide helpful error messages
- **Neglect documentation** - Users need usage examples
- **Ignore platform differences** - Path separators, line endings, etc.
- **Over-engineer** - Start simple, add complexity only when needed

## Performance Comparison

Example benchmark comparing native vs external command:

### Git.FindRoot Performance

| Implementation | Cold Start | Warm Start | Notes |
|----------------|-----------|-----------|-------|
| External `git rev-parse` | ~50-100ms | ~20-40ms | Process spawn overhead |
| Native C# | ~0.1-0.5ms | ~0.05-0.2ms | Pure in-process |
| **Improvement** | **100-200x faster** | **100x faster** | Especially on Windows |

### Benefits Beyond Speed
- No process spawning overhead
- No PATH resolution
- No executable loading
- No IPC overhead
- Better memory locality
- Predictable performance

## Related Documentation

- [Architectural Layers](../Conceptual/ArchitecturalLayers.md) - Complete overview of all six layers
- [ADR-0001: Static API Entry Point](../Conceptual/ArchitecturalDecisionRecords/Approved/0001-use-static-api-entry-point.md)
- [ADR-0002: Error Handling Philosophy](../Conceptual/ArchitecturalDecisionRecords/Approved/0002-error-handling-philosophy.md)
- [Native API Design](../../Analysis/Architecture/NativeApiDesign.md) - Direct vs Commands design decisions

## Summary

The Progressive Enhancement Pattern enables you to:

1. **Take Control** - Own the implementation of external commands
2. **Improve Performance** - Eliminate process spawning overhead
3. **Add Features** - Enhance tools without waiting for upstream
4. **Maintain Compatibility** - Zero breaking changes with catchall passthrough
5. **Serve Multiple Audiences** - C# developers, shell users, CLI users, production
6. **Deploy Flexibly** - Libraries, tools, or standalone AOT executables

**Start with Layers 1 & 2 (Direct + Commands)**. Add additional layers only when needed. Git serves as the reference implementation in `Kanban/ToDo/050_Implement-Git-Native-Commands.md`.
