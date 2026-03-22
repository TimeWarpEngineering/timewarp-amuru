# TimeWarp.Amuru.Native

A collection of cross-platform, in-process C# implementations for common shell commands, integrated into TimeWarp.Amuru. These "native" methods provide efficient, type-safe alternatives to external executables, enabling universal scripting that runs seamlessly on Windows, Linux, and macOS without PATH dependencies or process overhead.

## Why TimeWarp.Amuru.Native?

While the core Amuru API excels at executing external commands fluently, many shell tasks (e.g., listing files, reading content) can be handled natively in C# using .NET APIs like `System.IO`. This module addresses key developer needs:
- **Cross-Platform Portability:** No more "ls works on Linux but fails on Windows." Natives unify behaviors (e.g., ls/dir/Get-ChildItem) into one API.
- **Performance and Efficiency:** In-process execution avoids process startup costs, making it ideal for loops, frequent calls, or resource-constrained environments (e.g., CI/CD, embedded apps).
- **Safety and Integration:** Compile-time checks, direct `CommandOutput` results, and composability with Amuru's builders/pipes. No security risks from external tools.
- **Scalability for a "Amuru Shell":** Start simple, expand to a full suite of utilities. Use as building blocks for custom scripts or standalone exes.
- **No Dependencies:** Relies only on .NET Standard libraries—zero installs needed.

Natives complement external commands: Use them for simple ops; fall back to `Shell.Builder` for complex tools (e.g., git, npm).

## Installation and Usage

Native is part of the TimeWarp.Amuru NuGet package—no separate install.

```csharp
using TimeWarp.Amuru.Native;

// Simple usage
var lsResult = Native.Ls(".");
Console.WriteLine(lsResult.Stdout);

// Compose with Amuru
var grepInput = Native.Cat("log.txt").Stdout;
var errors = Native.Grep("ERROR", grepInput);  // Future overload for string input
if (!errors.Success)
{
    Console.WriteLine("No errors found.");
}
```

## Core Design

- **Namespace Structure:** To scale as we add more commands (potentially dozens for a full "Amuru Shell"), Native is organized as a namespace (`TimeWarp.Amuru.Native`) with individual static classes per command group or a flat structure. This avoids a monolithic single static class, improving organization and discoverability.
  - Example: `Native.File.Cat()`, `Native.Directory.Ls()`—or keep flat like `Native.Cat()`, `Native.Ls()` for simplicity.
  - Rationale: A giant static class could hit limits (e.g., IntelliSense overload, maintenance debt). Namespacing allows logical grouping (e.g., File ops, Text ops, System ops) without bloat.

- **Output Consistency:** All methods return `CommandOutput` (from core Amuru), with `Stdout`, `Stderr`, `ExitCode`, `Success`, and conveniences like `Lines`. Mimics shell behaviors (e.g., exit 1 on error).

- **Overloads for Flexibility:** Support paths, streams, strings; add flags as parameters (e.g., bool recursive).

- **Error Handling:** Graceful failures with meaningful `Stderr` and non-zero `ExitCode`. No exceptions by default—aligns with shell fail-fast.

- **Streaming Support:** For large data, methods offer IAsyncEnumerable<string> overloads to avoid memory spikes.

- **Extensibility:** Methods are public static; devs can extend via inheritance or composition for custom variants.

## Available Commands

Start with essentials; expand based on common shell patterns. Each includes cross-platform notes and examples.

### Cat (File Content Reader)
Equivalent to `cat file.txt`: Reads and returns file content.

```csharp
public static CommandOutput Cat(string filePath);
public static CommandOutput Cat(Stream inputStream);  // For piping/stdin
```

Example:
```csharp
var result = Native.Cat("config.json");
if (result.Success)
{
    var json = JsonSerializer.Deserialize<MyData>(result.Stdout);
}
```

- **Cross-Platform:** Handles Windows/Unix line endings transparently.
- **Use Case:** Quick file reads in scripts, parsing configs/logs.

### Ls (Directory Lister)
Equivalent to `ls -la` / `dir` / `Get-ChildItem`: Lists files/dirs with options.

```csharp
public static CommandOutput Ls(string path = ".", bool detailed = false, bool recursive = false, bool includeHidden = false);
```

Example:
```csharp
var result = Native.Ls("/app/src", detailed: true, recursive: true);
foreach (var line in result.Lines)
{
    Console.WriteLine(line);  // e.g., "d         - Aug 24 12:00 MyFolder"
}
```

- **Cross-Platform:** Uses `FileAttributes` for hidden checks; formats sizes/times consistently.
- **Use Case:** File discovery in build scripts, without external "find" or "ls".

### Planned Commands
- **Echo(string text):** Simple output, with options for no newline.
- **Grep(string pattern, string pathOrInput, ...):** Text search with regex, recursive, invert.
- **Wc(string inputOrFile, bool lines/words/chars):** Count lines/words/chars.
- **Pwd():** Current directory.
- **Mkdir(string path, bool parents):** Create directories.
- **Rm(string path, bool recursive/force):** Delete files/dirs.
- **More:** Cp, Mv, Touch, Head, Tail—prioritize based on ReadMe examples like find/grep.

## Building Standalone Executables
Natives shine in C# but can power cross-platform exes for any shell. Use TimeWarp.Nuru for CLI parsing in single-file apps.

Example for `cat.cs`:
```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Nuru@latest
#:package TimeWarp.Amuru@1.0.0-beta

using TimeWarp.Nuru;
using TimeWarp.Amuru.Native;

var builder = CliApplication.CreateBuilder();
builder.AddRoute("cat {filePath}", (string filePath) =>
{
    var result = Native.Cat(filePath);
    if (!result.Success) 
    {
        Console.Error.WriteLine(result.Stderr);
        Environment.Exit(result.ExitCode);
    }
    Console.Write(result.Stdout);
});

// Handle stdin if no args
builder.AddRoute("cat", () => Native.Cat(Console.OpenStandardInput()));

var app = builder.Build();
await app.RunAsync(args);
```

- **Publish:** `dotnet publish cat.cs -r <rid> --self-contained -p:PublishSingleFile=true -p:PublishAot=true`.
- **Benefits:** Reuses Native code; exes are small/fast with AOT. Distribute via GitHub/dotnet tools.

## Integration with Core Amuru
- **Piping:** Chain natives: `Native.Ls().Stdout` as input to `Native.Grep()`.
- **Builders:** Future: `Shell.NativeBuilder().Ls().Pipe(Native.Grep()).RunAsync();` for unified fluency.
- **Caching:** Apply Amuru's `.Cached()` to natives for repeated ops.

## Design Philosophy
- **Scale via Namespace:** Group into sub-namespaces (e.g., `Native.File`, `Native.Text`) as commands grow—prevents a massive class.
- **Shell-Like but C#-Better:** Mimic exit codes/outputs, but leverage params for options (no string parsing errors).
- **No Bloat:** Only add proven commands; community contributions welcome.
- **Testing:** Fully unit-testable in-process.

This module evolves Amuru toward a complete "shell in C#," reducing external reliance while enhancing the API. Feedback? Contribute on GitHub!

## License
Unlicense - Free and unencumbered.