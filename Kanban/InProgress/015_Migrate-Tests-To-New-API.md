# 015 Migrate Tests to New API

## Description

Update all existing tests to use the new API methods, replacing legacy methods with their new equivalents. Add new tests for RunAsync() behavior which is currently untested.

## Requirements

- Replace all GetStringAsync() usage
- Replace all GetLinesAsync() usage
- Replace ExecuteAsync() with appropriate new method
- Add tests for new streaming behavior
- Ensure all tests pass

## Checklist

### Migration Tasks
- [ ] Update Tests/Integration/Core/CommandExtensions.cs
  - [ ] Replace GetStringAsync() with CaptureAsync().Result.Stdout
  - [ ] Replace GetLinesAsync() with CaptureAsync().Result.Lines
  - [ ] Update assertions to check CommandOutput properties
  
- [ ] Update Tests/Integration/Core/CommandResult.*.cs files
  - [ ] CommandResult.Pipeline.cs
  - [ ] CommandResult.OutputFormats.cs
  - [ ] CommandResult.Interactive.cs
  - [ ] CommandResult.ErrorHandling.cs
  
- [ ] Update Tests/Integration/Core/RunBuilder.cs
  - [ ] Test new RunAsync() method
  - [ ] Test new CaptureAsync() method
  - [ ] Test streaming methods

### New Test Coverage
- [ ] Add RunAsync() tests
  - [ ] Verify output streams to console
  - [ ] Verify returns only exit code
  - [ ] Test with commands that output to stderr
  
- [ ] Add CaptureAsync() tests
  - [ ] Verify silent execution (no console output)
  - [ ] Verify captures both stdout and stderr
  - [ ] Verify CommandOutput structure
  
- [ ] Add streaming tests
  - [ ] Test StreamStdoutAsync() doesn't buffer
  - [ ] Test StreamStderrAsync() handles errors
  - [ ] Test StreamCombinedAsync() preserves order
  
- [ ] Add CancellationToken tests
  - [ ] Test cancellation stops execution
  - [ ] Test timeout creates cancellation
  - [ ] Test graceful vs forced termination

### Verification
- [ ] All tests compile
- [ ] All tests pass
- [ ] No references to removed methods remain
- [ ] Code coverage maintained or improved

## Notes

### Migration Patterns
```csharp
// Old pattern
string result = await Shell.Builder("echo", "test").GetStringAsync();
Assert.Equal("test", result.Trim());

// New pattern  
var output = await Shell.Builder("echo", "test").CaptureAsync();
Assert.Equal("test", output.Stdout.Trim());
Assert.Equal(0, output.ExitCode);
Assert.True(output.Success);
```

### Testing Console Output
For RunAsync() tests that verify console output:
```csharp
// Redirect console output for testing
var originalOut = Console.Out;
var consoleOutput = new StringWriter();
Console.SetOut(consoleOutput);

try
{
    int exitCode = await Shell.Builder("echo", "test").RunAsync();
    Assert.Equal(0, exitCode);
    Assert.Contains("test", consoleOutput.ToString());
}
finally
{
    Console.SetOut(originalOut);
}
```

## Dependencies

- 013_Remove-Legacy-Methods.md (old methods must be removed)
- 014_Implement-CommandMock.md (for mock testing)

## References

- Tests/Integration/Core/*.cs files
- Analysis/Architecture/CoreShellApi.md