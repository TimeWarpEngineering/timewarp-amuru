# TimeWarp.Amuru Testing Strategy

## Final Decision: Simple Mock with AsyncLocal

**Primary API: Thread-safe, simple mocking for scripts. DI support as future option.**

## The Problem

Real process execution makes unit testing difficult:
- Tests become slow (process startup overhead)
- Tests become flaky (external dependencies)
- Tests become environment-dependent (PATH, installed tools)
- Hard to test error scenarios
- Difficult to verify command calls and arguments

## Recommended Solution: Simple Mock Mode with AsyncLocal

### Primary API (Ships with Core Library)

Thread-safe, test-isolated mocking using AsyncLocal:

```csharp
namespace TimeWarp.Amuru.Testing;

public static class CommandMock
{
    private static readonly AsyncLocal<MockState?> state = new();
    
    // Enable mocking for current async context
    public static IDisposable Enable()
    {
        state.Value = new MockState();
        return new MockScope(() => state.Value = null);
    }
    
    // Setup a mock
    public static MockSetup Setup(string command, params string[] args)
    {
        if (state.Value == null)
            throw new InvalidOperationException("Call CommandMock.Enable() first");
        return state.Value.Setup(command, args);
    }
    
    // Verify calls
    public static void VerifyCalled(string command, params string[] args)
    {
        if (state.Value == null)
            throw new InvalidOperationException("Call CommandMock.Enable() first");
        state.Value.VerifyCalled(command, args);
    }
    
    public static int CallCount(string command, params string[] args)
        => state.Value?.CallCount(command, args) ?? 0;
}

public class MockSetup
{
    public MockSetup Returns(string stdout, string stderr = "", int exitCode = 0);
    public MockSetup ReturnsError(string stderr, int exitCode = 1);
    public MockSetup Throws<TException>() where TException : Exception, new();
    public MockSetup Delays(TimeSpan delay);
}
```

### Usage Patterns

```csharp
// Basic test with automatic cleanup
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

// Parallel test safety - each test isolated
[Test]
public async Task ParallelSafeTests()
{
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
}

// Error scenarios
[Test]
public async Task HandleCommandFailure()
{
    using (CommandMock.Enable())
    {
        CommandMock.Setup("docker", "build", ".")
            .ReturnsError("Cannot connect to Docker daemon", exitCode: 1);
        
        var result = await Shell.Builder("docker", "build", ".")
            .WithValidation(CommandResultValidation.None)
            .CaptureAsync();
        
        Assert.False(result.Success);
        Assert.Contains("Cannot connect", result.Stderr);
    }
}
```

### Benefits of AsyncLocal Approach

1. **Test Isolation**: Each test gets its own mock state
2. **Thread Safety**: AsyncLocal ensures no bleeding between parallel tests
3. **Automatic Cleanup**: IDisposable pattern ensures cleanup
4. **No Global State**: No static fields that persist between tests
5. **Simple API**: Just Enable() and Setup()

## Future Options (Separate Packages)

### Option 1: Full DI Support (TimeWarp.Amuru.Testing.DI)

Provide a mockable interface for all command execution:

```csharp
public interface ICommandExecutor
{
    Task<CommandOutput> ExecuteAsync(CommandRequest request, CancellationToken token = default);
    IAsyncEnumerable<CommandLine> StreamAsync(CommandRequest request, CancellationToken token = default);
}

public class CommandRequest
{
    public string Executable { get; set; }
    public string[] Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; }
    public string? StandardInput { get; set; }
    public TimeSpan? Timeout { get; set; }
}
```

### Mock Implementation

Full-featured mock with fluent setup API:

```csharp
public class MockCommandExecutor : ICommandExecutor
{
    private readonly Dictionary<string, MockSetup> setups = new();
    
    public MockSetup Setup(string command, params string[] args)
    {
        var key = $"{command} {string.Join(" ", args)}";
        var setup = new MockSetup();
        setups[key] = setup;
        return setup;
    }
    
    public bool WasCalled(string command, params string[] args) { }
    public int CallCount(string command, params string[] args) { }
}

public class MockSetup
{
    public MockSetup ReturnsOutput(string stdout, string stderr = "", int exitCode = 0);
    public MockSetup ReturnsError(string stderr, int exitCode = 1);
    public MockSetup Throws<TException>() where TException : Exception, new();
    public MockSetup DelaysFor(TimeSpan delay);
}
```


### Dependency Injection Support

For applications using DI (ASP.NET, TUIs with DI, hosted services):

```csharp
// Register in DI container
services.AddSingleton<ICommandExecutor, DefaultCommandExecutor>();

// Or for testing
services.AddScoped<ICommandExecutor>(sp => 
{
    var mock = new MockCommandExecutor();
    mock.Setup("git", "status").ReturnsOutput("mocked");
    return mock;
});

// In your service
public class GitService
{
    private readonly ICommandExecutor executor;
    
    public GitService(ICommandExecutor executor)
    {
        this.executor = executor;
    }
    
    public async Task<string> GetCurrentBranch()
    {
        var result = await executor.RunCommandAsync("git", "branch", "--show-current");
        return result.Stdout.Trim();
    }
}
```

### Option 2: Record and Replay (TimeWarp.Amuru.Testing.Replay)

For testing against real external systems:

```csharp
public static class CommandRecorder
{
    public static async Task<IDisposable> StartRecording(string file)
    {
        // Records real command outputs to file
    }
    
    public static IDisposable PlaybackFrom(string file)
    {
        // Replays recorded outputs from file
    }
}

// First run - record real outputs
using (await CommandRecorder.StartRecording("fixtures/git-test.json"))
{
    await Shell.Builder("git", "status").CaptureAsync();  // Real execution
    await Shell.Builder("git", "log", "--oneline", "-5").CaptureAsync();
}

// Subsequent runs - replay without real execution
using (CommandRecorder.PlaybackFrom("fixtures/git-test.json"))
{
    var status = await Shell.Builder("git", "status").CaptureAsync();  // From recording
    var log = await Shell.Builder("git", "log", "--oneline", "-5").CaptureAsync();
}
```

## Why This Design

### Primary Users: Script Writers
- Most users are writing scripts, not apps
- They need simple, zero-config testing
- AsyncLocal provides safety without complexity

### Key Benefits
1. **Zero Config**: Just `using (CommandMock.Enable())`
2. **Test Isolation**: Each test completely isolated
3. **Thread Safe**: Parallel tests work perfectly
4. **Auto Cleanup**: IDisposable ensures cleanup
5. **Familiar API**: Similar to popular mocking libraries

### Future-Proof
- Core library ships with simple mocking only
- DI support can be added as separate package if needed
- Record/replay can be added as extension package
- No breaking changes to add these later

## Implementation Priority

1. **Ship with v1.0**: Simple CommandMock with AsyncLocal
2. **Based on feedback**: Add DI package if requested
3. **Based on usage**: Add record/replay if patterns emerge

## Summary

The AsyncLocal-based CommandMock provides the perfect balance:
- Simple enough for scripts
- Safe enough for parallel tests
- Extensible enough for future needs
- No global state risks
- Automatic cleanup

This is the testing solution that matches our library philosophy: simple, predictable, and just works.