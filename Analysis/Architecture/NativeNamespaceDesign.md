# Native Namespace Design

## Problem Statement

A single giant `Native` static class would become unwieldy as we add dozens of commands. Even with partial classes for file organization, users would face:
- Verbose usage: `Native.Cat()`, `Native.Ls()`, `Native.Grep()`
- Poor IntelliSense experience with 50+ methods in one class
- No way to choose between CommandOutput vs Direct streaming APIs

## Solution: Namespace-Based Organization

Break Native commands into logical namespaces, each with two API styles that users control via global usings.

## Namespace Structure

```csharp
// TimeWarp.Amuru.Native.File namespace
namespace TimeWarp.Amuru.Native.File;

public static class Commands
{
    public static CommandOutput Cat(string path) { }
    public static CommandOutput Head(string path, int lines = 10) { }
    public static CommandOutput Tail(string path, int lines = 10) { }
    public static CommandOutput Touch(string path) { }
}

public static class Direct  
{
    public static IAsyncEnumerable<string> Cat(string path) { }
    public static IAsyncEnumerable<string> Head(string path, int lines = 10) { }
    public static IAsyncEnumerable<string> Tail(string path, int lines = 10) { }
}

// TimeWarp.Amuru.Native.Directory namespace
namespace TimeWarp.Amuru.Native.Directory;

public static class Commands
{
    public static CommandOutput Ls(string path = ".") { }
    public static CommandOutput Pwd() { }
    public static CommandOutput Mkdir(string path, bool parents = false) { }
    public static CommandOutput Rm(string path, bool recursive = false) { }
    public static CommandOutput Find(string path, string pattern) { }
}

public static class Direct
{
    public static IAsyncEnumerable<FileInfo> Ls(string path = ".") { }
    public static string Pwd() { }
    public static IAsyncEnumerable<string> Find(string path, string pattern) { }
}

// TimeWarp.Amuru.Native.Text namespace  
namespace TimeWarp.Amuru.Native.Text;

public static class Commands
{
    public static CommandOutput Grep(string pattern, string input) { }
    public static CommandOutput Sed(string pattern, string replacement, string input) { }
    public static CommandOutput Awk(string program, string input) { }
    public static CommandOutput Wc(string input, bool lines = true) { }
    public static CommandOutput Sort(string input) { }
    public static CommandOutput Uniq(string input) { }
}

public static class Direct
{
    public static IAsyncEnumerable<string> Grep(string pattern, IAsyncEnumerable<string> input) { }
    public static IAsyncEnumerable<string> Sed(string pattern, string replacement, IAsyncEnumerable<string> input) { }
    public static IAsyncEnumerable<string> Sort(IAsyncEnumerable<string> input) { }
    public static IAsyncEnumerable<string> Uniq(IAsyncEnumerable<string> input) { }
}

// TimeWarp.Amuru.Native.Process namespace
namespace TimeWarp.Amuru.Native.Process;

public static class Commands
{
    public static CommandOutput Ps() { }
    public static CommandOutput Kill(int pid, int signal = 15) { }
    public static CommandOutput Which(string command) { }
}

public static class Direct
{
    public static IAsyncEnumerable<ProcessInfo> Ps() { }
    public static string Which(string command) { }
}

// TimeWarp.Amuru.Native.System namespace
namespace TimeWarp.Amuru.Native.System;

public static class Commands
{
    public static CommandOutput Echo(string text) { }
    public static CommandOutput Sleep(int seconds) { }
    public static CommandOutput Date(string format = null) { }
    public static CommandOutput Env(string variable = null) { }
}

public static class Direct
{
    public static string Echo(string text) { }
    public static Task Sleep(int seconds) { }
    public static DateTime Date() { }
    public static Dictionary<string, string> Env() { }
}
```

## Usage Patterns

### Pattern 1: Shell-Style with Global Static Usings

```csharp
// GlobalUsings.cs
global using static TimeWarp.Amuru.Native.File.Commands;
global using static TimeWarp.Amuru.Native.Directory.Commands;
global using static TimeWarp.Amuru.Native.Text.Commands;
global using static TimeWarp.Amuru.Native.Process.Commands;
global using static TimeWarp.Amuru.Native.System.Commands;

// YourScript.cs - Clean, no prefixes!
var content = Cat("readme.md");
var files = Ls("/app");
var errors = Grep("ERROR", content.Stdout);
var processes = Ps();

// Everything returns CommandOutput - consistent shell semantics
if (!content.Success)
{
    Console.Error.WriteLine(content.Stderr);
    Environment.Exit(content.ExitCode);
}
```

### Pattern 2: Direct Streaming with LINQ

```csharp
// GlobalUsings.cs  
global using static TimeWarp.Amuru.Native.File.Direct;
global using static TimeWarp.Amuru.Native.Directory.Direct;
global using static TimeWarp.Amuru.Native.Text.Direct;
global using static TimeWarp.Amuru.Native.Process.Direct;
global using static TimeWarp.Amuru.Native.System.Direct;

// YourScript.cs - LINQ-powered streaming!
// Find large log files with errors from the last hour
var recentErrors = Find("/var/log", "*.log")
    .SelectMany(logFile => Cat(logFile)
        .Where(line => line.Contains("ERROR"))
        .Select(line => new { File = logFile, Line = line }))
    .Where(x => ParseTimestamp(x.Line) > DateTime.Now.AddHours(-1));

await foreach (var error in recentErrors)
{
    Console.WriteLine($"{error.File}: {error.Line}");
}

// Process files by size
var largeFiles = Ls("/data")
    .Where(f => f.Size > 100_000_000)
    .OrderByDescending(f => f.Size)
    .Take(10);
```

### Pattern 3: Selective Imports (Mix and Match)

```csharp
// GlobalUsings.cs
// Import namespaces (not static) for explicit usage
global using TimeWarp.Amuru.Native.File;
global using TimeWarp.Amuru.Native.Directory;

// Import Direct statically for Text operations
global using static TimeWarp.Amuru.Native.Text.Direct;

// YourScript.cs
// Explicit for File/Directory
var output = File.Commands.Cat("data.txt");
var stream = File.Direct.Cat("data.txt");

// But Text operations are direct (we imported Direct statically)
var filtered = Grep("pattern", stream);  // Direct.Grep, no prefix
```

### Pattern 4: Fully Qualified (Maximum Clarity)

```csharp
// No global usings for Native

// YourScript.cs - Explicit about everything
var shellOutput = TimeWarp.Amuru.Native.File.Commands.Cat("file.txt");
var directStream = TimeWarp.Amuru.Native.File.Direct.Cat("file.txt");

// Clear what API style you're using
if (shellOutput.Success)
{
    var matches = TimeWarp.Amuru.Native.Text.Commands.Grep("error", shellOutput.Stdout);
}
```

## Benefits

### 1. Zero Verbosity (When Desired)
With global static usings, commands are as concise as shell:
```csharp
Cat("file.txt")  // Not Native.Cat("file.txt")
Ls()             // Not Native.Ls()
Grep("pattern")  // Not Native.Grep("pattern")
```

### 2. User Choice
Users pick their preferred API style:
- Shell-lovers use Commands with CommandOutput
- LINQ-lovers use Direct with IAsyncEnumerable
- Can mix both in same project

### 3. Organized Code
- File operations in Native.File
- Directory operations in Native.Directory
- Text processing in Native.Text
- Clear separation of concerns

### 4. IntelliSense Friendly
- Smaller, focused classes
- Related commands grouped together
- No overwhelming list of 50+ methods

### 5. Composability
Both styles compose well:
```csharp
// Commands style - pipe CommandOutput
var result = Cat("file.txt");
var filtered = Grep("error", result.Stdout);

// Direct style - LINQ composition
var errors = Cat("file.txt")
    .Where(line => line.Contains("error"))
    .Select(line => line.ToUpper());
```

### 6. Progressive Discovery
New users can start with fully qualified names to learn the API:
```csharp
TimeWarp.Amuru.Native.File.Commands.Cat("file")  // Learning phase
```

Then progress to static imports:
```csharp
Cat("file")  // Expert phase
```

## Implementation Example

```csharp
// File: Native/File/Commands.cs
namespace TimeWarp.Amuru.Native.File;

public static class Commands
{
    public static CommandOutput Cat(string path)
    {
        try 
        {
            var content = System.IO.File.ReadAllText(path);
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
    }
}

// File: Native/File/Direct.cs
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

## Migration Path

For users currently expecting a single `Native` class:

1. **Compatibility Package**: Provide a `Native` facade that delegates:
```csharp
public static class Native
{
    public static CommandOutput Cat(string path) => 
        File.Commands.Cat(path);
    public static CommandOutput Ls(string path = ".") => 
        Directory.Commands.Ls(path);
}
```

2. **Documentation**: Clear migration guide showing global using patterns

3. **Samples**: Provide templates with pre-configured GlobalUsings.cs

## Conclusion

The namespace approach provides:
- **Flexibility**: Users choose their API style
- **Scalability**: Can add dozens of commands without bloat
- **Clarity**: Clear separation between shell-style and LINQ-style
- **Ergonomics**: Zero verbosity with static imports
- **Discoverability**: Organized, logical structure

This design scales from simple scripts to complex data processing pipelines while maintaining the simplicity that makes shells powerful.