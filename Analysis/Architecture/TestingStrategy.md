# TimeWarp.Amuru Testing Strategy

## The Problem

Real process execution makes unit testing difficult:
- Tests become slow (process startup overhead)
- Tests become flaky (external dependencies)
- Tests become environment-dependent (PATH, installed tools)
- Hard to test error scenarios
- Difficult to verify command calls and arguments

## Proposed Solution: Full Testing Infrastructure

### Core Abstraction (ICommandExecutor)

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

### Static Access with Override

For scripts that don't use DI:

```csharp
public static class CommandExecutor
{
    private static ICommandExecutor? customExecutor;
    private static readonly ICommandExecutor defaultExecutor = new DefaultCommandExecutor();
    
    public static ICommandExecutor Current => customExecutor ?? defaultExecutor;
    
    public static void UseExecutor(ICommandExecutor executor) => customExecutor = executor;
    public static void UseDefault() => customExecutor = null;
}

// Usage in tests
var mockExecutor = new MockCommandExecutor();
mockExecutor.Setup("git", "status")
    .ReturnsOutput("On branch main\nnothing to commit");

CommandExecutor.UseExecutor(mockExecutor);
// Run tests
CommandExecutor.UseDefault();
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

### Advanced Testing Features

#### Fluent Mock Builder
```csharp
var mock = new MockCommandExecutor()
    .WhenCommand("docker", "build", ".")
        .StreamsOutput("Building image...")
        .DelaysFor(TimeSpan.FromSeconds(2))
        .ThenStreamsOutput("Successfully built abc123")
        .Returns(0)
    .WhenCommand("docker", "push")
        .ThrowsOnCancellation()
    .WhenCommandMatches(cmd => cmd.StartsWith("find"))
        .ReturnsOutput("file1.txt\nfile2.txt");
```

#### Record and Replay
```csharp
// Record mode - captures real outputs
var recorder = new CommandRecorder("test-fixtures/git-commands.json");
CommandExecutor.UseExecutor(recorder);

// Run your code - real commands execute and get recorded
await Shell.Builder("git", "status").CaptureAsync();
await Shell.Builder("git", "log", "--oneline", "-5").CaptureAsync();

await recorder.SaveAsync();

// Replay mode - uses recorded outputs
var replay = await CommandReplay.LoadAsync("test-fixtures/git-commands.json");
CommandExecutor.UseExecutor(replay);

// Same commands now return recorded outputs - no real execution
var status = await Shell.Builder("git", "status").CaptureAsync();
```

#### Fake File System Integration
```csharp
// Using System.IO.Abstractions
var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
{
    { "/app/config.json", new MockFileData("{ \"key\": \"value\" }") },
    { "/app/data.txt", new MockFileData("test data") }
});

var mockExecutor = new MockCommandExecutor(fileSystem);
mockExecutor.Setup("cat", "/app/config.json")
    .ReturnsFileContent();  // Automatically uses mock file system
```

## Alternative: Simple Mock Mode

A simpler approach without DI or abstractions:

```csharp
// Enable mock mode for testing
CommandMock.Enable();

// Setup mocks
CommandMock.Setup("git", "status")
    .Returns(stdout: "On branch main", exitCode: 0);

// Your code runs normally
var result = await Shell.Builder("git", "status").CaptureAsync();

// Verify calls
Assert.True(CommandMock.WasCalled("git", "status"));

// Reset after test
CommandMock.Reset();
```

## Considerations

### For Scripts (Primary Use Case)
- Scripts typically don't use DI
- Simple mock mode might be sufficient
- Most script writers might not test at all

### For Applications (TUIs, Hosted Services)
- Nuru supports DI in console apps
- Long-lived TUIs might benefit from proper DI
- ASP.NET hosted services definitely need DI support
- Testability becomes more important

### Questions to Resolve
1. Do we support both approaches (simple mock + full DI)?
2. Is the complexity worth it for a scripting library?
3. Should DI support be in a separate package (TimeWarp.Amuru.Testing)?
4. Do we need record/replay or is that overengineering?

## Testing Strategy Comparison

| Feature | Simple Mock | Full DI/Abstraction |
|---------|------------|-------------------|
| Setup Complexity | Low | Medium |
| Script Friendly | ✅ | ❌ |
| DI Support | ❌ | ✅ |
| Verification | Basic | Full |
| Performance | Fast | Fast |
| Learning Curve | Easy | Moderate |
| TUI/App Support | Limited | Full |