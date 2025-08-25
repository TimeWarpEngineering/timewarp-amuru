# 016 Update Builders

## Description

Update all command builders (RunBuilder, DotNet, Fzf, Ghq, Gwq) to use the new API methods and ensure all expose CancellationToken parameters properly.

## Requirements

- Update RunBuilder to expose new methods
- Update all specialized builders
- Ensure CancellationToken flows through
- Maintain backward compatibility where possible

## Checklist

### Core Builder Updates
- [ ] Update RunBuilder class
  - [ ] Add RunAsync(CancellationToken) method
  - [ ] Add CaptureAsync(CancellationToken) method
  - [ ] Add streaming methods with CancellationToken
  - [ ] Remove GetStringAsync() and GetLinesAsync()
  - [ ] Remove any caching-related code
  
- [ ] Update ICommandBuilder interface
  - [ ] Add new method signatures
  - [ ] Remove deprecated method signatures
  - [ ] Ensure CancellationToken in all async methods

### Specialized Builders
- [ ] Update DotNet builders
  - [ ] DotNet.Build to use new methods
  - [ ] DotNet.Test to use new methods
  - [ ] DotNet.Run to use appropriate method (likely RunAsync)
  - [ ] All other DotNet.* commands
  
- [ ] Update Fzf builder
  - [ ] Fzf likely needs SelectAsync() for interactive mode
  - [ ] Update input/output handling
  - [ ] Ensure preview commands work
  
- [ ] Update Ghq builder
  - [ ] Update list/get/remove commands
  - [ ] These are hybrid native/external patterns
  - [ ] Ensure proper method usage
  
- [ ] Update Gwq builder
  - [ ] Similar to Ghq updates
  - [ ] Maintain worktree operations
  - [ ] Update status/add/remove commands

### Validation
- [ ] All builders compile
- [ ] Builder tests pass
- [ ] No references to removed methods
- [ ] CancellationToken properly threaded

## Notes

### Builder Pattern Updates
```csharp
// Old builder pattern
public async Task<string> BuildAsync()
{
    return await Build().GetStringAsync();
}

// New builder pattern  
public async Task<int> BuildAsync(CancellationToken cancellationToken = default)
{
    return await Build().RunAsync(cancellationToken);
}

public async Task<CommandOutput> BuildAndCaptureAsync(CancellationToken cancellationToken = default)
{
    return await Build().CaptureAsync(cancellationToken);
}
```

### DotNet.Run Special Case
```csharp
// DotNet.Run should probably use RunAsync() by default
public async Task<int> RunAsync(CancellationToken cancellationToken = default)
{
    // Stream to console like 'dotnet run' does
    return await Build().RunAsync(cancellationToken);
}
```

### Interactive Builders (Fzf)
```csharp
// Fzf needs special handling for interactive selection
public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
{
    // Use the interactive/passthrough mode
    return await Build().SelectAsync(cancellationToken);
}
```

## Dependencies

- 012_Implement-Core-Methods.md (need new methods)
- 013_Remove-Legacy-Methods.md (remove old methods)

## References

- Source/TimeWarp.Amuru/Core/RunBuilder.cs
- Source/TimeWarp.Amuru/DotNetCommands/*.cs
- Source/TimeWarp.Amuru/FzfCommand/*.cs
- Source/TimeWarp.Amuru/GhqCommand/*.cs
- Source/TimeWarp.Amuru/GwqCommand/*.cs