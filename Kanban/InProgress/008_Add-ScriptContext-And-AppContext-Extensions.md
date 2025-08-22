# Task 008: Add ScriptContext and AppContext Extensions

## Issue Reference
GitHub Issue: #6

## Status
🚧 InProgress

## Description
Add .NET 10 file-based app helper extensions and ScriptContext to TimeWarp.Amuru to simplify script development.

## Background
.NET 10 introduces native support for file-based apps with new AppContext data:
- `AppContext.GetData("EntryPointFilePath")` - Path to the script file
- `AppContext.GetData("EntryPointFileDirectoryPath")` - Path to the script directory

Every file-based app needs:
1. Access to script location without magic strings
2. Pattern to change working directory and restore it on completion/failure

## Requirements
- [ ] Implement AppContextExtensions class with EntryPointFilePath and EntryPointFileDirectoryPath methods
- [ ] Implement ScriptContext class with IDisposable pattern for directory management
- [ ] Add FromEntryPoint method for working in script directory
- [ ] Add FromRoot method for working from repository root
- [ ] Update existing scripts to use ScriptContext
- [ ] Add tests for new functionality
- [ ] Bump version to 1.0.0-beta.2

## Implementation Details

### AppContextExtensions
```csharp
public static class AppContextExtensions
{
    public static string? EntryPointFilePath(this AppContext _) 
        => AppContext.GetData("EntryPointFilePath") as string;
        
    public static string? EntryPointFileDirectoryPath(this AppContext _) 
        => AppContext.GetData("EntryPointFileDirectoryPath") as string;
}
```

### ScriptContext
- Sealed class implementing IDisposable
- Stores original directory and restores on disposal
- FromEntryPoint method for script directory
- FromRoot method for repository root navigation
- Properties: ScriptDirectory, ScriptFilePath

## Notes
- Follows disposable pattern for automatic cleanup
- No magic strings required
- Clean, readable scripts
- Reusable across all file-based apps

## References
- [.NET 10 Preview 6 SDK File-Based Apps](https://github.com/dotnet/core/blob/main/release-notes/10.0/preview/preview6/sdk.md#file-based-apps)