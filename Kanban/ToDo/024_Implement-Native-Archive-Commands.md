# 024 Implement Native Archive Commands

## Description

Implement native archive and compression commands in the Native/Archive namespace using PowerShell-inspired naming with bash aliases. These commands will provide in-process compression and extraction without spawning external tools like tar or zip, offering better performance and cross-platform consistency.

## Requirements

- Create archive manipulation commands with both Commands and Direct APIs
- Follow PowerShell verb-noun naming convention
- Provide bash aliases for familiar command names
- Support common archive formats (zip, tar, tar.gz, tar.bz2)
- Handle large archives with streaming
- Maintain consistency with existing implementations

## Checklist

### Archive Namespace Implementation
- [ ] Create Native/Archive/Commands.cs
  - [ ] Static class Commands with shell semantics (returns CommandOutput)
  - [ ] Implement CompressArchive(string[] paths, string destination, string? format = "zip")
  - [ ] Implement ExpandArchive(string path, string? destination = null)
  - [ ] Implement TestArchive(string path) - verify integrity
  - [ ] Implement GetArchiveContent(string path) - list contents
  - [ ] Implement AddToArchive(string archive, string[] paths) - add files
  - [ ] Implement RemoveFromArchive(string archive, string[] paths) - remove files
  - [ ] Implement UpdateArchive(string archive, string[] paths) - update files
  - [ ] Handle errors with proper stderr/exit codes
  - [ ] No exceptions thrown - shell semantics

- [ ] Create Native/Archive/Direct.cs
  - [ ] Static class Direct with C# semantics (strongly typed, can throw)
  - [ ] Implement CompressArchive returning Task with progress
  - [ ] Implement ExpandArchive returning IAsyncEnumerable<ExtractedFile>
  - [ ] Implement TestArchive returning ArchiveTestResult
  - [ ] Implement GetArchiveContent returning IAsyncEnumerable<ArchiveEntry>
  - [ ] Support streaming for large files
  - [ ] Support cancellation tokens
  - [ ] LINQ-composable operations

### Format Support
- [ ] Create format handlers
  - [ ] ZipHandler - .zip files (System.IO.Compression)
  - [ ] TarHandler - .tar files
  - [ ] GZipHandler - .gz compression
  - [ ] BZip2Handler - .bz2 compression
  - [ ] Format detection from file extension
  - [ ] Format detection from file header (magic bytes)

### Alias System Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Zip(paths, dest) => Archive.Commands.CompressArchive(format: "zip")
  - [ ] Unzip(archive, dest?) => Archive.Commands.ExpandArchive()
  - [ ] Tar(options, paths) => Archive.Commands.CompressArchive(format: "tar")
  - [ ] Untar(archive, dest?) => Archive.Commands.ExpandArchive()
  - [ ] Gzip(file) => Archive.Commands.CompressArchive(format: "gz")
  - [ ] Gunzip(file) => Archive.Commands.ExpandArchive()
  - [ ] ZipInfo(archive) => Archive.Commands.GetArchiveContent()

### Data Types
- [ ] Create ArchiveEntry class
  - [ ] Name property
  - [ ] FullPath property
  - [ ] CompressedSize property
  - [ ] UncompressedSize property
  - [ ] CompressionRatio property
  - [ ] LastModified property
  - [ ] IsDirectory property
  - [ ] Attributes property
  - [ ] CRC property

- [ ] Create ArchiveOptions class
  - [ ] CompressionLevel property (Fast/Optimal/None)
  - [ ] IncludeBaseDirectory property
  - [ ] PreserveAttributes property
  - [ ] Password property (for encrypted archives)
  - [ ] ExcludePatterns property
  - [ ] FollowSymlinks property

- [ ] Create ArchiveTestResult class
  - [ ] IsValid property
  - [ ] Errors collection
  - [ ] Warnings collection
  - [ ] TestedEntries count
  - [ ] CorruptedEntries collection

### Testing
- [ ] Test CompressArchive with single file
- [ ] Test CompressArchive with directory tree
- [ ] Test ExpandArchive to specified directory
- [ ] Test different compression formats
- [ ] Test large file handling (streaming)
- [ ] Test archive integrity verification
- [ ] Test listing archive contents
- [ ] Test adding/removing files from existing archive
- [ ] Test password-protected archives
- [ ] Test both Commands and Direct APIs
- [ ] Test bash aliases work correctly

## Implementation Notes

### Example Usage

```csharp
// PowerShell-style
var result = CompressArchive(new[] { "src/", "docs/" }, "backup.zip");
var expanded = ExpandArchive("archive.tar.gz", "./output");
var contents = GetArchiveContent("package.zip");

// Bash-style aliases
Zip(new[] { "*.txt" }, "texts.zip");
Untar("source.tar.gz", "/tmp/extracted");
var info = ZipInfo("archive.zip");

// Direct API for advanced scenarios
await Direct.CompressArchive(
    paths: sourceFiles,
    destination: "output.zip",
    options: new ArchiveOptions
    {
        CompressionLevel = CompressionLevel.Optimal,
        Progress = new Progress<int>(p => Console.Write($"\rCompressing: {p}%"))
    }
);

// Stream large archive extraction
await foreach (var file in Direct.ExpandArchive("huge.tar.gz"))
{
    Console.WriteLine($"Extracted: {file.Path} ({file.Size:N0} bytes)");
}

// Test archive integrity
var test = await Direct.TestArchive("potentially-corrupt.zip");
if (!test.IsValid)
{
    foreach (var error in test.Errors)
        Console.WriteLine($"Error: {error}");
}
```

### Compression Strategy

```csharp
// Auto-detect format from extension
.zip     -> ZIP format
.tar     -> TAR (uncompressed)
.tar.gz  -> TAR + GZIP
.tar.bz2 -> TAR + BZIP2
.gz      -> GZIP (single file)
.bz2     -> BZIP2 (single file)
```

### Design Considerations

1. **Memory Efficiency**: Stream large files instead of loading into memory
2. **Progress Reporting**: Support progress callbacks for long operations
3. **Atomic Operations**: Use temp files and rename for safety
4. **Compression Levels**: Balance speed vs size
5. **Cross-Platform**: Handle path separators and attributes correctly
6. **Encryption**: Support password protection where format allows

### Performance Optimization

- **Parallel Compression**: Use multiple threads for large archives
- **Buffer Sizes**: Optimize for different file sizes
- **Memory Pools**: Reuse buffers for streaming operations
- **Lazy Evaluation**: Don't decompress until needed

## Dependencies

- Task 019 (Native namespace structure) must be complete
- Builds on CommandOutput and dual API pattern
- System.IO.Compression for ZIP support
- Consider SharpZipLib or similar for extended format support

## References

- PowerShell Compress-Archive, Expand-Archive cmdlets
- Unix tar, zip, gzip, bzip2 commands
- .NET System.IO.Compression namespace
- ZIP and TAR file format specifications