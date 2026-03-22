# Async-First Design

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Design API for command execution that supports modern asynchronous patterns

## Context and Problem Statement

Command execution is inherently I/O-bound and can take significant time. The API needs to support non-blocking execution while remaining simple and natural for scripting scenarios.

## Decision Drivers

* Enable non-blocking command execution
* Support modern C# async/await patterns
* Allow concurrent command execution
* Prevent blocking the calling thread
* Maintain compatibility with async scripting scenarios

## Considered Options

* Async-first design (all methods async)
* Sync and async method pairs
* Synchronous API with async wrappers
* Callback-based asynchronous pattern

## Decision Outcome

Chosen option: "Async-first design", because it provides the most natural and efficient approach for I/O-bound command execution while aligning with modern C# practices.

### Positive Consequences

* Non-blocking command execution
* Enables concurrent command processing
* Aligns with modern C# async patterns
* Prevents thread pool exhaustion
* Natural integration with async scripting

### Negative Consequences

* Requires async/await knowledge
* No synchronous convenience methods
* Slightly more complex for simple scenarios

## Pros and Cons of the Options

### Async-first design (all methods async)

* Good, because non-blocking by default
* Good, because enables natural concurrency
* Good, because follows modern C# patterns
* Good, because prevents thread pool issues
* Good, because optimal for I/O-bound operations
* Bad, because requires async/await knowledge
* Bad, because no synchronous convenience

### Sync and async method pairs

* Good, because provides choice for different scenarios
* Good, because familiar .NET pattern
* Good, because supports both blocking and non-blocking
* Bad, because doubles API surface area
* Bad, because sync methods block threads unnecessarily
* Bad, because encourages poor practices for I/O-bound operations

### Synchronous API with async wrappers

* Good, because simple synchronous interface
* Good, because can add async wrappers later
* Bad, because blocks threads unnecessarily
* Bad, because poor performance for I/O-bound operations
* Bad, because doesn't align with modern patterns
* Bad, because makes concurrent execution difficult

### Callback-based asynchronous pattern

* Good, because avoids async/await complexity
* Good, because event-driven approach
* Bad, because outdated pattern in modern C#
* Bad, because callback hell for complex scenarios
* Bad, because doesn't integrate with async/await
* Bad, because harder to compose

## Implementation Details

All command execution methods are async:

```csharp
// String output
public async Task<string> GetStringAsync(CancellationToken cancellationToken = default)

// Line-by-line output
public async Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default)

// Fire-and-forget execution
public async Task ExecuteAsync(CancellationToken cancellationToken = default)
```

This enables natural concurrent execution:

```csharp
// Concurrent command execution
var task1 = Shell.Builder("git", "status").GetStringAsync();
var task2 = Shell.Builder("git", "log", "--oneline", "-5").GetLinesAsync();
var task3 = Shell.Builder("git", "diff", "--name-only").GetLinesAsync();

// Wait for all to complete
await Task.WhenAll(task1, task2, task3);
```

## Cancellation Support

All async methods accept optional `CancellationToken` parameters to support:
* Timeout scenarios
* Manual cancellation
* Graceful shutdown

```csharp
// Timeout example
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));
var result = await Shell.Builder("long-running-command").GetStringAsync(cts.Token);
```

## Links

* Related to [ADR-0001](0001-use-static-api-entry-point.md) - Use Static API Entry Point
* Related to [ADR-0006](0006-fluent-pipeline-api.md) - Fluent Pipeline API