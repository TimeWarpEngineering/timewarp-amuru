[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/timewarp-amuru?logo=github)](https://github.com/TimeWarpEngineering/timewarp-amuru)
[![workflow](https://github.com/TimeWarpEngineering/timewarp-amuru/actions/workflows/workflow.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-amuru/actions)
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

**TimeWarp.Amuru** is a fluent API library for elegant command-line execution in C#. It transforms shell scripting into a type-safe, IntelliSense-friendly experience with a simple static `Builder()` method, async operations, and shell-like error handling.

Designed for modern C# developers, TimeWarp.Amuru brings the power of shell scripting directly into your C# code. Whether you're building automation tools, DevOps scripts, or integrating command-line tools into your applications, TimeWarp.Amuru provides the elegant, type-safe API you need.

## Why TimeWarp.Amuru?

- **Zero Learning Curve**: If you know C#, you already know how to use TimeWarp.Amuru
- **IntelliSense Everything**: Full IDE support with autocomplete, parameter hints, and documentation
- **Type Safety**: Catch errors at compile-time, not runtime
- **No String Escaping Hell**: Use C# arrays and parameters naturally
- **Native AOT Ready**: Both packages declare and validate AOT/trimming compatibility
- **Built for .NET 10**: Modern C# features and first-class file-based app (runfile) support
- **Script or Library**: Use it in quick scripts or production applications

## Give a Star! :star:

If you find this project useful, please give it a star. Thanks!

## Installation

```bash
# Core library: process execution, mocking, native file operations
dotnet add package TimeWarp.Amuru

# Optional: fluent builders for dotnet/git/fzf plus repo services
dotnet add package TimeWarp.Amuru.Tools --prerelease
```

Or reference in your C# runfile:
```csharp
#:package TimeWarp.Amuru@<latest-version>
```

Both packages share the `TimeWarp.Amuru` namespace — adding the Tools reference lights up `DotNet.*`, `Git.*`, and `Fzf.*` with no code changes.

### Optional: CLI Tools

```bash
# Global CLI tool with additional utilities (private package)
dotnet tool install --global TimeWarp.Ganda --source https://nuget.pkg.github.com/TimeWarpEngineering/index.json
```

See the [Ganda repository](https://github.com/TimeWarpEngineering/timewarp-ganda) for details.

## Quick Start

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru

using TimeWarp.Amuru;
using static System.Console;

// Default behavior - stream to console (like bash/PowerShell)
await Shell.Builder("npm").WithArguments("install").RunAsync();

// Capture output when needed
CommandOutput result = await Shell.Builder("git").WithArguments("status").CaptureAsync();
if (result.Success)
{
    WriteLine($"Git says: {result.Stdout}");
}

// Stream large files without memory issues
await foreach (string line in Shell.Builder("tail").WithArguments("-f", "/var/log/app.log").StreamStdoutAsync())
{
    WriteLine($"Log: {line}");
}

// Chain commands with pipelines
CommandOutput found = await Shell.Builder("find")
    .WithArguments(".", "-name", "*.cs")
    .Pipe("grep", "async")
    .CaptureAsync();
WriteLine($"Found {found.GetLines().Length} async lines");

// Work with CommandOutput
CommandOutput output = await Shell.Builder("docker").WithArguments("ps").CaptureAsync();
WriteLine($"Exit code: {output.ExitCode}");
WriteLine($"Success: {output.Success}");
WriteLine($"Stdout: {output.Stdout}");
WriteLine($"Stderr: {output.Stderr}");
WriteLine($"Combined: {output.Combined}");

// Use the fluent builder API for complex commands
CommandOutput log = await Shell.Builder("git")
    .WithArguments("log", "--oneline", "-n", "10")
    .WithWorkingDirectory("/my/repo")
    .CaptureAsync(cancellationToken);

// Provide standard input to commands
CommandOutput grepResult = await Shell.Builder("grep")
    .WithArguments("pattern")
    .WithStandardInput("line1\nline2 with pattern\nline3")
    .CaptureAsync();

// Full interactive mode for stream-based tools (fzf, REPLs)
await Shell.Builder("fzf").PassthroughAsync();

// TUI applications (vim, nano, edit) need true TTY passthrough
await Shell.Builder("vim")
    .WithArguments("myfile.txt")
    .TtyPassthroughAsync();
```

### Tool Builders (TimeWarp.Amuru.Tools)

```csharp
// Global dotnet options
CommandOutput sdks = await DotNet.WithListSdks().CaptureAsync();
CommandOutput version = await DotNet.WithVersion().CaptureAsync();

// Base builder for custom arguments
CommandOutput custom = await DotNet.Builder()
    .WithArguments("--list-runtimes")
    .CaptureAsync();

// Build and test with streaming output
await DotNet.Build()
    .WithConfiguration("Release")
    .RunAsync();

await DotNet.Test()
    .WithFilter("Category=Unit")
    .RunAsync();

// Git operations with typed results
string? repoRoot = Git.FindRoot();
string porcelain = await Git.WorktreeListPorcelainAsync("/my/repo");
IReadOnlyList<WorktreeEntry> worktrees = Git.ParseWorktreeList(porcelain);

// Interactive selection with Fzf
string selectedFile = await Fzf.Builder()
    .FromInput("file1.txt", "file2.txt", "file3.txt")
    .WithPreview("cat {}")
    .SelectAsync();
```

## Key Features

- **Shell-Like Default**: `RunAsync()` streams to console just like bash/PowerShell
- **Explicit Capture**: `CaptureAsync()` for when you need to process output
- **Memory-Efficient Streaming**: `IAsyncEnumerable` for large data without buffering
- **One Result Type**: `CommandOutput` with Stdout, Stderr, Combined, ExitCode, Success, and RunTime — from every execution mode
- **Shell-Like Error Handling**: non-zero exit codes are values, not exceptions; strict validation is one opt-in call away
- **Built-In Command Mocking**: `CommandMock` with strict-by-default matching — tests can never silently run real commands
- **Pipeline Support**: Chain commands with Unix-like pipe semantics
- **Standard Input Support**: Provide stdin to commands with `.WithStandardInput()`
- **NO CACHING Philosophy**: Like shells, commands run fresh every time
- **Cancellation Support**: Full CancellationToken support throughout
- **Cross-Platform**: Works on Windows, Linux, and macOS (including `.cs` script execution on Windows via the dotnet host)
- **Interactive Commands**: `PassthroughAsync()` for stream-based tools, `TtyPassthroughAsync()` for TUI apps (vim, nano), `SelectAsync()` for selection tools
- **.NET 10 Script Support**: AppContext extensions and ScriptContext for file-based apps

## Output Handling

### Core API Methods

```csharp
// RunAsync() - Default shell behavior, streams to console
await Shell.Builder("npm").WithArguments("install").RunAsync();
// Returns: exit code (int)

// CaptureAsync() - Silent execution with full output capture
CommandOutput result = await Shell.Builder("git").WithArguments("status").CaptureAsync();
// Returns: CommandOutput; no console output

// RunAndCaptureAsync() - Stream to console AND capture
CommandOutput logged = await Shell.Builder("dotnet").WithArguments("build").RunAndCaptureAsync();

// PassthroughAsync() - Stream-based interactive tools (fzf, REPLs)
CommandOutput fzfResult = await Shell.Builder("fzf").PassthroughAsync();

// TtyPassthroughAsync() - True TTY for TUI applications (vim, nano, edit)
CommandOutput vimResult = await Shell.Builder("vim").WithArguments("file.txt").TtyPassthroughAsync();

// SelectAsync() - Selection tools (shows UI on stderr, captures stdout selection)
string selected = await Shell.Builder("fzf").SelectAsync();
```

### The CommandOutput Type

```csharp
CommandOutput output = await Shell.Builder("docker").WithArguments("ps").CaptureAsync();

// Access individual streams
Console.WriteLine($"Stdout: {output.Stdout}");
Console.WriteLine($"Stderr: {output.Stderr}");
Console.WriteLine($"Combined: {output.Combined}"); // Captured in arrival order

// Check status
Console.WriteLine($"Exit code: {output.ExitCode}");
Console.WriteLine($"Success: {output.Success}"); // ExitCode == 0
Console.WriteLine($"Runtime: {output.RunTime}");

// Line processing (interior blank lines preserved; no trailing empty entry)
foreach (string line in output.GetLines())
{
    ProcessLine(line);
}

// Or line-level access with source-stream metadata
foreach (OutputLine line in output.OutputLines)
{
    Console.WriteLine($"{(line.IsError ? "ERR" : "OUT")}: {line.Text}");
}

// Pretty-print a result with status coloring
output.WriteToConsole();
```

### Streaming Large Data

```csharp
// Stream lines as they arrive (no buffering)
await foreach (string line in Shell.Builder("tail")
    .WithArguments("-f", "/var/log/app.log")
    .StreamStdoutAsync(cancellationToken))
{
    Console.WriteLine($"Log: {line}");
}
```

### Method Comparison

| Method | Console Output | Captures | Returns | Primary Use Case |
|--------|---------------|----------|---------|------------------|
| `RunAsync()` | ✅ Real-time | ❌ | Exit code | Default scripting |
| `CaptureAsync()` | ❌ Silent | ✅ All streams | CommandOutput | Process output |
| `RunAndCaptureAsync()` | ✅ Real-time | ✅ All streams | CommandOutput | Logging + capture |
| `PassthroughAsync()` | ✅ Piped | ❌ | CommandOutput | Stream-based interactive |
| `TtyPassthroughAsync()` | ✅ TTY | ❌ | CommandOutput | TUI apps (vim, nano) |
| `SelectAsync()` | ✅ UI only | ✅ Selection | string | Selection tools |
| `StreamStdoutAsync()` | ❌ | ✅ As stream | IAsyncEnumerable | Large data |

### Design Philosophy: NO CACHING

TimeWarp.Amuru intentionally does NOT cache command results:

```csharp
// Shells don't cache - neither do we
await Shell.Builder("date").RunAsync();  // Shows current time
await Shell.Builder("date").RunAsync();  // Shows NEW current time

// If you need caching, it's trivial in C#:
private static CommandOutput? cachedResult;
CommandOutput result = cachedResult ??= await Shell.Builder("expensive-command").CaptureAsync();
```

## Error Handling

TimeWarp.Amuru handles failure the way shells do: **a non-zero exit code is a value you inspect, not an exception**.

### Default Behavior (Never Throws on Exit Codes)
```csharp
CommandOutput result = await Shell.Builder("ls").WithArguments("/nonexistent").CaptureAsync();

if (!result.Success)
{
    Console.WriteLine($"Command failed with exit code: {result.ExitCode}");
    Console.WriteLine($"Error: {result.Stderr}");
}
```

### Strict Validation (Opt-in Throwing)
```csharp
// Throws on any non-zero exit code
await Shell.Builder("git")
    .WithArguments("push")
    .WithZeroExitCodeValidation()
    .RunAsync();
```

### Commands That Never Ran
An empty/invalid command or a failed pipeline composition never throws, but it is never mistaken for success either — it reports `CommandResult.NeverRanExitCode` (-1):

```csharp
CommandOutput result = await Shell.Builder("").CaptureAsync();
// result.Success == false, result.ExitCode == CommandResult.NeverRanExitCode
```

Note: a **missing executable** (e.g. a typo'd command name) still throws at execution time — that is an environment error, not an exit code.

### Cancellation and Timeouts
```csharp
// With explicit cancellation token
using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
await Shell.Builder("long-running-command").RunAsync(cts.Token);
```

## Testing and Mocking

### CommandMock (Recommended)

`CommandMock` intercepts command execution in-process — no mock executables needed. It is **strict by default**: a command with no matching setup throws instead of silently running the real thing.

```csharp
using TimeWarp.Amuru.Testing;

using (CommandMock.Enable())
{
    CommandMock.Setup("git", "status")
        .Returns("On branch main\nnothing to commit");

    CommandOutput output = await Shell.Builder("git").WithArguments("status").CaptureAsync();
    // output.Stdout == "On branch main\nnothing to commit" — no real git ran

    CommandMock.VerifyCalled("git", "status");
}

// Simulate failures and exceptions
using (CommandMock.Enable())
{
    CommandMock.Setup("git", "push").ReturnsError("remote: Permission denied", 128);
    CommandMock.Setup("flaky-tool").Throws(new TimeoutException("simulated"));
    CommandMock.Setup("slow-tool").Delays(TimeSpan.FromSeconds(2));
    // ...
}

// Mixed mocked + real commands (opt out of strict mode)
using (CommandMock.Enable(MockBehavior.Loose))
{
    CommandMock.Setup("deploy-tool").Returns("deployed");
    // unmocked commands fall through to real execution
}
```

Mocking is scoped per async context (parallel tests stay isolated) and covers every execution mode — run, capture, streaming, select, and passthrough. `Pipe` compositions are the documented exception (use loose mode).

### CliConfiguration (Path Overrides)

For cases where you want a real replacement executable, redirect a command process-wide:

```csharp
CliConfiguration.SetCommandPath("fzf", "/tmp/mock-bin/fzf");
// ... code using fzf now runs the replacement ...
CliConfiguration.Reset();
```

API: `SetCommandPath`, `ClearCommandPath`, `Reset`, `HasCustomPath`, `AllCommandPaths`. Overrides are process-global by design — use `CommandMock` for per-test isolation.

## .NET 10 File-Based App Support

TimeWarp.Amuru provides specialized support for .NET 10's file-based apps (single-file C# scripts):

- **AppContext Extensions** — `AppContext.EntryPointFilePath()` / `EntryPointFileDirectoryPath()` without magic strings
- **ScriptContext** — scoped working-directory management with cleanup on dispose or process exit (contexts nest safely)
- **`.cs` as a command** — `Shell.Builder("script.cs")` runs another runfile (shebang on Unix, dotnet host on Windows)

📖 **[See the documentation](documentation/developer/how-to-guides/)** for detailed usage guides and examples.

## Architecture

- **Static Entry Point**: Minimal ceremony with `Shell.Builder()` / `Shell.Run()`
- **Two Packages, One Namespace**: `TimeWarp.Amuru` (stable core) and `TimeWarp.Amuru.Tools` (tool builders, own release cadence)
- **Shell Semantics**: exit codes are values; composition never throws; nothing is cached
- **Predictable Error Handling**: never-ran, non-zero-exit, and environment failures are all distinguishable
- **Opt-in Complexity**: Advanced features available when needed

See our [Architectural Decision Records](documentation/conceptual/architectural-decision-records/overview.md) for detailed design rationale.

## Documentation

- **[Documentation overview](documentation/overview.md)** - Entry point to conceptual, developer, and user docs
- **[Migration Guide](analysis/migration-guide.md)** - Guide for migrating from older versions
- **[command-extensions.cs](source/timewarp-amuru/core/command-extensions.cs)** - Collocated command construction design documentation
- **[command-result.cs](source/timewarp-amuru/core/command-result.cs)** - Collocated command execution design documentation
- **[Samples](samples/)** - Compiling example scripts referenced against the live source

## Unlicense

[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-amuru.svg?style=flat-square&logo=github)](https://unlicense.org)  
This project is licensed under the [Unlicense](https://unlicense.org).

## Related Packages

- **[TimeWarp.Amuru.Tools](https://www.nuget.org/packages/TimeWarp.Amuru.Tools/)** - Fluent dotnet/git/fzf builders and repo services on top of this library
- **[TimeWarp.Multiavatar](https://www.nuget.org/packages/TimeWarp.Multiavatar/)** - Avatar generation library ([repository](https://github.com/TimeWarpEngineering/timewarp-multiavatar))
- **[TimeWarp.Ganda](https://github.com/TimeWarpEngineering/timewarp-ganda)** - Shell toolkit CLI (private, separate repository)

## Contributing

Your contributions are welcome! Before starting any work, please open a [discussion](https://github.com/TimeWarpEngineering/timewarp-amuru/discussions).

See our [Kanban board](kanban/overview.md) for current development tasks and priorities.

## Contact

If you have an issue and don't receive a timely response, feel free to reach out on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
