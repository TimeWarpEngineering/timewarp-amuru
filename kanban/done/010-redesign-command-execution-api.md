# 010 Redesign Command Execution API

## Description

Complete redesign of the command execution API to provide clear, intuitive methods that match user expectations from bash/PowerShell scripting. The current API has confusing method names, loses stderr data, and lacks the most basic scripting functionality (streaming output to console).

## Requirements

- **Breaking Change**: This is an intentional breaking change to fix fundamental API design flaws
- **Data Integrity**: Never silently lose stderr output
- **Clear Intent**: Method names must clearly indicate streaming vs capturing behavior
- **Core Functionality**: Must support the default bash/PowerShell behavior (stream to console)
- **Backward Migration**: Update all existing code to use new API

## Checklist

### Design
- [ ] Review Analysis/CommandExecutionApiRedesign.md document
- [ ] Finalize method naming and signatures
- [ ] Design CommandOutput return type
- [ ] Plan migration strategy for existing code

### Implementation
- [ ] Create CommandOutput class with Stdout, Stderr, Combined, ExitCode properties
- [ ] Add RunAsync() - stream to console, return exit code (default scripting)
- [ ] Add CaptureAsync() - silent execution, return CommandOutput
- [ ] Add RunAndCaptureAsync() - stream AND capture using PipeTarget.Merge
- [ ] Rename ExecuteInteractiveAsync() → PassthroughAsync()
- [ ] Rename GetStringInteractiveAsync() → SelectAsync()
- [ ] Remove GetStringAsync() (loses stderr, redundant)
- [ ] Remove GetLinesAsync() (users can split strings)
- [ ] Remove/rename ExecuteAsync() (misleading name)

### Migration
- [ ] Update all tests to use new API
- [ ] Update all Scripts/ to use new API
- [ ] Update all Spikes/CsScripts/ examples
- [ ] Update Fzf, Ghq, Gwq, DotNet builders if affected

### Documentation
- [ ] Update README.md with new API examples
- [ ] Update CLAUDE.md with new method descriptions
- [ ] Create migration guide showing old → new mappings
- [ ] Document the CommandOutput type clearly

## Notes

### Current Problems
1. `GetStringAsync()`/`GetLinesAsync()` silently discard stderr (data loss!)
2. `ExecuteAsync()` name implies "just run it" but actually captures everything
3. No method for default scripting behavior (stream to console like bash/PowerShell)
4. `GetLinesAsync()` is just `GetStringAsync().Split('\n')` - pointless API bloat
5. Method names don't clearly indicate behavior

### Proposed API Summary

| Method | Console Output | Captures | Returns | Use Case |
|--------|---------------|----------|---------|----------|
| `RunAsync()` | ✅ Real-time | ❌ | Exit code | Default scripting (80%) |
| `CaptureAsync()` | ❌ | ✅ Both | CommandOutput | Process output (15%) |
| `RunAndCaptureAsync()` | ✅ Real-time | ✅ Both | CommandOutput | CI/CD logging (3%) |
| `PassthroughAsync()` | ✅ Direct | ❌ | void | Editors/REPLs (1%) |
| `SelectAsync()` | ✅ Stderr | ✅ Stdout | string | Selection UI (1%) |

### Migration Examples

```csharp
// Old (problematic)
var output = await Shell.Builder("ls").GetStringAsync();  // Lost stderr!
var lines = await Shell.Builder("ls").GetLinesAsync();    // Lost stderr!
var result = await Shell.Builder("ls").ExecuteAsync();    // Misleading name

// New (clear)
await Shell.Builder("npm", "install").RunAsync();         // Finally, just run it!
var result = await Shell.Builder("ls").CaptureAsync();    // Silent capture
var output = result.Stdout;  // or result.Combined
var lines = result.Lines;    // convenience property
```

See `Analysis/CommandExecutionApiRedesign.md` for full design rationale and implementation details.