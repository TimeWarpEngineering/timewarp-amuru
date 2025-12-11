# Implement TtyPassthroughAsync for TUI Applications

## Description

Add a new `TtyPassthroughAsync` method to support TUI (Text User Interface) applications like `vim`, `nano`, `edit`, etc. that require a real TTY (terminal) to function properly.

The current `PassthroughAsync` method uses CliWrap's stream piping which loses TTY characteristics. TUI applications check `isatty()` and fail with "Inappropriate ioctl for device" when stdin/stdout/stderr are pipes rather than TTYs.

**GitHub Issue**: #39

## Requirements

- New method must use raw `Process.Start` without any stream redirection
- Must preserve all builder configuration (working directory, environment variables)
- Must support cancellation tokens
- Must follow existing API patterns and graceful degradation

## Checklist

### Core Implementation
- [ ] Add `TtyPassthroughAsync` to `CommandResult.cs`
- [ ] Add `TtyPassthroughAsync` to `RunBuilder.cs`
- [ ] Handle environment variables from CliWrap Command
- [ ] Handle working directory from CliWrap Command
- [ ] Implement cancellation token support with process kill

### Builder Classes
- [ ] Add method to all builder classes that have `PassthroughAsync`
- [ ] Use `update-api-methods.sh` script pattern for consistency

### Tests
- [ ] Create `CommandResult.TtyPassthrough.cs` integration test
- [ ] Create `TtyEditorDemo.cs` manual test
- [ ] Test exit code propagation
- [ ] Test working directory support
- [ ] Test environment variable support
- [ ] Test cancellation behavior

### Documentation
- [ ] Update README.md with TtyPassthroughAsync usage
- [ ] Clarify PassthroughAsync limitations in XML docs
- [ ] Document when to use each method

## Notes

### Analysis Document
Full analysis available at: `.agent/workspace/2025-12-11T16-45-00_issue-39-tty-passthrough-analysis.md`

### Key Implementation Detail
```csharp
using var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = Command.TargetFilePath,
        Arguments = Command.Arguments,
        WorkingDirectory = Command.WorkingDirPath,
        UseShellExecute = false,
        // CRITICAL: Do NOT redirect any streams - this preserves TTY inheritance
        RedirectStandardInput = false,
        RedirectStandardOutput = false,
        RedirectStandardError = false
    }
};
```

### Why CliWrap Can't Help
- `PipeTarget.Null` does NOT mean "inherit from parent"
- Any pipe configuration causes stream redirection
- CliWrap fundamentally cannot provide TTY passthrough
- Reference: [CliWrap Issue #145](https://github.com/Tyrrrz/CliWrap/issues/145)

### Use Cases
- Opening config files in user's preferred editor
- Interactive TUI tools that need full terminal control
- SSH sessions with terminal allocation
- Any application that calls `isatty()` to verify terminal

### PassthroughAsync vs TtyPassthroughAsync
| Method | Uses | Works With | TTY Preserved |
|--------|------|------------|---------------|
| `PassthroughAsync` | CliWrap pipes | fzf, simple interactive | No |
| `TtyPassthroughAsync` | Raw Process.Start | vim, nano, edit, ssh | Yes |
