---
name: amuru
description: Use TimeWarp.Amuru for process execution instead of System.Diagnostics.Process
---

# Amuru Process Execution

> Agent guide for the TimeWarp.Amuru command contract (`CommandOutput`, default validation `None`). `AGENTS.md` and the library source win if this file drifts. The 2.0 release notes, including breaking changes, are in `documentation/release-notes/2.0.0.md`.

**ALWAYS use `TimeWarp.Amuru` for process execution in .NET.** Do NOT use `System.Diagnostics.Process.Start` directly.

## Package

In a runfile:

```csharp
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
```

`Shell`, `CommandOutput`, `CommandMock`, `ScriptContext`, and `CliConfiguration` are `TimeWarp.Amuru`. `DotNet`, `Git`, and `Fzf` are `TimeWarp.Amuru.Tools`. Both use the `TimeWarp.Amuru` namespace.

Or via Central Package Management in `Directory.Packages.props`:

```xml
<PackageVersion Include="TimeWarp.Amuru" Version="..." />
```

Find available versions:

```bash
dotnet package search TimeWarp.Amuru --exact-match --take 5 --prerelease
```

## Core API: Shell.Builder

All process execution starts with `Shell.Builder(executable)` and uses a fluent builder pattern.

### Execution Modes

```csharp
// RunAsync - streams output to console, returns exit code
int exitCode = await Shell.Builder("dotnet").WithArguments("build").RunAsync();

// CaptureAsync - captures output silently, returns CommandOutput
CommandOutput output = await Shell.Builder("git").WithArguments("status").CaptureAsync();

// RunAndCaptureAsync - streams to console AND captures output
CommandOutput output = await Shell.Builder("dotnet").WithArguments("test").RunAndCaptureAsync();

// PassthroughAsync - interactive stream piping (not a real TTY). Returns CommandOutput.
// Stdout/Stderr are empty because the streams go to the terminal.
CommandOutput passthrough = await Shell.Builder("fzf").PassthroughAsync();

// TtyPassthroughAsync - real TTY inheritance for TUI apps (vim, nano). Returns CommandOutput.
// Never throws on a non-zero exit, even with WithZeroExitCodeValidation.
CommandOutput tty = await Shell.Builder("vim").WithArguments("file.txt").TtyPassthroughAsync();
```

### CommandOutput Properties

```csharp
CommandOutput output = await Shell.Builder("git").WithArguments("log").CaptureAsync();

output.ExitCode      // int - process exit code (CommandResult.NeverRanExitCode / -1 if it never ran)
output.Success       // bool - true if ExitCode == 0
output.TimedOut      // bool - true when the command exceeded WithTimeout (exit 124)
output.RunTime       // TimeSpan - zero for mocks and commands that never ran
output.Stdout        // string - captured stdout (lazy, thread-safe)
output.Stderr        // string - captured stderr (lazy, thread-safe)
output.Combined      // string - interleaved stdout+stderr (lazy, thread-safe)
output.OutputLines   // IReadOnlyList<OutputLine> - timestamped lines

output.GetLines()          // string[] - combined output lines
output.GetStdoutLines()    // string[] - stdout lines only
output.GetStderrLines()    // string[] - stderr lines only
output.ToSummary()         // string - one-line summary
output.ToDetailedString()  // string - multi-line details
```

### Builder Configuration

```csharp
await Shell.Builder("myapp")
  .WithArguments("arg1", "arg2")            // Add arguments
  .WithWorkingDirectory("/path/to/dir")     // Set working directory
  .WithEnvironmentVariable("KEY", "value")  // Set env var
  .WithStandardInput("input text")          // Pipe string to stdin
  .WithNoValidation()                       // Explicit default: non-zero exit is not thrown
  .WithTimeout(TimeSpan.FromSeconds(30))    // Per-command limit. Null means no timeout
  .RunAsync(cancellationToken);             // All methods accept CancellationToken
```

### Streaming Output

```csharp
// Stream stdout lines as they arrive
await foreach (string line in Shell.Builder("tail").WithArguments("-f", "log.txt").StreamStdoutAsync())
{
  Console.WriteLine(line);
}

// Stream stderr lines
await foreach (string line in builder.StreamStderrAsync()) { }

// Stream combined (OutputLine has .Text, .IsError, .Timestamp)
await foreach (OutputLine line in builder.StreamCombinedAsync()) { }

// Stream to file
await Shell.Builder("curl").WithArguments("-s", url).StreamToFileAsync("output.json");
```

### Pipelines

```csharp
// Chain commands with .Pipe()
CommandOutput output = await Shell.Builder("find").WithArguments(".", "-name", "*.cs")
  .Pipe("grep", "async")
  .Pipe("sort")
  .CaptureAsync();

// Pipe through fzf for selection
string selected = await Shell.Builder("git").WithArguments("branch", "--list")
  .Build()
  .SelectWithFzf(fzf => fzf.WithHeader("Select branch"))
  .SelectAsync();
```

## DotNet Commands

Typed builders for `dotnet` CLI subcommands with IntelliSense-friendly options.

```csharp
// Build
await DotNet.Build("MyProject.csproj")
  .WithConfiguration("Release")
  .WithNoRestore()
  .WithProperty("WarningLevel", "0")
  .RunAsync();

// Publish
await DotNet.Publish("MyProject.csproj")
  .WithConfiguration("Release")
  .WithSelfContained()
  .WithSingleFile()
  .WithTrimmed()
  .WithRuntime("linux-x64")
  .RunAsync();

// Other subcommands: DotNet.Clean(), DotNet.Restore(), DotNet.Run(),
// DotNet.Test(), DotNet.Watch(), DotNet.Pack(), DotNet.New()

// Query dotnet info
CommandOutput sdks = await DotNet.WithListSdks().CaptureAsync();
CommandOutput version = await DotNet.WithVersion().CaptureAsync();
```

## Git Commands

High-level Git operations with typed results.

```csharp
// Find repo root. Synchronous only.
string? root = Git.FindRoot();
string repositoryPath = root ?? Directory.GetCurrentDirectory();

// Branch operations
GitBranchUpdateResult result = await Git.UpdateBranchAsync("main", repositoryPath);
// result.Success, result.BranchPath, result.ErrorMessage

GitDefaultBranchResult defaultBranch = await Git.GetDefaultBranchAsync(repositoryPath);
// defaultBranch.Success, defaultBranch.BranchName, defaultBranch.ErrorMessage

GitCommitCountResult ahead = await Git.GetCommitsAheadOfDefaultBranchAsync(repositoryPath);
// ahead.Success, ahead.Count, ahead.ErrorMessage

GitFetchResult fetched = await Git.FetchAsync(repositoryPath);
// fetched.Success, fetched.ErrorMessage

string? repoName = await Git.GetRepositoryNameAsync(repositoryPath);

// Worktree operations
bool isWorktree = Git.IsWorktree(repositoryPath);
string? worktreePath = await Git.GetWorktreePathAsync("main", repositoryPath);
GitWorktreeListResult listed = await Git.WorktreeListPorcelainAsync(repositoryPath);
// listed.Success, listed.Porcelain, listed.ErrorMessage

// For other git commands, use Shell.Builder
CommandOutput log = await Shell.Builder("git").WithArguments("log", "--oneline", "-10").CaptureAsync();
```

## Fzf (Fuzzy Finder)

Interactive selection with fzf.

```csharp
// Select from items
string selected = await Fzf.Builder()
  .FromInput("option1", "option2", "option3")
  .WithHeader("Pick one")
  .SelectAsync();

// Select from command output
string selected = await Fzf.Builder()
  .FromCommand("find . -name '*.cs'")
  .WithPreview("cat {}")
  .SelectAsync();

// Pipe any command through fzf
string file = await Shell.Builder("git").WithArguments("ls-files")
  .Build()
  .SelectWithFzf()
  .SelectAsync();
```

## ScriptContext

For runfiles, get the runfile location and manage working directory.

```csharp
using ScriptContext context = ScriptContext.FromEntryPoint(changeToScriptDirectory: true);
// context.ScriptDirectory - directory containing the script
// context.ScriptFilePath  - full path to the script file
// Dispose restores the original working directory
```

## Testing / Mocking

Mock command execution in tests without dependency injection.

```csharp
using IDisposable scope = CommandMock.Enable();

// Setup mock responses
CommandMock.Setup("git", "status")
  .Returns(stdout: "On branch main", exitCode: 0);

CommandMock.Setup("dotnet", "build")
  .ReturnsError(stderr: "Build failed", exitCode: 1);

CommandMock.Setup("slow-command")
  .Delays(TimeSpan.FromSeconds(2))
  .Returns("done");

CommandMock.Setup("hung-command").TimesOut(); // TimedOut, exit 124, same shape as a real timeout

// Execute code under test - it will use mocked responses
CommandOutput result = await Shell.Builder("git").WithArguments("status").CaptureAsync();

// Verify calls were made
CommandMock.VerifyCalled("git", "status");
int count = CommandMock.CallCount("git", "status");
```

## CLI Configuration

Override command paths (useful for testing or custom installations).

```csharp
CliConfiguration.SetCommandPath("git", "/usr/local/bin/git");
CliConfiguration.ClearCommandPath("git");
CliConfiguration.Reset();
```

## Error Handling

Default validation is `None`. A non-zero exit is reported on `CommandOutput.ExitCode` and `Success`. It is not thrown. `WithNoValidation()` states that default explicitly. `WithZeroExitCodeValidation()` opts into throwing. A command that never ran reports `CommandResult.NeverRanExitCode` (`-1`), not success.

`WithTimeout(TimeSpan)` on `ShellBuilder`, `CommandOptions`, or `DotNetBuilder` limits that command. On expiry the graceful signal is sent (SIGINT / Ctrl+C), then the process tree is killed after `WithTimeoutGracePeriod` (default 5 seconds). Default validation sets `CommandOutput.TimedOut` and exit `CommandResult.TimeoutExitCode` (124) and does not throw. Strict validation throws `TimeoutException`. Cancelling the caller's token still throws `OperationCanceledException` and does not set `TimedOut`. Windows non-console children do not receive SIGINT, so the grace period ends in the kill. See `documentation/developer/reference/command-execution.md`.

`TtyPassthroughAsync` is the exception: it never throws on a non-zero exit or a timeout, even when zero-exit validation is set. Inspect `ExitCode` / `Success` / `TimedOut`.

```csharp
CommandOutput output = await Shell.Builder("might-fail").CaptureAsync();

if (!output.Success)
{
  // output.ExitCode and output.Stderr describe the failure. Nothing was thrown.
}

CommandOutput strict = await Shell.Builder("must-succeed")
  .WithZeroExitCodeValidation()
  .CaptureAsync();
```

## Documentation

Command contract (`CommandOutput`, default validation `None`). This skill matches `AGENTS.md`. If they diverge, the library source wins:

- **Local**: Repository root (this is the source of truth for Amuru)
- **GitHub**: https://github.com/TimeWarpEngineering/timewarp-amuru
