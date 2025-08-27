# Debug Mode Rules (Non-Obvious Only)

## Debugging Patterns
- **Test script debugging**: Integration tests are executable `.cs` files that return exit codes - debug by running individual test files directly
- **Script directory context**: Scripts use `[CallerFilePath]` pattern - debug from script directory, not project root
- **Package cache debugging**: Clear `LocalNuGetCache/` directory when testing package changes during development
- **Process execution debugging**: Build scripts use raw `System.Diagnostics.Process` - debug build issues by examining process output directly

## Test Debugging
- **Test discovery**: Tests found via `find Integration/ -name "*.cs" -type f` command - verify test files exist in correct location
- **Exit code debugging**: Tests return exit codes - non-zero indicates failure, examine test output for details
- **Cache debugging**: Use `#:property RestoreNoCache true` in scripts when debugging package loading issues

## Build Debugging
- **Circular dependency avoidance**: Build scripts avoid TimeWarp.Amuru to prevent build failures - debug build by checking if library itself builds first
- **Argument handling**: C# scripts get `--` prefix - debug argument parsing by checking raw dotnet command execution
- **Working directory issues**: Scripts change directory using `[CallerFilePath]` - debug path issues by verifying current directory context