# Add Command Timeout Support

## Description

Add configurable timeout support to TimeWarp.Cli commands to prevent long-running commands from hanging indefinitely. This would enhance the library's robustness for automation scenarios where commands might exceed expected execution times.

## Parent
<Reference to potential epic like 001_enhance-command-control>

## Requirements

- Provide timeout configuration through CommandOptions
- Support both global and per-command timeout settings
- Gracefully handle timeout scenarios consistent with library's failure philosophy
- Integrate with existing cancellation token support
- Maintain backward compatibility

## Checklist

### Design
- [ ] Update API surface to include timeout options
- [ ] Update CommandOptions class with timeout configuration
- [ ] Add/Update Integration Tests for timeout scenarios
- [ ] Consider backward compatibility (ensure no breaking changes)

### Implementation
- [ ] Update CommandOptions with timeout properties
- [ ] Implement timeout logic in command execution
- [ ] Integrate with existing CancellationToken support
- [ ] Verify Cross-platform Functionality
- [ ] Validate Performance Impact (ensure minimal overhead)

### Documentation
- [ ] Update API Documentation with timeout examples
- [ ] Update Usage Examples showing timeout configuration
- [ ] Update CLAUDE.md with timeout guidance
- [ ] Create/Update ADR for timeout strategy (if significant)

## Notes

Example timeout usage:
```csharp
// Global timeout through CommandOptions
var options = new CommandOptions()
    .WithTimeout(TimeSpan.FromMinutes(5));
var result = await Shell.Builder("long-command", new string[0], options).GetStringAsync();

// Per-command timeout via CancellationToken
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));
var result = await Shell.Builder("quick-command").GetStringAsync(cts.Token);
```

## Implementation Notes

[This section would be filled during actual implementation with technical details, decisions made, and any discoveries during development]