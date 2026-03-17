# Integration Testing Strategy

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Choose testing approach that validates real command execution behavior

## Context and Problem Statement

The library wraps shell command execution, so testing should validate that commands actually work as expected in real environments. The question is whether to use integration tests with real commands or unit tests with mocked dependencies.

## Decision Drivers

* Validate real command execution behavior
* Test on actual target platforms
* Catch platform-specific issues
* Ensure commands work in real environments
* Maintain confidence in library functionality

## Considered Options

* Integration testing with real commands
* Unit testing with mocked dependencies
* Mixed approach (unit tests + integration tests)
* Container-based testing with isolated environments

## Decision Outcome

Chosen option: "Integration testing with real commands", because it provides the most valuable validation of actual command execution behavior and catches real-world issues that mocks cannot detect.

### Positive Consequences

* Tests validate real command behavior
* Catches platform-specific issues
* Builds confidence in actual usage scenarios
* Simpler test setup without mocking infrastructure
* Tests the complete execution path

### Negative Consequences

* Tests depend on system environment
* Potentially slower execution than unit tests
* May require specific tools to be installed
* Can be affected by system state

## Pros and Cons of the Options

### Integration testing with real commands

* Good, because tests real command behavior
* Good, because validates actual execution paths
* Good, because catches platform-specific issues
* Good, because builds confidence in real scenarios
* Good, because simpler than mocking complex command execution
* Bad, because depends on system environment
* Bad, because potentially slower than unit tests
* Bad, because may require specific tools

### Unit testing with mocked dependencies

* Good, because fast execution
* Good, because isolated from system environment
* Good, because predictable test results
* Good, because easier to test edge cases
* Bad, because tests mock behavior, not real behavior
* Bad, because can miss real-world issues
* Bad, because complex mocking setup required
* Bad, because less confidence in actual usage

### Mixed approach (unit tests + integration tests)

* Good, because combines benefits of both approaches
* Good, because provides multiple validation layers
* Good, because can test edge cases and real scenarios
* Bad, because increases maintenance burden
* Bad, because duplicates test coverage
* Bad, because complex test infrastructure

### Container-based testing with isolated environments

* Good, because provides consistent environment
* Good, because isolates tests from host system
* Good, because can test multiple platforms
* Bad, because adds complexity to test setup
* Bad, because requires container infrastructure
* Bad, because may not match actual deployment environments

## Implementation Details

Tests use real commands that are available on target platforms:

```csharp
// Test real command execution
[Test]
public async Task GetStringAsync_WithEchoCommand_ReturnsExpectedOutput()
{
    // Arrange
    var command = Shell.Builder("echo", "Hello World");
    
    // Act
    var result = await command.GetStringAsync();
    
    // Assert
    Assert.Equal("Hello World", result.Trim());
}

// Test error handling with real failing command
[Test]
public async Task GetStringAsync_WithNonExistentCommand_ReturnsEmptyString()
{
    // Arrange
    var command = Shell.Builder("nonexistentcommand");
    
    // Act
    var result = await command.GetStringAsync();
    
    // Assert
    Assert.Equal(string.Empty, result);
}
```

## Cross-Platform Considerations

Tests use commands available on all target platforms:
* `echo` - Available on Windows, Linux, macOS
* `date` - Available on Windows, Linux, macOS  
* Platform-specific tests are marked accordingly

## Dogfooding Approach

The library uses itself for testing infrastructure:
* Test runner is implemented using TimeWarp.Cli
* Build scripts use the library they're testing
* Provides real-world usage validation

## Test Environment Requirements

Tests require minimal environment setup:
* Standard shell commands (echo, date, etc.)
* Git (for git-related tests)
* No special tooling or mocking frameworks

## Confidence Benefits

Integration testing provides high confidence because:
* Tests validate actual command execution
* Catches real environment issues
* Validates complete execution paths
* Tests actual error handling scenarios
* Provides evidence the library works in real usage

## Links

* Related to [ADR-0002](0002-graceful-failure-philosophy.md) - Graceful Failure Philosophy
* Related to [ADR-0004](0004-omit-interface-abstractions.md) - Omit Interface Abstractions
* Related to [ADR-0009](0009-build-system-independence.md) - Build System Independence