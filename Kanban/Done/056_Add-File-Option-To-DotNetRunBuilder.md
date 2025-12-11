# Add --file Option Support to DotNetRunBuilder

## Description

Add support for the `--file` option to the `DotNetRunBuilder` class in TimeWarp.Amuru. The `--file` option allows specifying a path to a file-based app to run, which can be passed as the first argument when there's no project in the current directory.

## Requirements

- Add a private field to store the file path
- Add a public method `WithFile(string filePath)` to set the file path
- Add logic in the `Build()` method to include the `--file` argument when specified
- Follow existing patterns in the codebase for consistency

## Checklist

### Implementation
- [x] Add private `File` field to store file path
- [x] Add `WithFile(string filePath)` method
- [x] Add `--file` argument logic in Build() method
- [x] Verify the implementation follows existing patterns
- [x] Test the new functionality

### Documentation
- [x] Update XML documentation for the new method
- [x] Ensure method documentation follows existing style

## Notes

The `--file` option is used to specify the path to a file-based app to run. According to the .NET CLI documentation, this can also be passed as the first argument if there is no project in the current directory.

## Implementation Notes

This task involves modifying the `DotNetRunBuilder` class in `/Source/TimeWarp.Amuru/DotNetCommands/DotNet.Run.cs` to add the new functionality while maintaining consistency with the existing codebase patterns.