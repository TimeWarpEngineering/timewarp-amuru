# How to Use ScriptContext for .NET 10 File-Based Apps

## Overview

`ScriptContext` provides automatic working directory management for .NET 10 file-based apps (single-file C# scripts). It ensures your script's working directory is properly set and restored, even when using `Environment.Exit()`.

## Prerequisites

- .NET 10 SDK
- TimeWarp.Amuru package

## Basic Usage

### Working in Script's Directory

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru

using TimeWarp.Amuru;

// Automatically change to script's directory and restore on disposal
using (var context = ScriptContext.FromEntryPoint())
{
    Console.WriteLine($"Script: {context.ScriptFilePath}");
    Console.WriteLine($"Working in: {context.ScriptDirectory}");
    
    // Your script operations here
    // Working directory is the script's directory
    var files = await Shell.Run("ls").GetLinesAsync();
}
// Original directory automatically restored
```

## Advanced Scenarios

### Navigate to a Relative Path

When your script needs to work from a different directory relative to the script's location:

```csharp
using (var context = ScriptContext.FromRelativePath(".."))
{
    // Now working from parent directory
    await Shell.Run("dotnet", "build").ExecuteAsync();
}
// Original directory restored
```

### Custom Cleanup Actions

Add cleanup code that runs even if the script exits abruptly:

```csharp
using (var context = ScriptContext.FromEntryPoint(
    changeToScriptDirectory: true,
    onExit: () => 
    {
        Console.WriteLine("Cleaning up temporary files...");
        File.Delete("temp.txt");
    }))
{
    // Your script work
    if (error)
        Environment.Exit(1); // Cleanup still runs!
}
```

### Controlling Directory Change

Sometimes you want the context without changing directories:

```csharp
using (var context = ScriptContext.FromEntryPoint(changeToScriptDirectory: false))
{
    // Access script path without changing directory
    Console.WriteLine($"Script at: {context.ScriptFilePath}");
    Console.WriteLine($"Still in: {Directory.GetCurrentDirectory()}");
}
```

## How It Works

1. **On Creation**: Captures current directory, optionally changes to script or relative directory
2. **During Use**: Provides access to script metadata through properties
3. **On Disposal**: Restores original directory and runs cleanup callbacks
4. **On Process Exit**: Intercepts `Environment.Exit()` and unhandled exceptions to ensure cleanup

## Best Practices

1. **Always use `using` blocks** to ensure proper disposal
2. **Place ScriptContext at the start** of your script's Main method
3. **Use relative paths** after changing directory
4. **Add cleanup callbacks** for critical resources
5. **Prefer ScriptContext over manual directory management**

## Common Patterns

### Build Script Pattern
```csharp
using (var context = ScriptContext.FromRelativePath(".."))
{
    await DotNet.Build()
        .WithProject("./Source/Project.csproj")
        .ExecuteAsync();
}
```

### Test Runner Pattern
```csharp
using (var context = ScriptContext.FromEntryPoint())
{
    var testFiles = await Shell.Run("find")
        .WithArguments(".", "-name", "*.Test.cs")
        .GetLinesAsync();
        
    foreach (var test in testFiles)
    {
        await Shell.Run(test).ExecuteAsync();
    }
}
```

## Troubleshooting

### Script Path is Null
- Ensure you're running as a .NET 10 file-based app (not `dotnet run` on a project)
- Use the shebang: `#!/usr/bin/dotnet --`

### Directory Not Restored
- Check that you're using a `using` block or calling `Dispose()`
- The directory is restored even with `Environment.Exit()`, but not on process kill

### Cleanup Not Running
- Cleanup runs on normal disposal, `Environment.Exit()`, and unhandled exceptions
- It does NOT run on `kill -9` or Windows Task Manager force-quit

## See Also

- [How to Use AppContext Extensions](HowToUseAppContextExtensions.md)
- [.NET 10 File-Based Apps Documentation](https://docs.microsoft.com/dotnet/core/file-based-apps)