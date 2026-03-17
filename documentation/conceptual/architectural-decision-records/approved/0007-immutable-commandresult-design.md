# Immutable CommandResult Design

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Ensure thread safety and predictable behavior through immutable design

## Context and Problem Statement

CommandResult instances may be shared across threads or used in concurrent scenarios. The design should provide predictable behavior and thread safety while maintaining simplicity.

## Decision Drivers

* Ensure thread safety for concurrent usage
* Provide predictable behavior regardless of usage patterns
* Prevent accidental state mutations
* Enable safe sharing of CommandResult instances
* Align with functional programming principles

## Considered Options

* Immutable design with readonly fields
* Mutable design with thread-safe operations
* Copy-on-write pattern
* Synchronized wrapper approach

## Decision Outcome

Chosen option: "Immutable design with readonly fields", because it provides the strongest thread safety guarantees while maintaining simplicity and predictable behavior.

### Positive Consequences

* Complete thread safety without synchronization overhead
* Predictable behavior regardless of usage patterns
* Safe sharing of instances across threads
* No accidental state mutations
* Simplified reasoning about object lifecycle

### Negative Consequences

* Cannot modify instances after creation
* Must create new instances for variations (e.g., caching)
* Slightly higher memory usage for instance variations

## Pros and Cons of the Options

### Immutable design with readonly fields

* Good, because provides complete thread safety
* Good, because prevents accidental mutations
* Good, because enables safe sharing
* Good, because simplifies reasoning about state
* Good, because aligns with functional principles
* Bad, because cannot modify after creation
* Bad, because requires new instances for variations

### Mutable design with thread-safe operations

* Good, because allows in-place modifications
* Good, because potentially lower memory usage
* Good, because familiar object-oriented pattern
* Bad, because requires synchronization mechanisms
* Bad, because can have performance overhead
* Bad, because more complex to reason about

### Copy-on-write pattern

* Good, because efficient for read-heavy scenarios
* Good, because appears mutable while being safe
* Good, because optimizes memory usage
* Bad, because complex implementation
* Bad, because unexpected behavior for mutations
* Bad, because requires careful implementation

### Synchronized wrapper approach

* Good, because provides thread safety guarantees
* Good, because allows underlying mutability
* Good, because can be added as wrapper layer
* Bad, because synchronization overhead
* Bad, because potential for deadlocks
* Bad, because complex to implement correctly

## Implementation Details

All fields are readonly and cannot be modified after construction:

```csharp
public class CommandResult
{
    private readonly Command? Command;
    private readonly bool _enableCaching;
    private readonly CachedResult? _cachedResult;
    
    internal CommandResult(Command? command, bool enableCaching = false)
    {
        Command = command;
        _enableCaching = enableCaching;
        _cachedResult = null;
    }
    
    // Methods return new instances for variations
    public CommandResult Cached()
    {
        return new CommandResult(Command, true);
    }
    
    public CommandResult Pipe(string executable, params string[] arguments)
    {
        // Creates new CommandResult instance
        // Never modifies existing instance
    }
}
```

## Thread Safety Guarantees

The immutable design provides several thread safety guarantees:

1. **Instance Safety**: CommandResult instances can be safely shared across threads
2. **Static Safety**: Static members (NullCommandResult) are inherently thread-safe
3. **Method Safety**: All methods are safe for concurrent execution
4. **Pipeline Safety**: Pipe operations create new instances without modifying originals

## Concurrent Usage Patterns

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
```

## Memory Considerations

The immutable design creates new instances for variations:

```csharp
// Each operation creates a new instance
var original = Shell.Builder("echo", "test");
var cached = original.Cached();        // New instance
var piped = cached.Pipe("grep", "x");  // New instance

// But instances are lightweight and share underlying Command objects where possible
```

## Integration with Caching

The immutable design works well with caching:
* Cached results are stored in readonly fields
* Cache state is immutable after creation
* New instances are created for different cache configurations
* Thread-safe cache access without synchronization

## Links

* Related to [ADR-0003](0003-opt-in-caching-strategy.md) - Opt-in Caching Strategy
* Related to [ADR-0005](0005-async-first-design.md) - Async-First Design
* Related to [ADR-0006](0006-fluent-pipeline-api.md) - Fluent Pipeline API