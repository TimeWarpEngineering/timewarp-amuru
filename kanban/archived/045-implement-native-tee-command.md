# 045 Implement Native Tee Command

## Description

Investigate and implement the `tee` command as a Native command in TimeWarp.Amuru. The `tee` command reads from stdin and writes to both stdout and one or more files simultaneously, enabling output capture while maintaining console visibility.

## Use Case

```bash
cd Scripts && ./Build.cs 2>&1 | tee /tmp/build-output.txt
```

This allows build output to be both displayed in the console and saved to a file for later analysis.

## Requirements

- Implement tee command with both Commands and Direct APIs
- Support writing to multiple files simultaneously
- Support append mode (-a flag equivalent)
- Follow PowerShell verb-noun naming convention with bash alias
- Maintain consistency with existing FileSystem implementation
- Consider integration with Tests/Integration/Native/FileSystem/

## Checklist

### Design
- [ ] Determine appropriate PowerShell-style name (WriteObject? TeeObject?)
- [ ] Design API signature for Commands and Direct
- [ ] Decide on append vs overwrite behavior
- [ ] Consider multi-file output support
- [ ] Plan integration with existing pipeline operations

### Implementation
- [ ] Create Native/FileSystem/Commands/Commands.Tee.cs
  - [ ] Implement Tee(string input, string filePath, bool append = false)
  - [ ] Support multiple file paths
  - [ ] Handle errors with proper stderr/exit codes
  - [ ] No exceptions thrown - shell semantics

- [ ] Create Native/FileSystem/Direct/Direct.Tee.cs
  - [ ] Implement TeeAsync returning IAsyncEnumerable<string>
  - [ ] Stream processing for large inputs
  - [ ] LINQ-composable operations
  - [ ] Proper exception handling for file operations

### Alias System Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Tee(input, filePath, append) => FileSystem.Commands.Tee()

### Testing
- [ ] Create Tests/Integration/Native/FileSystem/TeeTests.cs
- [ ] Test basic tee functionality (stdout + file)
- [ ] Test append mode
- [ ] Test multiple file outputs
- [ ] Test with large input streams
- [ ] Test error handling (permission denied, disk full, etc.)
- [ ] Test both Commands and Direct APIs
- [ ] Test bash alias works correctly

## Implementation Notes

### Example Usage

```csharp
// PowerShell-style
var result = Tee(buildOutput, "/tmp/build-output.txt");

// With append mode
var result = Tee(logOutput, "/var/log/app.log", append: true);

// Multiple files
var result = Tee(output, new[] { "/tmp/output1.txt", "/tmp/output2.txt" });

// Bash-style alias
var result = Tee(output, "/tmp/output.txt");

// Direct API for streaming
await foreach (var line in Direct.TeeAsync(inputStream, "/tmp/output.txt"))
{
    // Line is written to file and yielded to stdout
    Console.WriteLine(line);
}
```

### Design Considerations

1. **Streaming**: Use IAsyncEnumerable for memory efficiency with large inputs
2. **Atomicity**: Consider buffering strategy for file writes
3. **Error Handling**: Decide how to handle partial failures (e.g., file write fails mid-stream)
4. **Encoding**: Handle UTF-8 by default, with options for other encodings
5. **Performance**: Minimize overhead compared to direct file writes
6. **Cross-platform**: Ensure behavior is consistent across Windows/Linux/macOS

### Open Questions

- Should tee integrate with existing pipeline operations?
- Should it support binary data or just text?
- How should it handle file locking scenarios?
- Should it support compression on-the-fly?

## Dependencies

- Builds on existing Native/FileSystem implementation
- Follows CommandOutput and dual API pattern established
- May relate to Task 023 (Native Text Commands) for pipeline integration

## References

- Unix `tee` command behavior
- PowerShell `Tee-Object` cmdlet
- .NET file I/O and streaming APIs
