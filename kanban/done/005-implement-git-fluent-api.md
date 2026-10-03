# Implement Git Fluent API

## Description

Create a strongly typed Git command API to provide an idiomatic C# interface for Git operations, starting with a FindRepositoryRoot() method. This will enable scripts to reliably locate repository boundaries and provide a foundation for additional Git operations using TimeWarp.Cli's fluent API patterns.

## Requirements

- Create a `Git` static class with fluent builder methods
- Implement `FindRepositoryRoot()` method that traverses directories to find `.git` folder
- Return null or empty string when not in a Git repository (consistent with library's failure philosophy)
- Support both direct execution and builder pattern
- Follow existing fluent API patterns from DotNet and Fzf classes
- Ensure cross-platform compatibility (Windows, Linux, macOS)

## Checklist

### Design
- [ ] Create Git static class following existing patterns
- [ ] Design FindRepositoryRoot() method signature
- [ ] Plan extension points for future Git operations
- [ ] Consider backward compatibility for future additions

### Implementation
- [ ] Create Git.cs with static builder methods
- [ ] Implement FindRepositoryRoot() using `git rev-parse --show-toplevel`
- [ ] Handle non-git directory scenarios gracefully
- [ ] Add appropriate error handling following library conventions
- [ ] Verify Cross-platform Functionality

### Documentation
- [ ] Update API Documentation with Git examples
- [ ] Update Usage Examples in CLAUDE.md
- [ ] Add Git section to fluent API documentation
- [ ] Document FindRepositoryRoot() behavior and usage

## Notes

Example usage patterns:
```csharp
// Direct static method
string repoRoot = await Git.FindRepositoryRoot();

// Builder pattern for consistency
string repoRoot = await Git.Run()
    .FindRepositoryRoot()
    .GetStringAsync();

// Future expansion possibilities
var status = await Git.Run()
    .WithWorkingDirectory("/path/to/project")
    .Status()
    .GetLinesAsync();

var branches = await Git.Run()
    .Branch()
    .WithRemotes()
    .GetLinesAsync();
```

The FindRepositoryRoot() method will use `git rev-parse --show-toplevel` internally, which:
- Returns the absolute path to the repository root
- Exits with non-zero code when not in a Git repository
- Works consistently across all platforms

## Implementation Notes

[To be filled during implementation]