# 009 Refactor Fluent API Naming Conventions

## Description

Refactor the fluent API naming conventions to fix semantic inconsistencies and establish clear patterns for command builders. The current `Run()` method name is misleading as it returns a builder rather than executing commands, and it creates naming conflicts with subcommands like `dotnet run`.

## Requirements

- **Breaking Change**: This is an intentional breaking change while in beta to prevent long-term technical debt
- **Consistency**: All command builders should follow the same naming pattern
- **Clarity**: Clear distinction between subcommands and pre-configured options
- **Type Safety**: Return types should help enforce correct usage patterns

## Checklist

### Design
- [ ] Rename static factory methods from `Run()` to `Builder()` for base commands
- [ ] Establish `WithXxx()` pattern for pre-configured global options
- [ ] Maintain verb-only pattern for subcommands
- [ ] Consider Shell class special case (keep `Run()` or change to `Builder()`)
- [ ] Update return types to distinguish subcommand builders from base builders

### Implementation
- [ ] Refactor Fzf.Run() → Fzf.Builder()
- [ ] Refactor Ghq.Run() → Ghq.Builder()
- [ ] Refactor Gwq.Run() → Gwq.Builder()
- [ ] Add DotNet.Builder() for base dotnet commands
- [ ] Add DotNet.WithListSdks(), WithListRuntimes(), WithVersion(), WithInfo()
- [ ] Keep DotNet.Build(), Test(), Run() etc. for subcommands
- [ ] Decide on Shell.Builder() vs Shell.Builder()
- [ ] Update all existing usages in tests and examples
- [ ] Update all existing usages in Scripts/

### Documentation
- [ ] Update README.md with new API examples
- [ ] Update CLAUDE.md with new patterns
- [ ] Document the naming convention rationale
- [ ] Update all code examples in documentation
- [ ] Create migration guide for beta users (if any)

## Notes

### Naming Pattern Decision

**Base Command Builders**: Use `Builder()` method
```csharp
Fzf.Builder()        // Returns FzfBuilder
Ghq.Builder()        // Returns GhqBuilder  
DotNet.Builder()     // Returns DotNetBuilder
```

**Pre-configured Options**: Use `WithXxx()` pattern
```csharp
DotNet.WithListSdks()      // Returns DotNetBuilder with --list-sdks
DotNet.WithVersion()       // Returns DotNetBuilder with --version
```

**Subcommands**: Use verb-only pattern (existing pattern)
```csharp
DotNet.Build()       // Returns DotNetBuildBuilder
DotNet.Test()        // Returns DotNetTestBuilder
DotNet.Run()         // Returns DotNetRunBuilder (for dotnet run subcommand)
```

### Rationale

1. **Semantic Correctness**: `Builder()` accurately describes what the method returns
2. **No Naming Conflicts**: Avoids collision with CLI subcommands
3. **Clear Intent**: `WithXxx()` clearly indicates configuration/options
4. **Type System Support**: Different return types prevent invalid command combinations
5. **Beta Advantage**: Making this breaking change now prevents eternal technical debt

### Special Considerations

- **Shell Class**: Consider keeping `Shell.Builder()` since it's more generic, or change to `Shell.Builder()` for consistency
- **Backwards Compatibility**: This is a breaking change, but justified during beta phase
- **Discovery**: IntelliSense will clearly show the distinction between `Builder()`, `WithXxx()`, and subcommand methods