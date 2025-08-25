# 011 Create CommandOutput Class

## Description

Create the new CommandOutput class that will be returned by CaptureAsync() and other methods. This class should properly capture stdout, stderr, and combined output without data loss, implementing the OutputLine structure with lazy computation as designed in our architecture documents.

## Requirements

- Capture stdout, stderr, and combined output
- Preserve ordering of interleaved stdout/stderr
- No memory duplication through lazy computation
- Remove ALL caching code (NO CACHING decision is final)
- Thread-safe design

## Checklist

### Design
- [ ] Review Analysis/Architecture/MemoryAndStreaming.md for OutputLine design
- [ ] Review Analysis/Architecture/CoreShellApi.md for CommandOutput requirements
- [ ] Ensure NO caching infrastructure remains

### Implementation
- [ ] Create OutputLine class with Text, IsError, and Timestamp properties
- [ ] Create CommandOutput class with:
  - [ ] Private List<OutputLine> for storage
  - [ ] Lazy-computed Stdout property
  - [ ] Lazy-computed Stderr property  
  - [ ] Lazy-computed Combined property
  - [ ] ExitCode property
  - [ ] Success property (ExitCode == 0)
  - [ ] Lines property (convenience for splitting)
- [ ] Remove all Cached() methods from CommandResult
- [ ] Remove all caching-related fields and properties
- [ ] Ensure thread-safe lazy computation

### Testing
- [ ] Test stdout-only capture
- [ ] Test stderr-only capture
- [ ] Test interleaved stdout/stderr preserves order
- [ ] Test lazy computation doesn't duplicate memory
- [ ] Test thread safety of lazy properties

## Notes

This is the foundation for our new API. The OutputLine structure allows us to:
1. Preserve exact output ordering
2. Avoid memory duplication
3. Reconstruct any view (stdout, stderr, combined)
4. Provide structured access for advanced users

Remember: NO CACHING anywhere. Users can cache results themselves in C# if needed.

## Dependencies

None - this is the first task in the migration.

## References

- Analysis/Architecture/MemoryAndStreaming.md
- Analysis/Architecture/CoreShellApi.md
- Analysis/Architecture/CachingStrategy.md (for NO CACHING decision)