# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TimeWarp.Amuru is a fluent API wrapper around CliWrap for elegant C# scripting. The library makes shell command execution feel natural and concise in C#, providing a simple static `Run()` method with async operations and graceful error handling.

**Target Framework:** .NET 10.0  
**Current Version:** 0.2.0  
**Package ID:** TimeWarp.Amuru

## Project Structure

- `Source/TimeWarp.Amuru/` - Main library (published as NuGet package)
  - `CommandExtensions.cs` - Static `Run()` method entry point
  - `CommandResult.cs` - Fluent result wrapper with async operations
- `Scripts/` - Build automation scripts (all use TimeWarp.Amuru itself)
- `Tests/` - Integration tests with custom test runner
- `Spikes/CsScripts/` - Example scripts demonstrating API usage
- `LocalNuGetFeed/` - Local NuGet packages for development
- `Scratch/` - Experimental workspace with relaxed code analysis rules
  - `Directory.Build.props` - Disables strict analysis for quick exploration scripts
- `Kanban/` - Task management system with the following structure:
  - `Backlog/` - Future features and ideas
  - `ToDo/` - Ready to implement tasks
  - `InProgress/` - Currently being worked on
  - `Done/` - Completed tasks
  - `Task-Template.md` - Template for creating new tasks

## Development Commands

### Build and Package Management
```bash
# Build the library (Release mode)
./Scripts/Build.cs

# Pack and publish to local feed
./Scripts/Pack.cs

# Clean all artifacts and packages
./Scripts/Clean.cs
```

### Testing
```bash
# Run all integration tests
./Tests/RunTests.cs
```

Tests are executable C# scripts that return exit codes. The test runner uses TimeWarp.Amuru itself to execute tests and report results.

**Important**: All test files include `#:property RestoreNoCache true` to ensure fresh package downloads and avoid caching issues during development.

### Local Development Workflow

1. Make changes to `Source/TimeWarp.Amuru/`
2. Run `./Scripts/Build.cs` to build
3. Run `./Scripts/Pack.cs` to publish to local feed
4. Test in scripts using `#:package TimeWarp.Amuru`
5. Run `./Tests/RunTests.cs` to verify functionality

### Creating GitHub Releases

**⚠️ IMPORTANT: NEVER create tags manually when doing releases - the GitHub release process creates the tag automatically. Creating a tag first will cause the release to FAIL.**

To create a release:
```bash
gh release create v1.0.0 --title "Release Title" --notes "Release notes" --prerelease
```

The GitHub Actions workflow will automatically:
1. Create the tag from the release
2. Build the NuGet package
3. Publish to NuGet.org

### Task Management Workflow

The project uses a Kanban board system located in the `Kanban/` directory:

1. **Creating Tasks**: Use `Task-Template.md` as a starting point
   - Tasks are numbered (e.g., `003_Implement-DotNet-Fluent-API.md`)
   - Place new tasks in appropriate folder (usually `ToDo/`)
   - Include description, requirements, and relevant checklist items

2. **Task Progression**:
   - `Backlog/` → `ToDo/` → `InProgress/` → `Done/`
   - Move task files between folders as work progresses
   - Update task content with implementation notes as needed

3. **Task Naming Convention**:
   - Format: `NNN_Brief-Description.md` (e.g., `003_Implement-DotNet-Fluent-API.md`)
   - Use sequential numbering for ordering
   - Use kebab-case for descriptions

## API Design

### Core API
```csharp
// Entry point - static Run method
public static CommandResult Run(string executable, params string[] arguments)

// CommandResult methods (all async)
public async Task<string> GetStringAsync()      // Get full output as string
public async Task<string[]> GetLinesAsync()     // Get output as line array
public async Task ExecuteAsync()                // Execute without capturing output
public CommandResult Pipe(string executable, params string[] arguments)  // Chain commands

// Interactive methods (NEW in v0.6.0)
public async Task<string> GetStringInteractiveAsync()    // Interactive UI with captured output
public async Task<ExecutionResult> ExecuteInteractiveAsync()  // Full interactive mode
```

### Usage Examples
```csharp
#!/usr/bin/dotnet run
#:package TimeWarp.Amuru

// Get command output
var date = await Run("date").GetStringAsync();

// Process lines
var files = await Run("find", ".", "-name", "*.cs").GetLinesAsync();
foreach (var file in files) Console.WriteLine(file);

// Execute without output
await Run("echo", "Hello World").ExecuteAsync();

// Pipeline commands (NEW in v0.2.0)
var filteredFiles = await Run("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .GetLinesAsync();

// Multi-stage pipelines
var count = await Run("git", "log", "--oneline", "-n", "10")
    .Pipe("head", "-5")
    .Pipe("wc", "-l")
    .GetStringAsync();

// Fluent builder API (NEW in v0.6.0)
var result = await Shell.Run("git")
    .WithArguments("log", "--oneline", "-n", "10")
    .WithWorkingDirectory("/my/repo")
    .WithEnvironmentVariable("GIT_PAGER", "cat")
    .WithNoValidation()
    .GetStringAsync();

// Complex command building
var output = await Shell.Run("docker")
    .WithArguments("run", "--rm")
    .WithArguments("-e", "NODE_ENV=production")
    .WithArguments("-v", "/host/path:/container/path")
    .WithArguments("node:latest", "npm", "test")
    .ExecuteAsync();

// Provide standard input to commands
var grepResult = await Shell.Run("grep")
    .WithArguments("pattern")
    .WithStandardInput("line1\nline2 with pattern\nline3")
    .GetStringAsync();

// Combine standard input with pipelines
var sortedTop3 = await Shell.Run("sort")
    .WithStandardInput("banana\napple\ncherry\ndate")
    .Pipe("head", "-3")
    .GetLinesAsync();

// Interactive command execution (NEW in v0.6.0)
// Select a file with FZF and capture the selection
var selectedFile = await Fzf.Builder()
    .FromInput("file1.txt", "file2.txt", "file3.txt")
    .WithPreview("cat {}")
    .GetStringInteractiveAsync();

// Interactive pipeline - find and select files
var chosenCsFile = await Shell.Run("find")
    .WithArguments(".", "-name", "*.cs")
    .Pipe("fzf", "--preview", "head -20 {}")
    .GetStringInteractiveAsync();

// Full interactive mode for editors, REPLs, etc.
await Shell.Run("vim")
    .WithArguments("config.json")
    .ExecuteInteractiveAsync();
```

### Error Handling
- **Default behavior**: Commands throw `CommandExecutionException` on non-zero exit codes
- Use `.WithValidation(CommandResultValidation.None)` to disable exception throwing:
  ```csharp
  // Disable validation for graceful degradation
  var options = new CommandOptions().WithValidation(CommandResultValidation.None);
  await Run("git", new[] { "status" }, options).ExecuteAsync();
  
  // Or with builders
  await DotNet.Build()
    .WithValidation(CommandResultValidation.None)
    .ExecuteAsync();
  ```
- When validation is disabled with `CommandResultValidation.None`:
  - `GetStringAsync()` returns empty string on failure
  - `GetLinesAsync()` returns empty array on failure
  - `ExecuteAsync()` completes without throwing
  - Pipeline commands maintain graceful degradation

### Testing and Mocking (NEW in v0.6.0)
The library provides `CliConfiguration` for mocking commands during testing:

```csharp
// Set up mock commands
CliConfiguration.SetCommandPath("fzf", "/path/to/mock/fzf");
CliConfiguration.SetCommandPath("git", "/path/to/mock/git");

// Commands now use the mocks
var result = await Fzf.Builder()
    .FromInput("option1", "option2")
    .GetStringAsync(); // Uses mock fzf

// Clean up
CliConfiguration.Reset();
```

API Methods:
- `CliConfiguration.SetCommandPath(command, path)` - Override command executable
- `CliConfiguration.ClearCommandPath(command)` - Remove override for specific command
- `CliConfiguration.Reset()` - Clear all overrides
- `CliConfiguration.HasCustomPath(command)` - Check if override exists
- `CliConfiguration.AllCommandPaths` - Get all current overrides

## Key Architecture Decisions

1. **Minimal API Surface**: Only two classes exposed (CommandExtensions, CommandResult)
2. **Static Entry Point**: Global `Run()` method for simplicity in scripts
3. **Async-First**: All operations are async for non-blocking execution
4. **No Exceptions**: Failed commands return empty results
5. **Pipeline Support**: Commands can be chained with `.Pipe()` for shell-like operations
6. **Dogfooding**: Test runner uses TimeWarp.Amuru itself for execution

## Build System Architecture

**CRITICAL**: The build scripts (`Scripts/Build.cs`, `Scripts/Pack.cs`, `Scripts/Clean.cs`) deliberately use raw `System.Diagnostics.Process` instead of TimeWarp.Amuru to avoid circular dependencies. This ensures:

- Build scripts remain self-contained and stable
- No chicken-and-egg problems when the library has issues
- Build can always succeed even if TimeWarp.Amuru is broken
- Once the library is stable, other scripts can dogfood it safely

**Rule**: Never make build scripts depend on the library they're building.

## Dependencies

- **CliWrap 3.9.0**: Core command execution and piping functionality
- No other external dependencies

## Script Execution Model

All scripts use shebang lines for direct execution:
```csharp
#!/usr/bin/dotnet run                          // Basic script
#!/usr/bin/dotnet run --package CliWrap        // With external package
#:package TimeWarp.Amuru                         // Reference local library
```

Scripts must have execute permissions (`chmod +x script.cs`).

### Script Directory Management Pattern

All scripts use a consistent push/pop directory pattern using `[CallerFilePath]`:

```csharp
public static async Task<int> Main(string[] args, [CallerFilePath] string scriptPath = "")
{
  string originalDirectory = Environment.CurrentDirectory;
  string scriptDirectory = Path.GetDirectoryName(scriptPath)!;
  
  try
  {
    Environment.CurrentDirectory = scriptDirectory;
    
    // Use relative paths from script location
    // e.g., "../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj"
    
    return 0;
  }
  finally
  {
    Environment.CurrentDirectory = originalDirectory;
  }
}
```

This pattern ensures:
- Scripts work when executed from any directory location
- Relative paths are always resolved from the script's location (not current directory)
- Original working directory is restored for the caller
- Equivalent to PowerShell's `$PSScriptRoot` pattern but for C#

**Important Note on ImplicitUsings**: C# script files automatically include most common namespaces via ImplicitUsings, but `System.Diagnostics` is NOT included. Scripts that use `Process` or `ProcessStartInfo` must explicitly include:
```csharp
#pragma warning disable IDE0005 // Using directive is unnecessary
using System.Diagnostics;
#pragma warning restore IDE0005
```
The pragma warnings suppress false IDE warnings about the using directive being unnecessary.

### Local Package Cache Management for Development

**Problem**: During development, C# scripts cache NuGet packages globally, making it difficult to test changes without constantly bumping version numbers.

**Solution**: Use `RestorePackagesPath` property to create local package caches per script/test directory.

**Integration Test Script Headers Should Include**:
```csharp
#!/usr/bin/dotnet run
#:package TimeWarp.Amuru@*-*
#:property RestoreNoCache true
#:property DisableImplicitNuGetFallbackFolder true
```

**Development Workflow**:
1. Make code changes to library
2. Run `./Scripts/Build.cs` and `./Scripts/Pack.cs`
3. Clear only our package cache: `rm -rf LocalNuGetCache/timewarp.amuru`
4. Run tests: `./Tests/RunTests.cs`

**Benefits**:
- ✅ Keeps other packages (CliWrap, etc.) cached for faster restores
- ✅ Only clears our specific package when needed
- ✅ Avoids version number pollution
- ✅ Gives precise control over package caching per script

**Note**: RunTests.cs itself cannot clear the cache because it depends on TimeWarp.Amuru package (chicken-and-egg problem). Cache clearing must be done manually or via separate shell script.

## NuGet Configuration

The repository includes `nuget.config` with two sources:
1. Official NuGet.org feed
2. Local feed at `./LocalNuGetFeed/`

This enables rapid development iteration with local packages.

## C# Coding Standards

This project follows specific C# coding standards defined in `.ai/04-csharp-coding-standards.md`:

### Formatting
- **Indentation**: 2 spaces (no tabs)
- **Line endings**: LF
- **Bracket style**: Allman style - all brackets on their own line
  ```csharp
  public void Method
  (
    string param1,
    string param2
  )
  {
    // implementation
  }
  ```

### Naming Conventions
- **Private fields**: No underscore prefix (`private readonly HttpClient httpClient;`)
- **Class scope**: PascalCase for all members (fields, properties, methods, events)
- **Method scope**: camelCase for parameters and local variables

### Language Features
- **Type declarations**: Use `var` only when type is apparent from right side
- **New operator**: Use targeted type new (`HttpClient client = new();`)
- **Namespaces**: Use file-scoped namespaces (`namespace TimeWarp.Amuru;`)
- **Using statements**: Prefer global usings in GlobalUsings.cs

### Example Following Standards
```csharp
namespace TimeWarp.Amuru;

public class CommandResult
{
  private readonly Command Command;
  
  public CommandResult(Command command)
  {
    Command = command;
  }
  
  public async Task<string> GetStringAsync()
  {
    StringBuilder output = new();
    await foreach (string line in Command.ListenAsync())
    {
      output.AppendLine(line);
    }
    return output.ToString();
  }
}
```