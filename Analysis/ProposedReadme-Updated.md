# TimeWarp.Amuru

A fluent API library that brings the simplicity of shell scripting to C#. Execute commands naturally with clear, intuitive methods that do exactly what their names suggest.

## Why TimeWarp.Amuru?

When you write a shell script, running a command is simple - you just run it and see the output. But in C#, command execution has traditionally been complex, verbose, and unclear about what happens to output. TimeWarp.Amuru fixes this.

```csharp
// Just run a command and see output - like in bash/PowerShell!
await Shell.Builder("docker", "build", ".").RunAsync();

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
await Shell.Builder("kubectl", "get", "pods").RunAsync();
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
await Shell.Builder("terraform", "apply").RunAsync();
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

// Process output from cloud CLIs
var instances = await Shell.Builder("aws", "ec2", "describe-instances", "--output", "json").CaptureAsync();
var json = JsonSerializer.Deserialize<Ec2Response>(instances.Stdout);

// Check container status
var containers = await Shell.Builder("docker", "ps", "--format", "json").CaptureAsync();
foreach (var line in containers.Lines)
{
    var container = JsonSerializer.Deserialize<ContainerInfo>(line);
    Console.WriteLine($"Container: {container.Name} is {container.Status}");
}
```

### 2b. Streaming Methods - Process Large Outputs Without Buffering
**Stream output line-by-line without loading into memory**

For commands with massive output:

```csharp
// Stream stdout and stderr separately
await foreach (var line in Shell.Builder("docker", "logs", "-f", "container").StreamStdoutAsync())
{
    await ProcessLogLine(line);
}

// Stream stderr for error monitoring
await foreach (var error in Shell.Builder("npm", "test").StreamStderrAsync())
{
    await AlertTeam(error);
}

// Stream combined output (interleaved stdout/stderr)
await foreach (var line in Shell.Builder("terraform", "plan").StreamCombinedAsync())
{
    Console.WriteLine(line.IsError ? $"[ERROR] {line.Text}" : line.Text);
}

// Stream directly to file, bypassing memory entirely
await Shell.Builder("pg_dump", "database").StreamToFileAsync("backup.sql");
```

### 3. RunAndCaptureAsync() - See AND Log
**Shows output in real-time AND captures it**

Perfect for CI/CD scenarios where you want to see progress but also need to save logs:

```csharp
// See output while running AND get it back for logging
var result = await Shell.Builder("npm", "run", "build").RunAndCaptureAsync();

// Save to log file
await File.WriteAllTextAsync("build.log", result.Combined);

// Check for errors
if (!result.Success)
{
    await SendAlert($"Build failed: {result.Stderr}");
}
```

### 4. PassthroughAsync() - Interactive Tools
**Full terminal control for editors, REPLs, etc.**

```csharp
// Launch interactive tools that need full terminal control
await Shell.Builder("vim", "config.json").PassthroughAsync();
await Shell.Builder("ssh", "server.example.com").PassthroughAsync();
await Shell.Builder("mysql", "-u", "root", "-p").PassthroughAsync();
```

### 5. SelectAsync() - Selection UIs
**For tools like fzf that show UI and return selection**

```csharp
// Show selection UI, get user's choice
var selected = await Fzf.Builder()
    .FromInput("staging", "production", "development")
    .SelectAsync();
Console.WriteLine($"Deploying to: {selected}");

// Select a Kubernetes pod
var pod = await Shell.Builder("kubectl", "get", "pods", "--no-headers")
    .Pipe("fzf", "--preview", "kubectl describe pod {}")
    .SelectAsync();
```

## Command Output Structure

The `CommandOutput` type returned by `CaptureAsync()` and `RunAndCaptureAsync()` provides:

```csharp
public class CommandOutput
{
    public string Stdout { get; }      // Standard output only
    public string Stderr { get; }      // Standard error only  
    public string Combined { get; }    // Both streams in order produced
    public int ExitCode { get; }       // Process exit code
    public bool Success { get; }       // True if ExitCode == 0
    
    // Convenience properties (computed on demand)
    public string[] Lines => Combined.Split('\n');
    public string[] StdoutLines => Stdout.Split('\n');
    public string[] StderrLines => Stderr.Split('\n');
}
```

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

### Cancellation and Timeout

All async methods accept `CancellationToken` for cooperative cancellation:

```csharp
// Simple timeout using CancellationTokenSource
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var result = await Shell.Builder("npm", "install").RunAsync(cts.Token);

// Graceful shutdown in ASP.NET/hosted services
public async Task ProcessAsync(CancellationToken stoppingToken)
{
    await Shell.Builder("kubectl", "apply", "-f", "deployment.yaml").RunAsync(stoppingToken);
}

// WithTimeout is syntactic sugar that creates internal CancellationTokenSource
await Shell.Builder("terraform", "apply")
    .WithTimeout(TimeSpan.FromMinutes(30))
    .RunAsync();
```

### Piping Commands

Chain commands together with Unix-style pipes:

```csharp
// Simple pipe
var count = await Shell.Builder("docker", "ps", "-q")
    .Pipe("wc", "-l")
    .CaptureAsync();

// Multi-stage pipeline
var result = await Shell.Builder("kubectl", "get", "pods", "--all-namespaces")
    .Pipe("grep", "Error")
    .Pipe("head", "-10")
    .RunAsync();  // Shows top 10 errored pods
```

### Standard Input

Provide input to commands:

```csharp
// Provide configuration to kubectl
var yaml = File.ReadAllText("config.yaml");
var result = await Shell.Builder("kubectl", "apply", "-f", "-")
    .WithStandardInput(yaml)
    .CaptureAsync();

// Pipe SQL to database
var sql = "SELECT * FROM users;";
var output = await Shell.Builder("mysql", "mydb")
    .WithStandardInput(sql)
    .CaptureAsync();
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
// Interactive environment selection
var env = await Fzf.Builder()
    .FromInput("dev", "staging", "prod")
    .WithPrompt("Select environment: ")
    .SelectAsync();

// Multi-select Kubernetes namespaces
var namespaces = await Shell.Builder("kubectl", "get", "ns", "-o", "name")
    .Pipe("fzf", "--multi", "--preview", "kubectl get pods -n {}")
    .SelectAsync();
```

## Error Handling

By default, commands throw on non-zero exit codes:

```csharp
try 
{
    await Shell.Builder("terraform", "destroy", "-auto-approve").RunAsync();
}
catch (CommandExecutionException ex)
{
    Console.WriteLine($"Command failed with exit code: {ex.ExitCode}");
}
```

Disable exceptions for graceful handling:

```csharp
var result = await Shell.Builder("git", "pull")
    .WithValidation(CommandResultValidation.None)
    .CaptureAsync();

if (!result.Success)
{
    Console.WriteLine("Pull failed, might need to resolve conflicts");
}
```

## Common Patterns

### CI/CD Pipelines

```csharp
// Run build steps with visibility
await Shell.Builder("npm", "ci").RunAsync();
await Shell.Builder("npm", "run", "build").RunAsync();
await Shell.Builder("npm", "test").RunAsync();

// Docker operations
await Shell.Builder("docker", "build", "-t", "myapp:latest", ".").RunAsync();
await Shell.Builder("docker", "push", "myapp:latest").RunAsync();

// Kubernetes deployment
var deployResult = await Shell.Builder("kubectl", "apply", "-f", "k8s/")
    .RunAndCaptureAsync();
File.WriteAllText("deploy.log", deployResult.Combined);
```

### Cloud Operations

```csharp
// AWS operations
var buckets = await Shell.Builder("aws", "s3", "ls").CaptureAsync();
foreach (var bucket in buckets.Lines)
{
    Console.WriteLine($"Bucket: {bucket}");
}

// Azure operations
await Shell.Builder("az", "group", "create", "-n", "mygroup", "-l", "eastus").RunAsync();

// GCP operations
var projects = await Shell.Builder("gcloud", "projects", "list", "--format=json")
    .CaptureAsync();
var projectList = JsonSerializer.Deserialize<GcpProject[]>(projects.Stdout);
```

### Database Operations

```csharp
// Database backups
await Shell.Builder("pg_dump", "-h", "localhost", "-U", "postgres", "mydb")
    .StreamToFileAsync("backup.sql");

// Run migrations
await Shell.Builder("flyway", "migrate").RunAsync();

// Redis operations
var keys = await Shell.Builder("redis-cli", "keys", "*").CaptureAsync();
```

## Method Reference

All async methods accept an optional `CancellationToken` parameter (default: `CancellationToken.None`):

| Method | Streams to Console | Buffers in Memory | Returns | Use Case |
|--------|-------------------|-------------------|---------|----------|
| `RunAsync(token)` | ✅ | ❌ | `int` (exit code) | Default scripting behavior |
| `CaptureAsync(token)` | ❌ | ✅ Full | `CommandOutput` | Process small/medium outputs |
| `StreamStdoutAsync(token)` | ❌ | ❌ | `IAsyncEnumerable<string>` | Stream stdout line-by-line |
| `StreamStderrAsync(token)` | ❌ | ❌ | `IAsyncEnumerable<string>` | Stream stderr line-by-line |
| `StreamCombinedAsync(token)` | ❌ | ❌ | `IAsyncEnumerable<OutputLine>` | Stream both with source info |
| `StreamToFileAsync(path, token)` | ❌ | ❌ | `int` (exit code) | Direct huge outputs to file |
| `RunAndCaptureAsync(token)` | ✅ | ✅ Full | `CommandOutput` | See output + save logs |
| `PassthroughAsync(token)` | ✅ | ❌ | `void` | Interactive tools |
| `SelectAsync(token)` | ✅ (UI only) | ✅ (selection only) | `string` | Selection interfaces |

## Design Philosophy

- **Method names describe behavior** - No guessing what `RunAsync()` vs `CaptureAsync()` do
- **Never lose data** - All methods that capture include both stdout and stderr  
- **Sensible defaults** - `RunAsync()` does what scripts expect: show output
- **Composable** - Pipe commands together naturally
- **Type-safe** - Full IntelliSense support and compile-time checking

## License

Unlicense - This is free and unencumbered software released into the public domain.