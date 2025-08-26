# TimeWarp.Amuru Architecture Analysis

This folder contains architectural analysis and design documents for TimeWarp.Amuru. These are working documents that will eventually become formal architecture documentation once decisions are finalized.

## Structure

### Core Shell API
- **[CoreShellApi.md](CoreShellApi.md)** - Core command execution API design (RunAsync, CaptureAsync, etc.)
- **[MemoryAndStreaming.md](MemoryAndStreaming.md)** - Memory management and streaming philosophy
- **[TestingStrategy.md](TestingStrategy.md)** - Testing and mocking approaches (simple vs DI)
- **[CachingStrategy.md](CachingStrategy.md)** - Whether and how to implement caching

### Strongly-Typed Commands
- **[StronglyTypedCommands.md](StronglyTypedCommands.md)** - Strongly-typed builders for external tools
- **[NativeCommands-Proposal.md](NativeCommands-Proposal.md)** - In-process implementations of common shell commands
- **[NativeApiDesign.md](NativeApiDesign.md)** - Design decisions for Native API (return types, etc.)
- **[NativeNamespaceDesign.md](NativeNamespaceDesign.md)** - Namespace organization with Commands/Direct classes
- **[BuilderExtensibility.md](BuilderExtensibility.md)** - Creating custom builders and native/external hybrid patterns

### User-Facing Documentation (Proposed)
- **[../ProposedReadme.md](../ProposedReadme.md)** - Original proposed README with all features
- **[../ProposedReadme-Updated.md](../ProposedReadme-Updated.md)** - Updated README using only external tools in examples

## Key Design Decisions Under Consideration

### 1. Core API Methods
- `RunAsync()` - Stream to console (like bash/PowerShell default) - NO CAPTURE
- `CaptureAsync()` - Silent execution with EXPLICIT output capture
- `StreamAsync()` variants - Handle large outputs without memory issues
- Clear naming that indicates behavior
- **Decision**: Follow shell principle - never buffer unless explicitly asked

### 2. Testing Approach
- **DECISION**: Simple mock with AsyncLocal for thread safety - FINAL
- **Implementation**: CommandMock.Enable() with automatic cleanup
- **Benefits**: Test isolation, parallel safety, no global state
- **Future**: DI support as optional package if needed

### 3. Caching
- **DECISION**: No built-in caching - FINAL
- **Rationale**: Shells don't cache, users can trivially cache in C# if needed
- **Principle**: Every command execution is fresh, just like in a shell

### 4. Native Commands
- **Structure**: Namespace organization with Commands/Direct classes
- **Returns**: CommandOutput for consistency and composability
- **Hybrid Pattern**: Natives can wrap externals for optimization (feature, not bug)
- **Scope**: Start with essential commands (Cat, Ls, Grep, etc.)

## Status

These documents represent ongoing analysis based on:
1. Initial design and implementation
2. AI feedback and critiques
3. Real-world usage patterns
4. Comparison with existing shells and scripting languages

Once we finalize decisions, these will be formalized into:
- Architectural Decision Records (ADRs)
- API documentation
- Implementation guides