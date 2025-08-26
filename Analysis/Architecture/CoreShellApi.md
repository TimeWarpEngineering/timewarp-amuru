# Command Execution API Redesign Analysis

## Current Problems

1. **Data Loss**: `GetStringAsync()`/`GetLinesAsync()` silently discard stderr
2. **Redundant Methods**: `GetLinesAsync()` just splits `GetStringAsync()` - users can do this themselves
3. **Misleading Names**: `ExecuteAsync()` doesn't execute to console, it captures
4. **Missing Core Functionality**: No way to stream to console (like bash/PowerShell default behavior)
5. **Unclear Intent**: Method names don't indicate what happens to output

## Real World Use Cases

### 1. Default Script Behavior (Most Common)
**Need**: Run command, see output in real-time, get exit code
```bash
# In bash/PowerShell, this is the default:
git pull
docker build .
npm install
```
**Current API**: ❌ Missing! (Have to use ExecuteInteractiveAsync which doesn't return exit code properly)

### 2. Silent Capture for Processing
**Need**: Run command silently, process output
```csharp
// Get list of files to process
var files = await Shell.Builder("find", ".", "-name", "*.cs").CaptureAsync();
// Parse JSON output
var json = await Shell.Builder("aws", "s3", "ls", "--output", "json").CaptureAsync();
```
**Current API**: ✅ ExecuteAsync() does this but name is wrong

### 3. Interactive Tools
**Need**: Full terminal control for editors, REPLs
```csharp
await Shell.Builder("vim", "file.txt").PassthroughAsync();
await Shell.Builder("python").PassthroughAsync();  // Python REPL
```
**Current API**: ✅ ExecuteInteractiveAsync() does this but name is unclear

### 4. Selection Tools (fzf pattern)
**Need**: Show UI on stderr, capture selection on stdout
```csharp
var selected = await Fzf.Builder()
    .FromInput("option1", "option2")
    .SelectAsync();  // Shows picker UI, returns selection
```
**Current API**: ✅ GetStringInteractiveAsync() does this but name is confusing

### 5. Debugging/Logging
**Need**: See output while running AND capture it for logs
```csharp
var result = await Shell.Builder("deploy.sh")
    .RunAndCaptureAsync();  // Shows output AND captures for logging
File.WriteAllText("deploy.log", result.Output);
```
**Current API**: ❌ Missing! Would need PipeTarget.Merge()

## Proposed API

### Core Methods (90% of use cases)

```csharp
// DEFAULT SCRIPTING - Stream to console, return exit code
Task<int> RunAsync()
// - Streams stdout/stderr to console in real-time
// - Returns exit code
// - This is what bash/PowerShell do by default

// SILENT CAPTURE - Run silently, return all output
Task<CommandOutput> CaptureAsync()  
// - No console output
// - Returns CommandOutput with Stdout, Stderr, Combined, ExitCode
// - Replaces ExecuteAsync/GetStringAsync/GetLinesAsync

// STREAM AND CAPTURE - See output AND get it back
Task<CommandOutput> RunAndCaptureAsync()
// - Streams to console in real-time
// - Also returns captured output
// - Uses PipeTarget.Merge internally
```

### Specialized Methods (10% of use cases)

```csharp
// INTERACTIVE TOOLS - Full terminal passthrough
Task PassthroughAsync()
// - For editors, REPLs, etc.
// - No capture possible
// - Replaces ExecuteInteractiveAsync

// SELECTION TOOLS - UI on stderr, result on stdout  
Task<string> SelectAsync()
// - For fzf-style selection tools
// - Shows UI, returns selection
// - Replaces GetStringInteractiveAsync
```

### The CommandOutput Type

```csharp
public class CommandOutput
{
    public string Stdout { get; }      // Just stdout
    public string Stderr { get; }      // Just stderr  
    public string Combined { get; }    // Both in order produced
    public int ExitCode { get; }
    public bool Success => ExitCode == 0;
    
    // Convenience for users who want lines
    public string[] Lines => Combined.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    public string[] StdoutLines => Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries);
}
```

## Why This Design?

1. **RunAsync()** - 80% of scripting is "just run it and show me"
2. **CaptureAsync()** - When you need to process output (grep, parse JSON, etc.)
3. **RunAndCaptureAsync()** - For CI/CD where you want to see AND log
4. **PassthroughAsync()** - Clear name for interactive tools
5. **SelectAsync()** - Purpose-built for selection UI pattern

## What We're Removing

- `GetStringAsync()` - Redundant, loses stderr
- `GetLinesAsync()` - Users can split strings themselves
- `ExecuteAsync()` - Misleading name, replaced by CaptureAsync()
- `ExecuteInteractiveAsync()` - Vague name, replaced by PassthroughAsync()
- `GetStringInteractiveAsync()` - Confusing name, replaced by SelectAsync()

## Migration Examples

```csharp
// Old (confusing)
var output = await Shell.Builder("ls").GetStringAsync();  // Lost stderr!
var lines = await Shell.Builder("ls").GetLinesAsync();    // Lost stderr!
var result = await Shell.Builder("ls").ExecuteAsync();    // Misleading name

// New (clear)
var result = await Shell.Builder("ls").CaptureAsync();
var output = result.Stdout;  // or result.Combined for both
var lines = result.Lines;    // if you need lines

// Old (missing functionality)
// No way to just run and see output!

// New
await Shell.Builder("npm", "install").RunAsync();  // Finally!
```

## Decision Matrix

| Method | Console Output | Captures | Returns | Primary Use Case |
|--------|---------------|----------|---------|------------------|
| `RunAsync()` | ✅ Real-time | ❌ | Exit code | Default scripting (80%) |
| `CaptureAsync()` | ❌ | ✅ Both | CommandOutput | Process output (15%) |
| `RunAndCaptureAsync()` | ✅ Real-time | ✅ Both | CommandOutput | CI/CD logging (3%) |
| `PassthroughAsync()` | ✅ Direct | ❌ | void | Editors/REPLs (1%) |
| `SelectAsync()` | ✅ Stderr | ✅ Stdout | string | Selection UI (1%) |

## Implementation Notes

1. Use `PipeTarget.Merge()` for RunAndCaptureAsync
2. CaptureAsync should capture BOTH stdout and stderr (fix current bug)
3. RunAsync should merge stdout/stderr to console (preserve order)
4. Consider adding streaming overload that yields lines as they come (IAsyncEnumerable)
5. **All async methods MUST accept CancellationToken parameter** (default to CancellationToken.None)
6. WithTimeout should create internal CancellationTokenSource and link with any passed token
7. Process termination on cancellation should be graceful (SIGTERM/CtrlC) with fallback to forceful

## Cancellation Token Integration

Every async method signature should follow this pattern:
```csharp
public async Task<int> RunAsync(CancellationToken cancellationToken = default)
public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
public async IAsyncEnumerable<CommandLine> StreamAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
```

WithTimeout implementation should link tokens:
```csharp
public CommandBuilder WithTimeout(TimeSpan timeout)
{
    this.timeoutDuration = timeout;
    return this;
}

// In execution methods:
private async Task<T> ExecuteWithTimeoutAsync<T>(Func<CancellationToken, Task<T>> execution, CancellationToken externalToken)
{
    if (timeoutDuration.HasValue)
    {
        using var timeoutCts = new CancellationTokenSource(timeoutDuration.Value);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, timeoutCts.Token);
        return await execution(linkedCts.Token);
    }
    return await execution(externalToken);
}

## Testing and Mocking

Testing strategy has been finalized - see [TestingStrategy.md](TestingStrategy.md) for details:
- Simple CommandMock with AsyncLocal for thread safety
- Automatic cleanup via IDisposable pattern
- No global state risks
- DI support as optional future package