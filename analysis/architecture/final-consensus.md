# Final Architecture Consensus

## Executive Summary

After extensive analysis and review by multiple AIs, we have reached consensus on TimeWarp.Amuru's architecture. The design is intentionally opinionated, prioritizing explicitness and predictability over flexibility and magic.

## Core Decisions - FINAL

### 1. API Design
- **RunAsync()** - Default shell behavior (stream to console, no capture)
- **CaptureAsync()** - Explicit capture (silent execution, full output)
- **Commands/Direct Split** - Intentional separation of mental models
- **No Magic** - Explicit over implicit, always

### 2. Native Commands
- **Namespace Organization** - TimeWarp.Amuru.Native.{File|Directory|Text|Process|System}
- **Dual APIs** - Commands (CommandOutput) and Direct (IAsyncEnumerable<T>)
- **Zero Verbosity** - Via global static usings
- **Hybrid Pattern** - Natives CAN wrap externals (feature, not bug)

### 3. Memory and Streaming
- **OutputLine Structure** - Lazy computation, no duplication
- **Shell Principle** - Stream by default, capture only when explicit
- **Direct for Large Data** - IAsyncEnumerable for LINQ composition
- **Clear Method Semantics** - Names indicate behavior

### 4. Testing
- **Simple Mock with AsyncLocal** - Thread-safe, isolated
- **IDisposable Pattern** - Automatic cleanup
- **No Global State** - Each test isolated
- **DI as Future Option** - Not in core library

### 5. Caching
- **NO CACHING** - Period. End of discussion.
- **User Responsibility** - Trivial in C# if needed
- **No Optional Package** - Not even as extension

### 6. Extensibility
- **Custom Builders** - Via inheritance from CommandBuilder
- **Native/External Hybrids** - Progressive optimization pattern
- **Community Packages** - Encouraged for specialized tools
- **No Auto-Fallback** - Explicit choice always

## What We're NOT Doing

1. **No Unified Native.Builder()** - Commands/Direct split is intentional
2. **No Auto-Fallback Magic** - Users choose explicitly
3. **No Native Mocking Framework** - Use standard .NET tools
4. **No Caching** - Not even optional
5. **No CommandOutput.AsStream()** - Use Direct for streaming
6. **No Automatic Conflict Resolution** - Namespaces handle it

## What We ARE Doing

### Immediate Actions
1. **Implement Namespace-Based Native** - Start with File operations
2. **Document Decision Trees** - When to use Commands vs Direct
3. **Create ADRs** - Formalize major decisions
4. **Add Hybrid Examples** - Show builder wrapper patterns

### Documentation Improvements
1. **Native.Overview.md** - Interactive getting started guide
2. **Error Semantics Guide** - Per API style
3. **Large Data Best Practices** - Emphasize Direct
4. **Testing Native Code** - Using standard .NET mocks

### Code Examples Priority
1. **Global Using Patterns** - All four styles
2. **Hybrid Implementations** - Platform-specific optimization
3. **LINQ Composition** - Direct streaming examples
4. **Custom Builder Creation** - Community contribution guide

## Philosophy Validation

### Core Principles (Unchanged)
- **Simple by Default** - Shell.Builder() just works
- **Progressive Enhancement** - Complexity only when needed
- **No Footguns** - Hence no caching, no auto-fallback
- **Shell-Like Where Appropriate** - But C# where it makes sense
- **Explicit Over Implicit** - No surprising behavior

### Mental Models
- **Commands** = Shell mindset (exit codes, stderr, stdout)
- **Direct** = C# mindset (LINQ, strong types, exceptions)
- **Users Choose** = Via namespace imports, not runtime magic

## Success Metrics

### What Makes This Design Exceptional
1. **Zero Setup** - Natives eliminate external dependencies
2. **Scalable** - Namespace organization prevents bloat
3. **Predictable** - No magic, no surprises
4. **Composable** - Natives, builders, and shell commands work together
5. **Testable** - Simple mocking without complexity
6. **Extensible** - Community can add without forking

### Comparison to Alternatives
- **vs CliWrap** - Better natives and builders
- **vs Cake** - More general-purpose scripting
- **vs PowerShell** - Better C# integration
- **vs Bash in WSL** - Cross-platform native

## Implementation Priority

### Phase 1: Core Native Implementation
- File.Commands.Cat, Head, Tail
- Directory.Commands.Ls, Pwd, Mkdir
- Text.Commands.Grep, Echo
- All with Direct counterparts

### Phase 2: Builder Enhancement
- Git, Docker, Kubectl builders
- Hybrid optimization examples
- Community builder templates

### Phase 3: Documentation and Polish
- Complete ADRs
- Interactive examples
- Migration guides
- Performance benchmarks

## Consensus Statement

All reviewers agree: The design successfully balances shell simplicity with C# power. The intentional constraints (no caching, explicit choices, clear separations) prevent technical debt while enabling powerful patterns (hybrids, LINQ streaming, progressive enhancement).

The architecture is ready for implementation. No further fundamental changes needed.

## Next Step

Begin implementation with Native.File namespace, focusing on:
1. Cat with both Commands and Direct
2. Global using examples
3. Unit tests using standard mocks
4. Documentation with decision tree

This will validate the design and provide a template for remaining implementations.