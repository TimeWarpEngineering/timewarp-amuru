# Implement Git Native Commands

## Description

Implement `Git.FindRoot()` and `Git.GetRepositoryName()` as native C# implementations following the progressive enhancement pattern. This provides better performance and control compared to spawning git processes, and serves as a reference implementation for the pattern.

## Requirements

- Pure C# implementation for FindRoot (no external git process)
- Native implementation for GetRepositoryName (may call git command)
- Follow Layers 1-4 of the progressive enhancement pattern
- Maintain compatibility with existing code
- Cross-platform support (Windows, Linux, macOS, worktrees)

## Checklist

### Phase 1: Native Direct API (Layer 1)

- [ ] Create `Source/TimeWarp.Amuru/Native/Git/Direct/Direct.FindRoot.cs`
- [ ] Implement directory tree walking to find .git folder/file
- [ ] Handle git worktrees (where .git is a file, not folder)
- [ ] Throw `InvalidOperationException` when not in repository
- [ ] Add integration tests for `Direct.FindRoot()`
- [ ] Verify cross-platform functionality (Windows, Linux, macOS)

- [ ] Create `Source/TimeWarp.Amuru/Native/Git/Direct/Direct.GetRepositoryName.cs`
- [ ] Call git command via Shell.Builder to get remote URL
- [ ] Parse repository name from URL
- [ ] Handle github.com, gitlab.com, bitbucket.org URLs
- [ ] Fallback to directory name if no remote configured
- [ ] Add integration tests for `Direct.GetRepositoryNameAsync()`

### Phase 2: Native Commands API (Layer 2)

- [ ] Create `Source/TimeWarp.Amuru/Native/Git/Commands/Commands.FindRoot.cs`
- [ ] Wrap `Direct.FindRoot()` with try/catch
- [ ] Return `CommandOutput` with exit code 0 on success (root in stdout)
- [ ] Return `CommandOutput` with exit code 1 and stderr on failure
- [ ] Add integration tests for `Commands.FindRoot()`

- [ ] Create `Source/TimeWarp.Amuru/Native/Git/Commands/Commands.GetRepositoryName.cs`
- [ ] Wrap `Direct.GetRepositoryNameAsync()` with try/catch
- [ ] Return `CommandOutput` with repo name in stdout
- [ ] Handle errors gracefully with stderr messages
- [ ] Add integration tests for `Commands.GetRepositoryNameAsync()`

### Phase 3: Bash Aliases (Layer 4)

- [ ] Add `GitRoot(string? startPath = null)` to `Source/TimeWarp.Amuru/Native/Aliases/Bash.cs`
- [ ] Add `GitRepoName(string? gitRoot = null)` to Bash aliases
- [ ] Provide both Direct and Commands versions (different return types)
- [ ] Document usage in README

### Phase 4: Migrate Existing Code

- [ ] Update `exe/display-avatar.cs` to use `Native.Git.Direct.FindRoot()`
- [ ] Update `exe/display-avatar.cs` to use `Native.Git.Direct.GetRepositoryNameAsync()`
- [ ] Update `exe/generate-avatar.cs` to use native Git Direct methods
- [ ] Update `Source/TimeWarp.Ganda/Program.cs` GenerateAvatarCommand to use Direct API
- [ ] Delete old `Source/TimeWarp.Amuru/GitCommands/` directory entirely
- [ ] Verify all existing functionality works correctly after migration

### Phase 5: Documentation

- [ ] Update `Documentation/Conceptual/ArchitecturalLayers.md` with Git as concrete example
- [ ] Add Native Git section to main README.md
- [ ] Update `CLAUDE.md` with Git native usage examples
- [ ] Document migration path from old `Git.*` to new `Native.Git.*`
- [ ] Add examples showing all consumption patterns (Direct, Commands, Aliases)

## Migration Guide

### Current Implementation (Old)

```csharp
using TimeWarp.Amuru;

string? gitRoot = Git.FindRoot();
if (gitRoot != null)
{
  string? repoName = await Git.GetRepositoryNameAsync(gitRoot);
  Console.WriteLine($"Repository: {repoName}");
}
```

### New Implementation - Direct API (Recommended)

```csharp
using TimeWarp.Amuru.Native.Git;

try
{
  string gitRoot = Direct.FindRoot();  // Throws if not in repo
  string repoName = await Direct.GetRepositoryNameAsync(gitRoot);
  Console.WriteLine($"Repository: {repoName}");
}
catch (InvalidOperationException ex)
{
  Console.WriteLine($"Not in a git repository: {ex.Message}");
}
```

### New Implementation - Commands API (Shell-Like)

```csharp
using TimeWarp.Amuru.Native.Git;

CommandOutput rootResult = Commands.FindRoot();
if (rootResult.Success)
{
  string gitRoot = rootResult.Stdout;
  CommandOutput nameResult = await Commands.GetRepositoryNameAsync(gitRoot);

  if (nameResult.Success)
  {
    string repoName = nameResult.Stdout;
    Console.WriteLine($"Repository: {repoName}");
  }
}
else
{
  Console.Error.WriteLine(rootResult.Stderr);
}
```

### New Implementation - Bash Aliases (Minimal Verbosity)

```csharp
global using static TimeWarp.Amuru.Native.Aliases.Bash;

try
{
  string gitRoot = GitRoot();  // Direct version
  string repoName = await GitRepoName(gitRoot);
  Console.WriteLine($"Repository: {repoName}");
}
catch (InvalidOperationException ex)
{
  Console.WriteLine($"Error: {ex.Message}");
}
```

## Notes

This task implements Layers 1, 2, and 4 of the progressive enhancement pattern. Future enhancements could add:

- **Layer 3**: Git.Builder() for fluent API (if complex configuration needed)
- **Layer 5**: Ganda `timewarp git-root` and `timewarp git-repo-name` commands
- **Layer 6**: Enhanced `git` AOT executable with catchall passthrough

See `Documentation/Developer/ProgressiveEnhancementPattern.md` for the complete pattern explanation and how to apply it to other commands.

## Benefits

### Performance
- Eliminates process spawning overhead for FindRoot (pure C#)
- Faster execution, especially on Windows
- Better startup time with AOT compilation

### Control
- Own the implementation, can fix bugs
- Add features git doesn't provide
- Better error messages and diagnostics
- Cross-platform consistency

### Developer Experience
- Multiple consumption patterns (Direct, Commands, Aliases)
- Type-safe C# instead of string-based CLI
- IntelliSense support for git operations
- LINQ-friendly for complex scenarios

## Success Criteria

- [ ] All Native/Git/Direct methods implemented and tested
- [ ] All Native/Git/Commands methods implemented and tested
- [ ] Bash aliases added and documented
- [ ] Existing code (exe scripts, Ganda) successfully migrated
- [ ] All integration tests passing on all platforms
- [ ] Documentation updated with examples
- [ ] Old GitCommands/ directory removed
- [ ] Cross-platform verification complete (Windows, Linux, macOS, worktrees)

## Related

- See `Documentation/Developer/ProgressiveEnhancementPattern.md` for the general pattern
- See `005_Implement-Git-Fluent-API.md` (superseded by this approach)
- Future: Apply same pattern to grep, find, sed, awk, curl
