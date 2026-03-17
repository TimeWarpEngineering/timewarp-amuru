# Test Runner Pattern for C# Scripts

* Status: approved
* Architect: Steven T. Cramer
* Date: 2025-01-13

Technical Story: Design a reusable test pattern for C# script-based integration tests

## Context and Problem Statement

Integration tests are written as executable C# scripts (.cs files with shebang). We need a consistent pattern for discovering and running test methods within these scripts that provides proper test reporting and exit codes.

## Decision Drivers

* Enable test discovery via reflection
* Provide consistent test output formatting
* Support async test methods
* Maintain script simplicity
* Enable code reuse across test files
* Follow C# conventions for test organization

## Considered Options

* Inline test execution with manual tracking
* Class-based tests with TestRunner pattern
* External test framework integration
* Custom attribute-based discovery

## Decision Outcome

Chosen option: "Class-based tests with TestRunner pattern", because it provides the best balance of organization, reusability, and convention following while working within C# script constraints.

### Positive Consequences

* Consistent test organization across all integration tests
* Reusable TestRunner infrastructure
* Clear test discovery via reflection
* Proper async/await support
* Automatic test counting and reporting
* Clean separation of test logic from execution

### Negative Consequences

* Requires understanding of the pattern
* Static classes cannot be used as generic parameters
* Analyzer warnings need suppression for script scenarios

## Implementation Pattern

### Test Script Structure

```csharp
#!/usr/bin/dotnet --

await RunTests<YourTestClass>();

internal sealed class YourTestClass
{
  // Static fields for test configuration
  static CommandOptions TestOptions = new CommandOptions();

  // Test methods: public static async Task
  public static async Task TestSomething()
  {
    // Use assertion methods directly (static imports)
    AssertTrue(condition, "failure message");
    
    // Or assert exceptions
    await AssertThrowsAsync<Exception>(
      async () => await SomeOperation(),
      "expected exception message"
    );
  }
}
```

### Key Requirements

1. **Test Class**:
   - Must be non-static (C# limitation for generic parameters)
   - Should be `internal sealed`
   - Suppress CA1812 warning for uninstantiated classes

2. **Test Methods**:
   - Must be `public static async Task`
   - No parameters required
   - Method name becomes test name in output

3. **Global Usings** (in Directory.Build.props):
   ```xml
   <Using Include="TimeWarp.Cli.Test.Helpers.TestRunner" Static="true" />
   <Using Include="TimeWarp.Cli.Test.Helpers.Asserts" Static="true" />
   <Using Include="TimeWarp.Cli.Test.Helpers.TestHelpers" Static="true" />
   ```

4. **TestRunner Features**:
   - Discovers all public static Task methods
   - Formats test names (PascalCase to spaced)
   - Tracks pass/fail counts
   - Provides summary output
   - Returns appropriate exit code

### Example Test File

```csharp
#!/usr/bin/dotnet --

await RunTests<CommandResultTests>();

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class CommandResultTests
#pragma warning restore CA1812
{
  static CommandOptions noValidation = new CommandOptions().WithNoValidation();

  public static async Task TestEmptyCommandReturnsEmptyString()
  {
    string result = await Shell.Builder("").GetStringAsync();
    AssertTrue(string.IsNullOrEmpty(result), "Empty command should return empty string");
  }

  public static async Task TestCommandThrowsOnError()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("false").ExecuteAsync(),
      "Command with non-zero exit should throw"
    );
  }
}
```

## Migration Path

1. Identify existing inline test scripts
2. Refactor to class-based structure
3. Update to use TestRunner.RunTests
4. Remove manual test tracking code
5. Verify test output and behavior

## Links

* Related to [ADR-0008](0008-integration-testing-strategy.md) - Integration Testing Strategy
* TestRunner implementation: `/Tests/TimeWarp.Cli.Test.Helpers/TestRunner.cs`
* Example usage: `/Tests/Integration/Core/CommandResult.ErrorHandling.cs`