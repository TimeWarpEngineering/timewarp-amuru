# CommandExtensions Class

The `CommandExtensions` class provides the main entry point for the TimeWarp.Cli library through its static `Run()` method.

## Purpose

This class serves as the primary API surface for executing shell commands with a fluent interface. It wraps CliWrap functionality to provide a simplified, PowerShell-like experience for C# scripting.

## Public API

### `Run(string executable, params string[] arguments)`

Creates a new command execution context that can be used to run shell commands asynchronously.

**Parameters:**
- `executable` (string): The executable command to run (e.g., "echo", "ls", "git")
- `arguments` (params string[]): Variable number of arguments to pass to the command

**Returns:**
- `CommandResult`: A fluent wrapper that provides async methods for executing and capturing output

### `Run(string executable, string[] arguments, CommandOptions options)`

Creates a new command execution context with advanced configuration options.

**Parameters:**
- `executable` (string): The executable command to run (e.g., "echo", "ls", "git")
- `arguments` (string[]): Array of arguments to pass to the command
- `options` (CommandOptions): Configuration options for working directory, environment variables, etc.

**Returns:**
- `CommandResult`: A fluent wrapper that provides async methods for executing and capturing output

**Example Usage:**
```csharp
// Simple command execution
var output = await Shell.Builder("echo", "Hello World").GetStringAsync();

// Command with multiple arguments
var files = await Shell.Builder("find", ".", "-name", "*.cs").GetLinesAsync();

// Execute without capturing output
await Shell.Builder("git", "add", ".").ExecuteAsync();

// Advanced configuration with working directory
var options = new CommandOptions()
    .WithWorkingDirectory("/path/to/project");
var result = await Shell.Builder("git", new[] { "status" }, options).GetStringAsync();

// Configuration with environment variables
var envOptions = new CommandOptions()
    .WithEnvironmentVariable("NODE_ENV", "production")
    .WithEnvironmentVariable("API_KEY", "secret123");
var output = await Shell.Builder("node", new[] { "build.js" }, envOptions).GetStringAsync();

// Combined configuration
var combinedOptions = new CommandOptions()
    .WithWorkingDirectory("/app")
    .WithEnvironmentVariable("PATH", "/custom/bin:/usr/bin")
    .WithEnvironmentVariable("DEBUG", "true");
await Shell.Builder("make", new[] { "install" }, combinedOptions).ExecuteAsync();
```

## CommandOptions Configuration

The `CommandOptions` class provides advanced configuration capabilities through a fluent interface:

### Working Directory Configuration
```csharp
var options = new CommandOptions()
    .WithWorkingDirectory("/path/to/project");

// Command will execute in the specified directory
var result = await Shell.Builder("ls", new string[0], options).GetLinesAsync();
```

### Environment Variables
```csharp
// Single environment variable
var options = new CommandOptions()
    .WithEnvironmentVariable("NODE_ENV", "production");

// Multiple environment variables via fluent chaining
var options = new CommandOptions()
    .WithEnvironmentVariable("API_KEY", "key123")
    .WithEnvironmentVariable("DEBUG", "true")
    .WithEnvironmentVariable("TIMEOUT", "30");

// Multiple environment variables via dictionary
var envVars = new Dictionary<string, string?>
{
    { "NODE_ENV", "production" },
    { "API_KEY", "secret" },
    { "DEBUG", "false" }
};
var options = new CommandOptions()
    .WithEnvironmentVariables(envVars);
```

### Configuration Features
- **Immutable Design**: Each `With*` method returns a new `CommandOptions` instance
- **Fluent Interface**: Chain multiple configuration methods together
- **Graceful Fallbacks**: Invalid configuration gracefully returns `NullCommandResult`
- **CliWrap Integration**: Seamlessly applies configuration to underlying CliWrap commands

### Backward Compatibility
The original `Run(executable, params arguments)` method continues to work unchanged. The new overload is purely additive and maintains full backward compatibility.

## Error Handling Philosophy

The `Run()` method follows a **graceful failure** approach:

- **Input validation**: Validates that `executable` is not null or whitespace
- **No exceptions thrown**: Failed command creation returns a `CommandResult` that will produce empty results
- **Shell-like behavior**: Mirrors how shell environments handle command failures without crashing
- **Defensive programming**: Catches all exceptions during command setup and returns safe defaults
- **Singleton pattern**: Uses `CommandResult.NullCommandResult` to avoid creating multiple identical null instances

## Implementation Details

### Command Creation Process

1. **Input Validation**: Checks that `executable` is not null or whitespace
2. **Wrap Command**: Uses `CliWrap.Cli.Wrap()` to create the base command
3. **Add Arguments**: Applies provided arguments using `WithArguments()`
4. **Disable Validation**: Sets `CommandResultValidation.None` to allow non-zero exit codes
5. **Return Wrapper**: Wraps the CliWrap command in a `CommandResult` for fluent API

### Exception Handling

```csharp
// Input validation
if (string.IsNullOrWhiteSpace(executable))
{
  return CommandResult.NullCommandResult;
}

try
{
  // Create CliWrap command with arguments
  Command cliCommand = CliWrap.Cli.Wrap(executable)
    .WithArguments(arguments)
    .WithValidation(CommandResultValidation.None);
    
  return new CommandResult(cliCommand);
}
catch
{
  // Any failure during command creation returns the singleton null-command result
  // This will produce empty strings/arrays when executed
  return CommandResult.NullCommandResult;
}
```

## Design Decisions

### Static Entry Point
- **Global accessibility**: `Run()` is available anywhere without instantiation
- **Scripting-friendly**: Reduces boilerplate code in C# scripts
- **PowerShell-inspired**: Mimics the simplicity of PowerShell cmdlets

### Parameter Design
- **Fluent interface**: Returns `CommandResult` for method chaining
- **Flexible arguments**: `params string[]` allows variable argument lists
- **Type safety**: Strongly-typed parameters prevent common scripting errors
- **Input validation**: Validates executable parameter to prevent null/empty command execution

### Integration with CliWrap
- **Leverages proven library**: Built on top of the robust CliWrap library
- **Adds convenience layer**: Simplifies common use cases while maintaining power
- **Async-first design**: All operations are naturally asynchronous

### NullCommandResult Pattern
- **Singleton efficiency**: Uses `CommandResult.NullCommandResult` static readonly instance
- **Memory optimization**: Avoids creating multiple identical null command instances
- **Consistent behavior**: All failure scenarios return the same shared instance
- **Thread safety**: Static readonly initialization is inherently thread-safe

## Related Classes

- **CommandResult**: The return type that provides async execution methods
- **GlobalUsings**: Provides necessary using statements for the class

## Usage in Scripts

This class is designed to be used in C# scripts with the following pattern:

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Cli

// Direct usage - Run is available globally
var result = await Shell.Builder("date").GetStringAsync();
Console.WriteLine($"Current date: {result}");

// Multiple commands
var files = await Shell.Builder("ls", "-la").GetLinesAsync();
foreach (var file in files)
{
  Console.WriteLine(file);
}
```

## Thread Safety

The `CommandExtensions` class is thread-safe:
- **Stateless design**: No instance state to synchronize
- **Immutable operations**: Each `Run()` call creates a new `CommandResult`
- **CliWrap safety**: Underlying CliWrap commands are thread-safe