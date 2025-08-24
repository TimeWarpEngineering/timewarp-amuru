# Native API Design Decisions

## 1. Namespace Structure: Flat with Partials

**Decision: Use one static class with partial files for organization**

```csharp
// Native.cs (main file)
namespace TimeWarp.Amuru;
public static partial class Native { }

// Native.FileOps.cs (partial)
public static partial class Native 
{
    public static CommandOutput Cat(string path) { }
    public static CommandOutput Head(string path, int lines = 10) { }
    public static CommandOutput Tail(string path, int lines = 10) { }
}

// Native.DirectoryOps.cs (partial)
public static partial class Native 
{
    public static CommandOutput Ls(string path = ".") { }
    public static CommandOutput Pwd() { }
    public static CommandOutput Mkdir(string path, bool parents = false) { }
}

// Native.TextOps.cs (partial)
public static partial class Native 
{
    public static CommandOutput Grep(string pattern, string input) { }
    public static CommandOutput Wc(string input) { }
    public static CommandOutput Echo(string text) { }
}
```

**Benefits:**
- Flat API: `Native.Cat()`, `Native.Ls()` - simple and discoverable
- Organized code: Each partial file groups related operations
- No nested namespaces to navigate
- IntelliSense shows all commands under `Native.`

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

### Option C: Hybrid Approach

**Provide both APIs:**

```csharp
public static partial class Native
{
    // Shell-compatible API (returns CommandOutput)
    public static CommandOutput Cat(string path) { }
    
    // C# native API in nested class
    public static class Direct
    {
        public static string Cat(string path) => File.ReadAllText(path);
        public static IEnumerable<FileSystemInfo> Ls(string path = ".") { }
    }
}

// Usage - choose your style
var shellStyle = Native.Cat("file.txt");  // Returns CommandOutput
var csharpStyle = Native.Direct.Cat("file.txt");  // Returns string
```

## Recommendation: Consistent CommandOutput

**Use CommandOutput for all native commands.**

Reasoning:
1. **We're building a shell in C#** - consistency matters more than C# idioms
2. **Composability is key** - being able to pipe native and external commands
3. **Predictable behavior** - users don't have to remember which commands are native
4. **Error handling consistency** - always check exit codes the same way
5. **Future piping** - enables `Native.Cat("file").Pipe(Native.Grep("pattern"))`

The slight verbosity (`result.Stdout`) is worth the consistency and composability gains.

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