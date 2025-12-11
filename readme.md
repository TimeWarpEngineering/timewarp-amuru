[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/timewarp-amuru?logo=github)](https://github.com/TimeWarpEngineering/timewarp-amuru)
[![workflow](https://github.com/TimeWarpEngineering/timewarp-amuru/actions/workflows/release-build.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-amuru/actions)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/timewarp-amuru)](https://github.com/TimeWarpEngineering/timewarp-amuru)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-amuru.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/timewarp-amuru/issues)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/timewarp-amuru.svg?logo=github)](https://github.com/TimeWarpEngineering/timewarp-amuru/issues)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/TimeWarpEngineering/timewarp-amuru/badge)](https://scorecard.dev/viewer/?uri=github.com/TimeWarpEngineering/timewarp-amuru)

[![nuget](https://img.shields.io/nuget/v/TimeWarp.Amuru?logo=nuget)](https://www.nuget.org/packages/TimeWarp.Amuru/)
[![nuget](https://img.shields.io/nuget/dt/TimeWarp.Amuru?logo=nuget)](https://www.nuget.org/packages/TimeWarp.Amuru/)

[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Ftimewarp-amuru)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/timewarp-amuru)
[![Dotnet](https://img.shields.io/badge/dotnet-10.0-blue)](https://dotnet.microsoft.com)

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)

<img src="https://raw.githubusercontent.com/TimeWarpEngineering/timewarpengineering.github.io/refs/heads/master/images/LogoNoMarginNoShadow.svg" alt="logo" height="120" style="float: right" />

# TimeWarp.Amuru

*Amuru means "command" in Swahili*

**TimeWarp.Amuru** is a powerful fluent API library for elegant command-line execution in C#. It transforms shell scripting into a type-safe, IntelliSense-friendly experience with a simple static `Builder()` method, async operations, and proper error handling.

Designed for modern C# developers, TimeWarp.Amuru brings the power of shell scripting directly into your C# code. Whether you're building automation tools, DevOps scripts, or integrating command-line tools into your applications, TimeWarp.Amuru provides the elegant, type-safe API you need.

## Why TimeWarp.Amuru?

- **Zero Learning Curve**: If you know C#, you already know how to use TimeWarp.Amuru
- **IntelliSense Everything**: Full IDE support with autocomplete, parameter hints, and documentation
- **Type Safety**: Catch errors at compile-time, not runtime
- **No String Escaping Hell**: Use C# arrays and parameters naturally
- **Built for .NET 10**: Modern C# features and performance optimizations
- **Script or Library**: Use it in quick scripts or production applications

## Give a Star! :star:

If you find this project useful, please give it a star. Thanks!

## Installation

```bash
# Core library for shell scripting and process execution
dotnet add package TimeWarp.Amuru
```

Or reference in your C# runfile:
```csharp
#:package TimeWarp.Amuru@<latest-version>
```

### Optional: CLI Tools

```bash
# Global CLI tool with additional utilities (private package)
dotnet tool install --global TimeWarp.Ganda --source https://nuget.pkg.github.com/TimeWarpEngineering/index.json
```

The CLI tool includes various utilities like timestamp conversion, color generation, and more. See the [Ganda repository](https://github.com/TimeWarpEngineering/timewarp-ganda) for details.

## Quick Start

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru

using TimeWarp.Amuru;
using static System.Console;

// Default behavior - stream to console (like bash/PowerShell)
await Shell.Builder("npm", "install").RunAsync();

// Capture output when needed
var result = await Shell.Builder("git", "status").CaptureAsync();
if (result.Success)
{
    WriteLine($"Git says: {result.Stdout}");
}

// Stream large files without memory issues
await foreach (var line in Shell.Builder("tail", "-f", "/var/log/app.log").StreamStdoutAsync())
{
    WriteLine($"Log: {line}");
}

// Chain commands with pipelines
var result = await Shell.Builder("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .CaptureAsync();
WriteLine($"Found {result.Lines.Length} async files");

// Work with CommandOutput
var output = await Shell.Builder("docker", "ps").CaptureAsync();
WriteLine($"Exit code: {output.ExitCode}");
WriteLine($"Success: {output.Success}");
WriteLine($"Stdout: {output.Stdout}");
WriteLine($"Stderr: {output.Stderr}");
WriteLine($"Combined: {output.Combined}");

// Use the fluent builder API for complex commands
var result = await Shell.Builder("git")
    .WithArguments("log", "--oneline", "-n", "10")
    .WithWorkingDirectory("/my/repo")
    .WithCancellationToken(cancellationToken)
    .CaptureAsync();

// Provide standard input to commands
var grepResult = await Shell.Builder("grep")
    .WithArguments("pattern")
    .WithStandardInput("line1\nline2 with pattern\nline3")
    .CaptureAsync();

// Interactive selection with Fzf
var selectedFile = await Fzf.Builder()
    .FromInput("file1.txt", "file2.txt", "file3.txt")
    .WithPreview("cat {}")
    .SelectAsync();

// Interactive pipeline - find and select files
var chosenFile = await Shell.Builder("find")
    .WithArguments(".", "-name", "*.cs")
    .Pipe("fzf", "--preview", "head -20 {}")
    .SelectAsync();

// Full interactive mode for stream-based tools (fzf, REPLs)
await Shell.Builder("fzf")
    .PassthroughAsync();

// TUI applications (vim, nano, edit) need true TTY passthrough
await Shell.Builder("vim")
    .WithArguments("myfile.txt")
    .TtyPassthroughAsync();
```

### DotNet Commands

```csharp
// Global dotnet options
var sdks = await DotNet.WithListSdks().CaptureAsync();
var runtimes = await DotNet.WithListRuntimes().CaptureAsync();
var version = await DotNet.WithVersion().CaptureAsync();

// Base builder for custom arguments
var result = await DotNet.Builder()
    .WithArguments("--list-sdks")
    .CaptureAsync();

// Build and test with streaming output
await DotNet.Build()
    .WithConfiguration("Release")
    .RunAsync();

await DotNet.Test()
    .WithFilter("Category=Unit")
    .RunAsync();
```

### Conditional Configuration

The `When()` extension method allows you to apply configuration conditionally without breaking the fluent chain:

```csharp
// Without When() - breaks the fluent chain
DotNetAddPackageBuilder builder = DotNet.AddPackage(packageName);
if (version != null)
{
    builder = builder.WithVersion(version);
}
else
{
    builder = builder.WithPrerelease();
}
await builder.CaptureAsync();

// With When() - keeps the fluent chain intact
await DotNet.AddPackage(packageName)
    .WithProject(projectFile)
    .When(version != null, b => b.WithVersion(version!))
    .When(version == null, b => b.WithPrerelease())
    .CaptureAsync();

// Works with all builders
await Shell.Builder("git")
    .WithArguments("push")
    .When(force, b => b.WithArguments("--force"))
    .When(dryRun, b => b.WithArguments("--dry-run"))
    .When(workDir != null, b => b.WithWorkingDirectory(workDir!))
    .RunAsync();
```

### Available Extension Methods

All extension methods work on any builder that implements `ICommandBuilder<T>`:

**`When(condition, configure)`** - Apply configuration when condition is true
```csharp
.When(version != null, b => b.WithVersion(version!))
```

**`WhenNotNull(value, configure)`** - Apply configuration when value is not null, passing the value
```csharp
.WhenNotNull(version, (b, v) => b.WithVersion(v))  // Cleaner than When!
```

**`Unless(condition, configure)`** - Apply configuration when condition is false
```csharp
.Unless(isProduction, b => b.WithVerbose())
```

**`Apply(configure)`** - Extract and reuse configuration logic
```csharp
static DotNetBuildBuilder AddProductionSettings(DotNetBuildBuilder b) =>
  b.WithConfiguration("Release").WithNoRestore();

await DotNet.Build()
  .Apply(AddProductionSettings)
  .RunAsync();
```

**`ForEach(items, configure)`** - Apply configuration for each item
```csharp
.ForEach(sources, (b, source) => b.WithSource(source))
```

**`Tap(action)`** - Side effects without modifying the builder (logging, debugging)
```csharp
.Tap(b => Console.WriteLine($"Building with config: {b}"))
```

These extensions:
- Maintain type safety and IntelliSense support
- Keep method chains fluent and readable
- Work with all command builders (Shell, DotNet, Fzf, etc.)
- Enable functional programming patterns

## Key Features

- **Shell-Like Default**: `RunAsync()` streams to console just like bash/PowerShell
- **Explicit Capture**: `CaptureAsync()` for when you need to process output
- **Memory-Efficient Streaming**: `IAsyncEnumerable` for large data without buffering
- **Complete Output Access**: CommandOutput with Stdout, Stderr, Combined, and ExitCode
- **Fluent Interface**: Chain operations naturally with `.Pipe()` and builder methods
- **Conditional Configuration**: `When()` extension for fluent conditional logic
- **Async-First Design**: All operations support modern async/await patterns
- **Smart Error Handling**: Commands throw on errors by default, with opt-in graceful degradation
- **Pipeline Support**: Chain commands with Unix-like pipe semantics
- **Standard Input Support**: Provide stdin to commands with `.WithStandardInput()`
- **NO CACHING Philosophy**: Like shells, commands run fresh every time
- **Configuration Options**: Working directory, environment variables, and more
- **Cancellation Support**: Full CancellationToken support for timeouts and manual cancellation
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Command Builders**: Fluent builders for complex commands (DotNet, Fzf, Ghq, Gwq)
- **Interactive Commands**: `PassthroughAsync()` for stream-based tools, `TtyPassthroughAsync()` for TUI apps (vim, nano), `SelectAsync()` for selection tools
- **.NET 10 Script Support**: AppContext extensions and ScriptContext for file-based apps

## Output Handling

### Core API Methods

TimeWarp.Amuru provides clear, purpose-built methods for different scenarios:

```csharp
// RunAsync() - Default shell behavior, streams to console
await Shell.Builder("npm", "install").RunAsync();
// Returns: exit code (int)
// Console output: real-time streaming

// CaptureAsync() - Silent execution with full output capture
var result = await Shell.Builder("git", "status").CaptureAsync();
// Returns: CommandOutput with all streams
// Console output: none (silent)

// PassthroughAsync() - Stream-based interactive tools (fzf, REPLs)
await Shell.Builder("fzf").PassthroughAsync();
// Returns: ExecutionResult
// Console output: piped through Console streams

// TtyPassthroughAsync() - True TTY for TUI applications (vim, nano, edit)
await Shell.Builder("vim", "file.txt").TtyPassthroughAsync();
// Returns: ExecutionResult
// Console output: inherits parent TTY (required for TUI apps)

// SelectAsync() - Selection tools (shows UI, captures selection)
var selected = await Fzf.Builder()
    .FromInput("option1", "option2")
    .SelectAsync();
// Returns: selected string
// Console output: UI on stderr, selection captured from stdout
```

### The CommandOutput Type

```csharp
var output = await Shell.Builder("docker", "ps").CaptureAsync();

// Access individual streams
Console.WriteLine($"Stdout: {output.Stdout}");
Console.WriteLine($"Stderr: {output.Stderr}");
Console.WriteLine($"Combined: {output.Combined}"); // Both in chronological order

// Check status
Console.WriteLine($"Exit code: {output.ExitCode}");
Console.WriteLine($"Success: {output.Success}"); // ExitCode == 0

// Convenience properties for line processing
foreach (var line in output.Lines) // Combined.Split('\n')
{
    ProcessLine(line);
}
```

### Streaming Large Data

For commands that produce large amounts of data:

```csharp
// Stream lines as they arrive (no buffering)
await foreach (var line in Shell.Builder("tail", "-f", "/var/log/app.log")
    .StreamStdoutAsync(cancellationToken))
{
    Console.WriteLine($"Log: {line}");
}

// Stream with LINQ-style processing
var errorLines = Shell.Builder("cat", "huge.log")
    .StreamStdoutAsync()
    .Where(line => line.Contains("ERROR"))
    .Take(100);

await foreach (var error in errorLines)
{
    LogError(error);
}
```

### Method Comparison

| Method | Console Output | Captures | Returns | Primary Use Case |
|--------|---------------|----------|---------|------------------|
| `RunAsync()` | ✅ Real-time | ❌ | Exit code | Default scripting (80%) |
| `CaptureAsync()` | ❌ Silent | ✅ All streams | CommandOutput | Process output (15%) |
| `PassthroughAsync()` | ✅ Piped | ❌ | ExecutionResult | Stream-based interactive (3%) |
| `TtyPassthroughAsync()` | ✅ TTY | ❌ | ExecutionResult | TUI apps (vim, nano) (1%) |
| `SelectAsync()` | ✅ UI only | ✅ Selection | string | Selection tools (1%) |
| `StreamStdoutAsync()` | ❌ | ✅ As stream | IAsyncEnumerable | Large data |

### Design Philosophy: NO CACHING

TimeWarp.Amuru intentionally does NOT cache command results:

```csharp
// Shells don't cache - neither do we
await Shell.Builder("date").RunAsync();  // Shows current time
await Shell.Builder("date").RunAsync();  // Shows NEW current time

// If you need caching, it's trivial in C#:
private static CommandOutput? cachedResult;
var result = cachedResult ??= await Shell.Builder("expensive-command").CaptureAsync();
```

**Why no caching?**
- Commands can have side effects
- Results change over time
- Shells don't cache
- Users can trivially cache in C# if needed

## Error Handling

TimeWarp.Amuru provides intelligent error handling that distinguishes between different failure types:

### Default Behavior (Throws Exceptions)
```csharp
// Throws CommandExecutionException on non-zero exit code
await Shell.Builder("ls", "/nonexistent").RunAsync();

// CaptureAsync also throws on failure by default
var result = await Shell.Builder("git", "invalid-command").CaptureAsync();
```

### Graceful Degradation (Opt-in)
```csharp
// Disable validation for graceful degradation
var result = await Shell.Builder("ls", "/nonexistent")
    .WithValidation(CommandResultValidation.None)
    .CaptureAsync();

if (!result.Success)
{
    Console.WriteLine($"Command failed with exit code: {result.ExitCode}");
    Console.WriteLine($"Error: {result.Stderr}");
}
```

### Cancellation and Timeouts
```csharp
// With explicit cancellation token
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await Shell.Builder("long-running-command")
    .RunAsync(cts.Token);

// With timeout via builder
await Shell.Builder("slow-command")
    .WithTimeout(TimeSpan.FromSeconds(10))
    .RunAsync();

// Timeout and external token are combined
await Shell.Builder("another-command")
    .WithTimeout(TimeSpan.FromSeconds(5))
    .RunAsync(userCancellationToken);
```

## Testing and Mocking

TimeWarp.Amuru provides built-in support for mocking commands during testing through the `CliConfiguration` class:

### Basic Mocking
```csharp
// Set up mock commands for testing
CliConfiguration.SetCommandPath("fzf", "/path/to/mock/fzf");
CliConfiguration.SetCommandPath("git", "/path/to/mock/git");

// Your code using these commands will now use the mocks
var selected = await Fzf.Builder()
    .FromInput("option1", "option2", "option3")
    .SelectAsync(); // Uses mock fzf

var status = await Shell.Builder("git", "status")
    .CaptureAsync(); // Uses mock git

// Clean up after tests
CliConfiguration.Reset();
```

### Creating Mock Executables
```csharp
// Create a simple mock script
File.WriteAllText("/tmp/mock-fzf", "#!/bin/bash\necho 'mock-selection'");
await Shell.Builder("chmod", "+x", "/tmp/mock-fzf").RunAsync();

// Configure TimeWarp.Amuru to use it
CliConfiguration.SetCommandPath("fzf", "/tmp/mock-fzf");

// Now SelectAsync will use the mock
var selected = await Fzf.Builder()
    .FromInput("a", "b", "c")
    .SelectAsync(); // Returns "mock-selection"
```

### Testing Interactive Commands
For commands like `fzf` that are normally interactive, you can either:
1. Use mock executables as shown above
2. Use non-interactive modes (e.g., `fzf --filter`)

### API Reference
- `CliConfiguration.SetCommandPath(command, path)` - Set custom executable path
- `CliConfiguration.ClearCommandPath(command)` - Remove custom path for a command
- `CliConfiguration.Reset()` - Clear all custom paths
- `CliConfiguration.HasCustomPath(command)` - Check if command has custom path
- `CliConfiguration.AllCommandPaths` - Get all configured paths

## .NET 10 File-Based App Support

TimeWarp.Amuru provides specialized support for .NET 10's new file-based apps (single-file C# scripts) with AppContext extensions and ScriptContext for directory management.

- **AppContext Extensions** - Clean access to script metadata without magic strings
- **ScriptContext** - Automatic working directory management with cleanup guarantees
- **ProcessExit Handling** - Cleanup runs even with `Environment.Exit()`

📖 **[See the documentation](Documentation/Developer/HowToGuides/)** for detailed usage guides and examples.

## Architecture

TimeWarp.Amuru is built on several key architectural principles:

- **Static Entry Point**: Minimal ceremony with global `Builder()` method
- **Immutable Design**: Thread-safe, readonly objects throughout
- **Integration Testing**: Real command validation over mocking
- **Predictable Error Handling**: Clear distinction between failure types
- **Opt-in Complexity**: Advanced features available when needed

See our [Architectural Decision Records](Documentation/Conceptual/ArchitecturalDecisionRecords/Overview.md) for detailed design rationale.

## Documentation

- **[Migration Guide](Analysis/MigrationGuide.md)** - Guide for migrating from older versions
- **[CommandExtensions.md](Source/TimeWarp.Amuru/CommandExtensions.md)** - Static API documentation
- **[CommandResult.md](Source/TimeWarp.Amuru/CommandResult.md)** - Fluent interface documentation
- **[Architectural Decisions](Documentation/Conceptual/ArchitecturalDecisionRecords/Overview.md)** - Design rationale and decisions

## Example Scripts

See [Spikes/CsScripts/](Spikes/CsScripts/) for example scripts demonstrating TimeWarp.Amuru usage patterns.

## Unlicense

[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-amuru.svg?style=flat-square&logo=github)](https://unlicense.org)  
This project is licensed under the [Unlicense](https://unlicense.org).

## Related Packages

- **[TimeWarp.Multiavatar](https://www.nuget.org/packages/TimeWarp.Multiavatar/)** - Avatar generation library (see [documentation](Source/TimeWarp.Multiavatar/README.md))
- **[TimeWarp.Ganda](https://github.com/TimeWarpEngineering/timewarp-ganda)** - Shell toolkit CLI (private, moved to separate repository)
- **[TimeWarp.Zana](https://github.com/TimeWarpEngineering/timewarp-ganda)** - Private utilities library (in timewarp-ganda repo)

## Contributing

Your contributions are welcome! Before starting any work, please open a [discussion](https://github.com/TimeWarpEngineering/timewarp-amuru/discussions).

See our [Kanban board](Kanban/Overview.md) for current development tasks and priorities.

## Contact

If you have an issue and don't receive a timely response, feel free to reach out on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)