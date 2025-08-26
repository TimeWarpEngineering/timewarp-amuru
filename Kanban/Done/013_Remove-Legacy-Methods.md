# 013 Remove Legacy Methods

## Description

Remove or rename legacy methods that don't fit our new API design. This includes methods that lose data, have misleading names, or implement features we've decided against (like caching).

## Requirements

- Remove methods that lose stderr data
- Remove redundant convenience methods
- Remove ALL caching infrastructure
- Rename misleading method names

## Checklist

### Removal Tasks
- [ ] Remove GetStringAsync() method
  - [ ] This method loses stderr - data loss bug!
  - [ ] Replace usages with CaptureAsync().Result.Stdout
  
- [ ] Remove GetLinesAsync() method
  - [ ] Just GetStringAsync().Split('\n') - redundant
  - [ ] Replace usages with CaptureAsync().Result.Lines
  
- [ ] Remove Cached() method completely
  - [ ] Remove from CommandResult
  - [ ] Remove all caching fields (CachedResult, EnableCaching, etc.)
  - [ ] Remove all caching logic from methods

- [ ] Remove ExecuteAsync() method
  - [ ] Misleading name - actually captures everything
  - [ ] Replace with CaptureAsync() for capture behavior
  - [ ] Or rename to ExecuteInteractiveAsync() if it's for interactive mode

### Renaming Tasks  
- [ ] Review ExecuteInteractiveAsync() 
  - [ ] If it passes through to terminal, rename to PassthroughAsync()
  - [ ] Update all references
  
- [ ] Review GetStringInteractiveAsync()
  - [ ] If it's for selection UI, rename to SelectAsync()
  - [ ] Update all references

### Cleanup
- [ ] Remove any orphaned helper methods
- [ ] Remove caching-related tests
- [ ] Update any error messages mentioning old methods

## Notes

### Migration Examples
```csharp
// Old (problematic)
var output = await Shell.Builder("ls").GetStringAsync();  // Lost stderr!
var lines = await Shell.Builder("ls").GetLinesAsync();    // Lost stderr!
var cached = Shell.Builder("ls").Cached().GetStringAsync(); // NO caching!

// New (clear)
var result = await Shell.Builder("ls").CaptureAsync();
var output = result.Stdout;  // Explicit about what you get
var lines = result.Lines;    // Convenience property
// No caching - user handles if needed
```

## Dependencies

- 012_Implement-Core-Methods.md (need new methods before removing old ones)

## References

- Analysis/Architecture/CachingStrategy.md (NO CACHING decision)
- Kanban/InProgress/010_Redesign-Command-Execution-API.md