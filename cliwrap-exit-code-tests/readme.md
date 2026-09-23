# CliWrap Exit Code Discrepancy Tests

This directory contains test cases demonstrating the exit code discrepancy between `ExecuteAsync()` and `ExecuteBufferedAsync()` when using `WithValidation(CommandResultValidation.None)`.

## Issue Summary

When using `WithValidation(CommandResultValidation.None)`, `ExecuteAsync()` returns incorrect exit codes for many commands that succeed, while `ExecuteBufferedAsync()` returns the correct exit codes.

## Test Files

1. **01-basic-comparison.cs** - Basic comparison showing the issue with echo command
2. **02-multiple-commands.cs** - Tests multiple commands (echo, ls, pwd, date) showing the pattern
3. **03-failing-commands.cs** - Tests commands that should fail to show both methods handle actual errors correctly
4. **04-without-validation.cs** - Shows that without setting validation, both methods work correctly

## How to Run

Each test file is a standalone C# script that can be run with:

```bash
dotnet run 01-basic-comparison.cs
```

Or make them executable:

```bash
chmod +x *.cs
./01-basic-comparison.cs
```

## Environment

- OS: Ubuntu 22.04.3 LTS on WSL2
- .NET Version: 10.0.100-preview.5
- CliWrap Version: 3.9.0

## Results Summary

### Commands that should succeed but ExecuteAsync reports failure:

| Command | ExecuteAsync (validation=None) | ExecuteBufferedAsync (validation=None) | Expected |
|---------|--------------------------------|----------------------------------------|----------|
| echo "test" | 1 | 0 | 0 |
| ls -la | 2 | 0 | 0 |
| pwd | 1 | 0 | 0 |
| date | 1 | 0 | 0 |

### Commands that actually fail (both methods report correctly):

| Command | ExecuteAsync | ExecuteBufferedAsync | Expected |
|---------|--------------|----------------------|----------|
| ls /nonexistent | 2 | 2 | 2 |
| cat /does/not/exist | 1 | 1 | 1 |
| false | 1 | 1 | 1 |
| sh -c "exit 42" | 42 | 42 | 42 |