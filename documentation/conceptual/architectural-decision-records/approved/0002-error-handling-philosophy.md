# Error Handling Philosophy

* Status: superseded
* Architect: Steven T. Cramer
* Date: 2025-01-10
* Updated: 2025-07-13

Technical Story: Design error handling strategy that provides proper error handling by default with opt-in graceful degradation

## Context and Problem Statement

Shell command execution can fail for various reasons (non-existent commands, permission issues, network problems). The library needs a consistent error handling strategy that provides predictable behavior for scripting scenarios while following .NET best practices.

## Decision Drivers

* Follow .NET conventions and best practices
* Provide clear error indication by default
* Enable debugging and error tracking
* Allow opt-in graceful degradation when needed
* Maintain flexibility for different use cases

## Considered Options

* Graceful failure with empty results
* Exception throwing on failures (default)
* Result pattern with success/failure indication
* Optional pattern with null returns

## Decision Outcome

Chosen option: "Exception throwing on failures (default)", because it follows .NET conventions, provides clear error indication, and makes debugging straightforward. Users can opt-in to graceful degradation when needed using `.WithValidation(CommandResultValidation.None)`.

### Positive Consequences

* Clear error indication by default
* Follows .NET best practices and conventions
* Easy debugging with explicit exceptions
* Prevents silent failures that could cause issues
* Opt-in graceful degradation when needed

### Negative Consequences

* Requires error handling for expected failures
* May require more try-catch blocks in some scenarios
* Breaking change from previous behavior

## Pros and Cons of the Options

### Graceful failure with empty results

* Good, because prevents script crashes
* Good, because provides consistent return types
* Good, because aligns with shell behavior
* Good, because requires minimal error handling
* Good, because enables graceful degradation
* Bad, because masks command failures
* Bad, because no built-in failure detection

### Exception throwing on failures (chosen)

* Good, because explicit error indication
* Good, because follows .NET conventions
* Good, because enables traditional error handling
* Good, because debugging is straightforward
* Good, because prevents silent failures
* Good, because opt-in graceful degradation available
* Bad, because requires error handling for expected failures
* Bad, because may require try-catch blocks

### Result pattern with success/failure indication

* Good, because explicit success/failure indication
* Good, because maintains return value on success
* Good, because enables conditional logic
* Good, because follows modern functional patterns
* Bad, because requires checking result status
* Bad, because adds complexity to simple scenarios
* Bad, because changes API surface significantly

### Optional pattern with null returns

* Good, because indicates failure with null
* Good, because enables null-conditional operations
* Good, because follows C# nullable patterns
* Bad, because requires null checking
* Bad, because inconsistent with collection returns
* Bad, because doesn't work well with arrays

## Implementation Details

By default, commands use CliWrap's default validation behavior (CommandResultValidation.ZeroExitCode), which throws exceptions on non-zero exit codes:

```csharp
// Default behavior - throws on failure
await Shell.Builder("git", "status").ExecuteAsync();

// Opt-in graceful degradation
var options = new CommandOptions().WithValidation(CommandResultValidation.None);
await Shell.Builder("git", "status", options).ExecuteAsync();
```

When validation is disabled with `CommandResultValidation.None`:

```csharp
// GetStringAsync returns empty string on failure
public async Task<string> GetStringAsync()
{
    if (Command == null) return string.Empty;
    
    try
    {
        var result = await Command.ExecuteBufferedAsync();
        return result.StandardOutput;
    }
    catch
    {
        // Only when validation is disabled
        return string.Empty;
    }
}

// ExecuteAsync allows exceptions to propagate
public async Task ExecuteAsync()
{
    if (Command == null) return;
    
    // No try-catch - exceptions propagate to caller
    await Command.ExecuteAsync();
}
```

## Links

* Related to [ADR-0001](0001-use-static-api-entry-point.md) - Use Static API Entry Point
* Related to [ADR-0008](0008-integration-testing-strategy.md) - Integration Testing Strategy