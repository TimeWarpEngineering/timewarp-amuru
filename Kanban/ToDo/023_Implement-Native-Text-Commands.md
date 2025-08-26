# 023 Implement Native Text Commands

## Description

Implement native text processing commands in the Native/Text namespace using PowerShell-inspired naming with bash aliases. These commands will provide in-process text manipulation without spawning external processes, offering better performance and type safety.

## Requirements

- Create text processing commands with both Commands and Direct APIs
- Follow PowerShell verb-noun naming convention
- Provide bash aliases for familiar command names
- Implement streaming where appropriate for large text processing
- Maintain consistency with existing FileSystem implementation

## Checklist

### Text Namespace Implementation
- [ ] Create Native/Text/Commands.cs
  - [ ] Static class Commands with shell semantics (returns CommandOutput)
  - [ ] Implement SelectString(string pattern, string input) - grep functionality
  - [ ] Implement ReplaceString(string pattern, string replacement, string input) - sed functionality
  - [ ] Implement SortObject(string input, bool descending = false) - sort functionality
  - [ ] Implement SelectObject(string input, int? first = null, int? last = null) - head/tail
  - [ ] Implement MeasureObject(string input) - wc functionality (lines, words, chars)
  - [ ] Implement JoinString(string[] lines, string delimiter = "\n")
  - [ ] Implement SplitString(string input, string delimiter) - cut/awk functionality
  - [ ] Handle errors with proper stderr/exit codes
  - [ ] No exceptions thrown - shell semantics

- [ ] Create Native/Text/Direct.cs
  - [ ] Static class Direct with C# semantics (strongly typed, can throw)
  - [ ] Implement SelectString returning IAsyncEnumerable<MatchResult>
  - [ ] Implement ReplaceString returning processed string
  - [ ] Implement SortObject returning IAsyncEnumerable<string>
  - [ ] Implement SelectObject returning IAsyncEnumerable<string>
  - [ ] Implement MeasureObject returning TextMeasurement struct
  - [ ] Stream processing for large text
  - [ ] LINQ-composable operations

### Alias System Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Grep(pattern, input) => Text.Commands.SelectString()
  - [ ] Sed(pattern, replacement, input) => Text.Commands.ReplaceString()
  - [ ] Sort(input) => Text.Commands.SortObject()
  - [ ] Head(input, n) => Text.Commands.SelectObject(first: n)
  - [ ] Tail(input, n) => Text.Commands.SelectObject(last: n)
  - [ ] Wc(input) => Text.Commands.MeasureObject()
  - [ ] Cut(input, delimiter) => Text.Commands.SplitString()
  - [ ] Awk(script, input) => Text.Commands.ProcessText() [advanced]

### Data Types
- [ ] Create TextMeasurement struct
  - [ ] LineCount property
  - [ ] WordCount property
  - [ ] CharacterCount property
  - [ ] ByteCount property

- [ ] Create MatchResult class
  - [ ] LineNumber property
  - [ ] Line property
  - [ ] Matches collection
  - [ ] FileName property (for future file search)

### Testing
- [ ] Test SelectString with various regex patterns
- [ ] Test ReplaceString with simple and complex replacements
- [ ] Test SortObject ascending and descending
- [ ] Test SelectObject for head/tail functionality
- [ ] Test MeasureObject counts accuracy
- [ ] Test JoinString and SplitString operations
- [ ] Test streaming behavior with large text
- [ ] Test both Commands and Direct APIs
- [ ] Test bash aliases work correctly

## Implementation Notes

### Example Usage

```csharp
// PowerShell-style
var matches = SelectString(@"\berror\b", logContent);
var sorted = SortObject(unsortedLines, descending: true);
var stats = MeasureObject(fileContent);

// Bash-style aliases
var errors = Grep("ERROR", logContent);
var modified = Sed("foo", "bar", content);
var topTen = Head(content, 10);

// Direct API for LINQ
await foreach (var match in Direct.SelectString(@"\d+", content))
{
    Console.WriteLine($"Found number on line {match.LineNumber}: {match.Line}");
}

// Measure large file efficiently
var stats = await Direct.MeasureObject(hugeFile);
Console.WriteLine($"Lines: {stats.LineCount:N0}");
```

### Design Considerations

1. **Regex Support**: SelectString should support full regex patterns
2. **Performance**: Use streaming for large text processing
3. **Memory Efficiency**: Avoid loading entire text into memory when possible
4. **Compatibility**: Match behavior of common Unix text tools
5. **Encoding**: Handle UTF-8 by default, with options for other encodings

## Dependencies

- Task 019 (Native namespace structure) must be complete
- Builds on CommandOutput and dual API pattern established

## References

- PowerShell Select-String cmdlet
- Unix grep, sed, awk, sort, head, tail, wc commands
- .NET Regex and string manipulation APIs