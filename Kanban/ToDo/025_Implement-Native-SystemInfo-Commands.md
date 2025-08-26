# 025 Implement Native SystemInfo Commands

## Description

Implement native system information and environment commands in the Native/SystemInfo namespace using PowerShell-inspired naming with bash aliases. These commands will provide in-process access to system information, environment variables, and path operations without spawning external processes.

## Requirements

- Create system information commands with both Commands and Direct APIs
- Follow PowerShell verb-noun naming convention
- Provide bash aliases for familiar command names
- Handle cross-platform differences appropriately
- Maintain consistency with existing implementations

## Checklist

### SystemInfo Namespace Implementation
- [ ] Create Native/SystemInfo/Commands.cs
  - [ ] Static class Commands with shell semantics (returns CommandOutput)
  - [ ] Implement GetEnvironmentVariable(string name) - read env var
  - [ ] Implement SetEnvironmentVariable(string name, string value) - set env var
  - [ ] Implement GetEnvironmentVariables() - list all env vars
  - [ ] Implement TestPath(string path) - check if path exists
  - [ ] Implement GetItemProperty(string path) - get file/dir properties
  - [ ] Implement ResolvePath(string path) - resolve to absolute path
  - [ ] Implement GetSystemInfo() - OS and hardware info
  - [ ] Implement GetUser() - current user info
  - [ ] Implement GetComputerName() - hostname
  - [ ] Handle errors with proper stderr/exit codes
  - [ ] No exceptions thrown - shell semantics

- [ ] Create Native/SystemInfo/Direct.cs
  - [ ] Static class Direct with C# semantics (strongly typed, can throw)
  - [ ] Implement GetEnvironmentVariable returning string?
  - [ ] Implement SetEnvironmentVariable returning void
  - [ ] Implement GetEnvironmentVariables returning IDictionary<string, string>
  - [ ] Implement TestPath returning bool
  - [ ] Implement GetItemProperty returning FileSystemInfo
  - [ ] Implement ResolvePath returning string
  - [ ] Implement GetSystemInfo returning SystemInfo object
  - [ ] Implement GetUser returning UserInfo object
  - [ ] Proper exception handling

### Alias System Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Env(name?) => SystemInfo.Commands.GetEnvironmentVariable()
  - [ ] Export(name, value) => SystemInfo.Commands.SetEnvironmentVariable()
  - [ ] Test(path) => SystemInfo.Commands.TestPath()
  - [ ] Stat(path) => SystemInfo.Commands.GetItemProperty()
  - [ ] Realpath(path) => SystemInfo.Commands.ResolvePath()
  - [ ] Uname(option?) => SystemInfo.Commands.GetSystemInfo()
  - [ ] Whoami() => SystemInfo.Commands.GetUser()
  - [ ] Hostname() => SystemInfo.Commands.GetComputerName()
  - [ ] Which(command) => SystemInfo.Commands.FindExecutable()

### Data Types
- [ ] Create SystemInfo class
  - [ ] OperatingSystem property
  - [ ] OSVersion property
  - [ ] Architecture property
  - [ ] ProcessorCount property
  - [ ] TotalMemory property
  - [ ] AvailableMemory property
  - [ ] MachineName property
  - [ ] DotNetVersion property
  - [ ] IsWSL property (Windows Subsystem for Linux)

- [ ] Create UserInfo class
  - [ ] UserName property
  - [ ] UserDomain property
  - [ ] HomeDirectory property
  - [ ] IsAdministrator/IsRoot property
  - [ ] Groups property

- [ ] Create ItemProperty class
  - [ ] Path property
  - [ ] Type property (File/Directory/Link)
  - [ ] Size property
  - [ ] CreatedTime property
  - [ ] ModifiedTime property
  - [ ] AccessedTime property
  - [ ] Attributes property
  - [ ] Owner property
  - [ ] Permissions property

### Testing
- [ ] Test GetEnvironmentVariable reads existing vars
- [ ] Test SetEnvironmentVariable modifies environment
- [ ] Test TestPath with files, directories, and non-existent paths
- [ ] Test GetItemProperty returns correct metadata
- [ ] Test ResolvePath handles relative and symbolic paths
- [ ] Test GetSystemInfo returns accurate system data
- [ ] Test cross-platform behavior (Windows/Linux/Mac)
- [ ] Test both Commands and Direct APIs
- [ ] Test bash aliases work correctly

## Implementation Notes

### Example Usage

```csharp
// PowerShell-style
var path = GetEnvironmentVariable("PATH");
var exists = TestPath("/etc/hosts");
var info = GetSystemInfo();

// Bash-style aliases
var home = Env("HOME");
Export("MY_VAR", "value");
var stats = Stat("file.txt");
var fullPath = Realpath("../relative/path");

// Direct API for programmatic use
if (Direct.TestPath(configFile))
{
    var props = await Direct.GetItemProperty(configFile);
    Console.WriteLine($"Config modified: {props.ModifiedTime}");
}

// System information
var sysInfo = await Direct.GetSystemInfo();
Console.WriteLine($"OS: {sysInfo.OperatingSystem} {sysInfo.OSVersion}");
Console.WriteLine($"Memory: {sysInfo.AvailableMemory / 1GB:F1} GB free");
```

### Design Considerations

1. **Cross-Platform**: Abstract OS-specific differences
2. **Environment Scope**: Handle process vs user vs system environment
3. **Path Handling**: Normalize path separators across platforms
4. **Permissions**: Handle permission errors gracefully
5. **WSL Detection**: Identify when running under WSL

### Platform-Specific Notes

- **Windows**: Use Environment.GetEnvironmentVariable with target scope
- **Linux/Mac**: Access /proc for system info, handle Unix permissions
- **Path Separators**: Handle \ on Windows, / on Unix
- **Case Sensitivity**: Be aware of filesystem case sensitivity differences

## Dependencies

- Task 019 (Native namespace structure) must be complete
- Builds on CommandOutput and dual API pattern
- System.Environment and System.IO for underlying implementation

## References

- PowerShell Get-Item, Test-Path, Resolve-Path cmdlets
- Unix env, test, stat, realpath, uname, whoami commands
- .NET Environment and FileSystemInfo classes