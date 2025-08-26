# 027 Build Process Commands (High ROI)

## Description

Implement essential process and environment commands specifically needed for build scripts and CI/CD pipelines. These commands handle tool execution, environment configuration, and output packaging - the core operations of any build system.

## Business Value

**ROI Score: 9/10** - Every build script needs these capabilities:
- **Tool Execution**: Run compilers, test runners, linters with proper error handling
- **Environment Setup**: Configure build variables across platforms
- **Package Creation**: Create release artifacts efficiently
- **Cross-Platform**: Same script works on Windows/Linux/Mac CI runners
- **Performance**: Native environment access and archive creation is 10-50x faster

## Requirements

- Implement commands essential for build orchestration
- Ensure robust error handling and exit code propagation
- Support streaming for long-running processes
- Provide cross-platform environment variable handling
- Efficient archive creation for release artifacts

## Checklist

### Process Execution
- [ ] **InvokeCommand** - ROI: 9
  - [ ] Execute with arguments
  - [ ] Capture exit code properly
  - [ ] Option to throw on non-zero exit
  - [ ] Working directory support
  - [ ] Environment variable injection
  - [ ] Timeout support

- [ ] **GetCommandOutput** - ROI: 9
  - [ ] Capture stdout separately
  - [ ] Capture stderr separately
  - [ ] Combined output option
  - [ ] Real-time streaming option
  - [ ] Handle large outputs efficiently

### Environment Management
- [ ] **GetEnvironmentVariable** - ROI: 9
  - [ ] Read with fallback value
  - [ ] Expand embedded variables
  - [ ] Process/User/Machine scope
  - [ ] Required vs optional variables

- [ ] **SetEnvironmentVariable** - ROI: 8
  - [ ] Set for current process
  - [ ] Persist for user (where supported)
  - [ ] Support for PATH manipulation
  - [ ] Batch updates for efficiency

- [ ] **GetPlatformInfo** - ROI: 8
  - [ ] Operating system detection
  - [ ] Architecture (x64, ARM, etc.)
  - [ ] Is running in CI detection
  - [ ] Is running in Docker/WSL
  - [ ] .NET runtime version

### Archive Operations
- [ ] **CompressArchive** (zip/tar) - ROI: 9
  - [ ] Create .zip files
  - [ ] Create .tar.gz files  
  - [ ] Include/exclude patterns
  - [ ] Compression level control
  - [ ] Progress reporting
  - [ ] Preserve file attributes

### Tool Discovery
- [ ] **FindExecutable** (which) - ROI: 7
  - [ ] Search PATH
  - [ ] Search common tool locations
  - [ ] Version checking support
  - [ ] Handle .exe on Windows

- [ ] **TestCommand** - ROI: 7
  - [ ] Check if command exists
  - [ ] Verify minimum version
  - [ ] Cache results for performance

### Alias Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Which => FindExecutable
  - [ ] Env => GetEnvironmentVariable  
  - [ ] Export => SetEnvironmentVariable
  - [ ] Zip => CompressArchive
  - [ ] Tar => CompressArchive -Format tar

### Testing
- [ ] Test command execution with various exit codes
- [ ] Test output capture for large outputs
- [ ] Test environment variable operations
- [ ] Test platform detection on different OS
- [ ] Test archive creation with various formats
- [ ] Test tool discovery in PATH
- [ ] Test timeout and cancellation

## Implementation Notes

### Example Usage

```csharp
// Typical build script pattern
if (!TestCommand("dotnet", minVersion: "8.0"))
{
    throw new Exception("dotnet SDK 8.0+ required");
}

// Set build configuration
SetEnvironmentVariable("CONFIGURATION", "Release");
SetEnvironmentVariable("VERSION", version);

// Run build
var buildResult = InvokeCommand("dotnet", "build -c Release");
if (!buildResult.Success)
{
    Console.Error.WriteLine(buildResult.StandardError);
    Environment.Exit(1);
}

// Run tests and capture output
var testOutput = GetCommandOutput("dotnet", "test --logger:trx");
File.WriteAllText("test-results.trx", testOutput.StandardOutput);

// Create release package
CompressArchive(
    source: new[] { "bin/Release/", "README.md", "LICENSE" },
    destination: $"release-{version}.zip",
    compression: CompressionLevel.Optimal
);

// Platform-specific logic
if (GetPlatformInfo().IsWindows)
{
    InvokeCommand("signtool", $"sign /f cert.pfx {artifact}");
}
```

### CI/CD Integration Patterns

```csharp
// Detect CI environment
var platform = GetPlatformInfo();
if (platform.IsCI)
{
    // CI-specific configuration
    SetEnvironmentVariable("CI", "true");
    SetEnvironmentVariable("TERM", "dumb"); // Disable colors
}

// Multi-platform archive creation
var archiveName = platform.IsWindows 
    ? $"app-{version}-win-x64.zip"
    : $"app-{version}-{platform.OS}-{platform.Arch}.tar.gz";

CompressArchive("publish/", archiveName);

// Tool resolution with fallbacks
var npm = FindExecutable("npm") ?? 
          FindExecutable("npm.cmd") ?? 
          throw new Exception("npm not found");

InvokeCommand(npm, "install");
```

### Performance Targets

| Operation | External Process | Native Target | Improvement |
|-----------|-----------------|---------------|-------------|
| Get env var | ~20ms | <1ms | 20x |
| Which command | ~30ms | <2ms | 15x |
| Create small zip | ~100ms | ~10ms | 10x |
| Create large zip | ~5000ms | ~500ms | 10x |

### Cross-Platform Considerations

- **PATH Separator**: ; on Windows, : on Unix
- **Executable Extensions**: .exe, .cmd, .bat on Windows
- **Environment Persistence**: Different per OS
- **Archive Formats**: Prefer zip on Windows, tar.gz on Linux
- **CI Detection**: Check common CI environment variables

### Error Handling Strategy

```csharp
// Build scripts should fail fast and clear
try
{
    var result = InvokeCommand("build-tool", args);
    if (!result.Success)
    {
        Console.Error.WriteLine($"Build failed: {result.StandardError}");
        Environment.Exit(result.ExitCode);
    }
}
catch (CommandNotFoundException ex)
{
    Console.Error.WriteLine($"Required tool not found: {ex.Command}");
    Console.Error.WriteLine($"Please install: {ex.InstallInstructions}");
    Environment.Exit(1);
}
```

## Dependencies

- Task 019 (Native namespace structure) must be complete
- System.Diagnostics.Process for process execution
- System.IO.Compression for archives
- System.Environment for environment variables

## References

- Unix env, export, which, tar, zip commands
- PowerShell Invoke-Command, Compress-Archive cmdlets
- Cake build ToolResolver and ProcessRunner
- GitHub Actions, Azure DevOps CI patterns