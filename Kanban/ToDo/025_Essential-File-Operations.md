# 025 Essential File Operations (High ROI)

## Description

Implement the most frequently used file operation commands that every build script needs. These native implementations will eliminate the overhead of spawning external processes for basic file operations that occur hundreds of times in typical build scripts.

## Business Value

**ROI Score: 10/10** - These commands are used in virtually every script and build pipeline. Native implementation provides:
- **Performance**: 10-50x faster than spawning cp/rm/mkdir processes
- **Cross-platform**: Eliminates platform-specific commands (copy vs cp, del vs rm)
- **Error Handling**: Consistent error reporting across platforms
- **Atomicity**: Better control over partial operations

## Requirements

- Implement the most essential file operations used in build scripts
- Provide both Commands (shell-like) and Direct (C# native) APIs
- Ensure cross-platform compatibility with consistent behavior
- Support common patterns like recursive operations and wildcards

## Checklist

### Core File Operations
- [ ] **CopyItem** (cp) - ROI: 10
  - [ ] Single file copy
  - [ ] Directory copy (recursive)
  - [ ] Overwrite options
  - [ ] Preserve attributes option
  - [ ] Progress reporting for large operations

- [ ] **MoveItem** (mv) - ROI: 9
  - [ ] File and directory move
  - [ ] Cross-drive support
  - [ ] Atomic operations where possible
  - [ ] Handle rename vs move

- [ ] **RemoveItem** (rm) - ROI: 10
  - [ ] File deletion
  - [ ] Directory deletion (recursive)
  - [ ] Force option for read-only
  - [ ] Safe deletion (no accidental root deletion)

- [ ] **NewItem** (mkdir/touch) - ROI: 9
  - [ ] Create directory (with parents)
  - [ ] Create empty file
  - [ ] Set initial content
  - [ ] Handle existing items

- [ ] **TestPath** (test -e) - ROI: 10
  - [ ] Check file exists
  - [ ] Check directory exists
  - [ ] Check any item exists
  - [ ] Return type checking (IsFile/IsDirectory)

### Pattern Matching
- [ ] **GetChildItem** with patterns (ls *.txt) - ROI: 8
  - [ ] Glob pattern support
  - [ ] Recursive search
  - [ ] Include/exclude patterns
  - [ ] Hidden file handling

- [ ] **FindItem** (find) - ROI: 8
  - [ ] Find by name pattern
  - [ ] Find by size
  - [ ] Find by date
  - [ ] Find by attributes

### File Information
- [ ] **GetItemProperty** (stat) - ROI: 8
  - [ ] Size, dates, attributes
  - [ ] Owner/permissions (where applicable)
  - [ ] Is symbolic link
  - [ ] Target of symbolic link

### Alias Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Cp => CopyItem
  - [ ] Mv => MoveItem
  - [ ] Rm => RemoveItem
  - [ ] Mkdir => NewItem
  - [ ] Touch => NewItem
  - [ ] Test => TestPath
  - [ ] Find => FindItem
  - [ ] Stat => GetItemProperty

### Testing
- [ ] Test copy operations (file, directory, overwrite)
- [ ] Test move operations (rename, cross-drive)
- [ ] Test delete operations (file, directory, recursive)
- [ ] Test path testing (exists, type checking)
- [ ] Test pattern matching with globs
- [ ] Test cross-platform behavior (Windows/Linux/Mac)
- [ ] Test error conditions (permissions, missing files)

## Implementation Notes

### Example Usage

```csharp
// Build script example - staging artifacts
if (TestPath("bin/Release"))
{
    RemoveItem("artifacts", recursive: true);
    NewItem("artifacts", ItemType.Directory);
    
    CopyItem("bin/Release/*.dll", "artifacts/");
    CopyItem("bin/Release/*.exe", "artifacts/");
    
    var configs = GetChildItem("**/*.config", recursive: true);
    foreach (var config in configs)
    {
        CopyItem(config, $"artifacts/{config.Name}");
    }
}

// Bash-style aliases
if (Test("build.lock"))
{
    Rm("build.lock");
}
Mkdir("output/bin");
Cp("src/*.cs", "backup/", recursive: true);

// Direct API for advanced scenarios
await Direct.CopyItem(
    source: largeFile,
    destination: backup,
    progress: new Progress<long>(bytes => 
        Console.Write($"\rCopying: {bytes / 1_048_576}MB"))
);
```

### Performance Targets

| Operation | External Process | Native Target | Improvement |
|-----------|-----------------|---------------|-------------|
| Copy 1 file | ~50ms | <5ms | 10x |
| Copy 100 files | ~5000ms | <100ms | 50x |
| Test path exists | ~30ms | <1ms | 30x |
| Remove directory | ~100ms | <10ms | 10x |

### Cross-Platform Considerations

- **Path Separators**: Normalize to forward slashes internally
- **Case Sensitivity**: Respect filesystem rules
- **Permissions**: Handle gracefully with clear error messages
- **Symbolic Links**: Follow or preserve based on options
- **Long Paths**: Support Windows long path (>260 chars)

## Dependencies

- Task 019 (Native namespace structure) must be complete
- System.IO for core operations
- Consider Microsoft.IO.RecyclableMemoryStream for large file copies

## References

- Unix cp, mv, rm, mkdir, find commands
- PowerShell Copy-Item, Move-Item, Remove-Item cmdlets
- Cake build FileHelper and DirectoryHelper
- .NET System.IO.File and Directory classes