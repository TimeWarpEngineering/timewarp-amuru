# Add PathResolver utility for finding executables in PATH

## Description

Add a `PathResolver` static class to Amuru that provides cross-platform PATH resolution — the .NET equivalent of `which` (Unix) or `where` (Windows). This enables discovering which executable would be invoked for a given command name.

**Why Amuru?** Amuru is the public cross-platform Unix-like utilities library. Nuru (public CLI framework) cannot depend on private repos (Zana), so this public utility belongs in Amuru.

**Use Case:** nuru-search needs to disambiguate between multiple CLIs with the same name (e.g., multiple `dev` tools from different repos).

**GitHub Issue:** #50

## Location

**File:** `Source/TimeWarp.Amuru/Native/PathResolver.cs`

Fits alongside other native system operations in `Native/` (FileSystem, Aliases, Utilities).

## API Design

```csharp
namespace TimeWarp.Amuru;

public static class PathResolver
{
  /// <summary>
  /// Finds the first instance of an executable in PATH.
  /// Returns null if not found.
  /// </summary>
  /// <param name="name">The executable name (without extension on Windows)</param>
  /// <returns>Full path to the executable, or null if not found</returns>
  public static string? ResolveExecutable(string name);
  
  /// <summary>
  /// Finds all instances of an executable in PATH, in order of precedence.
  /// </summary>
  /// <param name="name">The executable name (without extension on Windows)</param>
  /// <returns>List of full paths to all matching executables</returns>
  public static IReadOnlyList<string> ResolveAllExecutables(string name);
}
```

## Implementation Details

### Cross-Platform Concerns

| Concern | Unix | Windows |
|---------|------|---------|
| **PATH separator** | `:` | `;` |
| **Executable extensions** | None | `.exe`, `.cmd`, `.bat` + `PATHEXT` env var |
| **Case sensitivity** | Case-sensitive | Case-insensitive |
| **Path separator** | `/` | `\` |

### Algorithm

1. If `name` contains path separators (`/` or `\`), return as-is if file exists
2. Get PATH environment variable
3. Split by platform-specific separator
4. For each directory in PATH:
   - Skip if directory doesn't exist
   - On Windows: try each extension from `PATHEXT` (or default `.exe;.cmd;.bat`)
   - Check if file exists and is executable (Unix: check execute permission)
   - Return first match for `ResolveExecutable`, collect all for `ResolveAllExecutables`

### Edge Cases

- Empty or null PATH environment variable → return empty/null
- Relative paths in PATH → resolve relative to current directory
- Symlinks → return symlink path as-is (don't resolve to target)
- Non-existent directories in PATH → skip silently
- Name already contains path separators → return as-is if exists, else null
- Empty or whitespace name → throw `ArgumentException`

## Checklist

- [ ] Create `Source/TimeWarp.Amuru/Native/PathResolver.cs`
- [ ] Implement `ResolveExecutable(string name)` method
- [ ] Implement `ResolveAllExecutables(string name)` method
- [ ] Handle Unix PATH resolution (case-sensitive, no extensions)
- [ ] Handle Windows PATH resolution (case-insensitive, PATHEXT extensions)
- [ ] Handle edge cases (null PATH, relative paths, symlinks, path separators in name)
- [ ] Add XML documentation with examples
- [ ] Create test file `tests/.../native/path-resolver.cs`
- [ ] Test: `ResolveExecutable_Given_ExistingCommand_ReturnsPath`
- [ ] Test: `ResolveExecutable_Given_NonExistentCommand_ReturnsNull`
- [ ] Test: `ResolveAllExecutables_Given_MultipleMatches_ReturnsAll`
- [ ] Test: `ResolveExecutable_Given_PathInName_ReturnsAsIs`
- [ ] Test: `ResolveExecutable_Given_EmptyName_ThrowsArgumentException`
- [ ] Build and verify all tests pass
- [ ] Update version in `Source/Directory.Build.props`
- [ ] Commit and push

## Notes

### Existing Patterns to Follow

- Static class with public methods (like `Git`, `Shell`)
- XML documentation with `<summary>`, `<param>`, `<returns>`, `<example>`
- No result record needed — simple `string?` and `IReadOnlyList<string>` return types

### Testing Strategy

Unlike `CommandMock` tests, these tests need actual executables in PATH. Options:

1. Use well-known system commands (`ls`, `cat` on Unix; `cmd`, `notepad` on Windows)
2. Create temporary files in a temp directory added to PATH for the test
3. Skip tests that require specific executables if not found

### Platform Detection

```csharp
bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
char pathSeparator = isWindows ? ';' : ':';
```

### PATHEXT Handling (Windows)

```csharp
string[] extensions = Environment.GetEnvironmentVariable("PATHEXT")?.Split(';')
  ?? new[] { ".exe", ".cmd", ".bat" };
```

## Related

- GitHub Issue: https://github.com/TimeWarpEngineering/timewarp-amuru/issues/50
- Consumer: nuru-search (needs to disambiguate multiple CLIs with same name)
