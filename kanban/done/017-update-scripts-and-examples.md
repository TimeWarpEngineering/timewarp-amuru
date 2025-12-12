# 017 Update Scripts and Examples

## Description

Update all example scripts and spikes to use the new API. Note that Scripts/ directory should continue using raw Process to avoid circular dependencies.

## Requirements

- Update all Spikes/CsScripts examples
- Keep Scripts/ using raw Process
- Ensure examples demonstrate best practices
- Show variety of use cases

## Checklist

### Spikes/CsScripts Updates
- [ ] Update app.cs
  - [ ] Replace GetStringAsync() usage
  - [ ] Show RunAsync() for default behavior
  
- [ ] Update TestRun.cs
  - [ ] Use new testing infrastructure
  - [ ] Demonstrate CommandMock usage
  
- [ ] Update FindPs1Files.cs
  - [ ] Use streaming methods for large file lists
  - [ ] Show IAsyncEnumerable usage
  
- [ ] Update FindLunaPs1FilesWithFzfAsync.cs
  - [ ] Demonstrate piping to interactive tools
  - [ ] Use SelectAsync() for fzf
  
- [ ] Update CommandExtensions.cs spike
  - [ ] Show variety of new methods
  - [ ] Demonstrate error handling
  
- [ ] Update CliWrapApp.cs examples
  - [ ] Show migration from CliWrap patterns
  - [ ] Demonstrate equivalent functionality

### Example Patterns to Include
- [ ] Basic command execution (RunAsync)
- [ ] Capturing output (CaptureAsync)
- [ ] Streaming large outputs
- [ ] Error handling patterns
- [ ] CancellationToken usage
- [ ] Timeout patterns
- [ ] Pipeline examples
- [ ] Interactive tool integration

### Documentation Examples
- [ ] Create migration examples showing old vs new
- [ ] Create best practice examples
- [ ] Show common patterns
- [ ] Include error handling

### Validation
- [ ] All examples compile
- [ ] All examples run correctly
- [ ] Examples follow new best practices
- [ ] No usage of removed methods

## Notes

### Key Example Patterns

#### Default Script Pattern
```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru

// Most common - just run and show output
await Shell.Builder("npm", "install").RunAsync();

// Only capture when you need the output
var version = await Shell.Builder("node", "--version").CaptureAsync();
Console.WriteLine($"Node version: {version.Stdout}");
```

#### Streaming Large Files
```csharp
// Process large log file without loading into memory
await foreach (var line in Shell.Builder("cat", "/var/log/app.log").StreamStdoutAsync())
{
    if (line.Contains("ERROR"))
    {
        Console.WriteLine($"Found error: {line}");
    }
}
```

#### Error Handling
```csharp
var result = await Shell.Builder("git", "status")
    .WithWorkingDirectory("/some/path")
    .CaptureAsync();

if (!result.Success)
{
    Console.Error.WriteLine($"Git failed: {result.Stderr}");
    Environment.Exit(result.ExitCode);
}
```

### Scripts/ Directory Note
The Scripts/ directory (Build.cs, Pack.cs, Clean.cs) should continue using raw System.Diagnostics.Process to avoid circular dependencies. Add a comment explaining this:

```csharp
// This script uses raw Process instead of TimeWarp.Amuru
// to avoid circular dependencies when building the library itself
```

## Dependencies

- 016_Update-Builders.md (builders must be updated first)

## References

- Spikes/CsScripts/*.cs
- Scripts/*.cs (DO NOT UPDATE - keep using Process)