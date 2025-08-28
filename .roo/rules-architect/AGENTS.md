# Architect Mode Rules (Non-Obvious Only)

## Architecture Constraints
- **Circular dependency prevention**: Build scripts (`Scripts/*.cs`) must use raw `System.Diagnostics.Process` instead of TimeWarp.Amuru to avoid circular dependencies during library builds
- **Script execution architecture**: C# scripts require special argument handling (`--` prefix) to prevent dotnet CLI interception, affecting how scripts are designed and executed
- **Directory management architecture**: All scripts must use `[CallerFilePath]` pattern for relative path resolution, creating a consistent but non-standard execution model

## Design Decisions
- **No caching by design**: TimeWarp.Amuru intentionally does NOT cache command results, requiring users to implement caching explicitly if needed
- **Shell-like default behavior**: `RunAsync()` streams to console by default (80% use case), while `CaptureAsync()` is explicit for output processing
- **Complete output model**: CommandOutput provides stdout, stderr, combined output, and exit code - no information is lost
- **Cancellation first**: All async methods accept CancellationToken as first parameter after instance methods

## Package Management Architecture
- **Local development iteration**: `RestorePackagesPath` set to local directory (`LocalNuGetCache/`) enables rapid development without version pollution
- **Two-step build process**: Build then pack to local feed architecture allows testing changes without version bumps
- **Development package references**: `#:package TimeWarp.Amuru@*-*` enables using latest local builds during development

## Testing Architecture
- **Executable script tests**: Integration tests are executable C# files that return exit codes, using the library itself for test execution
- **Test discovery pattern**: Tests found via shell `find` command rather than framework discovery, enabling flexible test organization
- **Self-testing architecture**: Test runner uses TimeWarp.Amuru itself, creating a dogfooding pattern that validates the library works

## Error Handling Architecture
- **CommandExecutionException by default**: Commands throw exceptions on non-zero exit codes unless validation is disabled
- **Validation control**: `WithNoValidation()` allows graceful degradation for commands that may fail but shouldn't stop execution
- **Complete error information**: All error streams and exit codes are preserved in CommandOutput for comprehensive error analysis