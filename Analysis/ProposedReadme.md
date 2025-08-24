# TimeWarp.Amuru

A fluent API library that brings the simplicity of shell scripting to C#. Execute commands naturally with clear, intuitive methods that do exactly what their names suggest.

## Why TimeWarp.Amuru?

When you write a shell script, running a command is simple - you just run it and see the output. But in C#, command execution has traditionally been complex, verbose, and unclear about what happens to output. TimeWarp.Amuru fixes this.

```csharp
// Just run a command and see output - like in bash/PowerShell!
await Shell.Builder("npm", "install").RunAsync();

// Need to process the output? Capture it silently
var result = await Shell.Builder("git", "status").CaptureAsync();
if (result.Stderr.Contains("not a git repository"))
{
    Console.WriteLine("Not in a git repo!");
}
```

## Installation

```bash
dotnet add package TimeWarp.Amuru --prerelease
```

Or in .NET 10 single-file apps:
```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru@1.0.0-beta.4

using TimeWarp.Amuru;

// Your single-file app here
await Shell.Builder("echo", "Hello from .NET 10!").RunAsync();
```

## Core Concepts

TimeWarp.Amuru provides five primary methods that cover 99% of command execution needs:

### 1. RunAsync() - Just Run It (Default Scripting)
**Shows output in real-time, returns exit code**

This is what you want 80% of the time - just run a command and see what happens:

```csharp
// See output as it happens, just like in a terminal
int exitCode = await Shell.Builder("docker", "build", ".").RunAsync();
if (exitCode != 0)
{
    Console.WriteLine("Build failed!");
}

// Great for long-running commands where you want to see progress
await Shell.Builder("npm", "install").RunAsync();
await Shell.Builder("dotnet", "test").RunAsync();
```

### 2. CaptureAsync() - Silent Execution
**Runs silently, returns all output for processing**

When you need to process command output:

```csharp
// Get complete output with stdout, stderr, and exit code
var result = await Shell.Builder("git", "log", "--oneline", "-5").CaptureAsync();

Console.WriteLine($"Exit code: {result.ExitCode}");
Console.WriteLine($"Stdout: {result.Stdout}");
Console.WriteLine($"Stderr: {result.Stderr}");
Console.WriteLine($"Combined: {result.Combined}"); // Both in order produced

// Process output line by line (small outputs)
var files = await Shell.Builder("find", ".", "-name", "*.cs").CaptureAsync();
foreach (var line in files.Lines)
{
    Console.WriteLine($"Found: {line}");
}

// Parse JSON output
var awsResult = await Shell.Builder("aws", "s3", "ls", "--output", "json").CaptureAsync();
var json = JsonSerializer.Deserialize<S3Response>(awsResult.Stdout);
```

### 2b. StreamAsync() - Process Large Outputs Without Buffering
**Streams output line-by-line without loading into memory**

For commands with massive output (logs, database dumps, large finds):

```csharp
// Process each line as it arrives - no memory buildup!
await foreach (var line in Shell.Builder("docker", "logs", "container").StreamAsync())
{
    if (line.IsError)
        await LogError(line.Text);
    else
        await ProcessLogLine(line.Text);
}

// Or use a callback approach
await Shell.Builder("find", "/", "-name", "*.log")
    .StreamAsync(line => {
        Console.WriteLine($"Found: {line}");
        // Process immediately, no buffering
    });

// Stream directly to file, bypassing memory entirely
await Shell.Builder("pg_dump", "database")
    .StreamToFileAsync("backup.sql");

// Stream with filtering - only process what you need
await foreach (var error in Shell.Builder("npm", "test")
    .StreamAsync()
    .Where(line => line.IsError))
{
    await AlertTeam(error.Text);
}
```

### 3. RunAndCaptureAsync() - See AND Log
**Shows output in real-time AND captures it**

Perfect for CI/CD scenarios where you want to see progress but also need to save logs:

```csharp
// See output while running AND get it back for logging
var result = await Shell.Builder("./deploy.sh").RunAndCaptureAsync();

// Save to log file
await File.WriteAllTextAsync("deploy.log", result.Combined);

// Check for errors
if (!result.Success)
{
    await SendAlert($"Deploy failed: {result.Stderr}");
}
```

### 4. PassthroughAsync() - Interactive Tools
**Full terminal control for editors, REPLs, etc.**

```csharp
// Launch interactive tools that need full terminal control
await Shell.Builder("vim", "config.json").PassthroughAsync();
await Shell.Builder("python").PassthroughAsync();  // Python REPL
await Shell.Builder("ssh", "server.example.com").PassthroughAsync();
```

### 5. SelectAsync() - Selection UIs
**For tools like fzf that show UI and return selection**

```csharp
// Show selection UI, get user's choice
var selected = await Fzf.Builder()
    .FromInput("option1", "option2", "option3")
    .SelectAsync();
Console.WriteLine($"You selected: {selected}");

// Find and select a file
var file = await Shell.Builder("find", ".", "-type", "f")
    .Pipe("fzf", "--preview", "cat {}")
    .SelectAsync();
```

## Command Output Structure

The `CommandOutput` type returned by `CaptureAsync()` and `RunAndCaptureAsync()` provides:

```csharp
public class CommandOutput
{
    public string Stdout { get; }      // Standard output only
    public string Stderr { get; }      // Standard error only  
    public string Combined { get; }    // Both streams in order produced (optional)
    public int ExitCode { get; }       // Process exit code
    public bool Success { get; }       // True if ExitCode == 0
    
    // Convenience properties (computed on demand)
    public string[] Lines { get; }        // Combined.Split('\n')
    public string[] StdoutLines { get; }  // Stdout.Split('\n')
    public string[] StderrLines { get; }  // Stderr.Split('\n')
}

// For streaming scenarios
public struct CommandLine
{
    public string Text { get; }       // The line content
    public bool IsError { get; }      // True if from stderr
    public int LineNumber { get; }    // Line number in stream
}
```

**Memory Considerations:**
- Use `CaptureAsync()` for small/medium outputs (config files, short logs)
- Use `StreamAsync()` for large/unbounded outputs (continuous logs, database dumps)
- Use `.WithCombinedOutput(false)` to skip Combined property and save memory
- Use `StreamToFileAsync()` to bypass memory entirely for huge outputs

## Fluent Builder Features

### Arguments and Configuration

```csharp
var result = await Shell.Builder("git")
    .WithArguments("commit", "-m", "Initial commit")
    .WithWorkingDirectory("/path/to/repo")
    .WithEnvironmentVariable("GIT_AUTHOR_NAME", "CI Bot")
    .WithTimeout(TimeSpan.FromMinutes(5))
    .CaptureAsync();
```

### Piping Commands

Chain commands together with Unix-style pipes:

```csharp
// Simple pipe
var count = await Shell.Builder("ls", "-la")
    .Pipe("wc", "-l")
    .CaptureAsync();

// Multi-stage pipeline
var result = await Shell.Builder("cat", "log.txt")
    .Pipe("grep", "ERROR")
    .Pipe("head", "-10")
    .RunAsync();  // Shows top 10 errors
```

### Standard Input

Provide input to commands:

```csharp
// Provide string input
var result = await Shell.Builder("grep", "pattern")
    .WithStandardInput("line1\nline2 with pattern\nline3")
    .CaptureAsync();

// Pipe from file
var sorted = await Shell.Builder("sort")
    .WithStandardInputFile("unsorted.txt")
    .CaptureAsync();
```

### Caching Expensive Operations

Cache command results to avoid re-running expensive operations:

```csharp
// Create cached command
var findCommand = Shell.Builder("find", "/", "-name", "*.log").Cached();

// First call executes command
var logs1 = await findCommand.CaptureAsync();

// Subsequent calls return cached result instantly
var logs2 = await findCommand.CaptureAsync();  // No execution!
```

## Specialized Command Builders

### DotNet Commands

```csharp
// Fluent API for dotnet CLI
await DotNet.Build()
    .WithConfiguration("Release")
    .WithNoRestore()
    .RunAsync();

await DotNet.Test()
    .WithFilter("Category=Unit")
    .WithLogger("trx")
    .RunAsync();

// Global dotnet options
var sdks = await DotNet.WithListSdks().CaptureAsync();
var version = await DotNet.WithVersion().CaptureAsync();
```

### Fzf (Fuzzy Finder)

```csharp
// Interactive file selection
var file = await Fzf.Builder()
    .FromFiles("*.cs")
    .WithPreview("cat {}")
    .SelectAsync();

// Multi-select with custom options
var selections = await Fzf.Builder()
    .FromInput("Red", "Green", "Blue", "Yellow")
    .WithMulti()
    .WithPrompt("Choose colors: ")
    .SelectAsync();  // Returns newline-separated selections
```

## Error Handling

By default, commands throw on non-zero exit codes:

```csharp
try 
{
    await Shell.Builder("false").RunAsync();
}
catch (CommandExecutionException ex)
{
    Console.WriteLine($"Command failed with exit code: {ex.ExitCode}");
}
```

Disable exceptions for graceful handling:

```csharp
var result = await Shell.Builder("git", "status")
    .WithValidation(CommandResultValidation.None)
    .CaptureAsync();

if (!result.Success)
{
    Console.WriteLine("Not a git repository");
}
```

## Common Patterns

### Running Shell Scripts and .NET Single-File Apps

```csharp
// Shell scripts
await Shell.Builder("./build.sh").RunAsync();

// Python scripts  
await Shell.Builder("python", "analyze.py", "--verbose").RunAsync();

// Other .NET 10 single-file apps (these are compiled executables!)
await Shell.Builder("./other-app.cs", "--input", "data.json").RunAsync();

// You can even run AOT-compiled .NET apps
await Shell.Builder("./my-tool").RunAsync();
```

### CI/CD Pipelines

```csharp
// Run build steps with visibility
await Shell.Builder("npm", "ci").RunAsync();
await Shell.Builder("npm", "run", "build").RunAsync();
await Shell.Builder("npm", "test").RunAsync();

// Capture test results for analysis
var testResult = await Shell.Builder("dotnet", "test", "--logger", "trx").RunAndCaptureAsync();
File.WriteAllText("test-output.txt", testResult.Combined);

if (!testResult.Success)
{
    throw new Exception($"Tests failed:\n{testResult.Stderr}");
}
```

### Processing Command Output

```csharp
// Find all TODO comments
var todos = await Shell.Builder("grep", "-r", "TODO", ".", "--include=*.cs").CaptureAsync();
foreach (var todo in todos.Lines)
{
    Console.WriteLine($"Found: {todo}");
}

// Parse JSON from AWS CLI
var instances = await Shell.Builder("aws", "ec2", "describe-instances", "--output", "json")
    .CaptureAsync();
var json = JsonSerializer.Deserialize<Ec2Response>(instances.Stdout);
```

### Interactive Workflows

```csharp
// Let user select a git branch to checkout
var branch = await Shell.Builder("git", "branch", "-a")
    .Pipe("fzf", "--preview", "git log --oneline {}")
    .SelectAsync();

await Shell.Builder("git", "checkout", branch.Trim()).RunAsync();
```

## .NET 10 Single-File App Support

TimeWarp.Amuru is designed to work seamlessly with .NET 10's single-file apps (not scripts - these are real compiled applications!):

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru@1.0.0-beta.4

using TimeWarp.Amuru;

// This is a full .NET application that gets compiled!
// You can use all .NET features, reference packages, even publish as AOT

var files = await Shell.Builder("find", ".", "-name", "*.json").CaptureAsync();
foreach (var file in files.Lines)
{
    var content = await File.ReadAllTextAsync(file);
    var data = JsonSerializer.Deserialize<MyData>(content);
    // Process your data...
}

// These apps can be published as self-contained executables
// dotnet publish myapp.cs --self-contained
// Or even as AOT-compiled native binaries!
// dotnet publish myapp.cs --self-contained -p:PublishAot=true
```

## Method Reference

| Method | Streams to Console | Buffers in Memory | Returns | Use Case |
|--------|-------------------|-------------------|---------|----------|
| `RunAsync()` | ✅ | ❌ | `int` (exit code) | Default scripting behavior |
| `CaptureAsync()` | ❌ | ✅ Full | `CommandOutput` | Process small/medium outputs |
| `StreamAsync()` | ❌ | ❌ | `IAsyncEnumerable<CommandLine>` | Process large outputs line-by-line |
| `StreamToFileAsync()` | ❌ | ❌ | `int` (exit code) | Direct huge outputs to file |
| `RunAndCaptureAsync()` | ✅ | ✅ Full | `CommandOutput` | See output + save logs |
| `PassthroughAsync()` | ✅ | ❌ | `void` | Interactive tools |
| `SelectAsync()` | ✅ (UI only) | ✅ (selection only) | `string` | Selection interfaces |

## Why These Methods?

**Core Methods (90% of use cases):**
1. **RunAsync()** - This is what bash/PowerShell do by default. You run a command, you see output. Simple.
2. **CaptureAsync()** - When you need to process output (parse JSON, search for patterns, etc), you don't want it printing to console. Works great for small/medium outputs.
3. **RunAndCaptureAsync()** - CI/CD scenarios need both: show progress to users AND save logs for artifacts.

**Streaming Methods (for large outputs):**
4. **StreamAsync()** - Process massive outputs line-by-line without memory issues. Essential for log processing, large finds, database dumps.
5. **StreamToFileAsync()** - Direct output straight to disk, bypassing memory entirely. Perfect for backups and huge data exports.

**Specialized Methods:**
6. **PassthroughAsync()** - Some tools need full terminal control (vim, ssh, REPLs). This gives it to them.
7. **SelectAsync()** - The fzf pattern (show selection UI, return choice) is common enough to deserve its own method.

## Design Philosophy

- **Method names describe behavior** - No guessing what `RunAsync()` vs `CaptureAsync()` do
- **Never lose data** - All methods that capture include both stdout and stderr  
- **Sensible defaults** - `RunAsync()` does what scripts expect: show output
- **Composable** - Pipe commands together naturally
- **Type-safe** - Full IntelliSense support and compile-time checking

## License

Unlicense - This is free and unencumbered software released into the public domain.