# 014 Implement CommandMock

## Description

Create the CommandMock testing infrastructure using AsyncLocal for thread safety and test isolation. This provides simple, safe mocking for scripts without requiring dependency injection.

## Requirements

- Thread-safe using AsyncLocal
- Test isolation - no bleeding between tests
- Automatic cleanup via IDisposable
- No global state
- Simple API for script writers

## Checklist

### Implementation
- [ ] Create CommandMock static class
  - [ ] Use AsyncLocal<MockState?> for thread-safe state
  - [ ] Implement Enable() method returning IDisposable
  - [ ] Implement Setup() method for configuring mocks
  - [ ] Implement VerifyCalled() for assertions
  - [ ] Implement CallCount() for verification
  
- [ ] Create MockState internal class
  - [ ] Store mock setups per command
  - [ ] Track calls made during test
  - [ ] Handle pattern matching for arguments
  
- [ ] Create MockSetup fluent class
  - [ ] Returns(stdout, stderr, exitCode) method
  - [ ] ReturnsError(stderr, exitCode) method
  - [ ] Throws<TException>() method
  - [ ] Delays(TimeSpan) method for timing tests
  
- [ ] Create MockScope IDisposable
  - [ ] Clear AsyncLocal on disposal
  - [ ] Ensure cleanup even on test failure

### Integration
- [ ] Hook into Shell.Builder execution path
- [ ] Check if mock is enabled before real execution
- [ ] Return mock results when configured
- [ ] Track calls for verification

### Testing
- [ ] Test basic mock setup and return
- [ ] Test parallel test isolation
- [ ] Test automatic cleanup on disposal
- [ ] Test verification methods
- [ ] Test exception throwing
- [ ] Test no bleeding between tests

## Notes

### Usage Example
```csharp
[Test]
public async Task GitStatusTest()
{
    using (CommandMock.Enable())  // Scoped to this test only
    {
        CommandMock.Setup("git", "status")
            .Returns("On branch main\nnothing to commit");
        
        var result = await Shell.Builder("git", "status").CaptureAsync();
        
        Assert.Equal("On branch main\nnothing to commit", result.Stdout);
        CommandMock.VerifyCalled("git", "status");
    } // Automatically cleaned up
}
```

### Parallel Safety Example
```csharp
await Parallel.ForEachAsync(Enumerable.Range(1, 100), async (i, _) =>
{
    using (CommandMock.Enable())  // Each parallel execution isolated
    {
        CommandMock.Setup("echo", i.ToString())
            .Returns($"Echo {i}");
        
        var result = await Shell.Builder("echo", i.ToString()).CaptureAsync();
        Assert.Equal($"Echo {i}", result.Stdout);
    }
});
```

## Dependencies

- 012_Implement-Core-Methods.md (need CaptureAsync to test)

## References

- Analysis/Architecture/TestingStrategy.md
- Analysis/Architecture/FinalConsensus.md