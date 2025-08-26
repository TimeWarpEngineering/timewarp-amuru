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
- [ ] Create Source/TimeWarp.Amuru/Native/ directory structure
  - [ ] Native/FileSystem/ directory (handles files and directories)
  - [ ] Native/Text/ directory
  - [ ] Native/SystemInfo/ directory
  - [ ] Native/Process/ directory

### FileSystem Namespace Implementation
- [ ] Create Native/FileSystem/Commands.cs
  - [ ] Static class Commands
  - [ ] Implement GetContent(string path) returning CommandOutput (primary)
  - [ ] Implement GetChildItem(string path = ".") returning CommandOutput (primary)
  - [ ] Add bash aliases: Cat() => GetContent(), Ls() => GetChildItem()
  - [ ] Handle file not found with proper stderr/exit code
  - [ ] No exceptions - shell semantics

- [ ] Create Native/FileSystem/Direct.cs
  - [ ] Static class Direct
  - [ ] Implement GetContent(string path) returning IAsyncEnumerable<string>
  - [ ] Implement GetChildItem(string path = ".") returning IAsyncEnumerable<FileSystemInfo>
  - [ ] Stream results without buffering
  - [ ] Can throw exceptions (C# semantics)

### CommandOutput Integration
- [ ] Ensure Native.GetContent returns proper CommandOutput
  - [ ] Stdout contains file/directory content
  - [ ] Stderr contains error messages if any
  - [ ] ExitCode 0 for success, 1 for failure
  - [ ] Success property works correctly

### Global Usings Setup
- [ ] Create example GlobalUsings patterns
  - [ ] PowerShell-style: `global using static TimeWarp.Amuru.Native.FileSystem.Commands;`
  - [ ] Bash-style: `global using static TimeWarp.Amuru.Native.Aliases.Bash;`
  - [ ] Show how users control verbosity

### Alias System
- [ ] Create Native/Aliases/Bash.cs
  - [ ] Static methods that delegate to primary implementations
  - [ ] Cat() => FileSystem.Commands.GetContent()
  - [ ] Ls() => FileSystem.Commands.GetChildItem()
  - [ ] Allow both naming conventions

### Testing
- [ ] Test Commands.GetContent with existing file
- [ ] Test Commands.GetContent with missing file
- [ ] Test Commands.GetChildItem with valid directory
- [ ] Test bash aliases work correctly
- [ ] Test Direct.GetContent streaming behavior
- [ ] Test both APIs can coexist
- [ ] Test global using patterns

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