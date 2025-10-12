# Exit Code Discrepancy Between ExecuteAsync and ExecuteBufferedAsync with CommandResultValidation.None

## Summary

When using `WithValidation(CommandResultValidation.None)`, there is a critical discrepancy in how exit codes are reported between `ExecuteAsync` and `ExecuteBufferedAsync`. The `ExecuteAsync` method incorrectly reports exit code 0 for commands that actually return non-zero exit codes, while `ExecuteBufferedAsync` correctly reports the actual exit code.

## Environment

- **OS**: Ubuntu 22.04.3 LTS on WSL2 (Linux 5.15.167.4-microsoft-standard-WSL2)
- **.NET Version**: 10.0
- **CliWrap Version**: 3.9.0
- **TimeWarp.Cli Version**: 0.2.0

## Reproduction Steps

1. Create a test script that executes a command known to return a non-zero exit code
2. Use `WithValidation(CommandResultValidation.None)` to disable validation
3. Execute the command with both `ExecuteAsync` and `ExecuteBufferedAsync`
4. Compare the reported exit codes

## Minimal Reproduction Code

```csharp
#!/usr/bin/dotnet --
#:package CliWrap

using CliWrap;
using CliWrap.Buffered;

// Test command that always returns exit code 1
var command = Cli.Wrap("false")
    .WithValidation(CommandResultValidation.None);

// Test 1: ExecuteAsync
Console.WriteLine("=== ExecuteAsync Test ===");
var executeResult = await command.ExecuteAsync();
Console.WriteLine($"Exit Code: {executeResult.ExitCode}");  // Reports: 0 (INCORRECT)

// Test 2: ExecuteBufferedAsync  
Console.WriteLine("\n=== ExecuteBufferedAsync Test ===");
var bufferedResult = await command.ExecuteBufferedAsync();
Console.WriteLine($"Exit Code: {bufferedResult.ExitCode}"); // Reports: 1 (CORRECT)
```

## Detailed Test Results

### Test 1: Using `false` command (always returns exit code 1)

```
ExecuteAsync:        Exit Code: 0  ❌ (incorrect)
ExecuteBufferedAsync: Exit Code: 1  ✅ (correct)
```

### Test 2: Using `ls` with non-existent path

```bash
# Command: ls /nonexistent/path/12345
ExecuteAsync:        Exit Code: 0  ❌ (incorrect) 
ExecuteBufferedAsync: Exit Code: 2  ✅ (correct)
```

### Test 3: Using `grep` with no matches (returns exit code 1)

```bash
# Command: echo "hello" | grep "xyz"
ExecuteAsync:        Exit Code: 0  ❌ (incorrect)
ExecuteBufferedAsync: Exit Code: 1  ✅ (correct)
```

## Expected Behavior

When `CommandResultValidation.None` is used, both `ExecuteAsync` and `ExecuteBufferedAsync` should:
1. Not throw exceptions on non-zero exit codes (this works correctly)
2. Report the actual exit code returned by the process (this only works for ExecuteBufferedAsync)

## Actual Behavior

- `ExecuteAsync` always reports exit code 0 when validation is disabled, regardless of the actual exit code
- `ExecuteBufferedAsync` correctly reports the actual exit code

## Impact

This issue affects any code that:
1. Needs to check exit codes manually while having validation disabled
2. Implements custom error handling based on specific exit codes
3. Logs or monitors command execution results

## Workaround

Use `ExecuteBufferedAsync` instead of `ExecuteAsync` when you need accurate exit codes with validation disabled:

```csharp
// Instead of:
var result = await command
    .WithValidation(CommandResultValidation.None)
    .ExecuteAsync();
    
// Use:
var result = await command
    .WithValidation(CommandResultValidation.None)
    .ExecuteBufferedAsync();
```

## Root Cause Analysis

Looking at the CliWrap source code, it appears that when `CommandResultValidation.None` is set, the `ExecuteAsync` method may be taking a different code path that doesn't properly capture or propagate the exit code from the underlying process.

## Suggested Fix

The `ExecuteAsync` method should be updated to ensure that the actual process exit code is always captured and returned in the `CommandResult`, regardless of the validation setting. The validation setting should only control whether an exception is thrown, not whether the exit code is accurately reported.

## Additional Notes

This issue was discovered while implementing graceful error handling in TimeWarp.Cli, where we wanted to:
1. Disable exception throwing for non-zero exit codes
2. Still have access to the actual exit code for logging and conditional logic

The current behavior makes it impossible to implement proper error handling when using `ExecuteAsync` with validation disabled.