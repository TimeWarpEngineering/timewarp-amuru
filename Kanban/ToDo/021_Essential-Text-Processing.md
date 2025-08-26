# 021 Essential Text Processing (High ROI)

## Description

Implement the most critical text processing commands used in build scripts for searching code, updating version numbers, modifying configurations, and processing output from other tools. These native implementations eliminate the overhead of spawning grep/sed processes that are called repeatedly in builds.

## Business Value

**ROI Score: 10/10** - Text processing is fundamental to build automation:
- **Version Bumping**: Update version numbers across multiple files
- **Configuration Updates**: Modify settings without full file rewrites  
- **Search Operations**: Find patterns in code and logs
- **Output Processing**: Parse and transform tool outputs
- **Performance**: 20-100x faster than spawning grep/sed repeatedly

## Requirements

- Implement the most essential text processing for build scenarios
- Support both line-based and whole-file operations
- Provide regex support with named groups
- Handle large files with streaming where appropriate
- Ensure encoding compatibility (UTF-8 default)

## Checklist

### Pattern Matching
- [ ] **SelectString** (grep) - ROI: 10
  - [ ] Basic string matching
  - [ ] Full regex support
  - [ ] Case-insensitive option
  - [ ] Return line numbers
  - [ ] Context lines (before/after)
  - [ ] Invert match option
  - [ ] Multiple file support
  - [ ] Include/exclude file patterns

### Text Replacement
- [ ] **ReplaceInFiles** (sed -i) - ROI: 10
  - [ ] Simple string replacement
  - [ ] Regex replacement with groups
  - [ ] In-place file modification
  - [ ] Backup option before modification
  - [ ] Multiple file support with patterns
  - [ ] Dry-run mode to preview changes

### Data Format Conversion
- [ ] **ConvertToJson** - ROI: 9
  - [ ] Object to JSON serialization
  - [ ] Pretty print option
  - [ ] Camel case option

- [ ] **ConvertFromJson** (jq) - ROI: 9
  - [ ] JSON to object deserialization
  - [ ] Path queries (like jq)
  - [ ] Handle malformed JSON gracefully

### Line Operations
- [ ] **SelectObject** (head/tail) - ROI: 8
  - [ ] First N lines
  - [ ] Last N lines
  - [ ] Skip N lines
  - [ ] Line range selection

- [ ] **MeasureObject** (wc) - ROI: 7
  - [ ] Line count
  - [ ] Word count
  - [ ] Character count
  - [ ] Byte count

### Alias Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Grep => SelectString
  - [ ] Sed => ReplaceInFiles
  - [ ] Head => SelectObject -First
  - [ ] Tail => SelectObject -Last
  - [ ] Wc => MeasureObject
  - [ ] Jq => ConvertFromJson with path

### Testing
- [ ] Test grep with various regex patterns
- [ ] Test file replacement with backups
- [ ] Test JSON serialization/deserialization
- [ ] Test head/tail operations
- [ ] Test large file streaming
- [ ] Test encoding handling
- [ ] Test multi-file operations

## Implementation Notes

### Example Usage

```csharp
// Version bumping in build script
var version = "2.0.0";
ReplaceInFiles(
    pattern: @"<Version>[\d\.]+</Version>",
    replacement: $"<Version>{version}</Version>",
    files: "**/*.csproj"
);

// Search for TODO comments
var todos = SelectString(@"//\s*TODO:", "src/**/*.cs");
foreach (var match in todos)
{
    Console.WriteLine($"{match.File}:{match.Line}: {match.Text}");
}

// Parse package.json
var package = ConvertFromJson(File.ReadAllText("package.json"));
var deps = package.SelectPath("dependencies");

// Bash-style aliases
Grep("ERROR", "logs/*.log");
Sed("localhost", "production.server", "config/*.xml");
var top10 = Head("build.log", 10);

// Direct API for streaming
await foreach (var match in Direct.SelectString("pattern", hugeLogFile))
{
    if (match.LineNumber > 1000) break;
    ProcessMatch(match);
}
```

### Common Build Script Patterns

```csharp
// Update AssemblyInfo version
ReplaceInFiles(
    @"AssemblyVersion\(""[\d\.]+""\)",
    $"AssemblyVersion(\"{version}\")",
    "**/AssemblyInfo.cs"
);

// Find and process all test files
var testFiles = SelectString("TestMethod", "**/*Test.cs")
    .Select(m => m.FileName)
    .Distinct();

// Update multiple config values
var replacements = new Dictionary<string, string>
{
    ["DEBUG"] = "RELEASE",
    ["localhost"] = "prod-server",
    ["test-key"] = Environment.GetEnvironmentVariable("PROD_KEY")
};

foreach (var (pattern, replacement) in replacements)
{
    ReplaceInFiles(pattern, replacement, "appsettings.json");
}
```

### Performance Targets

| Operation | External Process | Native Target | Improvement |
|-----------|-----------------|---------------|-------------|
| Grep 1 file | ~40ms | <2ms | 20x |
| Grep 100 files | ~4000ms | <50ms | 80x |
| Replace in file | ~60ms | <5ms | 12x |
| Parse JSON | ~30ms | <1ms | 30x |

### Design Considerations

1. **Streaming**: Support IAsyncEnumerable for large files
2. **Memory**: Process line-by-line where possible
3. **Encoding**: Default UTF-8, detect when possible
4. **Atomicity**: Write to temp file then rename for safety
5. **Regex Caching**: Cache compiled regex for performance

## Dependencies

- Task 019 (Native namespace structure) must be complete
- System.Text.RegularExpressions for pattern matching
- System.Text.Json for JSON operations
- Consider JsonPath.Net for advanced JSON queries

## References

- Unix grep, sed, head, tail, wc commands
- PowerShell Select-String, Replace cmdlets
- jq for JSON processing patterns
- Cake build TextHelper methods