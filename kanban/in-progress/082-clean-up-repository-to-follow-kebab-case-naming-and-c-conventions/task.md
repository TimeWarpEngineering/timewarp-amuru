# Clean up repository to follow kebab-case naming and C# conventions

## Description

Audit and clean up the entire repository to ensure consistent kebab-case file naming and proper C# documentation conventions per the csharp skill. This includes renaming files from PascalCase to kebab-case and replacing all "TODO: Add purpose description" region comments with proper Purpose/Design/Responsibilities documentation.

## Scope

This is a large-scale style cleanup task affecting the entire codebase. It should be done in manageable chunks to keep PRs reviewable.

## Checklist

### Phase 1: Core Files (source/timewarp-amuru/)
- [ ] Rename `AppContextExtensions.cs` → `app-context-extensions.cs`
- [ ] Rename `CliConfiguration.cs` → `cli-configuration.cs`
- [ ] Rename `ScriptContext.cs` → `script-context.cs`
- [ ] Rename `PathResolver.cs` → `path-resolver.cs`
- [ ] Rename `Installer.cs` → `installer.cs`
- [ ] Update regions in all core files with proper Purpose/Design descriptions

### Phase 2: DotNet Commands (source/timewarp-amuru/dot-net-commands/)
- [ ] Rename `DotNet.cs` → `dot-net.cs`
- [ ] Rename `DotNet.*.cs` files → `dot-net.*.cs` (kebab-case)
- [ ] Rename tool subdirectory files
- [ ] Update regions in all dotnet command files

### Phase 3: Git Commands (source/timewarp-amuru/git-commands/)
- [ ] Rename `Git.cs` → `git.cs`
- [ ] Rename `Git.*.cs` → `git.*.cs` (kebab-case)
- [ ] Update regions in all git command files

### Phase 4: Fzf Commands (source/timewarp-amuru/fzf-command/)
- [ ] Rename `Fzf.cs` → `fzf.cs`
- [ ] Rename `Fzf.*.cs` → `fzf.*.cs` (kebab-case)
- [ ] Update regions in all fzf command files

### Phase 5: Native/FileSystem (source/timewarp-amuru/native/)
- [ ] Rename `Bash.cs` → `bash.cs`
- [ ] Rename file-system commands (`Commands.*.cs` → `commands.*.cs`)
- [ ] Rename file-system direct (`Direct.*.cs` → `direct.*.cs`)
- [ ] Rename utilities (`Post.cs`, `GenerateColor.cs`, etc.)
- [ ] Rename `PathResolver.cs`
- [ ] Update regions in all native files

### Phase 6: JSON-RPC (source/timewarp-amuru/json-rpc/)
- [ ] Rename `JsonRpcClient.cs` → `json-rpc-client.cs`
- [ ] Rename `JsonRpcClientBuilder.cs` → `json-rpc-client-builder.cs`
- [ ] Rename `IJsonRpcClient.cs` → `ijson-rpc-client.cs`
- [ ] Update regions

### Phase 7: Testing (source/timewarp-amuru/testing/)
- [ ] Rename `CommandMock.cs` → `command-mock.cs`
- [ ] Rename `MockScope.cs` → `mock-scope.cs`
- [ ] Rename `MockState.cs` → `mock-state.cs`
- [ ] Rename `MockSetup.cs` → `mock-setup.cs`
- [ ] Update regions

### Phase 8: Interfaces & Extensions
- [ ] Rename `ICommandBuilder.cs` → `icommand-builder.cs`
- [ ] Rename `CommandBuilderExtensions.cs` → `command-builder-extensions.cs`
- [ ] Update regions

### Phase 9: Tests (tests/)
- [ ] Audit test files for PascalCase naming
- [ ] Rename test files to kebab-case
- [ ] Update regions in test files

### Phase 10: Final Verification
- [ ] Run `dotnet build` to verify no compilation errors
- [ ] Run `dev test` to verify all tests pass
- [ ] Check for any remaining PascalCase files
- [ ] Check for any remaining "TODO: Add purpose" comments
- [ ] Update documentation references if needed

## Region Documentation Template

All files should follow this pattern (see csharp skill):

```csharp
#region Purpose
// One-line description of what this file/class does
#endregion

#region Design
// Key design decisions and rationale
// Constraints and dependencies
// Why certain approaches were chosen
#endregion

#region Responsibilities (optional)
// What this class/file is responsible for
// What it is NOT responsible for
#endregion

#region Open Questions (optional)
// Q1 (YYYY-MM-DD, author): question text
// A1 (YYYY-MM-DD, author): answer text
#endregion
```

## Notes

### Current State
- 96 files have "TODO: Add purpose description" in their regions
- Many files still use PascalCase naming (e.g., `AppContextExtensions.cs`, `CliConfiguration.cs`)
- The `core/` folder has been cleaned up (completed in tasks 080/081)
- The `nu-get/` folder has been cleaned up (completed in task 081)

### Approach
1. Work folder-by-folder to keep commits focused
2. Each rename + region update should be a single commit
3. Create small PRs (one folder per PR) for easier review
4. Run tests after each folder cleanup

### Reference
- See `.ai/04-csharp-coding-standards.md` for C# conventions
- See `csharp` skill for region documentation patterns
- Examples of good collocated docs:
  - `source/timewarp-amuru/core/command-extensions.cs`
  - `source/timewarp-amuru/core/command-result.cs`

### Files with TODO: comments (96 total)

**Core (6 files):**
- AppContextExtensions.cs
- CliConfiguration.cs
- Installer.cs
- ScriptContext.cs
- core/shell-builder.cs
- interfaces/ICommandBuilder.cs

**DotNet Commands (25 files):**
- DotNet.cs, DotNet.*.cs, tool/DotNet.*.cs

**Git Commands (17 files):**
- Git.cs, Git.*.cs

**Fzf Commands (10 files):**
- Fzf.cs, Fzf.*.cs

**JSON-RPC (3 files):**
- JsonRpcClient.cs, JsonRpcClientBuilder.cs, IJsonRpcClient.cs

**Native (14 files):**
- Bash.cs, utilities/*, file-system/commands/*, file-system/direct/*, PathResolver.cs

**Testing (4 files):**
- CommandMock.cs, MockScope.cs, MockState.cs, MockSetup.cs

**Extensions (1 file):**
- CommandBuilderExtensions.cs

**Tests (spike files, may be lower priority):**
- cliwrap-exit-code-tests/*.cs (9 files)

## Session

- Created: ses_27dd18c7effe1K4rnFRhnQezjn (2026-04-13)
