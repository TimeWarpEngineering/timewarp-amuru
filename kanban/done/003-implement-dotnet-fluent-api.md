# Implement DotNet Fluent API

## Description

Create a fluent API abstraction for .NET CLI commands, similar to the FileSystem.Find() example in Overview.md. This will provide strongly-typed, IntelliSense-friendly methods for common dotnet operations while building on the existing TimeWarp.Cli foundation.

Start with DotNet.Run() as the first implementation, then expand to Build, Test, and package management commands.

## Requirements

- Create `DotNet` static class with fluent interface methods
- Implement `DotNet.Run()` with fluent configuration options
- Support common dotnet CLI parameters (project, configuration, arguments, etc.)
- Maintain async-first design consistent with existing API
- Follow project's C# coding standards (Allman braces, 2-space indentation)
- Build on existing `CommandExtensions.Run()` rather than replacing it

## Checklist

### Design
- [x] Design DotNet static class API surface
- [x] Define fluent interface pattern for dotnet commands
- [x] Plan integration with existing CommandResult pipeline
- [x] Consider backward compatibility with raw Run() method

### Implementation
- [x] Create DotNet.Run() with fluent builder pattern
- [x] Add support for --project parameter
- [x] Add support for --configuration parameter  
- [x] Add support for program arguments (after --)
- [x] Add support for common dotnet run options
- [x] Implement async execution methods
- [x] Add integration tests for DotNet.Run()

### Documentation
- [x] Update API documentation
- [x] Add usage examples to Overview.md
- [x] Update CLAUDE.md with new API patterns

## Notes

The fluent API should feel natural and provide strong typing while maintaining the simplicity of the current Run() method. Example target syntax:

```csharp
// Simple execution
var output = await DotNet.Run()
    .WithProject("MyApp.csproj")
    .WithConfiguration("Release")
    .WithArguments("arg1", "arg2")
    .GetStringAsync();

// Integration with existing pipeline
var result = await DotNet.Run()
    .WithProject("MyConsoleApp.csproj")
    .Pipe("grep", "ERROR")
    .GetLinesAsync();
```

This should be the foundation for expanding to other dotnet commands like Build(), Test(), etc.

## Implementation Notes

✅ **COMPLETED** - DotNet.Run() fluent API implemented and tested

**Implementation Summary:**
- Created `DotNet` static class with `Run()` method that returns `DotNetRunBuilder`
- Implemented fluent builder pattern with methods like `WithProject()`, `WithConfiguration()`, `WithArguments()`, etc.
- Added support for all common dotnet run options (--project, --configuration, --framework, --runtime, --no-restore, --no-build, --verbosity)
- Integrated with existing `CommandOptions` for working directory and environment variables
- All methods return `DotNetRunBuilder` for method chaining
- Final execution via `Build()`, `GetStringAsync()`, `GetLinesAsync()`, or `ExecuteAsync()`
- Maintains backward compatibility with existing `Run()` method
- Added comprehensive integration tests - all 5 tests passing
- Version bumped to 0.6.0-beta4 to resolve package caching issues

**Files Created/Modified:**
- `Source/TimeWarp.Cli/DotNet.cs` - New fluent API implementation
- `Tests/Integration/DotNetFluentApi.cs` - Integration tests
- `Source/TimeWarp.Cli/TimeWarp.Cli.csproj` - Version bump
- `Overview.md` - Added usage examples
- `CLAUDE.md` - Updated with new API patterns