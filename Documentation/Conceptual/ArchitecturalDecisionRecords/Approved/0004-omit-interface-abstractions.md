# Omit Interface Abstractions

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Evaluate whether to include ICommandResult interface for improved testability

## Context and Problem Statement

Traditional .NET libraries often include interfaces to enable dependency injection and mocking for unit tests. The question is whether TimeWarp.Cli should include an ICommandResult interface or remain interface-free.

## Decision Drivers

* Maintain simplicity for scripting scenarios
* Align with library's static API design philosophy
* Leverage superior integration testing approach
* Avoid unnecessary enterprise patterns in scripting tools
* Keep API surface minimal and focused

## Considered Options

* Omit interfaces entirely
* Include ICommandResult interface
* Provide optional interfaces via separate package
* Use abstract base classes instead

## Decision Outcome

Chosen option: "Omit interfaces entirely", because the library's integration testing approach with real commands is superior to unit testing with mocks, and interfaces conflict with the static API design philosophy.

### Positive Consequences

* Maintains library simplicity and focus
* Aligns with static API design
* Reduces API surface area
* Integration testing approach is more valuable than unit testing
* Prevents over-engineering for scripting scenarios

### Negative Consequences

* Cannot easily mock CommandResult for unit tests
* No dependency injection integration
* Cannot substitute alternative implementations

## Pros and Cons of the Options

### Omit interfaces entirely

* Good, because maintains simplicity
* Good, because aligns with static API design
* Good, because reduces API surface
* Good, because prevents over-engineering
* Good, because integration testing is superior for this use case
* Bad, because cannot mock for unit tests
* Bad, because no dependency injection support

### Include ICommandResult interface

* Good, because enables mocking for unit tests
* Good, because supports dependency injection
* Good, because follows enterprise patterns
* Good, because allows alternative implementations
* Bad, because conflicts with static API design
* Bad, because adds complexity to simple scenarios
* Bad, because encourages less valuable unit testing approach

### Provide optional interfaces via separate package

* Good, because provides choice for different scenarios
* Good, because keeps core library simple
* Good, because enables advanced scenarios
* Bad, because fragments the ecosystem
* Bad, because adds maintenance burden
* Bad, because confuses the primary use case

### Use abstract base classes instead

* Good, because enables inheritance-based extension
* Good, because maintains some abstraction
* Good, because provides virtual methods for overriding
* Bad, because complicates the design
* Bad, because inheritance is less flexible than interfaces
* Bad, because still conflicts with static API design

## Integration Testing Superiority

The library's integration testing approach is actually superior to unit testing with mocks:

```csharp
// Integration test with real command (preferred)
var result = await Shell.Builder("echo", "test output").GetStringAsync();
Assert.Equal("test output", result.Trim());

// Unit test with mock (less valuable)
var mockResult = new Mock<ICommandResult>();
mockResult.Setup(r => r.GetStringAsync()).ReturnsAsync("test output");
// Tests the mock, not the real behavior
```

Real commands like `echo`, `date`, and `git` are:
* Deterministic and predictable
* Fast to execute
* Available on target platforms
* Test actual behavior, not mocked behavior

## Consumer Alternatives

If consumers need testability, they can create their own wrapper:

```csharp
// Consumer creates abstraction if needed
public interface ICommandRunner
{
    Task<string> RunAsync(string executable, params string[] args);
}

public class TimeWarpCommandRunner : ICommandRunner
{
    public Task<string> RunAsync(string executable, params string[] args)
        => Run(executable, args).GetStringAsync();
}
```

This keeps the library simple while allowing advanced users to add abstractions where needed.

## Links

* Related to [ADR-0001](0001-use-static-api-entry-point.md) - Use Static API Entry Point
* Related to [ADR-0008](0008-integration-testing-strategy.md) - Integration Testing Strategy