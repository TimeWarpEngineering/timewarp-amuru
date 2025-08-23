# Fluent Pipeline API

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Enable shell-like command chaining with Unix pipe semantics

## Context and Problem Statement

Shell environments support powerful command chaining through pipes (`|`), allowing the output of one command to become the input of another. TimeWarp.Cli should provide similar functionality while maintaining type safety and fluent interface design.

## Decision Drivers

* Enable shell-like command chaining
* Maintain fluent interface consistency
* Provide type-safe pipeline construction
* Support complex multi-stage pipelines
* Leverage CliWrap's native pipe support

## Considered Options

* Fluent `.Pipe()` method
* Operator overloading with `|`
* Builder pattern for pipeline construction
* Separate Pipeline class

## Decision Outcome

Chosen option: "Fluent `.Pipe()` method", because it provides the most natural and discoverable API while maintaining consistency with the existing fluent interface design.

### Positive Consequences

* Natural, discoverable API
* Consistent with existing fluent interface
* Type-safe pipeline construction
* Supports complex multi-stage pipelines
* Integrates seamlessly with caching and other features

### Negative Consequences

* Slightly more verbose than operator overloading
* Method chaining can become long for complex pipelines

## Pros and Cons of the Options

### Fluent `.Pipe()` method

* Good, because consistent with existing fluent interface
* Good, because discoverable through IntelliSense
* Good, because type-safe and validated
* Good, because integrates with other features (caching, etc.)
* Good, because supports complex pipeline construction
* Bad, because slightly more verbose than operators
* Bad, because can create long method chains

### Operator overloading with `|`

* Good, because matches shell syntax exactly
* Good, because very concise
* Good, because familiar to shell users
* Bad, because harder to discover
* Bad, because conflicts with bitwise OR operator
* Bad, because doesn't integrate well with fluent interface

### Builder pattern for pipeline construction

* Good, because explicit pipeline construction
* Good, because supports complex configurations
* Good, because clear separation of concerns
* Bad, because verbose for simple cases
* Bad, because breaks fluent interface consistency
* Bad, because requires pipeline finalization step

### Separate Pipeline class

* Good, because dedicated pipeline management
* Good, because can optimize pipeline execution
* Good, because clear abstraction boundaries
* Bad, because breaks unified CommandResult interface
* Bad, because requires learning additional concepts
* Bad, because complicates simple scenarios

## Implementation Details

The `.Pipe()` method creates a new CommandResult representing the entire pipeline:

```csharp
public CommandResult Pipe(string executable, params string[] arguments)
{
    // Input validation
    if (Command == null || string.IsNullOrWhiteSpace(executable))
    {
        return NullCommandResult;
    }
    
    try
    {
        // Use CommandExtensions.Run() to create the next command
        CommandResult nextCommandResult = CommandExtensions.Run(executable, arguments);
        
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
        return NullCommandResult;
    }
}
```

## Usage Examples

### Basic Pipeline
```csharp
// Find and filter files
var filteredFiles = await Shell.Builder("find", ".", "-name", "*.cs")
    .Pipe("grep", "async")
    .GetLinesAsync();
```

### Multi-stage Pipeline
```csharp
// Complex text processing
var words = await Shell.Builder("echo", "The quick brown fox jumps over the lazy dog")
    .Pipe("tr", " ", "\\n")
    .Pipe("grep", "o")
    .Pipe("sort")
    .GetLinesAsync();
```

### Pipeline with Caching
```csharp
// Cache expensive first operation
var files = Shell.Builder("find", "/large/dir", "-name", "*.log").Cached();
var errors = await files.Pipe("grep", "ERROR").GetLinesAsync();
var warnings = await files.Pipe("grep", "WARN").GetLinesAsync();
```

## Error Handling

Pipeline operations follow the same graceful failure philosophy:
* If any command in the pipeline fails, the entire pipeline returns empty results
* Invalid commands result in NullCommandResult
* Consistent with single command error handling

## Integration with Other Features

The pipeline API integrates seamlessly with:
* **Caching**: Cached state is preserved through pipe operations
* **Cancellation**: CancellationToken flows through entire pipeline
* **Configuration**: CommandOptions can be applied to individual commands

## Links

* Related to [ADR-0001](0001-use-static-api-entry-point.md) - Use Static API Entry Point
* Related to [ADR-0002](0002-graceful-failure-philosophy.md) - Graceful Failure Philosophy
* Related to [ADR-0003](0003-opt-in-caching-strategy.md) - Opt-in Caching Strategy