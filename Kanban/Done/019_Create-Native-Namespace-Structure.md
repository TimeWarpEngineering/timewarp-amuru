# 019 Create Native Namespace Structure

## Description

Set up the namespace structure for Native commands using PowerShell-inspired naming with bash aliases for familiarity. This establishes the foundation for in-process implementations of common shell commands with both Commands (shell-like) and Direct (LINQ-composable) APIs.

## Requirements

- Create PowerShell-inspired namespace structure with clear verbs (Get, Set, New, etc.)
- Implement first native command with both primary names and bash aliases
- Demonstrate both Commands and Direct patterns
- Set up for user control via global usings

## Checklist

### Namespace Structure
- [x] Create Source/TimeWarp.Amuru/Native/ directory structure
  - [x] Native/FileSystem/ directory (handles files and directories)
  - [x] Native/Text/ directory
  - [x] Native/SystemInfo/ directory
  - [x] Native/Process/ directory

### FileSystem Namespace Implementation
- [x] Create Native/FileSystem/Commands.cs
  - [x] Static class Commands
  - [x] Implement GetContent(string path) returning CommandOutput (primary)
  - [x] Implement GetChildItem(string path = ".") returning CommandOutput (primary)
  - [x] Add bash aliases: Cat() => GetContent(), Ls() => GetChildItem()
  - [x] Handle file not found with proper stderr/exit code
  - [x] No exceptions - shell semantics

- [x] Create Native/FileSystem/Direct.cs
  - [x] Static class Direct
  - [x] Implement GetContent(string path) returning IAsyncEnumerable<string>
  - [x] Implement GetChildItem(string path = ".") returning IAsyncEnumerable<FileSystemInfo>
  - [x] Stream results without buffering
  - [x] Can throw exceptions (C# semantics)

### CommandOutput Integration
- [x] Ensure Native.GetContent returns proper CommandOutput
  - [x] Stdout contains file/directory content
  - [x] Stderr contains error messages if any
  - [x] ExitCode 0 for success, 1 for failure
  - [x] Success property works correctly

### Global Usings Setup
- [x] Create example GlobalUsings patterns
  - [x] PowerShell-style: `global using static TimeWarp.Amuru.Native.FileSystem.Commands;`
  - [x] Bash-style: `global using static TimeWarp.Amuru.Native.Aliases.Bash;`
  - [x] Show how users control verbosity

### Alias System
- [x] Create Native/Aliases/Bash.cs
  - [x] Static methods that delegate to primary implementations
  - [x] Cat() => FileSystem.Commands.GetContent()
  - [x] Ls() => FileSystem.Commands.GetChildItem()
  - [x] Allow both naming conventions

### Testing
- [x] Test Commands.GetContent with existing file
- [x] Test Commands.GetContent with missing file
- [x] Test Commands.GetChildItem with valid directory
- [x] Test bash aliases work correctly
- [x] Test Direct.GetContent streaming behavior
- [x] Test both APIs can coexist
- [x] Test global using patterns

## Notes

### PowerShell-Inspired Naming Strategy

**Primary API (PowerShell-style):**
- `GetContent()` - reads file content (like `type` or `cat`)
- `GetChildItem()` - lists directory contents (like `ls` or `dir`)
- `SetContent()` - writes file content
- `NewItem()` - creates files/directories
- `CopyItem()` - copies files/directories
- `MoveItem()` - moves/renames files/directories
- `RemoveItem()` - deletes files/directories

**Bash Aliases (for familiarity):**
- `Cat()` => `GetContent()`
- `Ls()` => `GetChildItem()`
- `Dir()` => `GetChildItem()`

### Implementation Example - Commands.GetContent

```csharp
namespace TimeWarp.Amuru.Native.FileSystem;

public static class Commands
{
    public static CommandOutput GetContent(string path)
    {
        try
        {
            string content = System.IO.File.ReadAllText(path);
            return new CommandOutput(
                stdout: content,
                stderr: string.Empty,
                exitCode: 0
            );
        }
        catch (FileNotFoundException)
        {
            return new CommandOutput(
                stdout: string.Empty,
                stderr: $"GetContent: {path}: No such file or directory",
                exitCode: 1
            );
        }
        catch (Exception ex)
        {
            return new CommandOutput(
                stdout: string.Empty,
                stderr: $"GetContent: {path}: {ex.Message}",
                exitCode: 1
            );
        }
    }

    // Bash aliases for familiarity
    public static CommandOutput Cat(string path) => GetContent(path);
}
```

### Implementation Example - Commands.GetChildItem

```csharp
namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
    public static CommandOutput GetChildItem(string path = ".")
    {
        try
        {
            var dir = new DirectoryInfo(path);
            var entries = dir.GetFileSystemInfos();
            var output = string.Join("\n", entries.Select(e =>
                $"{e.Name}\t{(e is DirectoryInfo ? "<DIR>" : e is FileInfo f ? f.Length.ToString() : "")}"));

            return new CommandOutput(
                stdout: output,
                stderr: string.Empty,
                exitCode: 0
            );
        }
        catch (DirectoryNotFoundException)
        {
            return new CommandOutput(
                stdout: string.Empty,
                stderr: $"GetChildItem: {path}: No such directory",
                exitCode: 1
            );
        }
    }

    // Bash aliases
    public static CommandOutput Ls(string path = ".") => GetChildItem(path);
    public static CommandOutput Dir(string path = ".") => GetChildItem(path);
}
```

### Implementation Example - Direct.GetContent

```csharp
namespace TimeWarp.Amuru.Native.FileSystem;

public static class Direct
{
    public static async IAsyncEnumerable<string> GetContent(string path)
    {
        using var reader = System.IO.File.OpenText(path);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            yield return line;
        }
    }

    // Alias for familiarity
    public static IAsyncEnumerable<string> Cat(string path) => GetContent(path);
}
```

### Usage Examples

```csharp
// PowerShell-style with global using
global using static TimeWarp.Amuru.Native.FileSystem.Commands;

var fileContent = GetContent("readme.md");
var dirListing = GetChildItem("/app/src");
if (!fileContent.Success)
{
    Console.Error.WriteLine(fileContent.Stderr);
    Environment.Exit(fileContent.ExitCode);
}

// Bash-style with global using
global using static TimeWarp.Amuru.Native.Aliases.Bash;

var fileContent = Cat("readme.md");
var dirListing = Ls("/app/src");

// LINQ-style with global using
global using static TimeWarp.Amuru.Native.FileSystem.Direct;

await foreach (var line in GetContent("huge.log"))
{
    if (line.Contains("ERROR"))
        Console.WriteLine(line);
}

// Or with bash alias
await foreach (var line in Cat("huge.log"))
{
    ProcessLine(line);
}
```

### Implementation Example - Alias System

```csharp
// Native/Aliases/Bash.cs
namespace TimeWarp.Amuru.Native.Aliases;

public static class Bash
{
    // File operations
    public static CommandOutput Cat(string path) =>
        FileSystem.Commands.GetContent(path);

    public static IAsyncEnumerable<string> Cat(string path) =>
        FileSystem.Direct.GetContent(path);

    public static CommandOutput Ls(string path = ".") =>
        FileSystem.Commands.GetChildItem(path);

    // Text operations
    public static CommandOutput Grep(string pattern, string input) =>
        Text.Commands.SelectString(pattern, input);

    // System operations
    public static CommandOutput Pwd() =>
        SystemInfo.Commands.GetLocation();
}
```

## Benefits of This Approach

1. **PowerShell Intelligence**: Structured, consistent naming with clear verbs
2. **C# Compatibility**: No hyphens, follows .NET naming conventions
3. **Progressive Disclosure**: New users learn the full names, experts use aliases
4. **IDE Friendly**: IntelliSense shows both `GetContent` and `Cat`
5. **Cross-Platform Consistency**: Same API works everywhere

## Future Work

This task sets up the foundation. Future tasks will add:
- More FileSystem commands (SetContent, NewItem, CopyItem)
- Text commands (SelectString, MeasureObject, SortObject)
- SystemInfo commands (GetProcess, GetLocation, SetLocation)
- Hybrid implementations (native with external fallback)

## Dependencies

- 011_Create-CommandOutput-Class.md (need CommandOutput structure)
- All previous tasks should be complete

## References

- Analysis/Architecture/NativeNamespaceDesign.md
- Analysis/Architecture/NativeApiDesign.md
- Analysis/Architecture/BuilderExtensibility.md