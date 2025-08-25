# 019 Create Native Namespace Structure

## Description

Set up the namespace structure for Native commands as designed in our architecture documents. This establishes the foundation for in-process implementations of common shell commands with both Commands (shell-like) and Direct (LINQ-composable) APIs.

## Requirements

- Create namespace structure
- Implement first native command (Cat)
- Demonstrate both Commands and Direct patterns
- Set up for user control via global usings

## Checklist

### Namespace Structure
- [ ] Create Source/TimeWarp.Amuru/Native/ directory structure
  - [ ] Native/File/ directory
  - [ ] Native/Directory/ directory  
  - [ ] Native/Text/ directory
  - [ ] Native/Process/ directory
  - [ ] Native/System/ directory

### File Namespace Implementation
- [ ] Create Native/File/Commands.cs
  - [ ] Static class Commands
  - [ ] Implement Cat(string path) returning CommandOutput
  - [ ] Handle file not found with proper stderr/exit code
  - [ ] No exceptions - shell semantics
  
- [ ] Create Native/File/Direct.cs
  - [ ] Static class Direct
  - [ ] Implement Cat(string path) returning IAsyncEnumerable<string>
  - [ ] Stream lines without buffering
  - [ ] Can throw exceptions (C# semantics)

### CommandOutput Integration
- [ ] Ensure Native.Cat returns proper CommandOutput
  - [ ] Stdout contains file content
  - [ ] Stderr contains error messages if any
  - [ ] ExitCode 0 for success, 1 for failure
  - [ ] Success property works correctly

### Global Usings Setup
- [ ] Create example GlobalUsings patterns
  - [ ] Shell-style: `global using static TimeWarp.Amuru.Native.File.Commands;`
  - [ ] LINQ-style: `global using static TimeWarp.Amuru.Native.File.Direct;`
  - [ ] Show how users control verbosity

### Testing
- [ ] Test Commands.Cat with existing file
- [ ] Test Commands.Cat with missing file
- [ ] Test Direct.Cat streaming behavior
- [ ] Test both APIs can coexist
- [ ] Test global using patterns

## Notes

### Implementation Example - Commands.Cat
```csharp
namespace TimeWarp.Amuru.Native.File;

public static class Commands
{
    public static CommandOutput Cat(string path)
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
                stderr: $"cat: {path}: No such file or directory",
                exitCode: 1
            );
        }
        catch (Exception ex)
        {
            return new CommandOutput(
                stdout: string.Empty,
                stderr: $"cat: {path}: {ex.Message}",
                exitCode: 1
            );
        }
    }
}
```

### Implementation Example - Direct.Cat
```csharp
namespace TimeWarp.Amuru.Native.File;

public static class Direct
{
    public static async IAsyncEnumerable<string> Cat(string path)
    {
        using var reader = System.IO.File.OpenText(path);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            yield return line;
        }
    }
}
```

### Usage Examples
```csharp
// Shell-style with global using
global using static TimeWarp.Amuru.Native.File.Commands;

var result = Cat("readme.md");
if (!result.Success)
{
    Console.Error.WriteLine(result.Stderr);
    Environment.Exit(result.ExitCode);
}

// LINQ-style with global using
global using static TimeWarp.Amuru.Native.File.Direct;

await foreach (var line in Cat("huge.log"))
{
    if (line.Contains("ERROR"))
        Console.WriteLine(line);
}
```

## Future Work

This task sets up the foundation. Future tasks will add:
- More File commands (Head, Tail, Touch)
- Directory commands (Ls, Pwd, Mkdir)
- Text commands (Grep, Echo, Wc)
- Hybrid implementations (native with external fallback)

## Dependencies

- 011_Create-CommandOutput-Class.md (need CommandOutput structure)
- All previous tasks should be complete

## References

- Analysis/Architecture/NativeNamespaceDesign.md
- Analysis/Architecture/NativeApiDesign.md
- Analysis/Architecture/BuilderExtensibility.md