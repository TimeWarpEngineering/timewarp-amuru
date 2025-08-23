# CommandResult Class

The `CommandResult` class provides a fluent interface for executing shell commands and capturing their output in various formats. It wraps CliWrap's `Command` objects to provide a simplified, PowerShell-like experience with graceful error handling.

## Purpose

This class serves as the execution wrapper returned by `CommandExtensions.Run()`. It provides three distinct ways to interact with command execution:

1. **String Output**: Capture full command output as a single string
2. **Line-by-Line Output**: Capture output as an array of lines
3. **Fire-and-Forget**: Execute without capturing output

## Public API

### `GetStringAsync(CancellationToken cancellationToken = default)`

Executes the command and returns the complete standard output as a single string.

**Parameters:**
- `cancellationToken` (optional): Token to cancel the command execution

**Returns:**
- `Task<string>`: The full standard output of the command, including newlines

**Behavior:**
- Preserves original formatting and whitespace
- Includes all newlines and carriage returns
- Returns empty string on command failure, null command, or cancellation

**Example Usage:**
```csharp
// Get current date as formatted string
var dateOutput = await Shell.Builder("date").GetStringAsync();
Console.WriteLine($"Current date: {dateOutput}");

// Get git status with full formatting
var gitStatus = await Shell.Builder("git", "status").GetStringAsync();
Console.WriteLine(gitStatus);

// With cancellation token and timeout
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30 second timeout
var output = await Shell.Builder("long-running-command").GetStringAsync(cts.Token);
```

### `GetLinesAsync(CancellationToken cancellationToken = default)`

Executes the command and returns the standard output split into individual lines.

**Parameters:**
- `cancellationToken` (optional): Token to cancel the command execution

**Returns:**
- `Task<string[]>`: Array of strings, each representing a line of output

**Behavior:**
- Splits on both `\n` and `\r` characters
- Removes empty lines automatically (`StringSplitOptions.RemoveEmptyEntries`)
- Returns empty array on command failure, null command, or cancellation
- Ideal for processing output line-by-line

### `Pipe(string executable, params string[] arguments)`

Chains the current command with another command, creating a pipeline where the output of the first command becomes the input of the second command.

**Parameters:**
- `executable`: The command to pipe to
- `arguments`: Command line arguments for the piped command

**Returns:**
- `CommandResult`: A new CommandResult representing the entire pipeline

**Behavior:**
- **Input validation**: Validates that both current command and target executable are valid
- **Uses CommandExtensions.Run()**: Internally calls `CommandExtensions.Run()` to create the next command
- **CliWrap integration**: Uses CliWrap's pipe operator (`|`) to chain commands
- **Graceful error handling**: If any command in the pipeline fails, returns `NullCommandResult`
- **Chaining support**: Supports chaining multiple pipes together
- **Memory efficient**: Streams data directly between processes without buffering

**Implementation Details:**
```csharp
public CommandResult Pipe(string executable, params string[] arguments)
{
  // Input validation
  if (Command == null)
  {
    return NullCommandResult;
  }
  
  if (string.IsNullOrWhiteSpace(executable))
  {
    return NullCommandResult;
  }
  
  try
  {
    // Use Run() to create the next command instead of duplicating logic
    CommandResult nextCommandResult = CommandExtensions.Run(executable, arguments);
    
    // If Run() failed, it returned a CommandResult with null Command
    if (nextCommandResult.InternalCommand == null)
    {
      return NullCommandResult;
    }
    
    // Chain commands using CliWrap's pipe operator
    Command pipedCommand = Command | nextCommandResult.InternalCommand;
    return new CommandResult(pipedCommand);
  }
  catch
  {
    // Command creation failures return null command (graceful degradation)
    return NullCommandResult;
  }
}
```

**Example Usage:**
```csharp
// Basic pipeline - find and filter
var filteredFiles = await Shell.Builder("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .GetLinesAsync();

// Multi-stage pipeline - find, filter, and count
var count = await Shell.Builder("echo", "line1\nline2\nline3\nline4")
    .Pipe("grep", "line")
    .Pipe("wc", "-l")
    .GetStringAsync();

// Real-world example - git log with formatting
var recentCommits = await Shell.Builder("git", "log", "--oneline", "-n", "10")
    .Pipe("head", "-5")
    .GetLinesAsync();

// Text processing pipeline
var words = await Shell.Builder("echo", "The quick brown fox jumps over the lazy dog")
    .Pipe("tr", " ", "\n")
    .Pipe("grep", "o")
    .GetLinesAsync();
```

**Additional Examples:**
```csharp
// Process each file from ls command
var files = await Shell.Builder("ls", "-la").GetLinesAsync();
foreach (var file in files)
{
  Console.WriteLine($"File: {file}");
}

// Find all C# files and process them
var csharpFiles = await Shell.Builder("find", ".", "-name", "*.cs").GetLinesAsync();
var fileCount = csharpFiles.Length;
Console.WriteLine($"Found {fileCount} C# files");
```

### `ExecuteAsync(CancellationToken cancellationToken = default)`

Executes the command without capturing any output, useful for commands that perform actions rather than produce output.

**Parameters:**
- `cancellationToken` (optional): Token to cancel the command execution

**Returns:**
- `Task`: Completes when command execution finishes

**Behavior:**
- Does not capture or return any output
- Allows output to flow to console/terminal normally
- Silently handles command failures without throwing exceptions
- Supports cancellation for long-running commands
- Optimal for commands like `git add`, `mkdir`, `rm`, etc.

**Implementation:**
```csharp
if (Command == null)
{
  return;
}

try
{
  await Command.ExecuteAsync();
}
catch
{
  // Process start failures (non-existent commands, etc.) are silently ignored
  // This matches shell behavior where failed commands don't crash the shell
}
```

**Example Usage:**
```csharp
// Execute git commands without capturing output
await Shell.Builder("git", "add", ".").ExecuteAsync();
await Shell.Builder("git", "commit", "-m", "Update files").ExecuteAsync();

// Create directories or copy files
await Shell.Builder("mkdir", "-p", "build/output").ExecuteAsync();
await Shell.Builder("cp", "file1.txt", "backup/").ExecuteAsync();

// With cancellation for long operations
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5)); // 5 minute timeout
await Shell.Builder("large-file-copy", "source", "destination").ExecuteAsync(cts.Token);
```

## Caching Support

The `CommandResult` class supports opt-in caching through the `.Cached()` method. This enables efficient reuse of command results when appropriate.

### Cached() Method

Creates a new `CommandResult` instance with caching enabled. Subsequent calls to `GetStringAsync()`, `GetLinesAsync()`, or `ExecuteAsync()` will return cached results instead of re-executing the command.

**Returns:**
- `CommandResult`: A new CommandResult instance with caching enabled

**Behavior:**
- **Opt-in design**: Caching must be explicitly enabled via `.Cached()`
- **Instance-scoped**: Each CommandResult instance has its own cache
- **All methods cached**: GetStringAsync, GetLinesAsync, and ExecuteAsync all use caching when enabled
- **Pipeline preservation**: Caching state is preserved through `.Pipe()` operations
- **Thread-safe**: Cache access is safe for concurrent use

### Basic Caching Examples

```csharp
// Without caching - executes fresh each time
var dateCmd = Shell.Builder("date");
var time1 = await dateCmd.GetStringAsync(); // 14:30:15
var time2 = await dateCmd.GetStringAsync(); // 14:30:16 (different)

// With caching - executes once, returns cached result
var cachedCmd = Shell.Builder("echo", "hello world").Cached();
var result1 = await cachedCmd.GetStringAsync(); // Executes command
var result2 = await cachedCmd.GetStringAsync(); // Returns cached result
var lines = await cachedCmd.GetLinesAsync();    // Also uses cache

// Mixed usage on same base command
var gitLog = Shell.Builder("git", "log", "--oneline", "-n", "10");
var cached = await gitLog.Cached().GetLinesAsync(); // Cached result
var fresh = await gitLog.GetStringAsync();          // Fresh execution
```

### Pipeline Caching Scenarios

#### Cache Final Pipeline Result
```csharp
// Cache the entire pipeline result
var pipeline = Shell.Builder("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .Pipe("head", "-10")
    .Cached();

var files1 = await pipeline.GetLinesAsync(); // Executes full pipeline
var files2 = await pipeline.GetLinesAsync(); // Returns cached result
```

#### Cache Intermediate Steps
```csharp
// Cache expensive first operation
var findResult = Shell.Builder("find", "/large/codebase", "-name", "*.cs").Cached();

// Multiple filters on same cached base
var asyncFiles = await findResult.Pipe("grep", "async").GetLinesAsync();
var testFiles = await findResult.Pipe("grep", "Test").GetLinesAsync();
var allFiles = await findResult.GetLinesAsync();

// Only one expensive find operation executed!
```

#### Complex Pipeline Analysis
```csharp
// Cache git log data for multiple analyses
var logData = Shell.Builder("git", "log", "--since=1.week.ago", "--oneline").Cached();

// Different analyses of the same cached data
var bugFixes = await logData.Pipe("grep", "fix").GetLinesAsync();
var features = await logData.Pipe("grep", "feat").GetLinesAsync();
var authors = await logData
    .Pipe("cut", "-d", " ", "-f", "2-")
    .Pipe("sort")
    .Pipe("uniq")
    .GetLinesAsync();
```

### Caching Best Practices

#### When to Use Caching
- **Deterministic commands**: Commands that always return the same output
- **Expensive operations**: Large directory scans, complex git operations
- **Multiple consumers**: When the same result is needed multiple times
- **Pipeline branching**: When multiple analyses run on the same base data

#### When NOT to Use Caching
- **Time-sensitive commands**: `date`, `uptime`, system monitoring
- **Stateful commands**: Commands that depend on changing system state
- **Side-effect commands**: Commands that modify the system
- **Interactive commands**: Commands expecting user input

#### Performance Considerations
```csharp
// Efficient: Cache expensive operation, vary cheap filters
var files = Shell.Builder("find", "/", "-name", "*.log").Cached();
var errorLogs = await files.Pipe("grep", "ERROR").GetLinesAsync();
var warningLogs = await files.Pipe("grep", "WARN").GetLinesAsync();

// Inefficient: Caching cheap operations
var echo = Shell.Builder("echo", "test").Cached(); // Unnecessary for simple commands
```

### Cache Scope and Lifetime

```csharp
// Each instance has independent cache
var cmd1 = Shell.Builder("date").Cached();
var cmd2 = Shell.Builder("date").Cached();

var time1 = await cmd1.GetStringAsync(); // 14:30:15
var time2 = await cmd2.GetStringAsync(); // 14:30:16 (different cache)

// Cache lifetime = CommandResult instance lifetime
// Cache is garbage collected with the CommandResult
```

## Cancellation Support

All async methods in `CommandResult` support cancellation through optional `CancellationToken` parameters. This enables timeout functionality and manual cancellation of long-running commands.

### Cancellation Behavior

**Graceful Handling**: When a command is cancelled, the library follows its graceful failure philosophy:
- `GetStringAsync()`: Returns empty string
- `GetLinesAsync()`: Returns empty array  
- `ExecuteAsync()`: Completes without throwing

**Exception Handling**: While CliWrap may throw `OperationCanceledException`, TimeWarp.Cli catches these and returns safe defaults consistent with other failure scenarios.

### Cancellation Examples

```csharp
// Timeout scenario
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));

try 
{
    var result = await Shell.Builder("long-running-process").GetStringAsync(cts.Token);
    Console.WriteLine($"Command completed: {result}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Command was cancelled due to timeout");
}

// Manual cancellation
var manualCts = new CancellationTokenSource();

// Cancel after user input
Task.Run(async () => 
{
    Console.ReadKey(); // Wait for user input
    manualCts.Cancel();
});

var output = await Shell.Builder("interactive-command").GetStringAsync(manualCts.Token);

// Pipeline with cancellation
using var pipelineCts = new CancellationTokenSource();
pipelineCts.CancelAfter(TimeSpan.FromMinutes(1));

var result = await Shell.Builder("find", ".", "-name", "*.log")
    .Pipe("grep", "ERROR")
    .Pipe("head", "-100")
    .GetLinesAsync(pipelineCts.Token);
```

### Cancellation Best Practices

1. **Always use timeouts** for potentially long-running commands
2. **Dispose CancellationTokenSource** with `using` statements
3. **Handle both completion and cancellation** scenarios gracefully
4. **Consider shorter timeouts** for user-facing operations
5. **Use manual cancellation** for interactive scenarios

## Error Handling Philosophy

The `CommandResult` class implements a **graceful failure** approach across all methods:

### Null Command Handling
- **Safe defaults**: When the internal `Command` is null, methods return safe empty values
- **No exceptions**: Never throws exceptions for null commands
- **Predictable behavior**: Consistent return values regardless of command validity

### Exception Handling
- **Catch-all approach**: All exceptions during command execution are caught
- **Shell-like behavior**: Mirrors shell environments where failed commands don't crash the session
- **Empty results**: Failed commands return empty strings/arrays instead of throwing

### Command Failure Scenarios
```csharp
// Non-existent command - returns empty string/array
var result = await Shell.Builder("nonexistentcommand").GetStringAsync(); // Returns string.Empty
var lines = await Shell.Builder("nonexistentcommand").GetLinesAsync();   // Returns Array.Empty<string>()

// Command with non-zero exit code - returns captured output
var output = await Shell.Builder("ls", "/nonexistent/path").GetStringAsync(); // Returns error message

// Permission denied - returns empty results
var restricted = await Shell.Builder("cat", "/etc/shadow").GetStringAsync(); // Returns string.Empty
```

## Implementation Details

### Command Execution Pattern

Each public method follows the same defensive pattern:

1. **Null Check**: Verify the internal `Command` is not null
2. **Try-Catch Wrapper**: Wrap CliWrap execution in exception handling
3. **Safe Return**: Return appropriate empty value on any failure

```csharp
// GetStringAsync pattern
if (Command == null)
{
  return string.Empty;
}

try
{
  BufferedCommandResult result = await Command.ExecuteBufferedAsync();
  return result.StandardOutput;
}
catch
{
  // Process start failures (non-existent commands, etc.) return empty string
  return string.Empty;
}

// GetLinesAsync pattern
if (Command == null)
{
  return Array.Empty<string>();
}

try
{
  BufferedCommandResult result = await Command.ExecuteBufferedAsync();
  return result.StandardOutput.Split
  (
    new char[] { '\n', '\r' }, 
    StringSplitOptions.RemoveEmptyEntries
  );
}
catch
{
  // Process start failures (non-existent commands, etc.) return empty array
  return Array.Empty<string>();
}
```

### Output Processing

#### String Output (`GetStringAsync`)
- Uses `ExecuteBufferedAsync()` to capture all output
- Returns `result.StandardOutput` directly
- Preserves all formatting, whitespace, and newlines

#### Line Output (`GetLinesAsync`)
- Uses `ExecuteBufferedAsync()` to capture all output
- Splits output using `Split()` with newline characters
- Removes empty entries to provide clean line array

#### No Output (`ExecuteAsync`)
- Uses `ExecuteAsync()` for streaming execution
- Allows output to flow to console naturally
- No return value processing needed

### CliWrap Integration

The class leverages CliWrap's robust command execution:
- **Buffered Execution**: Uses `ExecuteBufferedAsync()` for output capture
- **Streaming Execution**: Uses `ExecuteAsync()` for fire-and-forget scenarios
- **Validation Disabled**: Commands are created with `CommandResultValidation.None`

## Internal API

### NullCommandResult Singleton
- **Static readonly field**: `internal static readonly CommandResult NullCommandResult = new(null)`
- **Shared instance**: All failure scenarios return the same singleton instance
- **Memory efficiency**: Avoids creating multiple identical null command instances
- **Thread safety**: Static readonly initialization is inherently thread-safe
- **Performance**: No allocation overhead for common failure scenarios

### InternalCommand Property
- **Private access**: `private Command? InternalCommand => Command`
- **Purpose**: Allows `Pipe()` method to access the `Command` field from other `CommandResult` instances
- **Same-class access**: C# allows private member access between instances of the same class
- **Type safety**: Returns nullable `Command` to handle null cases safely

```csharp
// Usage in Pipe() method
if (nextCommandResult.InternalCommand == null)
{
  return NullCommandResult;
}

Command pipedCommand = Command | nextCommandResult.InternalCommand;
```

## Design Decisions

### Internal Constructor
- **Controlled Creation**: Only `CommandExtensions.Run()` can create instances
- **Encapsulation**: Prevents direct instantiation by external code
- **Null Safety**: Accepts nullable `Command` for graceful failure handling
- **Singleton Integration**: Works with `NullCommandResult` pattern

### Async-First Design
- **Non-blocking**: All operations are asynchronous by default
- **Scalable**: Supports concurrent command execution
- **Modern C#**: Leverages async/await patterns throughout

### Return Type Choices
- **String vs StringBuilder**: Returns `string` for simplicity and immutability
- **Array vs List**: Returns `string[]` for performance and memory efficiency
- **Task vs ValueTask**: Uses `Task` for consistency with CliWrap

## Thread Safety

The `CommandResult` class is fully thread-safe and designed for concurrent use:

### Immutable Design
- **Readonly Fields**: All instance fields are `readonly`, preventing modification after construction
- **No Mutable State**: No instance variables are modified after object creation
- **Immutable Operations**: All methods return new values without modifying the instance
- **CliWrap Foundation**: Built on CliWrap's thread-safe, immutable Command objects

### Singleton Thread Safety
- **CLR Guaranteed**: The `NullCommandResult` singleton uses static readonly initialization
- **Memory Barriers**: CLR handles proper memory barriers during static initialization
- **Shared Instance**: Multiple threads can safely share the same `NullCommandResult` instance
- **No Race Conditions**: Static readonly prevents multiple initialization scenarios

### Concurrent Usage Patterns
```csharp
// Safe: Multiple threads can use the same CommandResult instance
CommandResult cmd = Shell.Builder("echo", "test");

// Thread 1
Task<string> stringTask = cmd.GetStringAsync();

// Thread 2  
Task<string[]> linesTask = cmd.GetLinesAsync();

// Thread 3
Task executeTask = cmd.ExecuteAsync();

// All execute safely in parallel
await Task.WhenAll(stringTask, linesTask, executeTask);

// Safe: Multiple threads creating CommandResults
Parallel.For(0, 100, i => 
{
    var result = Shell.Builder("echo", $"Thread {i}");
    // Each thread gets its own CommandResult instance
});
```

### Thread Safety Guarantees
1. **Instance Safety**: CommandResult instances can be shared across threads
2. **Static Safety**: Static members (NullCommandResult, NewlineCharacters) are thread-safe
3. **Method Safety**: All public methods are safe for concurrent execution
4. **Pipeline Safety**: Pipe operations create new instances without modifying originals

## Performance Considerations

### Memory Usage
- **Buffered Output**: `GetStringAsync()` and `GetLinesAsync()` load full output into memory
- **Streaming Alternative**: `ExecuteAsync()` doesn't buffer output
- **String Splitting**: `GetLinesAsync()` creates string array allocation

### Execution Efficiency
- **Single Execution**: Each method call executes the command once
- **No Caching**: Results are not cached between method calls
- **Exception Overhead**: Catch-all blocks have minimal performance impact

## Usage Patterns

### Command Chaining
```csharp
// Multiple operations on same command setup
CommandResult cmd = Shell.Builder("git", "log", "--oneline", "-10");
string fullOutput = await cmd.GetStringAsync();
string[] lines = await cmd.GetLinesAsync();
```

### Error Resilience
```csharp
// Graceful handling of potentially failing commands
var backupFiles = await Shell.Builder("find", "/backup", "-name", "*.bak").GetLinesAsync();
if (backupFiles.Length > 0)
{
  Console.WriteLine($"Found {backupFiles.Length} backup files");
}
else
{
  Console.WriteLine("No backup files found (or command failed)");
}
```

### Mixed Output Styles
```csharp
// Choose appropriate method based on use case
await Shell.Builder("git", "add", ".").ExecuteAsync();                    // Action command
var status = await Shell.Builder("git", "status", "--short").GetStringAsync(); // Formatted output
var files = await Shell.Builder("git", "ls-files").GetLinesAsync();       // List processing
```

## Related Classes

- **CommandExtensions**: Factory class that creates `CommandResult` instances
- **Command**: CliWrap command object wrapped by this class
- **BufferedCommandResult**: CliWrap result object used internally