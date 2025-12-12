# 012 Implement Core Methods

## Description

Implement the core command execution methods that provide clear, intuitive behavior matching shell expectations. These methods form the heart of our redesigned API.

## Requirements

- Clear method names that indicate behavior
- All async methods MUST accept CancellationToken
- Proper stdout/stderr handling without data loss
- Stream by default (shell principle)

## Checklist

### Implementation
- [ ] Add RunAsync() method
  - [ ] Stream output to console in real-time
  - [ ] Return only exit code (int)
  - [ ] Accept CancellationToken parameter
  - [ ] This is the DEFAULT for scripts (80% use case)
  
- [ ] Add CaptureAsync() method  
  - [ ] Silent execution (no console output)
  - [ ] Return CommandOutput with all data
  - [ ] Accept CancellationToken parameter
  - [ ] Capture both stdout and stderr properly
  
- [ ] Add StreamStdoutAsync() method
  - [ ] Return IAsyncEnumerable<string>
  - [ ] Stream stdout lines without buffering
  - [ ] Accept CancellationToken parameter
  
- [ ] Add StreamStderrAsync() method
  - [ ] Return IAsyncEnumerable<string>
  - [ ] Stream stderr lines without buffering
  - [ ] Accept CancellationToken parameter
  
- [ ] Add StreamCombinedAsync() method
  - [ ] Return IAsyncEnumerable<OutputLine>
  - [ ] Stream combined output with source info
  - [ ] Accept CancellationToken parameter

- [ ] Add StreamToFileAsync() method
  - [ ] Stream output directly to file
  - [ ] No memory buffering
  - [ ] Accept file path and CancellationToken

### Integration
- [ ] Update RunBuilder to expose new methods
- [ ] Ensure WithTimeout() creates linked CancellationTokenSource
- [ ] Handle cancellation gracefully (SIGTERM then force kill)

### Testing
- [ ] Test RunAsync() streams to console
- [ ] Test CaptureAsync() is silent but captures all
- [ ] Test streaming methods don't buffer
- [ ] Test CancellationToken stops execution
- [ ] Test timeout cancellation works

## Notes

Method behavior summary:
| Method | Console Output | Captures | Returns | Use Case |
|--------|---------------|----------|---------|----------|
| RunAsync() | ✅ Real-time | ❌ | Exit code | Default (80%) |
| CaptureAsync() | ❌ | ✅ Both | CommandOutput | Process output (15%) |
| StreamStdoutAsync() | ❌ | Per line | IAsyncEnumerable | Large files |
| StreamToFileAsync() | ❌ | To file | Task | Huge outputs |

## Dependencies

- 011_Create-CommandOutput-Class.md (must be completed first)

## References

- Analysis/Architecture/CoreShellApi.md
- Analysis/Architecture/MemoryAndStreaming.md