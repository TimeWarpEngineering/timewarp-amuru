# Native API Design Decisions

## 1. Namespace Structure: Organized by Operation Type

**Decision: Use separate namespaces with Commands and Direct classes**

```csharp
// Native/File/Commands.cs
namespace TimeWarp.Amuru.Native.File;
public static class Commands 
{
    public static CommandOutput Cat(string path) { }
    public static CommandOutput Head(string path, int lines = 10) { }
    public static CommandOutput Tail(string path, int lines = 10) { }
}

// Native/File/Direct.cs  
namespace TimeWarp.Amuru.Native.File;
public static class Direct
{
    public static IAsyncEnumerable<string> Cat(string path) { }
    public static IAsyncEnumerable<string> Head(string path, int lines = 10) { }
    public static IAsyncEnumerable<string> Tail(string path, int lines = 10) { }
}

// Native/Directory/Commands.cs
namespace TimeWarp.Amuru.Native.Directory;
public static class Commands 
{
    public static CommandOutput Ls(string path = ".") { }
    public static CommandOutput Pwd() { }
    public static CommandOutput Mkdir(string path, bool parents = false) { }
}

// Native/Directory/Direct.cs
namespace TimeWarp.Amuru.Native.Directory;
public static class Direct
{
    public static IAsyncEnumerable<FileInfo> Ls(string path = ".") { }
    public static string Pwd() { }
}

// Native/Text/Commands.cs
namespace TimeWarp.Amuru.Native.Text;
public static class Commands 
{
    public static CommandOutput Grep(string pattern, string input) { }
    public static CommandOutput Wc(string input) { }
    public static CommandOutput Echo(string text) { }
}

// Native/Text/Direct.cs
namespace TimeWarp.Amuru.Native.Text;
public static class Direct
{
    public static IAsyncEnumerable<string> Grep(string pattern, IAsyncEnumerable<string> input) { }
}
```

**Benefits:**
- User control via global usings: `global using static TimeWarp.Amuru.Native.File.Commands;`
- Zero verbosity when desired: Just `Cat("file")` not `Native.Cat("file")`
- Choice of API style: Commands (shell-like) or Direct (LINQ-able)
- Organized code: Each namespace groups related operations
- Scalable: Can add dozens of commands without bloat
- IntelliSense friendly: Smaller, focused classes

## 2. Return Type: CommandOutput vs Native C# Types

### Option A: Consistent CommandOutput (Shell-Like)

**Every native command returns CommandOutput with exit codes and stderr:**

```csharp
public static CommandOutput Cat(string path)
{
    try 
    {
        var content = File.ReadAllText(path);
        return new CommandOutput(
            stdout: content,
            stderr: "",
            exitCode: 0
        );
    }
    catch (FileNotFoundException)
    {
        return new CommandOutput(
            stdout: "",
            stderr: $"cat: {path}: No such file or directory",
            exitCode: 1
        );
    }
}

// Usage - consistent with external commands
var catResult = Native.Cat("file.txt");
var gitResult = await Shell.Builder("git", "status").CaptureAsync();
// Both return CommandOutput - can compose/pipe
```

**Pros:**
- **Consistent Interface**: Everything returns CommandOutput
- **Pipeable**: Can chain native and external commands seamlessly
- **Shell Semantics**: Exit codes, stderr for errors - familiar to shell users
- **Composability**: `Native.Cat("file.txt").Stdout` pipes into next command
- **Error Handling**: Consistent with shell - check exit code or Success property

**Cons:**
- Feels unnatural in C# to return exit codes for in-process operations
- Extra verbosity: `Native.Cat("file.txt").Stdout` vs just getting the string
- Not utilizing C# exceptions which are more idiomatic

### Option B: Native C# Types (C#-Like)

**Native methods return natural C# types and throw exceptions:**

```csharp
public static string Cat(string path)
{
    return File.ReadAllText(path); // Throws if file not found
}

public static IEnumerable<FileSystemInfo> Ls(string path = ".")
{
    var dir = new DirectoryInfo(path);
    return dir.EnumerateFileSystemInfos();
}

// Usage - more C# idiomatic
try 
{
    var content = Native.Cat("file.txt");
    var files = Native.Ls().Where(f => f.Name.EndsWith(".cs"));
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"File not found: {ex.FileName}");
}
```

**Pros:**
- Natural C# - methods return what you expect
- Less verbose - direct access to results
- LINQ-friendly for collections
- Idiomatic exception handling

**Cons:**
- **Breaks Composability**: Can't pipe native to external commands easily
- **Two Mental Models**: Native commands work differently than external
- **No stderr**: Lose the ability to capture error output
- **Inconsistent API**: Users must remember which commands are native

### Option C: Hybrid Approach (RECOMMENDED)

**Provide both APIs via namespace organization:**

```csharp
namespace TimeWarp.Amuru.Native.File;

// Shell-compatible API (returns CommandOutput)
public static class Commands
{
    public static CommandOutput Cat(string path) { }
}

// C# native streaming API
public static class Direct
{
    public static IAsyncEnumerable<string> Cat(string path) { }
}

// Usage - user chooses via global usings
// Option 1: Shell style
global using static TimeWarp.Amuru.Native.File.Commands;
var shellStyle = Cat("file.txt");  // Returns CommandOutput

// Option 2: Direct streaming
global using static TimeWarp.Amuru.Native.File.Direct;
await foreach (var line in Cat("file.txt")) { }  // IAsyncEnumerable<string>
```

## Recommendation: Hybrid Approach with Namespaces

**Provide BOTH APIs through namespace organization.**

Reasoning:
1. **User choice** - Shell users get Commands, LINQ users get Direct
2. **Zero verbosity** - Global static usings eliminate all prefixes
3. **Best of both worlds** - Shell composability AND LINQ power
4. **Scalability** - Namespace organization prevents giant class bloat
5. **Progressive disclosure** - Start fully qualified, move to static imports

The namespace approach with Commands/Direct classes gives maximum flexibility:
- `Commands` classes return `CommandOutput` for shell consistency
- `Direct` classes return `IAsyncEnumerable<T>` for LINQ composition
- Users control their experience via global usings

## 3. Standalone Executables

**This should be a separate project: TimeWarp.Amuru.Native.Executables**

```csharp
// cat.cs - in separate project
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru.Native

using TimeWarp.Amuru.Native;

var result = Native.Cat(args.FirstOrDefault() ?? "-");
Console.Write(result.Stdout);
if (!result.Success)
{
    Console.Error.Write(result.Stderr);
    Environment.Exit(result.ExitCode);
}
```

Build process:
```bash
dotnet publish cat.cs --self-contained -p:PublishAot=true -o ./bin
```

This keeps the core library focused while enabling the "build your own coreutils" scenario.