# Issue #39 Analysis: TtyPassthroughAsync Method for TUI Applications

## Executive Summary

The current `PassthroughAsync` method in `CommandResult.cs` uses stream piping via CliWrap, which loses TTY characteristics required by TUI applications like `vim`, `nano`, and `edit`. There is **no existing functionality** in TimeWarp.Amuru that provides true TTY passthrough. A new `TtyPassthroughAsync` method using raw `Process.Start` without stream redirection is needed.

## Scope

- **Issue**: GitHub Issue #39 - Add TtyPassthroughAsync method for TUI applications
- **Affected Files**: `Source/TimeWarp.Amuru/Core/CommandResult.cs`, `Source/TimeWarp.Amuru/Core/RunBuilder.cs`
- **Related Components**: `PassthroughAsync`, `SelectAsync`, all fluent builder classes

## Methodology

1. Searched codebase for TTY/PTY/terminal inheritance patterns
2. Analyzed `PassthroughAsync` implementation in `CommandResult.cs`
3. Examined `RunBuilder.cs` for execution method patterns
4. Reviewed existing tests in `Tests/Integration/Core/CommandResult.Interactive.cs`
5. Checked for any existing workarounds using raw `Process.Start`

## Findings

### 1. Current PassthroughAsync Implementation

**Location**: `Source/TimeWarp.Amuru/Core/CommandResult.cs` (lines 26-57)

```csharp
public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
{
    // ...
    await using Stream stdIn = Console.OpenStandardInput();
    await using Stream stdOut = Console.OpenStandardOutput();
    await using Stream stdErr = Console.OpenStandardError();
    
    Command interactiveCommand = Command
        .WithStandardInputPipe(PipeSource.FromStream(stdIn))
        .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
        .WithStandardErrorPipe(PipeTarget.ToStream(stdErr));
    
    CliWrap.CommandResult result = await interactiveCommand.ExecuteAsync(cancellationToken);
    // ...
}
```

**Problem**: This creates **streams**, not TTY file descriptors. TUI applications check `isatty()` and fail when stdin/stdout/stderr are pipes rather than TTYs.

### 2. Confirmation: No Existing TTY Passthrough

Searched for:
- `inherit`, `terminal`, `tty`, `pty`, `pseudo` - No relevant results for TTY inheritance
- `UseShellExecute` patterns - Only found in `Native/Utilities/SshKeyHelper.cs` which always redirects streams
- No existing method that avoids stream redirection

**Conclusion**: There is no existing functionality that preserves TTY characteristics.

### 3. Documentation Claims vs Reality

The README and method documentation claim `PassthroughAsync` works for "vim, fzf, or REPLs":

```markdown
// Full interactive mode for editors, REPLs, etc.
await Shell.Builder("vim", "file.txt").PassthroughAsync();
```

**Reality**: 
- Works for **fzf** because fzf gracefully handles piped input
- **Does NOT work** for true TUI applications (vim, nano, edit, etc.) that require TTY

### 4. Existing Test Coverage

`Tests/Integration/Core/CommandResult.Interactive.cs`:
- Tests use `echo` commands (not TUI applications)
- Tests mock fzf behavior
- No tests verify actual TUI editor functionality

### 5. CliWrap Limitation

From the issue's referenced CliWrap discussion (#145):
- `PipeTarget.Null` does NOT mean "inherit from parent"
- Any pipe configuration causes stream redirection
- CliWrap cannot provide true TTY passthrough

## Test Strategy

### Test to Reproduce the Issue

Create a test script that demonstrates the failure:

```csharp
#!/usr/bin/dotnet --
// File: Tests/Manual/TtyPassthroughIssue.cs

using TimeWarp.Amuru;

Console.WriteLine("Testing PassthroughAsync with TUI editors...\n");

// Test 1: Current PassthroughAsync with edit (will fail)
Console.WriteLine("Test 1: Using PassthroughAsync() with 'edit' command");
Console.WriteLine("Expected: TUI should render properly");
Console.WriteLine("Actual: Will likely fail with 'Inappropriate ioctl for device'\n");

try
{
    string testFile = Path.GetTempFileName();
    File.WriteAllText(testFile, "# Test file for TUI editor\nEdit this content.");
    
    ExecutionResult result = await Shell.Builder("edit")
        .WithArguments(testFile)
        .PassthroughAsync();
    
    Console.WriteLine($"Exit code: {result.ExitCode}");
    File.Delete(testFile);
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
}

// Test 2: Direct Process.Start (will work)
Console.WriteLine("\n\nTest 2: Using raw Process.Start (workaround)");
Console.WriteLine("Expected: TUI should render properly\n");

try
{
    string testFile = Path.GetTempFileName();
    File.WriteAllText(testFile, "# Test file for TUI editor\nEdit this content.");
    
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "edit",
            Arguments = testFile,
            UseShellExecute = false,
            // NO stream redirection - inherit TTY from parent
        }
    };
    
    process.Start();
    await process.WaitForExitAsync();
    
    Console.WriteLine($"Exit code: {process.ExitCode}");
    File.Delete(testFile);
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
}
```

### Automated Test (for CI)

Since TUI tests can't run in CI, use a program that **checks** for TTY without needing interaction:

```csharp
#!/usr/bin/dotnet --
// File: Tests/Integration/Core/CommandResult.TtyPassthrough.cs

using TimeWarp.Amuru;
using Shouldly;

// Test using `tty` command which returns TTY device name or "not a tty"
// This validates that the child process sees a real TTY

await RunTests<TtyPassthroughTests>();

internal sealed class TtyPassthroughTests
{
    public static async Task TestPassthroughAsync_DoesNotPreserveTty()
    {
        // The `test -t 0` command checks if stdin is a terminal
        // Exit code 0 = is a TTY, Exit code 1 = not a TTY
        
        CommandOutput result = await Shell.Builder("sh")
            .WithArguments("-c", "test -t 0; echo $?")
            .CaptureAsync();
        
        // Current PassthroughAsync creates pipes, so this should return "1" (not a TTY)
        // This test documents the current behavior
        result.Stdout.Trim().ShouldBe("1", "PassthroughAsync should NOT preserve TTY (current behavior)");
    }
    
    public static async Task TestTtyPassthroughAsync_PreservesTty()
    {
        // After implementing TtyPassthroughAsync, this test should pass
        // The child process should see stdin as a real TTY
        
        // Note: This test can only pass when run from an actual terminal
        // Skip in CI environments
        if (Environment.GetEnvironmentVariable("CI") != null)
        {
            Console.WriteLine("Skipping TTY test in CI environment");
            return;
        }
        
        // TODO: Implement after TtyPassthroughAsync is added
        // ExecutionResult result = await Shell.Builder("sh")
        //     .WithArguments("-c", "test -t 0")
        //     .TtyPassthroughAsync();
        // 
        // result.ExitCode.ShouldBe(0, "TtyPassthroughAsync should preserve TTY");
    }
}
```

## Implementation Plan

### Phase 1: Add Core Method to CommandResult.cs

**File**: `Source/TimeWarp.Amuru/Core/CommandResult.cs`

Add after `PassthroughAsync` (line ~57):

```csharp
/// <summary>
/// Executes the command with true TTY passthrough for TUI applications.
/// Unlike PassthroughAsync which pipes Console streams, this method
/// uses Process.Start directly without any stream redirection, allowing
/// the child process to inherit the terminal's TTY characteristics.
/// </summary>
/// <remarks>
/// Use this method for TUI applications like vim, nano, edit, etc. that
/// require a real terminal (TTY) to function properly. These applications
/// check isatty() and fail when stdin/stdout/stderr are pipes.
/// 
/// Note: Output cannot be captured with this method since streams are
/// not redirected. Use PassthroughAsync for non-TUI interactive commands
/// where you want stream access.
/// </remarks>
/// <param name="cancellationToken">Cancellation token for the operation</param>
/// <returns>The execution result (output strings will be empty since streams are inherited)</returns>
public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
{
    if (Command == null)
    {
        return new ExecutionResult(
            new CliWrap.CommandResult(0, DateTimeOffset.MinValue, DateTimeOffset.MinValue),
            string.Empty,
            string.Empty
        );
    }

    DateTimeOffset startTime = DateTimeOffset.Now;
    
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = Command.TargetFilePath,
            Arguments = Command.Arguments,
            WorkingDirectory = string.IsNullOrEmpty(Command.WorkingDirPath) 
                ? null 
                : Command.WorkingDirPath,
            UseShellExecute = false,
            // CRITICAL: Do NOT redirect any streams - this preserves TTY inheritance
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        }
    };

    // Apply environment variables if configured
    if (Command.EnvironmentVariables.Count > 0)
    {
        foreach (KeyValuePair<string, string?> envVar in Command.EnvironmentVariables)
        {
            if (envVar.Value != null)
            {
                process.StartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
            }
            else
            {
                process.StartInfo.EnvironmentVariables.Remove(envVar.Key);
            }
        }
    }

    process.Start();
    
    // Register cancellation
    await using CancellationTokenRegistration registration = cancellationToken.Register(() =>
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Process may have already exited
        }
    });
    
    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    
    DateTimeOffset exitTime = DateTimeOffset.Now;

    return new ExecutionResult(
        new CliWrap.CommandResult(process.ExitCode, startTime, exitTime),
        string.Empty,
        string.Empty
    );
}
```

### Phase 2: Add Method to RunBuilder.cs

**File**: `Source/TimeWarp.Amuru/Core/RunBuilder.cs`

Add after `PassthroughAsync` (line ~99):

```csharp
/// <summary>
/// Executes the command with true TTY passthrough for TUI applications.
/// Unlike PassthroughAsync which pipes Console streams, this method
/// allows the child process to inherit the terminal's TTY characteristics.
/// </summary>
/// <param name="cancellationToken">Cancellation token for the operation</param>
/// <returns>The execution result (output strings will be empty since output is inherited)</returns>
public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
{
    return await Build().TtyPassthroughAsync(cancellationToken);
}
```

### Phase 3: Add to Other Fluent Builders (Pattern)

The same method signature should be added to all builder classes that have `PassthroughAsync`:

- `DotNet.Run.cs`
- `DotNet.Test.cs`
- `DotNet.Build.cs`
- `Fzf.cs`
- `Ghq.cs`
- `Gwq.cs`
- All other fluent builder classes

**Template** (use script to apply):

```csharp
/// <summary>
/// Executes the command with true TTY passthrough for TUI applications.
/// </summary>
/// <param name="cancellationToken">Cancellation token for the operation</param>
/// <returns>The execution result</returns>
public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
{
    return await Build().TtyPassthroughAsync(cancellationToken);
}
```

### Phase 4: Add Tests

**File**: `Tests/Integration/Core/CommandResult.TtyPassthrough.cs`

```csharp
#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<TtyPassthroughTests>();

internal sealed class TtyPassthroughTests
{
    public static async Task TestTtyPassthroughAsync_WithEcho_ReturnsExitCode()
    {
        // Basic test - echo command should work with TTY passthrough
        ExecutionResult result = await Shell.Builder("echo")
            .WithArguments("Hello TTY")
            .TtyPassthroughAsync();
        
        result.ExitCode.ShouldBe(0);
        result.IsSuccess.ShouldBeTrue();
        // Output is empty because streams are not captured
        result.StandardOutput.ShouldBeNullOrEmpty();
    }
    
    public static async Task TestTtyPassthroughAsync_WithNullCommand_ReturnsSuccess()
    {
        // Test graceful degradation with empty command
        CommandResult nullCommand = Shell.Builder("").Build();
        
        ExecutionResult result = await nullCommand.TtyPassthroughAsync();
        result.ExitCode.ShouldBe(0);
    }
    
    public static async Task TestTtyPassthroughAsync_WithWorkingDirectory()
    {
        string tempDir = Path.GetTempPath();
        
        ExecutionResult result = await Shell.Builder("pwd")
            .WithWorkingDirectory(tempDir)
            .TtyPassthroughAsync();
        
        result.ExitCode.ShouldBe(0);
    }
    
    public static async Task TestTtyPassthroughAsync_WithEnvironmentVariable()
    {
        ExecutionResult result = await Shell.Builder("sh")
            .WithArguments("-c", "exit ${MY_EXIT_CODE:-1}")
            .WithEnvironmentVariable("MY_EXIT_CODE", "42")
            .TtyPassthroughAsync();
        
        result.ExitCode.ShouldBe(42);
    }
    
    public static async Task TestTtyPassthroughAsync_WithCancellation()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        
        try
        {
            await Shell.Builder("sleep")
                .WithArguments("10")
                .TtyPassthroughAsync(cts.Token);
            
            // Should not reach here
            throw new InvalidOperationException("Expected cancellation");
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }
}
```

**File**: `Tests/Manual/TtyEditorDemo.cs`

```csharp
#!/usr/bin/dotnet --
#:project ../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

using TimeWarp.Amuru;

// MANUAL TEST: Run this to verify TUI editors work with TtyPassthroughAsync

Console.WriteLine("TUI Editor Test with TtyPassthroughAsync");
Console.WriteLine("========================================\n");

string testFile = Path.Combine(Path.GetTempPath(), $"tty-test-{Guid.NewGuid():N}.txt");
File.WriteAllText(testFile, "# Test file for TUI editor\n\nEdit this content and save to verify TTY passthrough works.\n");

Console.WriteLine($"Test file: {testFile}");
Console.WriteLine("Opening in 'nano' editor...\n");
Console.WriteLine("(Press Ctrl+X to exit nano, Y to save, Enter to confirm)\n");

try
{
    ExecutionResult result = await Shell.Builder("nano")
        .WithArguments(testFile)
        .TtyPassthroughAsync();
    
    Console.WriteLine($"\nEditor exited with code: {result.ExitCode}");
    Console.WriteLine($"Runtime: {result.RunTime}");
    
    string content = await File.ReadAllTextAsync(testFile);
    Console.WriteLine($"\nFile contents after editing:\n{content}");
}
finally
{
    File.Delete(testFile);
    Console.WriteLine("\nTest file cleaned up.");
}
```

### Phase 5: Update Documentation

1. **README.md**: Add section explaining when to use `TtyPassthroughAsync` vs `PassthroughAsync`
2. **CommandResult.md**: Document the new method
3. **Update XML docs** on `PassthroughAsync` to clarify its limitations

## Task Checklist

### Core Implementation
- [ ] Add `TtyPassthroughAsync` to `CommandResult.cs`
- [ ] Add `TtyPassthroughAsync` to `RunBuilder.cs`
- [ ] Add `using System.Diagnostics` directive if needed
- [ ] Handle environment variables from CliWrap Command
- [ ] Handle working directory from CliWrap Command
- [ ] Implement cancellation token support

### Builder Classes (add TtyPassthroughAsync to each)
- [ ] `DotNet.Run.cs`
- [ ] `DotNet.Test.cs`
- [ ] `DotNet.Build.cs`
- [ ] `DotNet.Clean.cs`
- [ ] `DotNet.Restore.cs`
- [ ] `DotNet.Pack.cs`
- [ ] `DotNet.Publish.cs`
- [ ] `DotNet.Watch.cs`
- [ ] `Fzf.cs`
- [ ] `Ghq.cs`
- [ ] `Gwq.cs`
- [ ] All other builder classes with PassthroughAsync

### Tests
- [ ] Create `CommandResult.TtyPassthrough.cs` integration test
- [ ] Create `TtyEditorDemo.cs` manual test
- [ ] Add test for exit code propagation
- [ ] Add test for working directory
- [ ] Add test for environment variables
- [ ] Add test for cancellation

### Documentation
- [ ] Update README.md with TtyPassthroughAsync usage
- [ ] Update CommandResult.md
- [ ] Clarify PassthroughAsync limitations in XML docs

## References

- [GitHub Issue #39](https://github.com/TimeWarpEngineering/timewarp-amuru/issues/39)
- [CliWrap Issue #145](https://github.com/Tyrrrz/CliWrap/issues/145) - Discussion on pipe behavior
- [.NET ProcessStartInfo Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo)
- POSIX `isatty()` - Used by TUI applications to detect TTY

## Conclusion

The implementation is straightforward - use raw `Process.Start` without stream redirection. The main work is:
1. Core implementation in `CommandResult.cs` with proper environment/working directory support
2. Propagating the method through all fluent builder classes
3. Creating tests that verify TTY behavior without requiring interactive input
