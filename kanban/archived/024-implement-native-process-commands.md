# 024 Implement Native Process Commands

## Description

Implement native process management commands in the Native/Process namespace using PowerShell-inspired naming with bash aliases. These commands will provide in-process control over system processes, offering better integration and type safety than spawning external process tools.

## Requirements

- Create process management commands with both Commands and Direct APIs
- Follow PowerShell verb-noun naming convention
- Provide bash aliases for familiar command names
- Handle process lifecycle management safely
- Maintain consistency with existing implementations

## Checklist

### Process Namespace Implementation
- [ ] Create Native/Process/Commands.cs
  - [ ] Static class Commands with shell semantics (returns CommandOutput)
  - [ ] Implement GetProcess(string? name = null) - list processes
  - [ ] Implement StopProcess(int processId, bool force = false) - terminate process
  - [ ] Implement StartProcess(string fileName, string? arguments = null) - start process
  - [ ] Implement WaitProcess(int processId, int? timeout = null) - wait for completion
  - [ ] Implement GetProcessOutput(string fileName, string? arguments = null) - capture output
  - [ ] Implement TestProcess(int processId) - check if process exists
  - [ ] Handle errors with proper stderr/exit codes
  - [ ] No exceptions thrown - shell semantics

- [ ] Create Native/Process/Direct.cs
  - [ ] Static class Direct with C# semantics (strongly typed, can throw)
  - [ ] Implement GetProcess returning IAsyncEnumerable<ProcessInfo>
  - [ ] Implement StopProcess returning bool (success)
  - [ ] Implement StartProcess returning Process
  - [ ] Implement WaitProcess with async Task
  - [ ] Implement GetProcessOutput returning ProcessOutput
  - [ ] Proper exception handling and resource disposal
  - [ ] LINQ-composable operations

### Alias System Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Ps(name?) => Process.Commands.GetProcess()
  - [ ] Kill(pid) => Process.Commands.StopProcess()
  - [ ] Killall(name) => Process.Commands.StopProcessByName()
  - [ ] Exec(command, args) => Process.Commands.StartProcess()
  - [ ] Wait(pid) => Process.Commands.WaitProcess()
  - [ ] Pgrep(pattern) => Process.Commands.FindProcess()
  - [ ] Top() => Process.Commands.GetTopProcesses() [resource usage]

### Data Types
- [ ] Create ProcessInfo class
  - [ ] Id property (PID)
  - [ ] Name property
  - [ ] CommandLine property
  - [ ] WorkingDirectory property
  - [ ] StartTime property
  - [ ] CpuUsage property
  - [ ] MemoryUsage property
  - [ ] ParentId property
  - [ ] User property

- [ ] Create ProcessOutput class
  - [ ] StandardOutput property
  - [ ] StandardError property
  - [ ] ExitCode property
  - [ ] StartTime property
  - [ ] EndTime property
  - [ ] Duration property

### Testing
- [ ] Test GetProcess lists current processes
- [ ] Test GetProcess filters by name
- [ ] Test StopProcess with valid PID
- [ ] Test StopProcess handles invalid PID gracefully
- [ ] Test StartProcess launches new process
- [ ] Test WaitProcess with timeout
- [ ] Test GetProcessOutput captures stdout/stderr
- [ ] Test process tree operations (parent/child)
- [ ] Test both Commands and Direct APIs
- [ ] Test bash aliases work correctly

## Implementation Notes

### Example Usage

```csharp
// PowerShell-style
var processes = GetProcess("chrome");
var stopped = StopProcess(1234);
var result = GetProcessOutput("ls", "-la");

// Bash-style aliases
var allProcs = Ps();
Kill(1234);
var nodeProcs = Pgrep("node");

// Direct API for advanced scenarios
await foreach (var proc in Direct.GetProcess())
{
    if (proc.CpuUsage > 80)
        Console.WriteLine($"High CPU: {proc.Name} ({proc.Id})");
}

// Start and wait for process
var proc = await Direct.StartProcess("npm", "test");
await Direct.WaitProcess(proc.Id, TimeSpan.FromMinutes(5));
```

### Design Considerations

1. **Security**: Validate permissions before process operations
2. **Cross-Platform**: Handle Windows/Linux/Mac differences
3. **Resource Management**: Properly dispose of Process objects
4. **Child Processes**: Handle process trees appropriately
5. **Signals**: Support different termination signals (SIGTERM, SIGKILL)

### Platform-Specific Notes

- **Windows**: Use Process.Kill() for termination
- **Linux/Mac**: Support signal-based termination (SIGTERM, then SIGKILL)
- **Process Names**: Handle .exe extension on Windows
- **User Context**: Respect process ownership and permissions

## Dependencies

- Task 019 (Native namespace structure) must be complete
- Builds on CommandOutput and dual API pattern
- System.Diagnostics.Process for underlying implementation

## References

- PowerShell Get-Process, Stop-Process, Start-Process cmdlets
- Unix ps, kill, pgrep, top commands
- .NET System.Diagnostics.Process class