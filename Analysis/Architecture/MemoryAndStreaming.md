# Memory Management and Streaming Design

## The Shell Principle

Normal shells (bash, PowerShell, cmd) operate on a fundamental principle:
- **Default behavior**: Stream output directly to terminal
- **No capturing**: Output flows through without buffering
- **User responsibility**: If you want to capture, YOU redirect (`>`, `|`, `$()`)

This is why shells are memory-efficient - they never hold output in memory unless explicitly asked.

## Our API Design Alignment

We've already got this right with our method naming:

### 1. RunAsync() - The Shell Default
**Behaves like a normal shell - streams to console, no memory usage**

```csharp
// Just like typing "docker build ." in terminal
await Shell.Builder("docker", "build", ".").RunAsync();
// Output streams to console in real-time
// Memory usage: ~0 (just the exit code)
```

### 2. CaptureAsync() - Explicit Memory Choice
**User explicitly asks to capture - they accept memory cost**

```csharp
// Like $(git status) or `git status` in shell
var result = await Shell.Builder("git", "status").CaptureAsync();
// User chose to capture, they get the memory cost
```

### 3. Stream Methods - Memory-Conscious Processing
**Process large outputs without buffering**

```csharp
// Like "docker logs -f container | while read line; do ...; done"
await foreach (var line in Shell.Builder("docker", "logs", "-f", "container").StreamStdoutAsync())
{
    ProcessLine(line); // No memory accumulation
}
```

## CommandOutput Memory Optimization

Given the shell principle, here's the recommendation:

### Current Design (Simple but Duplicates)
```csharp
public class CommandOutput
{
    public string Stdout { get; }      // Full stdout
    public string Stderr { get; }      // Full stderr  
    public string Combined { get; }    // Duplicates data
}
```

### Problem: Output Ordering
When stdout and stderr are produced, they come at different times and can interleave:
- Process writes "Starting..." to stdout
- Process writes "Warning: config missing" to stderr  
- Process writes "Processing..." to stdout
- Process writes "Error: failed" to stderr

The terminal shows them in the order they were produced. But if we capture stdout and stderr separately, we lose this ordering information. We can't reconstruct the true Combined output from separate strings.

**How shells handle this**: Both stdout and stderr write to the terminal, which displays them as they arrive. The streams are typically line-buffered when connected to a terminal (write full lines at once) to avoid character-level interleaving. But the ordering between the two streams is preserved based on when each line was actually written.

### How Different Shells Handle Capture

#### Bash/POSIX Shells
```bash
# Capture stdout only (stderr still goes to terminal)
output=$(command)

# Capture stdout only, redirect stderr to /dev/null
output=$(command 2>/dev/null)

# Capture both but LOSE separation (combined into stdout)
output=$(command 2>&1)

# Capture separately (but lose ordering!)
stdout=$(command 2>/tmp/stderr)
stderr=$(cat /tmp/stderr)

# No built-in way to capture both AND preserve ordering AND keep separation
```

#### PowerShell
```powershell
# Capture stdout only
$output = command

# Capture both streams but KEEP separation (PowerShell advantage!)
$output = command 2>&1
# $output is an array where each item knows its stream:
# $output | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] }

# Can reconstruct ordering because objects have metadata
```

#### Our Approach vs Shells

| Feature | Bash | PowerShell | TimeWarp.Amuru |
|---------|------|------------|----------------|
| Capture stdout only | ✅ `$()` | ✅ default | ✅ `.Stdout` |
| Capture stderr only | 🔶 workaround | ✅ streams | ✅ `.Stderr` |
| Capture combined | ✅ `2>&1` | ✅ `2>&1` | ✅ `.Combined` |
| Preserve ordering | ❌ lost | ✅ objects | ✅ `OutputLine` |
| Keep separation | ❌ lost | ✅ objects | ✅ `.IsError` |
| Memory efficient | ✅ stream | 🔶 objects | ✅ single list |

**Key Insight**: We're actually providing MORE than bash (which loses separation when combining) and matching PowerShell's advanced capabilities (which uses objects with metadata).

### Solution: Capture with Source Information
```csharp
public class OutputLine
{
    public string Text { get; }
    public bool IsError { get; }  // true = stderr, false = stdout
    public DateTime Timestamp { get; }
}

public class CommandOutput
{
    private readonly List<OutputLine> lines;
    
    // Computed on demand from lines collection
    public string Stdout => string.Join("\n", 
        lines.Where(l => !l.IsError).Select(l => l.Text));
    
    public string Stderr => string.Join("\n",
        lines.Where(l => l.IsError).Select(l => l.Text));
    
    public string Combined => string.Join("\n",
        lines.Select(l => l.Text));  // Preserves original order
    
    public IReadOnlyList<OutputLine> Lines => lines;
}
```

This approach:
- Preserves the exact ordering of output
- No memory duplication (single storage of lines)
- Allows reconstruction of any view (stdout only, stderr only, combined)
- Provides structured access for advanced scenarios

### Why Not WithCombinedOutput Flag?

The flag approach adds complexity without solving the real issue:
- Users who call `CaptureAsync()` already chose memory usage
- They likely need stdout/stderr separately for error handling
- Combined is often just for logging/debugging
- Lazy computation gives best of both worlds

## Memory Guidelines

### For Users

1. **Default to RunAsync()** - Like shell, stream by default
2. **Use CaptureAsync() consciously** - Know you're buffering
3. **Use Stream methods for large outputs** - Process without buffering
4. **Use StreamToFileAsync() for huge outputs** - Direct to disk

### For Library

1. **Never buffer unless asked** - RunAsync() doesn't capture
2. **Be explicit about buffering** - Method names indicate behavior
3. **Provide streaming alternatives** - For every buffer operation
4. **Optimize captured data** - Lazy computation, no duplication

## Native.Direct - In-Process Streaming with LINQ

For Native commands, we could provide direct IAsyncEnumerable streams that bypass CommandOutput entirely:

```csharp
// Traditional Native approach (returns CommandOutput)
var result = Native.Ls("/path");
foreach (var line in result.Lines)
{
    Console.WriteLine(line);
}

// Native.Direct approach (returns IAsyncEnumerable<T>)
await foreach (var file in Native.Direct.Ls("/path"))
{
    Console.WriteLine($"{file.Name} - {file.Size}");
}

// The power of LINQ with async streams
var largeFiles = Native.Direct.Ls("/path")
    .Where(f => f.Size > 1_000_000)
    .OrderByDescending(f => f.Size)
    .Take(10);

await foreach (var file in largeFiles)
{
    Console.WriteLine($"{file.Name}: {file.Size:N0} bytes");
}

// Compose Native operations with LINQ
var errors = Native.Direct.Cat("app.log")
    .Where(line => line.Contains("ERROR"))
    .Select(line => new { 
        Line = line, 
        Timestamp = ExtractTimestamp(line) 
    })
    .Where(x => x.Timestamp > DateTime.Now.AddHours(-1));

// Chain multiple Native operations
var configFiles = Native.Direct.Find(".", "*.config")
    .SelectMany(path => Native.Direct.Cat(path)
        .Select(content => new { Path = path, Content = content }))
    .Where(x => x.Content.Contains("debug=true"));
```

### Benefits of Native.Direct

1. **True Streaming**: No buffering at all - process items as they arrive
2. **Type Safety**: Return strongly-typed objects instead of strings
3. **LINQ Composability**: Full async LINQ support for filtering, projecting, etc.
4. **Memory Efficiency**: Process TB of data with constant memory
5. **Cancellation**: Natural cancellation through IAsyncEnumerable

### API Design with Namespaces

```csharp
// Organized by namespace instead of one giant class
namespace TimeWarp.Amuru.Native.File;

public static class Commands
{
    // Shell-compatible API - returns CommandOutput
    public static CommandOutput Cat(string path);
    public static CommandOutput Head(string path, int lines = 10);
}

public static class Direct
{
    // LINQ-composable streaming API
    public static IAsyncEnumerable<string> Cat(string path);
    public static IAsyncEnumerable<string> Head(string path, int lines = 10);
}

// Users control verbosity via global usings:
// global using static TimeWarp.Amuru.Native.File.Commands; // For Cat("file")
// global using static TimeWarp.Amuru.Native.File.Direct;   // For streaming Cat("file")
```

This gives users ultimate control:
- Use `Commands` for shell-like CommandOutput
- Use `Direct` for LINQ-powered streaming
- Use global static usings to eliminate all prefixes

## Conclusion

Our API already follows shell principles correctly:
- `RunAsync()` = shell default (stream, no capture)
- `CaptureAsync()` = explicit capture choice
- Stream methods = memory-conscious processing

But we actually go BEYOND traditional shells:
- **Bash limitation**: Can't capture both streams while preserving order AND separation
- **PowerShell advantage**: Uses objects with metadata (we match this)
- **Our solution**: `OutputLine` structure provides everything:
  - Original ordering preserved
  - Stream separation maintained
  - No memory duplication
  - Clean API access to any view
- **Native.Direct bonus**: LINQ-composable async streams for in-process commands

The structured `OutputLine` approach with lazy-computed properties is the optimal solution for external commands, while Native.Direct provides superior streaming for in-process operations.