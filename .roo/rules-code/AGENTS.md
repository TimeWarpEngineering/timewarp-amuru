# Code Mode Rules (Non-Obvious Only)

## Coding Patterns
- **Build scripts use raw Process**: Never use TimeWarp.Amuru in build scripts (`Scripts/*.cs`) - use `System.Diagnostics.Process` directly to avoid circular dependencies
- **C# script argument handling**: When executing `.cs` files, arguments are prefixed with `--` to prevent dotnet CLI interception
- **CallerFilePath directory pattern**: All scripts must use `[CallerFilePath]` parameter and change directory to script location for relative path resolution
- **Pragma warnings for Process**: Scripts using `System.Diagnostics` must include `#pragma warning disable IDE0005` and `#pragma warning restore IDE0005`

## Package Management
- **Local development packages**: Use `#:package TimeWarp.Amuru@*-*` in scripts for latest local build
- **Cache management**: Set `#:property RestoreNoCache true` for fresh package downloads during development
- **Local feed**: Packages published to `./LocalNuGetFeed/` for development iteration

## File Structure
- **Test scripts**: Integration tests are executable `.cs` files in `Tests/Integration/` that return exit codes
- **Build scripts**: Located in `Scripts/` directory, avoid depending on the library being built
- **Analyzer overrides**: Different analyzer settings apply in `Scripts/`, `Tests/`, `Samples/` directories

## Code Style
- **Bracket style**: Allman style - opening brackets on new line, aligned with parent construct
- **Indentation**: 2 spaces, no tabs
- **Naming**: Private fields use PascalCase (no underscore prefix), file-scoped namespaces
- **Var usage**: Only use `var` when type is apparent from right side