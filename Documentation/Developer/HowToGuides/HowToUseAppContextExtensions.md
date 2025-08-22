# How to Use AppContext Extensions for .NET 10 File-Based Apps

## Overview

TimeWarp.Amuru provides extension methods for `AppContext` using C# 14's new extension syntax, giving you clean access to .NET 10's file-based app metadata without magic strings.

## Prerequisites

- .NET 10 SDK
- TimeWarp.Amuru package
- C# script file (not a project)

## Basic Usage

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;

// Access script metadata using extension methods
string? scriptPath = AppContext.EntryPointFilePath();
string? scriptDir = AppContext.EntryPointFileDirectoryPath();

Console.WriteLine($"Script: {scriptPath}");
Console.WriteLine($"Directory: {scriptDir}");
```

## What These Extensions Provide

### EntryPointFilePath()
Returns the full path to the currently executing script file.

**Example Output**: `/home/user/scripts/MyScript.cs`

### EntryPointFileDirectoryPath()
Returns the directory containing the currently executing script.

**Example Output**: `/home/user/scripts`

## Common Use Cases

### Locating Related Files

```csharp
string? scriptDir = AppContext.EntryPointFileDirectoryPath();
if (scriptDir != null)
{
    string configFile = Path.Combine(scriptDir, "config.json");
    if (File.Exists(configFile))
    {
        var config = File.ReadAllText(configFile);
        // Process configuration
    }
}
```

### Script Self-Identification

```csharp
string? scriptPath = AppContext.EntryPointFilePath();
if (scriptPath != null)
{
    Console.WriteLine($"Running: {Path.GetFileName(scriptPath)}");
    Console.WriteLine($"Extension: {Path.GetExtension(scriptPath)}");
    Console.WriteLine($"Last modified: {File.GetLastWriteTime(scriptPath)}");
}
```

### Finding Sibling Scripts

```csharp
string? scriptDir = AppContext.EntryPointFileDirectoryPath();
if (scriptDir != null)
{
    var otherScripts = Directory.GetFiles(scriptDir, "*.cs")
        .Where(f => f != AppContext.EntryPointFilePath())
        .ToList();
    
    Console.WriteLine("Other scripts in directory:");
    foreach (var script in otherScripts)
    {
        Console.WriteLine($"  - {Path.GetFileName(script)}");
    }
}
```

## How It Works

These extension methods wrap the .NET 10 AppContext data:
- `AppContext.GetData("EntryPointFilePath")` → `AppContext.EntryPointFilePath()`
- `AppContext.GetData("EntryPointFileDirectoryPath")` → `AppContext.EntryPointFileDirectoryPath()`

The C# 14 extension syntax allows us to add these methods to the static `AppContext` class, providing a cleaner API.

## When to Use

**Use AppContext Extensions when:**
- You need to know where your script is located
- You want to access files relative to the script
- You're building self-aware scripts
- You need script metadata for logging or debugging

**Use ScriptContext instead when:**
- You need to change the working directory
- You want automatic directory restoration
- You need cleanup callbacks
- You're managing directory state

## Null Handling

These methods return `null` when:
- Not running as a file-based app (e.g., compiled project)
- Running in environments that don't set this data

Always check for null:

```csharp
string? scriptPath = AppContext.EntryPointFilePath();
if (scriptPath == null)
{
    Console.WriteLine("Not running as a file-based app");
    return;
}
// Safe to use scriptPath
```

## Comparison with Traditional Approaches

### Before (Magic Strings)
```csharp
string? scriptPath = AppContext.GetData("EntryPointFilePath") as string;
string? scriptDir = AppContext.GetData("EntryPointFileDirectoryPath") as string;
```

### After (Extension Methods)
```csharp
string? scriptPath = AppContext.EntryPointFilePath();
string? scriptDir = AppContext.EntryPointFileDirectoryPath();
```

## Requirements

The script must:
1. Use the shebang: `#!/usr/bin/dotnet --`
2. Be executable: `chmod +x script.cs`
3. Include preview features for C# 14 extensions:
   ```csharp
   #:property LangVersion=preview
   #:property EnablePreviewFeatures=true
   ```

## See Also

- [How to Use ScriptContext](HowToUseScriptContext.md)
- [.NET 10 File-Based Apps Documentation](https://docs.microsoft.com/dotnet/core/file-based-apps)
- [C# 14 Extension Everything](https://github.com/dotnet/csharplang/blob/main/proposals/extensions.md)