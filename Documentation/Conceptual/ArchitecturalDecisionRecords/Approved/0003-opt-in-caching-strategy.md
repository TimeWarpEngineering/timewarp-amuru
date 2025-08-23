# Opt-in Caching Strategy

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Enable command result caching to avoid redundant executions while preserving time-sensitive commands

## Context and Problem Statement

Some commands are expensive to execute (large directory scans, complex git operations), and users may want to reuse results multiple times. However, many commands are time-sensitive (date, uptime, system monitoring) and should not be cached automatically.

## Decision Drivers

* Avoid redundant execution of expensive commands
* Enable efficient pipeline branching scenarios
* Preserve time-sensitive command behavior by default
* Maintain backward compatibility
* Keep API surface minimal and discoverable

## Considered Options

* Opt-in caching via `.Cached()` method
* Automatic caching with cache invalidation
* Cache configuration through constructor
* Global caching with command-specific overrides

## Decision Outcome

Chosen option: "Opt-in caching via `.Cached()` method", because it provides explicit control over caching behavior while maintaining backward compatibility and preventing accidental caching of time-sensitive commands.

### Positive Consequences

* Explicit control over which commands are cached
* Backward compatibility maintained
* Time-sensitive commands remain fresh by default
* Efficient pipeline branching scenarios enabled
* Simple, discoverable API surface

### Negative Consequences

* Requires explicit opt-in for caching benefits
* No automatic optimization of expensive commands
* Cache lifetime tied to CommandResult instance

## Pros and Cons of the Options

### Opt-in caching via `.Cached()` method

* Good, because explicit and intentional
* Good, because prevents accidental caching of time-sensitive commands
* Good, because maintains backward compatibility
* Good, because simple API surface
* Good, because clear ownership of cached results
* Bad, because requires explicit opt-in
* Bad, because no automatic optimization

### Automatic caching with cache invalidation

* Good, because automatic optimization
* Good, because transparent to users
* Good, because handles expensive commands automatically
* Bad, because complex invalidation logic required
* Bad, because can break time-sensitive commands
* Bad, because unpredictable behavior

### Cache configuration through constructor

* Good, because allows fine-grained control
* Good, because can configure cache lifetime
* Good, because supports various caching strategies
* Bad, because complicates API surface
* Bad, because breaks static method pattern
* Bad, because requires configuration knowledge

### Global caching with command-specific overrides

* Good, because provides defaults with customization
* Good, because can optimize common cases
* Good, because maintains some automatic behavior
* Bad, because requires global configuration
* Bad, because complex override mechanism
* Bad, because unpredictable without knowing defaults

## Implementation Details

```csharp
// Basic caching usage
var cachedCmd = Shell.Builder("expensive-command").Cached();
var result1 = await cachedCmd.GetStringAsync(); // Executes command
var result2 = await cachedCmd.GetStringAsync(); // Returns cached result

// Pipeline caching scenarios
var files = Shell.Builder("find", "/large/dir", "-name", "*.log").Cached();
var errorLogs = await files.Pipe("grep", "ERROR").GetLinesAsync();
var warningLogs = await files.Pipe("grep", "WARN").GetLinesAsync();
// Only one expensive find operation executed
```

The `.Cached()` method returns a new CommandResult instance with caching enabled, preserving the original command while adding caching behavior.

## Cache Scope and Behavior

* **Instance-scoped**: Each CommandResult instance has its own cache
* **Method-agnostic**: All async methods (GetStringAsync, GetLinesAsync, ExecuteAsync) use the same cache
* **Pipeline-preserved**: Caching state is maintained through .Pipe() operations
* **Thread-safe**: Cache access is safe for concurrent use

## Links

* Related to [ADR-0006](0006-fluent-pipeline-api.md) - Fluent Pipeline API
* Related to [ADR-0007](0007-immutable-commandresult-design.md) - Immutable CommandResult Design