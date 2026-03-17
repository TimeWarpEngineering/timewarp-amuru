# Use Static API Entry Point

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Design fluent API for C# shell scripting that minimizes boilerplate

## Context and Problem Statement

TimeWarp.Cli needs to provide a simple, intuitive API for shell command execution in C# scripts. The API should feel natural and minimize ceremony while maintaining type safety and discoverability.

## Decision Drivers

* Reduce boilerplate code in scripting scenarios
* Provide immediate access without object instantiation
* Ensure global availability similar to shell built-ins
* Maintain simplicity for one-off command execution
* Enable fluent chaining of operations

## Considered Options

* Static method entry point (`Run()`)
* Instance-based factory pattern
* Dependency injection container approach
* Builder pattern with constructor

## Decision Outcome

Chosen option: "Static method entry point", because it provides the most natural scripting experience with minimal ceremony and aligns with the library's focus on shell-like command execution.

### Positive Consequences

* Zero boilerplate - scripts can immediately call `Shell.Builder("command")`
* Feels natural for shell scripting scenarios
* No object lifetime management required
* Globally available like shell built-ins
* Maintains fluent interface for chaining operations

### Negative Consequences

* Cannot easily mock or substitute for testing (mitigated by integration testing approach)
* No dependency injection integration (not needed for scripting scenarios)
* Single implementation per process (acceptable for command execution)

## Pros and Cons of the Options

### Static method entry point

* Good, because requires zero setup or configuration
* Good, because feels natural for scripting scenarios
* Good, because globally available like shell commands
* Good, because minimal ceremony for one-off commands
* Bad, because harder to mock for unit testing
* Bad, because cannot use dependency injection

### Instance-based factory pattern

* Good, because allows for configuration injection
* Good, because easier to mock for testing
* Good, because follows traditional OOP patterns
* Bad, because requires object instantiation boilerplate
* Bad, because adds ceremony to simple command execution
* Bad, because less natural for scripting scenarios

### Dependency injection container approach

* Good, because highly testable and configurable
* Good, because follows enterprise patterns
* Good, because supports multiple implementations
* Bad, because massive overkill for scripting library
* Bad, because requires DI container setup
* Bad, because adds significant complexity

### Builder pattern with constructor

* Good, because allows for command configuration
* Good, because follows fluent interface principles
* Good, because can be extended with options
* Bad, because requires constructor call before use
* Bad, because adds ceremony to simple scenarios
* Bad, because less discoverable than static method

## Links

* Refined by [ADR-0006](0006-fluent-pipeline-api.md) - Fluent Pipeline API
* Related to [ADR-0004](0004-omit-interface-abstractions.md) - Omit Interface Abstractions