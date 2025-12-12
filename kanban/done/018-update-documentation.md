# 018 Update Documentation

## Description

Update all documentation to reflect the new API design, including README.md, CLAUDE.md, and create a migration guide for users moving from the old API to the new one.

## Requirements

- Update all documentation with new API
- Create clear migration guide
- Include best practices
- Show common patterns

## Checklist

### README.md Updates
- [x] Update API overview section
- [x] Replace all GetStringAsync() examples
- [x] Replace all GetLinesAsync() examples
- [x] Add RunAsync() as the primary example
- [x] Add section on NO CACHING philosophy
- [x] Update method comparison table
- [x] Add streaming examples
- [x] Add CancellationToken examples

### CLAUDE.md Updates
- [x] Update API method descriptions
- [x] Update usage examples
- [x] Add new method signatures
- [x] Remove references to caching
- [x] Add CommandOutput documentation
- [x] Update best practices section

### Create Migration Guide
- [x] Create Analysis/MigrationGuide.md
- [x] Document all method mappings (old → new)
- [x] Provide code examples for each migration
- [x] Explain rationale for changes
- [x] Include breaking change warnings
- [x] Add troubleshooting section

### Documentation Structure
- [ ] API Reference
  - [ ] RunAsync() - default streaming behavior
  - [ ] CaptureAsync() - silent capture
  - [ ] Stream methods - large data handling
  - [ ] CommandOutput structure
  
- [ ] Best Practices
  - [ ] When to use RunAsync() vs CaptureAsync()
  - [ ] Handling large outputs
  - [ ] Error handling patterns
  - [ ] CancellationToken usage
  
- [ ] Common Patterns
  - [ ] Basic script execution
  - [ ] CI/CD pipelines
  - [ ] Interactive tools
  - [ ] Error handling

## Notes

### README Example Update
```markdown
## Quick Start

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru

using TimeWarp.Amuru;

// Default behavior - stream to console (like bash/PowerShell)
await Shell.Builder("npm", "install").RunAsync();

// Capture output when needed
var result = await Shell.Builder("git", "status").CaptureAsync();
if (result.Success)
{
    Console.WriteLine($"Git says: {result.Stdout}");
}

// Stream large files without memory issues
await foreach (var line in Shell.Builder("tail", "-f", "/var/log/app.log").StreamStdoutAsync())
{
    Console.WriteLine($"Log: {line}");
}
```
```

### Migration Guide Structure
```markdown
# Migration Guide: v0.x to v1.0

## Breaking Changes

### Removed Methods
- `GetStringAsync()` - Use `CaptureAsync().Result.Stdout`
- `GetLinesAsync()` - Use `CaptureAsync().Result.Lines`
- `Cached()` - No replacement (NO CACHING by design)

### Renamed Methods
- `ExecuteAsync()` → `CaptureAsync()` or `RunAsync()`
- `ExecuteInteractiveAsync()` → `PassthroughAsync()`
- `GetStringInteractiveAsync()` → `SelectAsync()`

## Migration Examples

### Basic Command Execution
```csharp
// Old
await Shell.Builder("echo", "test").ExecuteAsync();

// New - for streaming to console
await Shell.Builder("echo", "test").RunAsync();

// New - for capturing output
var result = await Shell.Builder("echo", "test").CaptureAsync();
```
```

### Philosophy Documentation
```markdown
## Design Philosophy

### No Caching
TimeWarp.Amuru does NOT cache command results. This is intentional:
- Shells don't cache
- Commands can have side effects
- Results can change over time
- Users can trivially cache in C# if needed

```csharp
// If you need caching, do it yourself:
private static CommandOutput? cachedResult;
var result = cachedResult ??= await Shell.Builder("expensive-command").CaptureAsync();
```
```

## Dependencies

- All previous tasks should be complete

## References

- README.md
- CLAUDE.md
- Analysis/Architecture/*.md documents