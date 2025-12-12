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
- [x] Update RunBuilder class
  - [x] Add RunAsync(CancellationToken) method
  - [x] Add CaptureAsync(CancellationToken) method
  - [x] Add streaming methods with CancellationToken
  - [x] Temporarily keep obsolete methods with [Obsolete] attribute
  - Note: Will fully remove obsolete methods in final cleanup task
  
- [x] Update ICommandBuilder interface
  - No changes needed - interface doesn't contain obsolete methods

### Specialized Builders
- [x] Update DotNet.Base.cs
  - [x] Added new API methods: RunAsync, CaptureAsync, PassthroughAsync, SelectAsync
  - [x] Kept obsolete methods with [Obsolete] attribute for compatibility
  - Note: Individual DotNet builders inherit from Base and will work
  
- Note: Fzf, Ghq, Gwq builders don't override obsolete methods
  - They use the builder pattern and will inherit new methods

### Validation
- [x] All builders compile
- [x] Builder tests pass (NewApiTests: 15/15 passed)
- [x] Temporary compatibility maintained via [Obsolete] methods
- [x] CancellationToken properly threaded

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