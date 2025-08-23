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

**TimeWarp.Amuru** (formerly TimeWarp.Cli) is a powerful fluent API library for elegant command-line execution in C#. It transforms shell scripting into a type-safe, IntelliSense-friendly experience with a simple static `Run()` method, async operations, and proper error handling.

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

## Quick Start

```csharp
#!/usr/bin/dotnet run
#:package TimeWarp.Amuru

using TimeWarp.Amuru;

// Get command output as string
var date = await Run("date").GetStringAsync();
Console.WriteLine($"Current date: {date}");

// Process output line by line
var files = await Run("find", ".", "-name", "*.cs").GetLinesAsync();
foreach (var file in files)
{
    Console.WriteLine($"Found: {file}");
}

// Execute without capturing output
await Run("echo", "Hello World").ExecuteAsync();

// Chain commands with pipelines
var filteredFiles = await Run("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .GetLinesAsync();

// Use caching for expensive operations
var files = Run("find", "/large/dir", "-name", "*.log").Cached();
var errors = await files.Pipe("grep", "ERROR").GetLinesAsync();
var warnings = await files.Pipe("grep", "WARN").GetLinesAsync();
// Only one expensive find operation executed!

// C# scripts with arguments work seamlessly
await Run("./myscript.cs", "--verbose", "-o", "output.txt").ExecuteAsync();

// Use the new fluent builder API for complex commands
var result = await Shell.Run("git")
    .WithArguments("log", "--oneline", "-n", "10")
    .WithWorkingDirectory("/my/repo")
    .GetStringAsync();

// Provide standard input to commands
var grepResult = await Shell.Run("grep")
    .WithArguments("pattern")
    .WithStandardInput("line1\nline2 with pattern\nline3")
    .GetStringAsync();

// Use fluent command builders for .NET commands
var packages = await DotNet.ListPackages()
    .WithOutdated()
    .AsJson()
    .ToListAsync();

// Interactive file selection with Fzf (NEW in v0.6.0)
// Use GetStringInteractiveAsync() to show FZF UI and capture selection
var selectedFile = await Fzf.Builder()
    .FromInput("file1.txt", "file2.txt", "file3.txt")
    .WithPreview("cat {}")
    .GetStringInteractiveAsync();

// Interactive pipeline - find files and select with FZF
var chosenFile = await Shell.Run("find")
    .WithArguments(".", "-name", "*.cs")
    .Pipe("fzf", "--preview", "head -20 {}")
    .GetStringInteractiveAsync();

// Multi-select with interactive FZF
var selectedItems = await Fzf.Builder()
    .FromInput("Red", "Green", "Blue", "Yellow")
    .WithMulti()
    .GetStringInteractiveAsync();

// Full interactive mode (e.g., for vim, nano, etc.)
await Shell.Run("vim")
    .WithArguments("myfile.txt")
    .ExecuteInteractiveAsync();
```

## Installation

```console
dotnet add package TimeWarp.Cli
```

Or reference in your C# script:
```csharp
#:package TimeWarp.Cli
```

Check out the latest NuGet package: [TimeWarp.Cli](https://www.nuget.org/packages/TimeWarp.Cli/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.Cli?logo=nuget)](https://www.nuget.org/packages/TimeWarp.Cli/)

### DotNet Commands

```csharp
// Global dotnet options
var sdks = await DotNet.WithListSdks().GetLinesAsync();
var runtimes = await DotNet.WithListRuntimes().GetLinesAsync();
var version = await DotNet.WithVersion().GetStringAsync();
var info = await DotNet.WithInfo().GetStringAsync();

// Base builder for custom arguments
await DotNet.Builder().WithArguments("--list-sdks").GetLinesAsync();

// Subcommands (existing API)
await DotNet.Build().WithConfiguration("Release").ExecuteAsync();
await DotNet.Test().WithFilter("Category=Unit").ExecuteAsync();
```

## Key Features

- **Simple Static API**: Global `Run()` method for immediate access
- **Fluent Interface**: Chain operations naturally with `.Pipe()`, `.Cached()`, etc.
- **Async-First Design**: All operations support modern async/await patterns
- **Smart Error Handling**: Commands throw on errors by default, with opt-in graceful degradation
- **Pipeline Support**: Chain commands with Unix-like pipe semantics
- **Standard Input Support**: Provide stdin to commands with `.WithStandardInput()`
- **Opt-in Caching**: Cache expensive command results with `.Cached()` method
- **Configuration Options**: Working directory, environment variables, and more
- **Cancellation Support**: Full CancellationToken support for timeouts and manual cancellation
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **C# Script Support**: Seamless execution of C# scripts with proper argument handling
- **Command Builders**: Fluent builders for complex commands (DotNet, Fzf, Ghq, Gwq)
- **Interactive Commands**: Support for interactive tools like FZF with `GetStringInteractiveAsync()` and `ExecuteInteractiveAsync()`
- **.NET 10 Script Support**: AppContext extensions and ScriptContext for file-based apps

## Error Handling

TimeWarp.Cli provides intelligent error handling that distinguishes between different failure types:

### Default Behavior (Throws Exceptions)
```csharp
// Throws CommandExecutionException on non-zero exit code
await Run("ls", "/nonexistent").GetStringAsync();

// Throws exception if command not found
await Run("nonexistentcommand").GetStringAsync();
```

### Graceful Degradation (Opt-in)
```csharp
// Returns empty string/array on command failure
var options = new CommandOptions().WithValidation(CommandResultValidation.None);
var result = await Run("ls", "/nonexistent", options).GetStringAsync(); // ""

// Note: Process start failures (command not found) always throw
await Run("nonexistentcommand", options).GetStringAsync(); // Still throws!
```

### Special Cases
- Empty/whitespace commands return empty results (no exception)
- Null command options return empty results (defensive programming)
- Pipeline failures propagate based on validation settings

## Testing and Mocking

TimeWarp.Cli provides built-in support for mocking commands during testing through the `CliConfiguration` class:

### Basic Mocking
```csharp
// Set up mock commands for testing
CliConfiguration.SetCommandPath("fzf", "/path/to/mock/fzf");
CliConfiguration.SetCommandPath("git", "/path/to/mock/git");

// Your code using these commands will now use the mocks
var selected = await Fzf.Builder()
    .FromInput("option1", "option2", "option3")
    .GetStringAsync(); // Uses mock fzf

// Clean up after tests
CliConfiguration.Reset();
```

### Creating Mock Executables
```csharp
// Create a simple mock script
File.WriteAllText("/tmp/mock-fzf", "#!/bin/bash\necho 'mock-selection'");
Run("chmod", "+x", "/tmp/mock-fzf");

// Configure TimeWarp.Cli to use it
CliConfiguration.SetCommandPath("fzf", "/tmp/mock-fzf");
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

TimeWarp.Cli is built on several key architectural principles:

- **Static Entry Point**: Minimal ceremony with global `Run()` method
- **Immutable Design**: Thread-safe, readonly objects throughout
- **Integration Testing**: Real command validation over mocking
- **Predictable Error Handling**: Clear distinction between failure types
- **Opt-in Complexity**: Advanced features available when needed

See our [Architectural Decision Records](Documentation/Conceptual/ArchitecturalDecisionRecords/Overview.md) for detailed design rationale.

## Documentation

- **[CLAUDE.md](CLAUDE.md)** - Complete API reference and usage guide
- **[CommandExtensions.md](Source/TimeWarp.Cli/CommandExtensions.md)** - Static API documentation
- **[CommandResult.md](Source/TimeWarp.Cli/CommandResult.md)** - Fluent interface documentation
- **[Architectural Decisions](Documentation/Conceptual/ArchitecturalDecisionRecords/Overview.md)** - Design rationale and decisions

## Example Scripts

See [Spikes/CsScripts/](Spikes/CsScripts/) for example scripts demonstrating TimeWarp.Cli usage patterns.

## Unlicense

[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-amuru.svg?style=flat-square&logo=github)](https://unlicense.org)  
This project is licensed under the [Unlicense](https://unlicense.org).

## Contributing

Your contributions are welcome! Before starting any work, please open a [discussion](https://github.com/TimeWarpEngineering/timewarp-amuru/discussions).

See our [Kanban board](Kanban/Overview.md) for current development tasks and priorities.

## Contact

If you have an issue and don't receive a timely response, feel free to reach out on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)