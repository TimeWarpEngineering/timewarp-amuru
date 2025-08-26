# Migration Guide: v0.x to v1.0

## Overview

TimeWarp.Amuru v1.0 introduces a redesigned API that better aligns with shell scripting expectations while providing more complete access to command output. This guide will help you migrate from the old API to the new one.

## Philosophy Changes

### Old: Multiple specialized methods
The old API provided different methods for different output formats (`GetStringAsync()`, `GetLinesAsync()`, `ExecuteAsync()`), which was confusing and lost important information (stderr was often discarded).

### New: Clear purpose-driven methods
The new API provides methods that clearly indicate their behavior:
- `RunAsync()` - Default shell behavior (stream to console)
- `CaptureAsync()` - Silent execution with full output capture
- `PassthroughAsync()` - Interactive tools
- `SelectAsync()` - Selection tools

## Breaking Changes

### Removed Methods

| Old Method | New Approach | Notes |
|------------|--------------|-------|
| `GetStringAsync()` | `CaptureAsync().Result.Stdout` | Now preserves stderr |
| `GetLinesAsync()` | `CaptureAsync().Result.Lines` | Includes all output |
| `ExecuteAsync()` | `CaptureAsync()` or `RunAsync()` | Choose based on needs |
| `ExecuteInteractiveAsync()` | `PassthroughAsync()` | Clearer name |
| `GetStringInteractiveAsync()` | `SelectAsync()` | Purpose-built for selection |
| `Cached()` | No replacement | NO CACHING by design |

### Renamed Concepts

- `CommandResult` → `CommandBuilder` (for building)
- `ExecutionResult` → `CommandOutput` (for results)
- "Execute" terminology → "Run" or "Capture" (clearer intent)

## Migration Examples

### Basic Command Execution

#### Old Way
```csharp
// Confusing - "Execute" doesn't actually show output
await Shell.Builder("npm", "install").ExecuteAsync();

// Lost stderr!
var output = await Shell.Builder("git", "status").GetStringAsync();
```

#### New Way
```csharp
// Clear - streams to console like a shell
await Shell.Builder("npm", "install").RunAsync();

// Captures everything
var result = await Shell.Builder("git", "status").CaptureAsync();
var output = result.Stdout;  // or result.Combined for both stdout and stderr
```

### Getting Command Output

#### Old Way
```csharp
// Lost stderr, no exit code access
var text = await Shell.Builder("cat", "file.txt").GetStringAsync();
var lines = await Shell.Builder("ls").GetLinesAsync();
```

#### New Way
```csharp
// Full access to all output
var result = await Shell.Builder("cat", "file.txt").CaptureAsync();
var text = result.Stdout;
var lines = result.Lines;
var exitCode = result.ExitCode;
var success = result.Success;
```

### Pipeline Commands

#### Old Way
```csharp
var filtered = await Shell.Builder("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .GetLinesAsync();  // Lost stderr from both commands
```

#### New Way
```csharp
var result = await Shell.Builder("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .CaptureAsync();

var filtered = result.Lines;  // All output preserved
if (!result.Success)
{
    Console.WriteLine($"Pipeline failed: {result.Stderr}");
}
```

### Interactive Commands

#### Old Way
```csharp
// Unclear what this does
await Shell.Builder("vim", "file.txt").ExecuteInteractiveAsync();

// Confusing name
var selected = await Fzf.Builder()
    .FromInput("a", "b", "c")
    .GetStringInteractiveAsync();
```

#### New Way
```csharp
// Clear - full terminal passthrough
await Shell.Builder("vim", "file.txt").PassthroughAsync();

// Purpose-built for selection
var selected = await Fzf.Builder()
    .FromInput("a", "b", "c")
    .SelectAsync();
```

### Error Handling

#### Old Way
```csharp
// Had to use special options object
var options = new CommandOptions().WithValidation(CommandResultValidation.None);
var result = await Shell.Builder("git", "status", options).GetStringAsync();
// result is empty string on failure - no error info!
```

#### New Way
```csharp
// Fluent API with full error information
var result = await Shell.Builder("git", "status")
    .WithValidation(CommandResultValidation.None)
    .CaptureAsync();

if (!result.Success)
{
    Console.WriteLine($"Exit code: {result.ExitCode}");
    Console.WriteLine($"Error: {result.Stderr}");
}
```

### Caching

#### Old Way
```csharp
// Built-in caching (now removed)
var cached = Shell.Builder("expensive-command").Cached();
var result1 = await cached.GetStringAsync();
var result2 = await cached.GetStringAsync();  // Used cache
```

#### New Way
```csharp
// NO CACHING - be explicit if you need it
private static CommandOutput? cachedResult;

var result = cachedResult ??= await Shell.Builder("expensive-command").CaptureAsync();
// Or use any standard C# caching pattern you prefer
```

### Cancellation Support

#### Old Way
```csharp
// No built-in cancellation support
await Shell.Builder("long-command").ExecuteAsync();
```

#### New Way
```csharp
// All async methods accept CancellationToken
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await Shell.Builder("long-command").RunAsync(cts.Token);

// Or use timeout
await Shell.Builder("slow-command")
    .WithTimeout(TimeSpan.FromSeconds(10))
    .RunAsync();
```

## Common Patterns

### Show Progress While Building
```csharp
// Old: Had to use ExecuteAsync() which didn't stream
await Shell.Builder("npm", "install").ExecuteAsync();

// New: Use RunAsync() for default shell behavior
await Shell.Builder("npm", "install").RunAsync();
```

### Parse JSON Output
```csharp
// Old: Lost stderr if command failed
var json = await Shell.Builder("aws", "s3", "ls", "--output", "json").GetStringAsync();

// New: Full error information available
var result = await Shell.Builder("aws", "s3", "ls", "--output", "json").CaptureAsync();
if (result.Success)
{
    var json = JsonSerializer.Deserialize<S3Output>(result.Stdout);
}
else
{
    Console.WriteLine($"AWS CLI failed: {result.Stderr}");
}
```

### CI/CD Pipeline
```csharp
// Old: No way to see output while capturing
await Shell.Builder("dotnet", "test").ExecuteAsync();

// New: Stream to console by default
var exitCode = await Shell.Builder("dotnet", "test").RunAsync();
if (exitCode != 0)
{
    throw new Exception($"Tests failed with exit code {exitCode}");
}
```

### Large File Processing
```csharp
// Old: Had to load entire file into memory
var lines = await Shell.Builder("cat", "huge.log").GetLinesAsync();

// New: Stream lines without buffering
await foreach (var line in Shell.Builder("cat", "huge.log").StreamStdoutAsync())
{
    ProcessLine(line);
}
```

## Decision Tree

### Which method should I use?

1. **Do you want to see output in the console?**
   - Yes, and don't need to capture → `RunAsync()`
   - No, need to process it → `CaptureAsync()`

2. **Is it an interactive tool (vim, nano, REPL)?**
   - Yes → `PassthroughAsync()`

3. **Is it a selection tool (fzf, dialog)?**
   - Yes → `SelectAsync()`

4. **Processing very large output?**
   - Yes → `StreamStdoutAsync()` or `StreamStderrAsync()`

## Troubleshooting

### Q: My commands no longer show output
**A:** Replace `ExecuteAsync()` with `RunAsync()`. The old `ExecuteAsync()` captured output, the new `RunAsync()` streams it.

### Q: I can't find stderr output
**A:** Use `CaptureAsync()` which returns `CommandOutput` with separate `Stdout` and `Stderr` properties.

### Q: How do I get just the string output like before?
**A:** Use `CaptureAsync().Result.Stdout` to get just stdout, or `.Combined` for both streams.

### Q: My cached commands are running multiple times
**A:** Caching was removed by design. Implement your own caching if needed:
```csharp
private static readonly Dictionary<string, CommandOutput> cache = new();
var result = cache.TryGetValue(key, out var cached) 
    ? cached 
    : cache[key] = await Shell.Builder(cmd).CaptureAsync();
```

### Q: How do I handle timeouts?
**A:** Use the new timeout support:
```csharp
await Shell.Builder("slow-command")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .RunAsync();
```

## Benefits of the New API

1. **Predictable Behavior**: Method names clearly indicate what happens
2. **Complete Information**: Never lose stderr or exit codes
3. **Shell-Like Defaults**: `RunAsync()` behaves like running in a shell
4. **Better Error Handling**: Full access to all error information
5. **Memory Efficient**: Streaming APIs for large data
6. **Cancellation Support**: All methods accept CancellationToken
7. **No Hidden State**: No caching means no surprises

## Getting Help

If you encounter issues not covered in this guide:
1. Check the updated README.md for examples
2. Review the Architecture documents in `/Analysis/Architecture/`
3. Look at test files for usage patterns
4. Open an issue on GitHub with your specific migration challenge